Param(
    [string]$ApiKey = $env:NUGET_API_KEY,
    [string]$Source = "https://api.nuget.org/v3/index.json",
    [string]$Configuration = "Release",
    [string]$Version,
    [switch]$PackOnly
)

# Pack and push all packable projects under the src directory.
# Usage:
#   $env:NUGET_API_KEY = "<your-key>"
#   ./scripts/publish.ps1 [-ApiKey xxx] [-Source url] [-Configuration Release] [-Version 1.0.0] [-PackOnly]

if (-not $ApiKey -and -not $PackOnly) {
    Write-Error "Missing NuGet ApiKey. Pass -ApiKey or set env NUGET_API_KEY."
    exit 1
}

$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

$output = Join-Path $repoRoot "nupkgs"
New-Item -ItemType Directory -Force -Path $output | Out-Null
Get-ChildItem $output -Recurse -Include *.nupkg, *.snupkg -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue

# Get all projects from the 'src' directory
$srcPath = Join-Path $repoRoot "src"
$csprojs = Get-ChildItem -Path $srcPath -Recurse -Filter *.csproj |
    Where-Object {
        $_.FullName -notmatch '\\test\\' -and
        $_.FullName -notmatch '\\Aneiang\.Pa\.McpServer\\Aneiang\.Pa\.McpServer\.csproj$'
    }

if (-not $csprojs) {
    Write-Error "No packable projects found in the 'src' directory."
    exit 1
}

foreach ($proj in $csprojs) {
    Write-Host "Packing $($proj.FullName)..."
    $packArgs = @("pack", $proj.FullName, "-c", $Configuration, "-o", $output, "--nologo")
    if ($Version) { $packArgs += "-p:Version=$Version" }
    dotnet @packArgs
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

$packages = Get-ChildItem $output -Filter *.nupkg -ErrorAction SilentlyContinue
if (-not $packages) {
    Write-Error "No nupkg generated."
    exit 1
}

if ($PackOnly) {
    Write-Host "[OK] Pack done (PackOnly). Output: $output"
    exit 0
}

foreach ($pkg in $packages) {
    Write-Host "Pushing $($pkg.Name)..."
    dotnet nuget push $pkg.FullName --skip-duplicate --source $Source --api-key $ApiKey
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Write-Host "[OK] Pack and push done. Output: $output"
