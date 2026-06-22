---
title: "一行代码抓遍全网热榜！Aneiang.Pa 4.0 发布 — 极简 .NET 爬虫库"
date: 2026-06-22
tags: [.NET, 爬虫, 开源, C#]
---

## 一行代码能做什么？

```csharp
var data = await Pa.Source("WeiBo").GetAsync();
```

就这一行，微博热搜就到手了。

这不是 Demo 代码，这是 **Aneiang.Pa 4.0** 的真实 API。

---

## 为什么还要一个爬虫库？

.NET 生态里不缺爬虫工具：HttpClient + HtmlAgilityPack 能抓，AngleSharp 能解析，DotnetSpider 能调度。

但问题是——**每次写爬虫，你都在重复造轮子**。

- 配 HttpClient、设超时、处理 Cookie
- 写 try/catch、加重试、防被封
- 解析 HTML/JSON、映射到模型
- 加缓存、加日志、加限流

这些事情，**每个爬虫都要做一遍**。

Aneiang.Pa 的想法很简单：**把"怎么抓"和"怎么解析"从代码里抽出来，变成声明式的 Recipe（配方）。**

---

## 四种 Recipe 来源

### 1. YAML 文件（新增平台 0 行代码）

```yaml
name: Github.Trending
category: News
fetch:
  url: https://github.com/trending
parse:
  type: html
  container: "article.Box-row"
  fields:
    title:  { selector: "h2 a", trim: true }
    url:    { selector: "h2 a", attr: href, base: "https://github.com" }
    lang:   { selector: "span[itemprop=programmingLanguage]" }
```

把文件丢进 `recipes/` 目录，立即可用：

```csharp
Pa.Configure(c => c.UseRecipesFolder("./recipes"));
var data = await Pa.Source("Github.Trending").GetAsync();
```

### 2. Builder DSL（C# 内联）

```csharp
Pa.Define("Github.Releases", b => b
    .Get("https://api.github.com/repos/dotnet/runtime/releases")
    .ParseJson(p => p.Items("$").Field("tag", "tag_name").Field("name", "name")));
```

### 3. 特性标注（强类型）

```csharp
[Recipe("Trending", Category = "Demo")]
[Get("https://github.com/trending")]
[Container("article.Box-row")]
public class TrendingRow
{
    [Selector("h2 a")] public string? Title { get; set; }
    [Selector("h2 a", Attr = "href")] public string? Url { get; set; }
    [Selector("span[itemprop=programmingLanguage]")] public string? Lang { get; set; }
}
```

### 4. 内置 18 个平台，开箱即用

微博、知乎、B站、百度、抖音、头条、虎扑、腾讯、掘金、澎湃、豆瓣、凤凰网、CSDN、博客园、IT之家、36氪、双色球、大乐透。

```bash
dotnet add package Aneiang.Pa
```

```csharp
var weibo = await Pa.Source("WeiBo").GetAsync();
var lottery = await Pa.Source("Lottery.SSQ").GetAsync();
```

---

## 生产特性默认开启

| 中间件 | 默认行为 |
|--------|----------|
| 日志 | `ILogger<>` 自动注入 |
| 指标 | Meter `Aneiang.Pa`，Prometheus 兼容 |
| 追踪 | ActivitySource `Aneiang.Pa`，OpenTelemetry 兼容 |
| 缓存 | Memory 5 分钟 |
| 重试 | 3 次指数退避 |
| 超时 | 30 秒硬超时 |
| 熔断 | 连续 5 次失败熔断 60 秒 |
| 限流 | 滑动窗口令牌桶 |

**零配置，全自动。** 你只管写解析逻辑，剩下的交给管道。

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
- `GET /api/pa/sources` — 列出所有源
- `GET /api/pa/source/{name}?noCache=1` — 抓取
- `GET /api/pa/health` — 健康检查

---

## 从 23 个包到 4 个包

旧版 Aneiang.Pa 每个平台一个 NuGet 包，共 23 个。用户不知道引用哪个。

4.0 重构后：

| 包 | 说明 |
|----|------|
| `Aneiang.Pa` | 核心 + 18 内置平台，单包搞定 90% 用户 |
| `Aneiang.Pa.AspNetCore` | Web API 集成 |
| `Aneiang.Pa.Client` | HTTP 强类型客户端 |
| `Aneiang.Pa.Abstractions` | 接口与模型（扩展用） |

---

## 实测数据

```
=== Aneiang.Pa 4.0 Hello World ===
已注册 18 个内置 Recipe：
  [Lottery] Lottery.DLT   大乐透开奖
  [Lottery] Lottery.SSQ   双色球开奖
  [News   ] BaiDu         百度热榜
  [News   ] WeiBo         微博热搜
  [News   ] ZhiHu         知乎热榜
  ...（共 18 个）

抓取 GitHub Trending…
成功！共 17 条:
  1. palmier-io / palmier-pro
  2. penpot / penpot
  3. calesthio / OpenMontage
```

---

## 快速开始

```bash
dotnet new console -n MyScraper
cd MyScraper
dotnet add package Aneiang.Pa
```

```csharp
using Aneiang.Pa;

var data = await Pa.Source("BaiDu").GetAsync();
foreach (var item in data.Data)
    Console.WriteLine(item["title"]);
```

```bash
dotnet run
```

---

## 开源地址

GitHub: [https://github.com/AneiangSoft/Aneiang.Pa](https://github.com/AneiangSoft/Aneiang.Pa)

欢迎 Star、Issue、PR。新增平台只需提交一份 YAML，无需 C# 代码。

---

*Aneiang.Pa 4.0 — 让爬虫像 LINQ 一样自然。*
