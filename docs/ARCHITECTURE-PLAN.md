# Aneiang.Pa 深度优化设计方案

> 本方案是 Aneiang.Pa 从 2.x 演进到 3.0 的架构级升级路线图。
> 状态：执行中  |  起始版本：2.1.7  |  目标版本：3.0.0

---

## 一、总览：从"能用"到"好用"再到"专业级"

当前项目经过上一轮重构已有了清晰的分层（Core/Modules/Hosting）和统一抽象（`IScraper<T>` + `IScraperRegistry`），属于"骨架级"完成度。本方案从架构师视角对项目做下一阶段的深度演进，聚焦四个核心维度：

```
                    ┌──────────────────────────────────┐
                    │     Pipeline & Reliability       │  ← 管道化 + 可靠性
                    ├──────────────────────────────────┤
                    │     Observability & DevEx        │  ← 可观测性 + 开发体验
                    ├──────────────────────────────────┤
                    │     Plugin Discovery             │  ← 插件化 + 自动发现
                    ├──────────────────────────────────┤
                    │  IScraper<T> (现有统一抽象基础)   │
                    └──────────────────────────────────┘
```

---

## 二、设计目标与原则

| 目标 | 描述 |
|------|------|
| **生产可用** | 重试/熔断/限流/超时/取消，端到端可用于生产环境 |
| **零侵入观测** | 任意爬虫自动获得日志/指标/追踪，使用者无需关心 |
| **插件化扩展** | 第三方包可通过约定自动注册，无需修改 Core 代码 |
| **配置即治理** | 通过 `appsettings.json` 调整全局/单爬虫行为，无需改代码 |
| **向后兼容** | 现有 `INewsScraper`/`ILotteryScraper` API 不破坏，渐进迁移 |

**核心原则**：横切关注点（重试/缓存/限流/日志）集中在管道层，业务爬虫只关心"如何抓"。

---

## 三、核心架构升级：Pipeline 模型

### 3.1 现状问题

每个爬虫都自己处理：try/catch、HttpClient 配置、UA、Cookie、错误转换。横切关注点（日志/重试/缓存）无处下手。

### 3.2 新设计：执行管道（Scraping Pipeline）

借鉴 ASP.NET Core 中间件思想，引入爬虫执行管道：

```
ScrapeRequest
    ↓
[ Logging Middleware ]            ← 自动记录耗时、状态
    ↓
[ Cache Middleware ]              ← 命中返回，未命中继续
    ↓
[ RateLimit Middleware ]          ← 全局/分平台限流
    ↓
[ Retry + CircuitBreaker ]        ← Polly 策略
    ↓
[ Metrics Middleware ]            ← 上报指标
    ↓
[ Tracing Middleware ]            ← OpenTelemetry Span
    ↓
ScrapeAsync (爬虫具体实现)
    ↓
ScraperResult<T>
```

#### 关键接口

```csharp
public class ScrapeContext
{
    public ScraperDescriptor Descriptor { get; }
    public IDictionary<string, object> Items { get; }
    public CancellationToken CancellationToken { get; }
    public IServiceProvider Services { get; }
}

public interface IScrapeMiddleware
{
    int Order { get; }
    Task<ScraperResult<T>> InvokeAsync<T>(
        ScrapeContext context,
        ScrapeDelegate<T> next) where T : class;
}

public delegate Task<ScraperResult<T>> ScrapeDelegate<T>(ScrapeContext ctx) where T : class;

public interface IScrapeInvoker
{
    Task<ScraperResult<T>> InvokeAsync<T>(
        IScraper<T> scraper, 
        CancellationToken ct = default) where T : class;
}
```

#### 内置中间件

| 中间件 | 职责 | 顺序 |
|--------|------|------|
| LoggingMiddleware | 进/出/耗时/错误日志 | 0 |
| MetricsMiddleware | Prometheus 指标 | 10 |
| TracingMiddleware | OpenTelemetry Span | 20 |
| CacheMiddleware | 缓存命中提前返回 | 30 |
| RateLimitMiddleware | 令牌桶限流 | 40 |
| CircuitBreakerMiddleware | 熔断 | 50 |
| RetryMiddleware | 指数退避重试 | 60 |
| TimeoutMiddleware | 单次执行超时控制 | 70 |

---

## 四、可靠性增强（Polly 集成）

### 4.1 全局 + 分平台策略配置

```json
{
  "Scraper": {
    "Resilience": {
      "Default": {
        "Retry": { "Count": 3, "BackoffMs": [500, 1500, 3000] },
        "Timeout": "00:00:30",
        "CircuitBreaker": {
          "FailureThreshold": 5,
          "BreakDuration": "00:01:00"
        }
      },
      "Overrides": {
        "News:WeiBo": {
          "Retry": { "Count": 1 },
          "Timeout": "00:00:10"
        }
      }
    }
  }
}
```

### 4.2 代理池增强

```csharp
public interface IProxyPool
{
    ProxyEntry? GetNext();
    void ReportSuccess(ProxyEntry p);
    void ReportFailure(ProxyEntry p);
    IReadOnlyList<ProxyHealthInfo> GetHealthSnapshot();
}
```

策略：连续失败 3 次 → 禁用 5 分钟 → 半开探测。

---

## 五、可观测性体系

### 5.1 结构化日志（自动注入）

每条日志自动包含 `Category`/`Source`/`CorrelationId`。

### 5.2 指标系统

使用 .NET 内置 `System.Diagnostics.Metrics`：

