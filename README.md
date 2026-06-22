<p align="center">
    <img src="assets/logo.png" alt="Aneiang.Pa" width="600" style="vertical-align:middle;border-radius:8px;">
</p>

<p align="center">
  一个基于 .NET 极简、声明式的爬虫库。一行代码抓数据，一份 YAML 扩平台。
</p>

<p align="center">
  中文 | <a href="README.en.md">English</a>
</p>

<p align="center">
  <a href="https://www.nuget.org/packages/Aneiang.Pa"><img src="https://img.shields.io/nuget/v/Aneiang.Pa.svg?style=flat-square&logo=nuget" /></a>
  <a href="https://www.nuget.org/packages/Aneiang.Pa"><img src="https://img.shields.io/nuget/dt/Aneiang.Pa.svg?style=flat-square&logo=nuget" /></a>
  <img src="https://img.shields.io/badge/target-netstandard2.1%20%7C%20net8.0-blue?style=flat-square" />
  <img src="https://img.shields.io/badge/status-active-success?style=flat-square" />
  <img src="https://img.shields.io/github/stars/AneiangSoft/Aneiang.Pa" />
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
    public string? Url { get; set; }
    public string? Lang { get; set; }
}
```

---

## 📦 安装

```bash
# 单包搞定 90% 用户（内置 18 个平台）
dotnet add package Aneiang.Pa

# Web API 集成
dotnet add package Aneiang.Pa.AspNetCore

# HTTP 强类型客户端
dotnet add package Aneiang.Pa.Client
```

---

## ✨ 亮点特性

| 特性 | 说明 |
|------|------|
| 🧱 **极简 API** | `Pa.Source("WeiBo").GetAsync()` 一行抓数据 |
| 📜 **声明式 Recipe** | YAML / Builder DSL / 特性标注，**新增平台 0 行 C# 代码** |
| 🏭 **内置 18 平台** | 16 个新闻热榜 + 2 个彩票，开箱即用 |
| 🛤 **执行管道** | 日志 / 缓存 / 重试 / 熔断 / 超时 / 限流 / 指标 / 追踪 默认开启 |
| 📊 **可观测性** | `System.Diagnostics.Metrics` + `ActivitySource`，OpenTelemetry 兼容 |
| 🌐 **代理池** | 轮询/随机 + 健康跟踪 + 连续失败自动禁用 |
| 🚀 **ASP.NET Core** | `app.MapPaApi()` 一行起 REST API |
| 🔌 **插件化** | 第三方包通过 `[assembly: PaScraperModule]` 自动发现 |

---

## 🏪 内置 18 个平台

| Category | Source | 描述 |
|----------|--------|------|
| News | WeiBo / ZhiHu / Bilibili / BaiDu / DouYin / TouTiao | 微博 / 知乎 / B站 / 百度 / 抖音 / 头条 |
| News | HuPu / Tencent / JueJin / ThePaper / DouBan / IFeng | 虎扑 / 腾讯 / 掘金 / 澎湃 / 豆瓣 / 凤凰网 |
| News | Csdn / CnBlog / ItHome / _36kr | CSDN / 博客园 / IT之家 / 36氪 |
| Lottery | Lottery.SSQ / Lottery.DLT | 双色球 / 大乐透 |

```csharp
foreach (var r in Pa.Sources())
    Console.WriteLine($"{r.Name} - {r.Category}/{r.DisplayName}");
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
| **YAML/JSON** | 配置外置，社区共享 | 文件 + `Pa.Configure(c => c.UseRecipesFolder(...))` |
| **Builder DSL** | 动态构造、运行时定义 | `Pa.Define("Name", b => b.Get(...).ParseHtml(...))` |
| **特性标注** | 强类型、IDE 友好 | `[Recipe][Get][Container][Selector]` 标注类 |
| **内置 YAML** | 开箱即用 | 18 个平台已嵌入 dll |

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
- `GET /api/pa/sources` — 列出所有 source
- `GET /api/pa/source/{name}?noCache=1` — 抓取数据
- `GET /api/pa/health` — 健康检查

---

## 📁 项目结构

```
src/
├── Aneiang.Pa.Abstractions/    ← 接口与模型（独立包）
├── Aneiang.Pa/                 ← 核心引擎 + 18 内置 YAML
├── Aneiang.Pa.AspNetCore/      ← MapPaApi() 一行集成
└── Aneiang.Pa.Client/          ← HTTP 强类型客户端
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

## 📄 许可证

Aneiang.Pa 采用 [MIT 许可证](LICENSE)。
