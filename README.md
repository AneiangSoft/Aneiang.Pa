<p align="center">
    <img src="assets/logo.png" alt="Aneiang.Pa" width="600" style="vertical-align:middle;border-radius:8px;">
</p>

<p align="center">
  一个基于 .NET 极简、声明式的爬虫库。<br>
  <strong>一行代码抓数据，一份 YAML 扩平台。</strong>
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
  <a href="#-一行起步">快速开始</a> ·
  <a href="#-内置-20-个平台">内置平台</a> ·
  <a href="#-新增一个平台--0-行-c-代码">自定义 Recipe</a> ·
  <a href="#-aspnet-core-集成5-行">Web API</a> ·
  <a href="#-生产特性默认开启">生产特性</a> ·
  <a href="README.en.md">English</a>
</p>

---

## 🚀 一行起步

```csharp
using Aneiang.Pa;

var data = await Pa.Source("BaiDu").GetAsync();
foreach (var item in data.Data)
    Console.WriteLine(item["title"]);
```

强类型映射：

```csharp
var repos = await Pa.Source("Github.Trending").GetAsync<Repo>();

public class Repo
{
    public string? Title { get; set; }
    public string? Url   { get; set; }
    public string? Lang  { get; set; }
}
```

异步流式枚举：

```csharp
await foreach (var item in Pa.Source("WeiBo").StreamAsync())
    Console.WriteLine(item["title"]);
```

单次覆盖选项：

```csharp
var fresh = await Pa.Source("WeiBo")
    .NoCache()                          // 跳过缓存
    .NoRetry()                          // 跳过重试
    .WithTimeout(TimeSpan.FromSeconds(10))  // 自定义超时
    .WithProxy("http://127.0.0.1:7890")    // 临时代理
    .GetAsync();
```

---

## 📦 安装

```bash
# 主包：核心引擎 + 20 个内置平台（单包搞定 90% 用户）
dotnet add package Aneiang.Pa

# Web API 集成（可选）
dotnet add package Aneiang.Pa.AspNetCore

# HTTP 强类型客户端 SDK（可选）
dotnet add package Aneiang.Pa.Client
```

| 包 | 说明 |
|----|------|
| `Aneiang.Pa` | 核心引擎 + 20 内置 Recipe + 8 中间件 + 4 解析器 |
| `Aneiang.Pa.AspNetCore` | `app.MapPaApi()` 一行起 REST API |
| `Aneiang.Pa.Client` | HTTP 强类型客户端，调用远程 Pa API 服务 |
| `Aneiang.Pa.Abstractions` | 接口与模型（写扩展包时引用） |

---

## ✨ 亮点特性

| 特性 | 说明 |
|------|------|
| 🧱 **极简 API** | `Pa.Source("WeiBo").GetAsync()` 一行抓数据 |
| 📜 **声明式 Recipe** | YAML / Builder DSL / 特性标注，**新增平台 0 行 C# 代码** |
| 🏭 **内置 20 平台** | 16 个新闻热榜 + 2 个 GitHub + 2 个彩票，开箱即用 |
| 🛤 **执行管道** | 日志 / 缓存 / 重试 / 熔断 / 超时 / 限流 / 指标 / 追踪 默认开启 |
| 📊 **可观测性** | `System.Diagnostics.Metrics` + `ActivitySource`，OpenTelemetry 兼容 |
| 🌐 **代理池** | 轮询/随机 + 健康跟踪 + 连续失败自动禁用 |
| 🚀 **ASP.NET Core** | `app.MapPaApi()` 一行起 REST API |
| 🔌 **插件化** | 第三方包通过 `[assembly: PaScraperModule]` 自动发现 |
| 🤖 **特殊站点** | 抖音 Cookie 自动获取（内置 `DouYinCookieMiddleware`） |

---

## 🏪 内置 20 个平台

| 分类 | Source | 描述 |
|------|--------|------|
| **新闻热榜** | `WeiBo` `ZhiHu` `Bilibili` `BaiDu` `DouYin` `TouTiao` | 微博 / 知乎 / B站 / 百度 / 抖音 / 头条 |
| | `HuPu` `Tencent` `JueJin` `ThePaper` `DouBan` `IFeng` | 虎扑 / 腾讯 / 掘金 / 澎湃 / 豆瓣 / 凤凰网 |
| | `Csdn` `CnBlog` `ItHome` `_36kr` | CSDN / 博客园 / IT之家 / 36氪 |
| **GitHub** | `Github.Trending` `Github.Releases` | GitHub Trending / dotnet/runtime Releases |
| **彩票** | `Lottery.SSQ` `Lottery.DLT` | 双色球 / 大乐透 |

```csharp
foreach (var r in Pa.Sources())
    Console.WriteLine($"[{r.Category}] {r.Name} — {r.DisplayName}");
```

---

## 📜 新增一个平台 — 0 行 C# 代码

`recipes/news/cnbeta.yaml`：

