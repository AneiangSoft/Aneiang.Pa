using Aneiang.Pa;
using Aneiang.Pa.Abstractions;

/*
 * Aneiang.Pa 4.0 — 全能力展示 Demo
 *
 * 本 Demo 覆盖 v4 的所有对外能力：
 *   ① 静态门面 Pa.Source(name).GetAsync()
 *   ② 强类型映射 GetAsync<T>()
 *   ③ 异步流式 StreamAsync()
 *   ④ Pa.Configure 全局配置（选项 / 中间件 / HttpHandler / Recipe 来源）
 *   ⑤ 四种 Recipe 来源：YAML 文件夹、Builder DSL、特性标注、内置嵌入
 *   ⑥ Fluent 句柄选项：With / WithPaging / NoCache / NoRetry / WithProxy / WithTimeout
 *   ⑦ 列出所有已注册 Recipe (Pa.Sources)
 *   ⑧ 自定义中间件 IScrapeMiddleware
 *   ⑨ ScrapeResult 元数据
 *   ⑩ 错误处理
 */

Console.WriteLine("======================================================");
Console.WriteLine("  Aneiang.Pa 4.0 — 全能力 Demo");
Console.WriteLine("======================================================\n");

// ─────────────────────────────────────────────────────────────────────────────
// ① Pa.Configure — 全局配置（选项 + 中间件 + Recipe 来源）
// ─────────────────────────────────────────────────────────────────────────────
Pa.Configure(c =>
{
    // 1.1 选项调整
    c.Options.DefaultTimeout = TimeSpan.FromSeconds(15);
    c.Options.DefaultRetryCount = 2;
    c.Options.DefaultCacheDuration = TimeSpan.FromMinutes(3);
    c.Options.EnableMetrics = true;
    c.Options.EnableTracing = true;

    // 1.2 注入自定义中间件（演示用：打印每次调用）
    c.UseMiddleware(new AuditMiddleware());

    // 1.3 加载外部 YAML 文件夹（如果存在）
    var folder = Path.Combine(AppContext.BaseDirectory, "recipes");
    if (Directory.Exists(folder)) c.UseRecipesFolder(folder);

    // 1.4 自定义 HttpHandler（演示禁用 cookie 容器，避免不同 Recipe 互相污染）
    c.UseHttpHandler(() => new HttpClientHandler { UseCookies = false });
});

// ─────────────────────────────────────────────────────────────────────────────
// ② Builder DSL 动态注册 Recipe（无需文件、无需类）
//    这里演示如何用 Builder 定义一个自定义仓库的 Releases（与内置 Github.Releases 区分）
// ─────────────────────────────────────────────────────────────────────────────
Pa.Define("Github.AspNetCore.Releases", b => b
    .Category("Demo")
    .DisplayName("ASP.NET Core Releases")
    .Get("https://api.github.com/repos/dotnet/aspnetcore/releases")
    .WithUserAgent("Aneiang.Pa/4.0")
    .Header("Accept", "application/vnd.github+json")
    .ParseJson(p => p
        .Items("$")
        .Field("tag", "tag_name")
        .Field("name", "name")
        .Field("publishedAt", "published_at")
        .Field("htmlUrl", "html_url")));

// ─────────────────────────────────────────────────────────────────────────────
// ③ 特性标注 Recipe — 一键自动发现
// ─────────────────────────────────────────────────────────────────────────────
Pa.DiscoverFromLoadedAssemblies();

// ─────────────────────────────────────────────────────────────────────────────
// ④ 列出所有 Recipe（内置 + Builder + 特性）
// ─────────────────────────────────────────────────────────────────────────────
Section("① 所有已注册 Recipe");
var groups = Pa.Sources()
    .OrderBy(s => s.Category)
    .ThenBy(s => s.Name)
    .GroupBy(s => s.Category ?? "(none)");
foreach (var g in groups)
{
    Console.WriteLine($"  [{g.Key}]");
    foreach (var r in g)
        Console.WriteLine($"    {r.Name,-22} {r.DisplayName}");
}
Console.WriteLine($"  共 {Pa.Sources().Count} 个\n");

// ─────────────────────────────────────────────────────────────────────────────
// ⑤ 基础调用：动态字典
// ─────────────────────────────────────────────────────────────────────────────
Section("② 基础调用 — 百度热榜");
var baidu = await Pa.Source("BaiDu").GetAsync();
PrintResult(baidu, item => $"{item.GetValueOrDefault("title")}");

// ─────────────────────────────────────────────────────────────────────────────
// ⑥ 强类型映射 GetAsync<T> — 使用内置 Github.Releases
// ─────────────────────────────────────────────────────────────────────────────
Section("③ 强类型映射 — 内置 Github.Releases (dotnet/runtime)");
var releases = await Pa.Source("Github.Releases").GetAsync<DotnetRelease>();
if (releases.IsSuccess)
{
    foreach (var r in releases.Data.Take(5))
        Console.WriteLine($"  [{r.Tag,-12}] {r.Name}  by {r.Author}  @ {r.PublishedAt}");
}
else Console.WriteLine($"  → {releases.ErrorMessage}");

Section("③.5 Builder DSL — 自定义仓库 Github.AspNetCore.Releases");
var aspRel = await Pa.Source("Github.AspNetCore.Releases").GetAsync<DotnetRelease>();
if (aspRel.IsSuccess)
{
    foreach (var r in aspRel.Data.Take(3))
        Console.WriteLine($"  [{r.Tag,-12}] {r.Name}  @ {r.PublishedAt}");
}
else Console.WriteLine($"  → {aspRel.ErrorMessage}");

