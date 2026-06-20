using Aneiang.Pa;

Console.WriteLine("=== Aneiang.Pa 4.0 — 自定义 Recipe 三种方式 ===\n");

// 方式 1：从外部 YAML 文件夹加载
Pa.Configure(c => c
    .UseRecipesFolder(Path.Combine(AppContext.BaseDirectory, "recipes"))
);

// 方式 2：Builder DSL 内联定义
Pa.Define("Github.Releases", b => b
    .Category("Demo")
    .DisplayName(".NET Releases")
    .Get("https://api.github.com/repos/dotnet/runtime/releases")
    .WithUserAgent("Aneiang.Pa/4.0")
    .Header("Accept", "application/vnd.github+json")
    .ParseJson(p => p
        .Items("$")
        .Field("tag", "tag_name")
        .Field("name", "name")
        .Field("publishedAt", "published_at")));

// 方式 3：[Recipe] 特性类型自动发现
Pa.DiscoverFromLoadedAssemblies();

Console.WriteLine("已注册 Recipe（含三种来源 + 内置）：");
foreach (var r in Pa.Sources().OrderBy(x => x.Name))
    Console.WriteLine($"  - {r.Name,-22} {r.Category}/{r.DisplayName}");
Console.WriteLine();

// 测试 Builder DSL 注册的源
Console.WriteLine("→ 抓取 dotnet/runtime 最近 3 个 release：");
var rel = await Pa.Source("Github.Releases").GetAsync();
if (rel.IsSuccess)
{
    foreach (var item in rel.Data.Take(3))
        Console.WriteLine($"  [{item["tag"]}] {item["name"]}  @ {item["publishedAt"]}");
}
else
{
    Console.WriteLine($"  失败：{rel.ErrorMessage}");
}

Console.WriteLine();

// 测试 [Recipe] 特性
Console.WriteLine("→ 抓取 GitHub Trending（特性方式）：");
var trending = await Pa.Source("Trending.Attr").GetAsync<TrendingRow>();
if (trending.IsSuccess)
{
    foreach (var t in trending.Data.Take(3))
        Console.WriteLine($"  [{t.Lang,-12}] {t.Title}");
}

[Recipe("Trending.Attr", Category = "Demo", DisplayName = "Trending（特性）")]
[Get("https://github.com/trending")]
[Container("article.Box-row")]
public class TrendingRow
{
    [Selector("h2 a")]
    public string? Title { get; set; }

    [Selector("h2 a", Attr = "href", Base = "https://github.com")]
    public string? Url { get; set; }

    [Selector("span[itemprop=programmingLanguage]")]
    public string? Lang { get; set; }
}
