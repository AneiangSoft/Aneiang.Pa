<p align="center">
    <img src="assets/logo.png" alt="Aneiang.Pa" width="600" style="vertical-align:middle;border-radius:8px;">
</p>

<p align="center">
  A minimal, declarative .NET web scraping library.<br>
  <strong>One line to fetch data, one YAML to add a platform.</strong>
</p>

<p align="center">
  <a href="https://www.nuget.org/packages/Aneiang.Pa"><img src="https://img.shields.io/nuget/v/Aneiang.Pa.svg?style=flat-square&logo=nuget" /></a>
  <a href="https://www.nuget.org/packages/Aneiang.Pa"><img src="https://img.shields.io/nuget/dt/Aneiang.Pa.svg?style=flat-square&logo=nuget" /></a>
  <img src="https://img.shields.io/badge/.NET-netstandard2.1%20%7C%20net8.0-blue?style=flat-square" />
  <img src="https://img.shields.io/badge/platforms-20%20built--in-success?style=flat-square" />
  <img src="https://img.shields.io/badge/license-MIT-green?style=flat-square" />
  <img src="https://img.shields.io/github/stars/AneiangSoft/Aneiang.Pa?style=flat-square" />
</p>

<p align="center">
  <a href="#-hello-world">Quick Start</a> ·
  <a href="#-20-built-in-platforms">Platforms</a> ·
  <a href="#-add-a-platform--0-lines-of-c">Custom Recipe</a> ·
  <a href="#-aspnet-core-integration-5-lines">Web API</a> ·
  <a href="#-production-defaults">Production</a> ·
  <a href="README.md">中文</a>
</p>

---

## 🚀 Hello World

```csharp
using Aneiang.Pa;

var data = await Pa.Source("BaiDu").GetAsync();
foreach (var item in data.Data)
    Console.WriteLine(item["title"]);
```

Strongly typed mapping:

```csharp
var repos = await Pa.Source("Github.Trending").GetAsync<Repo>();

public class Repo
{
    public string? Title { get; set; }
    public string? Url   { get; set; }
    public string? Lang  { get; set; }
}
```

Async streaming:

```csharp
await foreach (var item in Pa.Source("WeiBo").StreamAsync())
    Console.WriteLine(item["title"]);
```

Per-call overrides:

```csharp
var fresh = await Pa.Source("WeiBo")
    .NoCache()                              // bypass cache
    .NoRetry()                              // skip retry
    .WithTimeout(TimeSpan.FromSeconds(10))  // custom timeout
    .WithProxy("http://127.0.0.1:7890")     // temporary proxy
    .GetAsync();
```

---

## 📦 Installation

```bash
# Main package: core engine + 20 built-in platforms (covers 90% of users)
dotnet add package Aneiang.Pa

# Web API integration (optional)
dotnet add package Aneiang.Pa.AspNetCore

# HTTP client SDK (optional)
dotnet add package Aneiang.Pa.Client
```

| Package | Description |
|---------|-------------|
| `Aneiang.Pa` | Core engine + 20 built-in Recipes + 8 middlewares + 4 parsers |
| `Aneiang.Pa.AspNetCore` | `app.MapPaApi()` — one line to REST API |
| `Aneiang.Pa.Client` | HTTP client SDK for calling remote Pa API services |
| `Aneiang.Pa.Abstractions` | Interfaces & models (for extension packages) |

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🧱 **Minimal API** | `Pa.Source("WeiBo").GetAsync()` — one line to fetch |
| 📜 **Declarative Recipe** | YAML / Builder DSL / Attributes — **0 lines of C# to add a platform** |
| 🏭 **20 Built-in Platforms** | 16 news + 2 GitHub + 2 lottery, ready to use |
| 🛤 **Execution Pipeline** | Logging / Cache / Retry / CircuitBreaker / Timeout / RateLimit / Metrics / Tracing |
| 📊 **Observability** | `System.Diagnostics.Metrics` + `ActivitySource`, OpenTelemetry compatible |
| 🌐 **Proxy Pool** | RoundRobin/Random + health tracking + auto-disable on failure |
| 🚀 **ASP.NET Core** | `app.MapPaApi()` — one line to expose REST API |
| 🔌 **Plugin Discovery** | `[assembly: PaScraperModule]` for third-party auto-registration |
| 🤖 **Special Sites** | DouYin Cookie auto-fetch (built-in `DouYinCookieMiddleware`) |

