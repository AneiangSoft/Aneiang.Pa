---
title: "2026 年了，.NET 爬虫应该长什么样？"
date: 2026-06-22
tags: [.NET, 爬虫, 开源, 技术选型]
---

## 2026 年的 .NET 生态

2026 年，.NET 9 已经发布，AOT 编译成为主流，OpenTelemetry 成为可观测性标准。

但 .NET 的爬虫生态呢？

- **DotnetSpider**：功能强大但太重，学习曲线陡
- **ScrapySharp**：多年未更新
- **Abot**：专注于爬取框架，不是数据提取
- **自己写 HttpClient + HtmlAgilityPack**：每个项目重复造轮子

**2026 年的 .NET 爬虫，应该长什么样？**

---

## 1. 声明式，而非命令式

**旧时代：**

```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.UserAgent.ParseAdd("...");
var html = await client.GetStringAsync(url);
var doc = new HtmlDocument();
doc.LoadHtml(html);
var nodes = doc.DocumentNode.SelectNodes("//div[@class='title']");
foreach (var node in nodes) { ... }
```

**2026 年：**

```yaml
name: WeiBo
fetch:
  url: https://s.weibo.com/top/summary
parse:
  type: html
  container: "tr"
  fields:
    title: { selector: "a", trim: true }
```

```csharp
var data = await Pa.Source("WeiBo").GetAsync();
```

**"怎么抓"是配置，"怎么解析"是声明。** 代码只负责"我要什么"。

---

## 2. 管道化，而非散落各处

旧时代：每个爬虫自己处理重试、超时、日志、缓存。

```csharp
try
{
    // 爬取逻辑
}
catch (Exception e)
{
    // 错误处理
}
```

2026 年：**横切关注点集中在管道层。**

```
请求 → 日志 → 指标 → 追踪 → 缓存 → 限流 → 熔断 → 重试 → 超时 → 抓取 → 解析 → 响应
```

业务代码零侵入，可观测性自动获得。

---

## 3. 可观测性原生支持

旧时代：自己打日志、自己算指标。

```csharp
_logger.LogInformation("开始爬取...");
var sw = Stopwatch.StartNew();
// ...
_logger.LogInformation("爬取完成，耗时 {Elapsed}", sw.Elapsed);
```

2026 年：**框架自动产生结构化日志、Metrics、Tracing。**

```csharp
// 用户代码
var data = await Pa.Source("WeiBo").GetAsync();

// 框架自动：
// - 写日志: [Pa] Start WeiBo / [Pa] OK WeiBo Items=50 Elapsed=842ms
// - 记指标: pa_scrape_total{name="WeiBo",status="ok"} +1
// - 创建 Span: Activity "Pa WeiBo" with duration
```

直接对接 Grafana + Prometheus + Jaeger。

---

## 4. 插件化，而非硬编码

旧时代：新增一个平台 = 新建一个项目 + 写一个类 + 注册 DI。

```csharp
// 新增平台要写这么多
public interface INewPlatformScraper : INewsScraper { }
public class NewPlatformScraper : INewPlatformScraper { ... }
services.AddScraper<INewPlatformScraper, NewPlatformScraper>(...);
```

2026 年：**新增一个平台 = 写一份 YAML。**

```yaml
name: MyPlatform
fetch:
  url: https://example.com/hot
parse:
  type: html
  container: "article"
  fields:
    title: { selector: "h2" }
```

第三方包通过 `[assembly: PaScraperModule]` 自动发现，用户只需 `dotnet add package`。

---

## 5. 单包策略，而非微包策略

旧时代：23 个 NuGet 包，用户不知道选哪个。

2026 年：**一个核心包搞定 90% 用户。**

```bash
dotnet add package Aneiang.Pa
```

内置 18 个平台，开箱即用。需要 Web API 再加一个包：

```bash
dotnet add package Aneiang.Pa.AspNetCore
```

---

## 6. 强类型 + 动态类型双支持

```csharp
// 动态类型（快速验证）
var data = await Pa.Source("WeiBo").GetAsync();
Console.WriteLine(data.Data[0]["title"]);

// 强类型（生产环境）
var repos = await Pa.Source("Github.Trending").GetAsync<Repo>();
Console.WriteLine(repos.Data[0].Title);
```

同一个 Recipe，两种使用方式。

---

## 7. 生产就绪默认值

| 特性 | 默认值 |
|------|--------|
| 超时 | 30 秒 |
| 重试 | 3 次指数退避 |
| 缓存 | 5 分钟内存缓存 |
| 熔断 | 5 次失败熔断 60 秒 |
| 日志 | ILogger 自动注入 |
| 指标 | System.Diagnostics.Metrics |
| 追踪 | OpenTelemetry ActivitySource |

**零配置，直接上生产。**

---

## 总结

2026 年的 .NET 爬虫应该：

1. **声明式** — YAML 描述"抓什么"，而非 C# 描述"怎么抓"
2. **管道化** — 横切关注点集中管理
3. **可观测** — 日志/指标/追踪原生支持
4. **插件化** — 第三方扩展零配置接入
5. **单包** — 一个包搞定 90% 场景
6. **生产就绪** — 默认值就是最佳实践

---

## Aneiang.Pa 4.0

这就是 Aneiang.Pa 4.0 的设计理念。

GitHub: [https://github.com/AneiangSoft/Aneiang.Pa](https://github.com/AneiangSoft/Aneiang.Pa)

```bash
dotnet add package Aneiang.Pa
```

```csharp
var data = await Pa.Source("WeiBo").GetAsync();
```

欢迎 Star、Issue、PR。
