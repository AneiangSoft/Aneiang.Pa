---
title: "用 .NET 写爬虫的正确姿势：声明式 Recipe + 中间件管道"
date: 2026-06-22
tags: [.NET, 爬虫, 架构, 中间件]
---

## 传统爬虫怎么写？

```csharp
public async Task<List<NewsItem>> ScrapeWeiBoAsync()
{
    var client = new HttpClient();
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0...");
    
    var html = await client.GetStringAsync("https://s.weibo.com/top/summary");
    var doc = new HtmlDocument();
    doc.LoadHtml(html);
    
    var items = new List<NewsItem>();
    var rows = doc.DocumentNode.SelectNodes("//tr");
    foreach (var row in rows.Skip(1))
    {
        var title = row.SelectSingleNode(".//a")?.InnerText?.Trim();
        if (!string.IsNullOrEmpty(title))
            items.Add(new NewsItem { Title = title });
    }
    return items;
}
```

这段代码的问题：
- ❌ 没有超时控制
- ❌ 没有重试机制
- ❌ 没有日志
- ❌ 没有缓存
- ❌ 没有限流
- ❌ 没有错误处理
- ❌ 硬编码 URL 和选择器
- ❌ 换个平台全部重写

---

## 问题出在哪？

**"怎么抓"和"怎么解析"混在一起了。**

- "怎么抓"：URL、Method、Headers、Cookie、超时、重试、代理
- "怎么解析"：HTML 还是 JSON？选择器是什么？字段怎么映射？

这两件事应该**完全解耦**。

---

## 声明式 Recipe

Aneiang.Pa 的做法：用 YAML 描述"怎么抓"和"怎么解析"。

```yaml
name: WeiBo
fetch:
  url: https://s.weibo.com/top/summary?cate=realtimehot
parse:
  type: html
  container: "xpath://*[@id='pl_top_realtimehot']//table/tbody/tr[position()>1]"
  fields:
    title: { selector: "xpath:.//td[@class='td-02']/a", trim: true }
    url:   { selector: "xpath:.//td[@class='td-02']/a", attr: href, base: "https://s.weibo.com" }
```

**YAML 只描述"要什么"，不描述"怎么做"。**

"怎么做"由框架的执行管道负责。

---

## 中间件管道

```
请求
  ↓
[Logging]       ← 自动记录开始/结束/耗时
  ↓
[Metrics]       ← 自动记录调用次数/耗时分布
  ↓
[Tracing]       ← 自动创建 OpenTelemetry Span
  ↓
[Cache]         ← 命中直接返回
  ↓
[RateLimit]     ← 滑动窗口限流
  ↓
[CircuitBreaker]← 连续失败熔断
  ↓
[Retry]         ← 指数退避重试
  ↓
[Timeout]       ← 硬超时控制
  ↓
[Fetch]         ← 实际 HTTP 请求
  ↓
[Parse]         ← 按 Recipe 解析响应
  ↓
[Map]           ← 映射到强类型
  ↓
  响应
```

每个中间件只做一件事，通过 `Order` 控制顺序。

---

## 管道的好处

### 1. 零侵入

爬虫 Recipe 不需要知道管道存在：

```yaml
# 这个 YAML 不需要写任何管道配置
name: BaiDu
fetch:
  url: https://top.baidu.com/board?tab=realtime
parse:
  type: embedded
  extract_pattern: "<!--s-data:(.*?)-->"
  items_path: $.data.cards[0].content
  fields:
    title: { selector: "word" }
```

### 2. 可观测性自动获得

```csharp
// 用户代码
var data = await Pa.Source("BaiDu").GetAsync();

// 背后自动产生：
// - 日志: [Pa] Start BaiDu / [Pa] OK BaiDu Items=50 Elapsed=842ms
// - 指标: pa_scrape_total{name="BaiDu",status="ok"} 1
// - 追踪: Activity "Pa BaiDu" with duration
```

### 3. 弹性策略可配置

```json
{
  "Scraper": {
    "Resilience": {
      "Default": {
        "RetryCount": 3,
        "Timeout": "00:00:30",
        "CircuitBreakerFailureThreshold": 5
      },
      "Overrides": {
        "WeiBo": { "RetryCount": 1, "Timeout": "00:00:10" }
      }
    }
  }
}
```

### 4. 单次调用可覆盖

```csharp
// 全局默认 3 次重试，但这次不要重试
var data = await Pa.Source("WeiBo").NoRetry().GetAsync();

// 全局默认 5 分钟缓存，但这次不要缓存
var fresh = await Pa.Source("WeiBo").NoCache().GetAsync();

// 临时用代理
var proxied = await Pa.Source("WeiBo").WithProxy("http://127.0.0.1:7890").GetAsync();
```

---

## 自定义中间件

```csharp
public class MyAuthMiddleware : IScrapeMiddleware
{
    public int Order => 5;
    public string Name => "MyAuth";

    public async Task<ScrapeResult> InvokeAsync(ScrapeContext ctx, ScrapeDelegate next)
    {
        // 在请求前注入 Token
        ctx.Variables["token"] = await GetTokenAsync();
        return await next(ctx);
    }
}

// 注册
Pa.Configure(c => c.UseMiddleware(new MyAuthMiddleware()));
```

---

## 总结

| 传统方式 | Aneiang.Pa 方式 |
|----------|----------------|
| 每个爬虫一个类 | 每个平台一个 YAML |
| 手动处理重试/超时/日志 | 管道自动处理 |
| 硬编码选择器 | 声明式字段映射 |
| 无法观测 | Metrics + Tracing 自动 |
| 新增平台要写代码 | 新增平台写 YAML |

---

## 快速体验

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

GitHub: [https://github.com/AneiangSoft/Aneiang.Pa](https://github.com/AneiangSoft/Aneiang.Pa)