// ─────────────────────────────────────────────────────────────────────────────
// ⑦ 特性标注 Recipe 调用（来自下面 TrendingRow 类）
// ─────────────────────────────────────────────────────────────────────────────
Section("④ 特性标注 — GitHub Trending");
var trending = await Pa.Source("Trending.Attr").GetAsync<TrendingRow>();
if (trending.IsSuccess)
{
    foreach (var t in trending.Data.Take(5))
        Console.WriteLine($"  [{t.Lang,-12}] {t.Title}");
}
else Console.WriteLine($"  → {trending.ErrorMessage}");

// ─────────────────────────────────────────────────────────────────────────────
// ⑧ Fluent 句柄：NoCache / NoRetry / WithTimeout / WithProxy / With(自定义变量)
// ─────────────────────────────────────────────────────────────────────────────
Section("⑤ Fluent 句柄选项");
Console.WriteLine("  → 单次跳过缓存 + 自定义超时 + 自定义变量");
var fluent = await Pa.Source("BaiDu")
    .NoCache()
    .NoRetry()
    .WithTimeout(TimeSpan.FromSeconds(10))
    .With("foo", "bar")
    .GetAsync();
Console.WriteLine($"  结果: IsSuccess={fluent.IsSuccess}, 条目数={fluent.Data.Count}\n");

// ─────────────────────────────────────────────────────────────────────────────
// ⑨ 异步流式 StreamAsync（自动展平为 IAsyncEnumerable）
// ─────────────────────────────────────────────────────────────────────────────
Section("⑥ 异步流式枚举 StreamAsync");
var i = 0;
await foreach (var item in Pa.Source("BaiDu").StreamAsync())
{
    if (++i > 3) break;
    Console.WriteLine($"  {i}. {item.GetValueOrDefault("title")}");
}
Console.WriteLine();

// ─────────────────────────────────────────────────────────────────────────────
// ⑩ 缓存命中演示（第二次调用很快）
// ─────────────────────────────────────────────────────────────────────────────
Section("⑦ 缓存中间件验证");
var sw = System.Diagnostics.Stopwatch.StartNew();
await Pa.Source("BaiDu").GetAsync();
Console.WriteLine($"  第二次调用 BaiDu（应命中缓存）: {sw.ElapsedMilliseconds}ms\n");

// ─────────────────────────────────────────────────────────────────────────────
// ⑪ 错误处理：访问不存在的 Recipe
// ─────────────────────────────────────────────────────────────────────────────
Section("⑧ 错误处理");
try
{
    await Pa.Source("__not_exists__").GetAsync();
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"  捕获到异常: {ex.Message}\n");
}

Console.WriteLine("======================================================");
Console.WriteLine("  Demo 结束。覆盖能力：");
Console.WriteLine("  ✓ 静态门面 Pa.Source(name).GetAsync()");
Console.WriteLine("  ✓ 强类型 GetAsync<T>()");
Console.WriteLine("  ✓ 异步流式 StreamAsync()");
Console.WriteLine("  ✓ Pa.Configure（选项 / 中间件 / HttpHandler / Recipe 来源）");
Console.WriteLine("  ✓ 四种 Recipe 来源：内置 / YAML 文件夹 / Builder DSL / 特性标注");
Console.WriteLine("  ✓ Fluent 句柄：With/WithPaging/NoCache/NoRetry/WithTimeout/WithProxy");
Console.WriteLine("  ✓ 自定义 IScrapeMiddleware");
Console.WriteLine("  ✓ Pa.Sources() 列举 / Pa.Options 读取");
Console.WriteLine("  ✓ 缓存中间件 / 重试中间件");
Console.WriteLine("  ✓ 错误处理");
Console.WriteLine("======================================================");

// ─────────────────────────────────────────────────────────────────────────────
// 辅助方法 / 类型
// ─────────────────────────────────────────────────────────────────────────────
static void Section(string title)
{
    Console.WriteLine($"\n── {title} ──────────────────────────────────────");
}

static void PrintResult(ScrapeResult result, Func<IReadOnlyDictionary<string, string?>, string> format)
{
    if (!result.IsSuccess)
    {
        Console.WriteLine($"  → {result.ErrorMessage}");
        return;
    }
    Console.WriteLine($"  成功 {result.Data.Count} 条:");
    foreach (var item in result.Data.Take(5))
        Console.WriteLine($"    - {format(item)}");
}

/// <summary>强类型模型：GitHub Releases</summary>
public class DotnetRelease
{
    public string? Tag { get; set; }
    public string? Name { get; set; }
    public string? Author { get; set; }
    public string? PublishedAt { get; set; }
    public string? HtmlUrl { get; set; }
}

/// <summary>特性标注 Recipe：GitHub Trending</summary>
[Recipe("Trending.Attr", Category = "Demo", DisplayName = "GitHub Trending（特性）")]
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

/// <summary>自定义审计中间件：演示如何在管道里插入逻辑</summary>
public sealed class AuditMiddleware : IScrapeMiddleware
{
    public int Order => 1; // 紧跟 Logging（0）
    public string Name => "Audit";

    public async Task<ScrapeResult> InvokeAsync(ScrapeContext ctx, ScrapeDelegate next)
    {
        var start = DateTime.Now;
        var result = await next(ctx);
        Console.WriteLine($"  [Audit] {ctx.Recipe.Name,-18} @ {start:HH:mm:ss.fff} → IsSuccess={result.IsSuccess}, Items={result.Data.Count}");
        return result;
    }
}
