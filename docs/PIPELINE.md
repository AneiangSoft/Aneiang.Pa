| 文件 | 内容 |
|------|------|
| `docs/ARCHITECTURE-PLAN.md` | 架构设计完整方案 |
| `docs/PIPELINE.md` | 管道与中间件使用文档 |
| `docs/PLUGIN-MODULE.md` | 第三方插件开发指南 |

# Aneiang.Pa 管道（Pipeline）使用文档

## 概述

3.0 引入"爬取管道"（Scraping Pipeline）模型，将横切关注点（日志/重试/缓存/限流/熔断/超时/指标/追踪）从业务爬虫抽离到独立的中间件。爬虫只关心"如何抓"，管道负责"怎么调"。

## 启用管道

```csharp
services.AddPaScraper(builder.Configuration);
// AddPaScraper 内部已自动调用 AddPaScrapePipeline，无需重复
```

或显式控制：

```csharp
services.AddNewsScraper(builder.Configuration);
services.AddScraperRegistry();
services.AddPaScrapePipeline(builder.Configuration);
```

## 调用方式

```csharp
public class MyController : ControllerBase
{
    private readonly IScrapeInvoker _invoker;
    private readonly IScraperRegistry _registry;

    public MyController(IScrapeInvoker invoker, IScraperRegistry registry)
    {
        _invoker = invoker;
        _registry = registry;
    }

    [HttpGet("baidu")]
    public async Task<IActionResult> GetBaidu(CancellationToken ct)
    {
        // 方式 A：通过分类+源名
        var result = await _invoker.InvokeAsync<NewsItem>("News", "BaiDu", ct);

        // 方式 B：通过爬虫实例
        var scraper = _registry.GetScraper("News", "BaiDu") as IScraper<NewsItem>;
        var r2 = await _invoker.InvokeAsync(scraper!, ct);

        return Ok(result);
    }
}
```

## 内置中间件（按 Order 升序）

| 顺序 | 中间件 | 职责 |
|------|--------|------|
| 0 | `LoggingMiddleware` | 进/出/耗时/错误日志，自动注入 CorrelationId |
| 10 | `MetricsMiddleware` | `pa_scrape_total` / `pa_scrape_duration_seconds` / `pa_scrape_items_total` |
| 20 | `TracingMiddleware` | OpenTelemetry Activity（Source 名 `Aneiang.Pa`） |
| 30 | `CacheMiddleware` | 命中缓存直接返回 |
| 40 | `RateLimitMiddleware` | 滑动窗口令牌桶限流 |
| 50 | `CircuitBreakerMiddleware` | 连续失败超阈值触发熔断 |
| 60 | `RetryMiddleware` | 失败重试 + 指数退避 |
| 70 | `TimeoutMiddleware` | 单次执行硬超时 |

## 配置

```json
{
  "Scraper": {
    "Resilience": {
      "Default": {
        "RetryCount": 3,
        "RetryBackoffMs": [500, 1500, 3000],
        "Timeout": "00:00:30",
        "CircuitBreakerFailureThreshold": 5,
        "CircuitBreakerBreakDuration": "00:01:00"
      },
      "Overrides": {
        "News:WeiBo": {
          "RetryCount": 1,
          "Timeout": "00:00:10"
        },
        "Lottery:*": {
          "RetryCount": 5
        }
      }
    },
    "RateLimit": {
      "Global": { "PermitsPerWindow": 100, "Window": "00:01:00" },
      "PerSource": {
        "News:WeiBo": { "PermitsPerWindow": 5, "Window": "00:01:00" }
      }
    },
    "CachePipeline": {
      "DefaultDuration": "00:00:00",
      "PerSource": {
        "News:*": "00:05:00",
        "Lottery:*": "01:00:00"
      }
    }
  }
}
```

> 配置基于 `IOptionsMonitor` 实现，**支持运行时热更新**。

## 自定义中间件

```csharp
public class MyAuthMiddleware : IScrapeMiddleware
{
    public int Order => 5;
    public async Task<ScraperResult<T>> InvokeAsync<T>(ScrapeContext ctx, ScrapeDelegate<T> next) where T : class
    {
        // ... 在调用前后插入自定义逻辑 ...
        return await next(ctx);
    }
}

services.AddPaScrapeMiddleware<MyAuthMiddleware>();
```

## 可观测性集成

### Prometheus（通过 prometheus-net）

```csharp
using Prometheus;

services.AddPrometheusOpenTelemetry(); // 或类似的桥接组件，监听 Meter "Aneiang.Pa"
```

### OpenTelemetry

```csharp
services.AddOpenTelemetry()
    .WithTracing(t => t.AddSource("Aneiang.Pa"))
    .WithMetrics(m => m.AddMeter("Aneiang.Pa"));
```

### 标准 ASP.NET Core 健康检查

```csharp
services.AddHealthChecks()
    .AddPaScrapers(name: "pa-scrapers", configure: opt =>
    {
        opt.Categories = new[] { "News" };
        opt.Timeout = TimeSpan.FromSeconds(5);
    });

app.MapHealthChecks("/health");
```
