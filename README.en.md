<p align="center">
    <img src="assets/logo.png" alt="Aneiang.Pa" width="600" style="vertical-align:middle;border-radius:8px;">
</p>

<p align="center">
  A minimal, declarative .NET web scraping library. One line to fetch data, one YAML to add a platform.
</p>

<p align="center">
  <a href="README.md">中文</a> | English
</p>

<p align="center">
  <a href="https://www.nuget.org/packages/Aneiang.Pa"><img src="https://img.shields.io/nuget/v/Aneiang.Pa.svg?style=flat-square&logo=nuget" /></a>
  <a href="https://www.nuget.org/packages/Aneiang.Pa"><img src="https://img.shields.io/nuget/dt/Aneiang.Pa.svg?style=flat-square&logo=nuget" /></a>
  <img src="https://img.shields.io/badge/target-netstandard2.1%20%7C%20net8.0-blue?style=flat-square" />
  <img src="https://img.shields.io/badge/status-active-success?style=flat-square" />
  <img src="https://img.shields.io/github/stars/AneiangSoft/Aneiang.Pa" />
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
    public string? Url { get; set; }
    public string? Lang { get; set; }
}
```

---

## 📦 Installation

```bash
# Single package covers 90% of users (18 built-in platforms)
dotnet add package Aneiang.Pa

# Web API integration
dotnet add package Aneiang.Pa.AspNetCore

# HTTP client SDK
dotnet add package Aneiang.Pa.Client
```

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🧱 **Minimal API** | `Pa.Source("WeiBo").GetAsync()` — one line to fetch |
| 📜 **Declarative Recipe** | YAML / Builder DSL / Attributes — **0 lines of C# to add a platform** |
| 🏭 **18 Built-in Platforms** | 16 news + 2 lottery, ready to use |
| 🛤 **Execution Pipeline** | Logging / Cache / Retry / CircuitBreaker / Timeout / RateLimit / Metrics / Tracing |
| 📊 **Observability** | `System.Diagnostics.Metrics` + `ActivitySource`, OpenTelemetry compatible |
| 🌐 **Proxy Pool** | RoundRobin/Random + health tracking + auto-disable on failure |
| 🚀 **ASP.NET Core** | `app.MapPaApi()` — one line to expose REST API |
| 🔌 **Plugin Discovery** | `[assembly: PaScraperModule]` for third-party auto-registration |

---

## 🏪 18 Built-in Platforms

| Category | Source | Description |
|----------|--------|-------------|
| News | WeiBo / ZhiHu / Bilibili / BaiDu / DouYin / TouTiao | Weibo / Zhihu / Bilibili / Baidu / Douyin / Toutiao |
| News | HuPu / Tencent / JueJin / ThePaper / DouBan / IFeng | Hupu / Tencent / Juejin / ThePaper / Douban / IFeng |
| News | Csdn / CnBlog / ItHome / _36kr | CSDN / CnBlog / ItHome / 36kr |
| Lottery | Lottery.SSQ / Lottery.DLT | Welfare / Sport Lottery |

```csharp
foreach (var r in Pa.Sources())
    Console.WriteLine($"{r.Name} - {r.Category}/{r.DisplayName}");
```

---

## 📜 Add a Platform — 0 Lines of C#

`recipes/news/cnbeta.yaml`:

```yaml
name: CnBeta
category: News
display_name: CnBeta Headlines
fetch:
  url: https://www.cnbeta.com.tw/
parse:
  type: html
  container: ".items-area .item"
  fields:
    title:  { selector: "h2 a", trim: true }
    url:    { selector: "h2 a", attr: href }
    desc:   { selector: ".desc" }
```

```csharp
Pa.Configure(c => c.UseRecipesFolder("./recipes"));
var data = await Pa.Source("CnBeta").GetAsync();
```

---

## 🛠 Four Recipe Sources

| Source | Use Case | How |
|--------|----------|-----|
| **YAML/JSON** | Config-driven, community sharing | File + `Pa.Configure(c => c.UseRecipesFolder(...))` |
| **Builder DSL** | Dynamic, runtime definition | `Pa.Define("Name", b => b.Get(...).ParseHtml(...))` |
| **Attributes** | Strongly typed, IDE friendly | `[Recipe][Get][Container][Selector]` annotated class |
| **Built-in YAML** | Out-of-box | 18 platforms embedded in the DLL |

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
- `GET /api/pa/sources` — list all sources
- `GET /api/pa/source/{name}?noCache=1` — fetch data
- `GET /api/pa/health` — health check

---

## 📁 Project Structure

```
src/
├── Aneiang.Pa.Abstractions/    ← Interfaces & models (standalone package)
├── Aneiang.Pa/                 ← Core engine + 18 built-in YAML recipes
├── Aneiang.Pa.AspNetCore/      ← MapPaApi() one-line integration
└── Aneiang.Pa.Client/          ← HTTP client SDK
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

## 📄 License

Aneiang.Pa is licensed under the [MIT License](LICENSE).
