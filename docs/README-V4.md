# Aneiang.Pa 4.0 — 极简、声明式 .NET 爬虫库

> **让爬虫像 LINQ 一样自然 — 一行代码抓数据，一份 YAML 扩平台。**

---

## 一行起步

```csharp
using Aneiang.Pa;

var data = await Pa.Source("Github.Trending").GetAsync();
foreach (var item in data.Data)
    Console.WriteLine(item["title"]);
```

强类型映射：

```csharp
var posts = await Pa.Source("Github.Trending").GetAsync<Repo>();

public class Repo
{
    public string? Title { get; set; }
    public string? Url { get; set; }
    public string? Lang { get; set; }
}
```

---

## 新增一个平台 — 0 行 C# 代码

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

## 四种 Recipe 来源

| 方式 | 适用场景 | 写法 |
|------|----------|------|
| **YAML/JSON** | 配置外置，社区共享 | 文件 + `Pa.Configure(c => c.UseRecipesFolder(...))` |
| **Builder DSL** | 动态构造、运行时定义 | `Pa.Define("Name", b => b.Get(...).ParseHtml(...))` |
| **特性标注** | 强类型、IDE 友好 | `[Recipe][Get][Container][Selector]` 标注类 |
| **内置 YAML** | 开箱即用 | 12 个平台已嵌入 dll |

详见 [docs/RECIPES.md](RECIPES.md)。

---

## 内置 12 平台

| Category | Source | 描述 |
|----------|--------|------|
| News | WeiBo / ZhiHu / Bilibili / BaiDu / HuPu / JueJin / ItHome / ThePaper | 国内新闻热榜 |
| News | HackerNews / Github.Trending | 国际科技 |
| Lottery | Lottery.SSQ / Lottery.DLT | 双色球 / 大乐透 |

```csharp
foreach (var r in Pa.Sources())
    Console.WriteLine($"{r.Name} - {r.Category}/{r.DisplayName}");
```

---

## 生产特性默认开启

| 中间件 | 默认行为 | 关闭/调整 |
|--------|----------|-----------|
| Logging | `ILogger<>` 注入 | 不注入即生效空日志 |
| Metrics | Meter `Aneiang.Pa` | `Pa.Configure(c => c.Options.EnableMetrics = false)` |
| Tracing | ActivitySource `Aneiang.Pa` | 同上 `EnableTracing` |
| Cache | Memory 5min | `.NoCache()` 单次跳过 |
| Retry | 3 次指数退避 | `.NoRetry()` 单次跳过 |
| Timeout | 30s | `.WithTimeout(TimeSpan)` |
| RateLimit | 不启用 | 显式开启 + 配置 |
| CircuitBreaker | 5 次失败熔断 60s | 自动 |

---

## ASP.NET Core 集成（5 行）

```csharp
using Aneiang.Pa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPa(opt => opt.DefaultCacheDuration = TimeSpan.FromMinutes(5));

var app = builder.Build();
app.MapPaApi();
app.Run();
```

自动暴露：
- `GET /api/pa/sources` 列表
- `GET /api/pa/source/{name}?noCache=1` 抓取
- `GET /api/pa/health`

---

## 安装

```bash
# 单包搞定 90% 用户
dotnet add package Aneiang.Pa

# Web 集成
dotnet add package Aneiang.Pa.AspNetCore

# HTTP 客户端
dotnet add package Aneiang.Pa.Client
```

---

## 项目状态

- 版本：**4.0.0**
- 目标框架：`netstandard2.1` + `net8.0`
- 测试覆盖：15 个单元测试，0 失败
- CI：GitHub Actions

---

## 许可证

MIT

---

## 设计与路线图

完整方案见 [docs/REDESIGN-PLAN-V4.md](REDESIGN-PLAN-V4.md)。