---

## 🏪 20 Built-in Platforms

| Category | Source | Description |
|----------|--------|-------------|
| **News** | `WeiBo` `ZhiHu` `Bilibili` `BaiDu` `DouYin` `TouTiao` | Weibo / Zhihu / Bilibili / Baidu / Douyin / Toutiao |
| | `HuPu` `Tencent` `JueJin` `ThePaper` `DouBan` `IFeng` | Hupu / Tencent / Juejin / ThePaper / Douban / IFeng |
| | `Csdn` `CnBlog` `ItHome` `_36kr` | CSDN / CnBlog / ItHome / 36kr |
| **GitHub** | `Github.Trending` `Github.Releases` | GitHub Trending / dotnet/runtime Releases |
| **Lottery** | `Lottery.SSQ` `Lottery.DLT` | Welfare / Sport Lottery |

```csharp
foreach (var r in Pa.Sources())
    Console.WriteLine($"[{r.Category}] {r.Name} — {r.DisplayName}");
```

---

## 📜 Add a Platform — 0 Lines of C#

`recipes/news/hackernews.yaml`:

```yaml
name: HackerNews
category: News
display_name: Hacker News
fetch:
  url: https://news.ycombinator.com/
parse:
  type: html
  container: "tr.athing"
  fields:
    title:  { selector: "span.titleline a", trim: true }
    url:    { selector: "span.titleline a", attr: href }
    rank:   { selector: "span.rank" }
```

```csharp
Pa.Configure(c => c.UseRecipesFolder("./recipes"));
var data = await Pa.Source("HackerNews").GetAsync();
```

---

## 🛠 Four Recipe Sources

| Source | Use Case | How |
|--------|----------|-----|
| **Built-in YAML** | Out-of-box | 20 platforms embedded in DLL |
| **External YAML/JSON** | Config-driven, community sharing | `Pa.Configure(c => c.UseRecipesFolder(...))` |
| **Builder DSL** | Dynamic, runtime definition | `Pa.Define("Name", b => b.Get(...).ParseHtml(...))` |
| **Attributes** | Strongly typed, IDE friendly | `[Recipe][Get][Container][Selector]` annotated class |

### Builder DSL Example

```csharp
Pa.Define("MyAPI", b => b
    .Category("Custom")
    .Get("https://api.example.com/items?page={pageNo}")
    .Header("Authorization", "Bearer xxx")
    .ParseJson(p => p
        .Items("$.data")
        .Field("id", "id")
        .Field("title", "title")));

var data = await Pa.Source("MyAPI").WithPaging(1, 30).GetAsync();
```

### Attribute Example

```csharp
[Recipe("MyTrending", Category = "Tech")]
[Get("https://github.com/trending")]
[Container("article.Box-row")]
public class Repo
{
    [Selector("h2 a")]                                    public string? Title { get; set; }
    [Selector("h2 a", Attr = "href", Base = "https://github.com")] public string? Url { get; set; }
    [Selector("span[itemprop=programmingLanguage]")]      public string? Lang { get; set; }
}

Pa.DiscoverFromLoadedAssemblies();
var data = await Pa.Source("MyTrending").GetAsync<Repo>();
```

---

## 🛡 Production Defaults

| Middleware | Default | Override |
|-----------|---------|----------|
| Logging | `ILogger<>` injected | No injection = no-op |
| Metrics | Meter `Aneiang.Pa` | `Pa.Configure(c => c.Options.EnableMetrics = false)` |
| Tracing | ActivitySource `Aneiang.Pa` | Same `EnableTracing` |
| Cache | Memory 5min | `.NoCache()` per call |
| Retry | 3 attempts, exponential backoff | `.NoRetry()` per call |
| Timeout | 30s | `.WithTimeout(TimeSpan)` |
| RateLimit | Disabled | Enable + configure |
| CircuitBreaker | 5 failures → 60s break | Automatic |
| DouYinCookie | Auto-fetch DouYin cookie | Only for `DouYin` recipe |