| 指标 | 类型 | 标签 |
|------|------|------|
| `pa_scrape_total` | Counter | category,source,status |
| `pa_scrape_duration_seconds` | Histogram | category,source |
| `pa_scrape_items_total` | Counter | category,source |
| `pa_cache_hit_total` | Counter | category,source |
| `pa_circuit_state` | Gauge | category,source |

### 5.3 OpenTelemetry 追踪

每次 `InvokeAsync` 创建一个 root Span。

### 5.4 健康检查升级

集成标准 ASP.NET Core `IHealthCheck`，可被 K8s 探针使用。

---

## 六、插件化与自动发现

### 6.1 程序集级别声明

```csharp
[assembly: PaScraperModule(typeof(BaiDuModule))]

public class BaiDuModule : IScraperModule
{
    public string Name => "Aneiang.Pa.BaiDu";
    public void Register(IScraperModuleBuilder builder)
    {
        builder.AddScraper<IBaiDuNewScraper, BaiDuNewScraper, BaiDuScraperOptions>("Scraper:BaiDu");
    }
}
```

### 6.2 自动扫描注册

```csharp
services.AddPaScraperFromLoadedAssemblies(configuration);
// 内部遍历 AppDomain.GetAssemblies()，找到所有 PaScraperModuleAttribute
```

---

## 七、配置体系优化

### 7.1 分层配置 / 7.2 配置热更新 / 7.3 配置校验

引入 `IOptionsMonitor` 支持热更新；`IValidateOptions` 启动校验。

---

## 八、性能优化

### 8.1 ExtendableObjectExtensions 重写

改用 `[JsonExtensionData]` 方案，O(n²) → O(1)。

### 8.2 反射元数据缓存

`DynamicScraper` 使用 `ConcurrentDictionary<Type, ScraperTypeMetadata>` 缓存。

### 8.3 HttpClient 默认超时

修复 100 秒默认超时隐患。

---

## 九、API 设计现代化

### 9.1 ScrapeRequest 参数模型
### 9.2 统一 Controller API（v2）
### 9.3 Aneiang.Pa.Client SDK

---

## 十、安全性与合规

- 请求频率自律（MinIntervalMs）
- robots.txt 检查（可选）
- 凭据外部化（Azure Key Vault / AWS Secrets）

---

## 十一、测试体系

```
test/
├── Aneiang.Pa.Core.Tests/
├── Aneiang.Pa.Pipeline.Tests/
├── Aneiang.Pa.Modules.News.Tests/
├── Aneiang.Pa.Modules.Lottery.Tests/
├── Aneiang.Pa.AspNetCore.Tests/
└── Aneiang.Pa.Integration.Tests/
```

覆盖率目标：Core ≥ 80%，整体 ≥ 70%。

---

## 十二、CI/CD 与发布

### 12.1 .gitignore 治理（nupkgs/）
### 12.2 GitHub Actions 自动发布
### 12.3 Directory.Packages.props (CPM)
### 12.4 DocFX 文档站

---

## 十三、目录结构演进

```
src/
├── Core/
│   ├── Aneiang.Pa.Core/
│   ├── Aneiang.Pa.Pipeline/         ★ 新增
│   ├── Aneiang.Pa.Resilience/       ★ 新增
│   └── Aneiang.Pa.Observability/    ★ 新增
├── Modules/
├── Hosting/
│   ├── Aneiang.Pa.AspNetCore/
│   └── Aneiang.Pa.McpServer/
├── Client/                          ★ 新增
│   └── Aneiang.Pa.Client/
└── Aneiang.Pa/

docs/                                ★ 新增
.github/workflows/                   ★ 新增
```

---

## 十四、实施路线图

| Phase | 周期 | 交付 | 状态 |
|-------|------|------|------|
| **Phase 1：性能与稳定性基础** | 1 周 | HttpClient 超时、ExtendableObject 重写、反射缓存、.gitignore | 进行中 |
| **Phase 2：管道与可靠性** | 2-3 周 | IScrapeMiddleware 管道、Polly、代理池健康跟踪、CancellationToken 全链路 | 待开始 |
| **Phase 3：可观测性** | 2 周 | 结构化日志、Metrics、OpenTelemetry、健康检查升级 | 待开始 |
| **Phase 4：插件化** | 1-2 周 | IScraperModule 自动发现、配置热更新 | 待开始 |
| **Phase 5：测试与文档** | 2-3 周 | 单元测试矩阵、CI/CD、DocFX、Aneiang.Pa.Client SDK | 待开始 |
| **Phase 6：API 现代化** | 1-2 周 | v2 Controller API、ScrapeRequest、兼容验证 | 待开始 |
| **Phase 7：发布 v3.0** | 1 周 | 综合验证、CHANGELOG、NuGet 发布、3.0.0 | 待开始 |

---

## 十五、关键决策（已决定）

1. **引入 Polly** — 工业标准
2. **引入 OpenTelemetry** — 拆为可选包 `Aneiang.Pa.Observability`
3. **做插件自动发现** — 开放第三方扩展
4. **拆出 Client SDK 包** — Phase 5 实现
5. **目标框架** — Core 保持 `netstandard2.1`，新包用 `net8.0`
6. **版本号** — 跳到 `3.0.0`

---

## 十六、风险与应对

| 风险 | 应对 |
|------|------|
| 管道改造影响现有调用方 | `IScrapeInvoker` 是新增 API，旧调用方式仍有效 |
| Polly 依赖体积膨胀 | 拆分到独立包 `Aneiang.Pa.Resilience` |
| 配置热更新引发不一致 | 限定可热更新字段 |
| 自动发现启动开销 | 静态字段缓存扫描结果 |
| 测试覆盖不足 | Phase 5 完成后再发布 |
