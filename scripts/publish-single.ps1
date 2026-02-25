Param(
    [string]$ApiKey = $env:NUGET_API_KEY,
    [string]$Source = "https://api.nuget.org/v3/index.json",
    [string]$Configuration = "Release",
    [string]$Version,

    # One of these should be provided
    [string]$Project,
    [string]$PackageId,

    # Optional: only pack without push
    [switch]$PackOnly,

    # Optional: include symbols
    [switch]$IncludeSymbols,

    # Optional: skip duplicates when pushing
    [switch]$SkipDuplicate = $true,

    # Optional: publish self-contained zip
    [switch]$SelfContained,
    [string]$RuntimeIdentifier = "win-x64",
    [switch]$SingleFile = $true,
    [switch]$Trimmed = $false,
    [string]$PublishOutput,
    [switch]$Zip = $true
)

# Pack and (optionally) push a single project/package.
# Also supports producing a self-contained publish folder (and zip) for non-.NET users.
#
# Usage examples:
#   $env:NUGET_API_KEY = "<your-key>"
#   ./scripts/publish-single.ps1 -Project "src/Aneiang.Pa.McpServer/Aneiang.Pa.McpServer.csproj"
#   ./scripts/publish-single.ps1 -PackageId "Aneiang.Pa.McpServer"
#   ./scripts/publish-single.ps1 -Project "src/Aneiang.Pa.McpServer/Aneiang.Pa.McpServer.csproj" -Version 0.1.0
#   ./scripts/publish-single.ps1 -Project "...csproj" -PackOnly
#
# Self-contained publish examples:
#   ./scripts/publish-single.ps1 -Project "src/Aneiang.Pa.McpServer/Aneiang.Pa.McpServer.csproj" -SelfContained -RuntimeIdentifier win-x64
#   ./scripts/publish-single.ps1 -PackageId "Aneiang.Pa.McpServer" -SelfContained -RuntimeIdentifier win-x64 -Version 0.1.0
#   ./scripts/publish-single.ps1 -Project "...csproj" -SelfContained -RuntimeIdentifier osx-arm64 -Zip

if (-not $ApiKey -and -not $PackOnly) {
    # Pack-only or self-contained-only doesn't require ApiKey
    if (-not $SelfContained) {
        Write-Error "Missing NuGet ApiKey. Pass -ApiKey or set env NUGET_API_KEY."
        exit 1
    }
}

$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

$output = Join-Path $repoRoot "nupkgs"
New-Item -ItemType Directory -Force -Path $output | Out-Null

function Find-ProjectByPackageId([string]$id) {
    $srcPath = Join-Path $repoRoot "src"
    $csprojs = Get-ChildItem -Path $srcPath -Recurse -Filter *.csproj | Where-Object { $_.FullName -notmatch '\\test\\' }
    foreach ($proj in $csprojs) {
        try {
            $xml = [xml](Get-Content -Raw -Path $proj.FullName)
            $pgs = @($xml.Project.PropertyGroup)
            foreach ($pg in $pgs) {
                if ($pg.PackageId -and $pg.PackageId.'#text' -eq $id) {
                    return $proj.FullName
                }
            }
        }
        catch {
            # ignore unreadable csproj
        }
    }
    return $null
}

if (-not $Project) {
    if ($PackageId) {
        $Project = Find-ProjectByPackageId $PackageId
        if (-not $Project) {
            Write-Error "Could not find a csproj under src/ with <PackageId>$PackageId</PackageId>. Pass -Project explicitly."
            exit 1
        }
    }
    else {
        Write-Error "You must provide either -Project <path-to-csproj> or -PackageId <id>."
        exit 1
    }
}

if (-not (Test-Path $Project)) {
    Write-Error "Project not found: $Project"
    exit 1
}

function Get-ProjectNameFromPath([string]$path) {
    return [System.IO.Path]::GetFileNameWithoutExtension($path)
}

$projectName = Get-ProjectNameFromPath $Project

# Self-contained publish path
if ($SelfContained) {
    if (-not $PublishOutput) {
        $PublishOutput = Join-Path $repoRoot ("artifacts/" + $projectName + "-" + $RuntimeIdentifier)
    }

    Write-Host "Publishing self-contained: $Project"
    Write-Host "  RID: $RuntimeIdentifier"
    Write-Host "  Output: $PublishOutput"

    $publishArgs = @(
        "publish", $Project,
        "-c", $Configuration,
        "-r", $RuntimeIdentifier,
        "--self-contained", "true",
        "-o", $PublishOutput,
        "--nologo"
    )

    if ($Version) { $publishArgs += "-p:Version=$Version" }

    if ($SingleFile) { $publishArgs += "-p:PublishSingleFile=true" }
    else { $publishArgs += "-p:PublishSingleFile=false" }

    if ($Trimmed) { $publishArgs += "-p:PublishTrimmed=true" }

    dotnet @publishArgs
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    if ($Zip) {
        $zipPath = $PublishOutput.TrimEnd('\\','/') + ".zip"
        if (Test-Path $zipPath) { Remove-Item -Force $zipPath -ErrorAction SilentlyContinue }

        Write-Host "Zipping to: $zipPath"
        Compress-Archive -Path (Join-Path $PublishOutput '*') -DestinationPath $zipPath -Force

        Write-Host "[OK] Self-contained publish + zip done. Output: $PublishOutput"
        Write-Host "[OK] Zip: $zipPath"
    }
    else {
        Write-Host "[OK] Self-contained publish done. Output: $PublishOutput"
    }

    # If SelfContained is requested, we still allow packing/pushing unless user explicitly asked PackOnly.
}

Write-Host "Packing $Project..."
$packArgs = @(
    "pack", $Project,
    "-c", $Configuration,
    "-o", $output,
    "--nologo"
)
if ($Version) { $packArgs += "-p:Version=$Version" }
if ($IncludeSymbols) { $packArgs += "-p:IncludeSymbols=true"; $packArgs += "-p:SymbolPackageFormat=snupkg" }

dotnet @packArgs
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Find resulting nupkg (prefer matching PackageId if provided)
$packages = Get-ChildItem $output -Filter *.nupkg -ErrorAction SilentlyContinue | Where-Object { $_.Name -notmatch '\.symbols\.nupkg$' }
if (-not $packages) {
    Write-Error "No nupkg generated in $output."
    exit 1
}

$pkgToPush = $null
if ($PackageId) {
    $pkgToPush = $packages | Where-Object { $_.Name -like "$PackageId.*.nupkg" } | Select-Object -First 1
}
if (-not $pkgToPush) {
    # If only one exists, use it; otherwise pick the newest
    if ($packages.Count -eq 1) {
        $pkgToPush = $packages[0]
    }
    else {
        $pkgToPush = $packages | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    }
}

Write-Host "Generated package: $($pkgToPush.FullName)"

if ($PackOnly) {
    Write-Host "[OK] Pack done (PackOnly). Output: $output"
    exit 0
}

Write-Host "Pushing $($pkgToPush.Name)..."
$pushArgs = @(
    "nuget", "push", $pkgToPush.FullName,
    "--source", $Source,
    "--api-key", $ApiKey
)
if ($SkipDuplicate) { $pushArgs += "--skip-duplicate" }

dotnet @pushArgs
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "[OK] Pack and push single package done. Output: $output"