### Pipeline Order

```
Request → Logging → Metrics → Tracing → Cache → RateLimit → CircuitBreaker → Retry → Timeout → DouYinCookie → Fetch → Parse → Response
```

---

## 🚀 ASP.NET Core Integration (5 lines)

```csharp
using Aneiang.Pa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPa(opt => opt.DefaultCacheDuration = TimeSpan.FromMinutes(5));

var app = builder.Build();
app.MapPaApi();
app.Run();
```

Exposes:

| Endpoint | Description |
|----------|-------------|
| `GET /api/pa/sources` | List all sources |
| `GET /api/pa/source/{name}?noCache=1` | Fetch data |
| `GET /api/pa/health` | Health check |

---

## 🤖 Special Site Handling

### DouYin (TikTok China)

DouYin's API requires visiting `login.douyin.com` first to get a Cookie, otherwise 403. Aneiang.Pa has a **built-in `DouYinCookieMiddleware`** that handles this automatically:

```csharp
var data = await Pa.Source("DouYin").GetAsync();  // zero config
```

### Custom Middleware

For other sites requiring special handling (signatures, tokens, sessions, etc.), write a middleware:

```csharp
public class MyAuthMiddleware : IScrapeMiddleware
{
    public int Order => 5;
    public string Name => "MyAuth";

    public async Task<ScrapeResult> InvokeAsync(ScrapeContext ctx, ScrapeDelegate next)
    {
        if (ctx.Recipe.Name == "MySite")
            ctx.Variables["token"] = await GetTokenAsync();
        return await next(ctx);
    }
}

Pa.Configure(c => c.UseMiddleware(new MyAuthMiddleware()));
```

Use `{token}` placeholder in Recipe:

```yaml
headers:
  Authorization: "Bearer {token}"
```

---

## 📁 Project Structure

```
src/
├── Aneiang.Pa.Abstractions/    ← Interfaces & models (standalone package)
├── Aneiang.Pa/                 ← Core engine + 20 built-in YAML + 8 middlewares
├── Aneiang.Pa.AspNetCore/      ← MapPaApi() one-line integration
└── Aneiang.Pa.Client/          ← HTTP client SDK

samples/
├── 01-hello-world/             ← Full capability demo
├── 02-custom-recipe/           ← Three ways to define custom Recipe
├── 03-asp-net-core/            ← Web API integration
└── 04-douyin-demo/             ← DouYin (special site handling)

tests/
└── Aneiang.Pa.Tests/           ← 15 unit tests
```

---

## 📊 Observability

Aneiang.Pa automatically produces structured logs, Metrics, and Tracing — compatible with Grafana + Prometheus + Jaeger:

| Metric | Type | Labels |
|--------|------|--------|
| `pa_scrape_total` | Counter | name, category, status |
| `pa_scrape_duration_seconds` | Histogram | name, category |
| `pa_scrape_items_total` | Counter | name, category |

```csharp
// User code
var data = await Pa.Source("WeiBo").GetAsync();

// Framework auto-produces:
// - Log:    [Pa] Start WeiBo / [Pa] OK WeiBo Items=50 Elapsed=842ms
// - Metric: pa_scrape_total{name="WeiBo",status="ok"} +1
// - Trace:  Activity "Pa WeiBo" with duration
```

---

## ⚠️ Disclaimer

> **Recommended scraping interval ≥ 5 minutes** to avoid IP blocking.
>
> **Scraped data is for personal learning, research, or public welfare only. Commercial sale, attacks, or any illegal activities are strictly prohibited.**

---

## 🤝 Contributing

- PRs and Issues are welcome, especially for new platform Recipes (YAML only, no C# required)
- Please maintain consistent code style and include brief descriptions and necessary tests
- If you wish to publish your new platform in the NuGet package, please discuss the approach in an Issue first

### Contributing a Recipe in 3 Steps

1. Create a `.yaml` file in the `recipes/` directory
2. Fill in `name` / `fetch` / `parse` sections
3. Submit a PR

---

## 📄 License

Aneiang.Pa is licensed under the [MIT License](LICENSE).