```yaml
name: CnBeta
category: News
display_name: CnBeta 头条
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

## 🛠 四种 Recipe 来源

| 方式 | 适用场景 | 写法 |
|------|----------|------|
| **内置 YAML** | 开箱即用 | 20 个平台已嵌入 dll |
| **外部 YAML/JSON** | 配置外置，社区共享 | `Pa.Configure(c => c.UseRecipesFolder(...))` |
| **Builder DSL** | 动态构造、运行时定义 | `Pa.Define("Name", b => b.Get(...).ParseHtml(...))` |
| **特性标注** | 强类型、IDE 友好 | `[Recipe][Get][Container][Selector]` 标注类 |

### Builder DSL 示例

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

### 特性标注示例

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

## 🛡 生产特性默认开启

| 中间件 | 默认行为 | 关闭/调整 |
|--------|----------|-----------|
| Logging | `ILogger<>` 注入 | 不注入即空日志 |
| Metrics | Meter `Aneiang.Pa` | `Pa.Configure(c => c.Options.EnableMetrics = false)` |
| Tracing | ActivitySource `Aneiang.Pa` | 同上 `EnableTracing` |
| Cache | Memory 5min | `.NoCache()` 单次跳过 |
| Retry | 3 次指数退避 | `.NoRetry()` 单次跳过 |
| Timeout | 30s | `.WithTimeout(TimeSpan)` |
| RateLimit | 不启用 | 显式开启 + 配置 |
| CircuitBreaker | 5 次失败熔断 60s | 自动 |
| DouYinCookie | 抖音 Cookie 自动获取 | 仅对 `DouYin` 生效 |

### 管道执行顺序

```
请求 → Logging → Metrics → Tracing → Cache → RateLimit → CircuitBreaker → Retry → Timeout → DouYinCookie → Fetch → Parse → 响应
```

---

## 🚀 ASP.NET Core 集成（5 行）

```csharp
using Aneiang.Pa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPa(opt => opt.DefaultCacheDuration = TimeSpan.FromMinutes(5));

var app = builder.Build();
app.MapPaApi();
app.Run();
```

自动暴露：

| 端点 | 说明 |
|------|------|
| `GET /api/pa/sources` | 列出所有 source |
| `GET /api/pa/source/{name}?noCache=1` | 抓取数据 |
| `GET /api/pa/health` | 健康检查 |

---

## 🤖 特殊站点处理

### 抖音热搜

抖音接口需要先访问 `login.douyin.com` 获取 Cookie，否则 403。Aneiang.Pa **内置 `DouYinCookieMiddleware`** 自动处理：

```csharp
var data = await Pa.Source("DouYin").GetAsync();  // 零配置
```

### 自定义中间件

其他需要特殊处理的站点（签名、Token、登录态等），写一个中间件即可：

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

Recipe 中用 `{token}` 占位：

```yaml
headers:
  Authorization: "Bearer {token}"
```

---

## 📁 项目结构

```
src/
├── Aneiang.Pa.Abstractions/    ← 接口与模型（独立包）
├── Aneiang.Pa/                 ← 核心引擎 + 20 内置 YAML + 8 中间件
├── Aneiang.Pa.AspNetCore/      ← MapPaApi() 一行集成
└── Aneiang.Pa.Client/          ← HTTP 强类型客户端

samples/
├── 01-hello-world/             ← 全能力展示 Demo
├── 02-custom-recipe/           ← 自定义 Recipe 三种方式
├── 03-asp-net-core/            ← Web API 集成
└── 04-douyin-demo/             ← 抖音热搜（特殊站点）

tests/
└── Aneiang.Pa.Tests/           ← 15 个单元测试
```

---

## 📊 可观测性

Aneiang.Pa 自动产生结构化日志、Metrics 和 Tracing，可直接对接 Grafana + Prometheus + Jaeger：

| 指标 | 类型 | 标签 |
|------|------|------|
| `pa_scrape_total` | Counter | name, category, status |
| `pa_scrape_duration_seconds` | Histogram | name, category |
| `pa_scrape_items_total` | Counter | name, category |

```csharp
// 用户代码
var data = await Pa.Source("WeiBo").GetAsync();

// 框架自动：
// - 日志: [Pa] Start WeiBo / [Pa] OK WeiBo Items=50 Elapsed=842ms
// - 指标: pa_scrape_total{name="WeiBo",status="ok"} +1
// - 追踪: Activity "Pa WeiBo" with duration
```

---

## ⚠️ 免责声明

> **建议抓取间隔 ≥ 5 分钟**，避免频繁抓取导致 IP 被封禁。
>
> **爬取的数据仅限用于个人学习、研究或公益目的。不得用于商业售卖、攻击他人或任何非法活动，否则需自行承担法律责任。**

---

## 🤝 贡献

- 欢迎 PR / Issue，尤其是新增平台 Recipe（YAML 即可，无需 C#）
- 提交前请保持代码风格一致，并附带简要说明和必要的测试
- 如果希望在 NuGet 包中发布你新增的平台，请在 Issue 先讨论方案

### 贡献 Recipe 只需 3 步

1. 在 `recipes/` 目录新建 `.yaml` 文件
2. 填写 `name` / `fetch` / `parse` 三个部分
3. 提交 PR

---

## 📄 许可证

Aneiang.Pa 采用 [MIT 许可证](LICENSE)。
