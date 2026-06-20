# Aneiang.Pa 版本变更记录

## 3.0.0 (2026-06-21) — 架构级演进

### 重大变更（Breaking + Compatible）
- **统一抽象层**：所有爬虫现在实现 `IScraper<TResult>` 与 `ScraperDescriptor`，支持按 Category/Source 字符串注册（取代 `ScraperSource` 枚举的硬绑定，旧枚举仍保留向后兼容）。
- **目录重组**：`src/News/` → `src/Modules/News/`；`src/Lotteries/` → `src/Modules/Lottery/`；`src/Core/Aneiang.Pa.Dynamic/` → `src/Modules/Dynamic/`；新增 `src/Client/Aneiang.Pa.Client/`。

### 新增（Added）
- **管道架构（Aneiang.Pa.Core.Pipeline）**：基于中间件模型，统一处理日志/缓存/限流/重试/熔断/超时/指标/追踪等横切关注点。
  - `IScrapeInvoker` / `IScrapeMiddleware` / `ScrapeContext`
  - 内置中间件：`LoggingMiddleware`、`MetricsMiddleware`、`TracingMiddleware`、`CacheMiddleware`、`RateLimitMiddleware`、`CircuitBreakerMiddleware`、`RetryMiddleware`、`TimeoutMiddleware`
  - 配置模型：`ScrapeResilienceOptions`、`ScrapeRateLimitOptions`、`ScrapeCacheOptions`，**支持热更新（IOptionsMonitor）**
- **可观测性**：基于 `System.Diagnostics.Metrics` 暴露 `pa_scrape_total` / `pa_scrape_duration_seconds` / `pa_scrape_items_total`；`ActivitySource` `Aneiang.Pa` 兼容 OpenTelemetry。
- **代理池健康跟踪**：`ProxyEntry` 增加 `SuccessCount`/`FailureCount`/`ConsecutiveFailures`/`BannedUntilUtc`，连续失败自动临时禁用 + 半开探测。
- **插件化模块发现**：`IScraperModule` + `[assembly: PaScraperModule]`，第三方包通过 `AddPaScraperFromLoadedAssemblies` 自动注册。
- **统一注册表**：`IScraperRegistry` 跨 News/Lottery/Dynamic 统一查找。
- **健康检查**：`AddPaScrapers()` 集成标准 ASP.NET Core `IHealthCheck`，可被 K8s 探针使用。
- **v2 API**：`ScraperV2Controller` 提供基于管道的统一爬取入口（`/api/scraper/v2/*`）。
- **强类型 SDK**：新增 `Aneiang.Pa.Client` 包，内置 `PaClient.News` / `PaClient.Lottery` / `PaClient.Registry` 子端点。
- **测试矩阵**：新增 `Aneiang.Pa.Core.Tests` 项目（xUnit + FluentAssertions），覆盖 Pipeline、Registry、ProxyPool、ExtendableObject。
- **CI/CD**：GitHub Actions `ci.yml`（PR 构建 + 测试 + 覆盖率）和 `release.yml`（tag 触发 NuGet 发布）。
- **文档**：`docs/ARCHITECTURE-PLAN.md`、`docs/PIPELINE.md`、`docs/PLUGIN-MODULE.md`。

### 性能（Performance）
- **`ExtendableObjectExtensions`**：从全量序列化 O(n²) 改造为基于 `JsonNode` 的增量修改，存储开销显著降低。
- **`DynamicScraper`**：反射元数据通过 `ConcurrentDictionary<Type, DynamicScraperMetadata>` 缓存，预编译 XPath。
- **HttpClient 默认超时**：所有爬虫默认 30 秒超时（修复之前默认 100 秒的隐患）。

### 修复（Fixed）
- `LotteryScraper` 构造函数注释从 "知乎热门爬虫" 修复为 "彩票爬虫"。
- `ScraperHealthCheckService` 超时机制改进（虽然新管道下应使用 `IScrapeInvoker` + `TimeoutMiddleware` 替代）。

### 兼容性（Compatibility）
- 现有 `INewsScraper.GetNewsAsync()`、`ILotteryScraper.GetWelfareLotteryAsync()` 等 API 完全保留。
- `AneiangGenericListResult<T>.IsSuccessd` 拼写问题暂保留（旧调用方仍可用），新 `ScraperResult<T>.IsSuccess` 与之并存。
- 升级建议：保持现有调用，逐步迁移到 `IScrapeInvoker`。

### 弃用（Deprecated）
- `INewsScraperFactory` 仍在但建议迁移到 `IScraperRegistry`。
- 旧 `Controller`（`/api/scraper/news/{source}` 等）继续可用，新代码请用 `/api/scraper/v2/*`。

---

## 2.1.6 (2026-01-15)

- 新增（News）：新增 IT之家热榜爬虫（`Aneiang.Pa.ItHome`）。
- 新增（News）：新增 36氪「48小时人气阅读」抓取支持（`Aneiang.Pa.36kr`）。
- 更新（News）：`Aneiang.Pa.News` 增补注册与 `ScraperSource` 源枚举以支持新平台。
- 优化（Dynamic）：动态爬虫实现细节调整（见 `src/Sectors/Aneiang.Pa.Dynamic/DynamicScraper.cs` 相关提交）。
- 更新（Demo）：示例与配置随新增源做了同步调整。
- 文档：README.md 更新。
- 改进（AspNetCore）：修复没有配置缓存配置是，Redis注册报错问题。

## 2.1.4 (2026-01-07)
- 新增（AspNetCore）：支持“爬取数据缓存”而非响应缓存，支持 None / Memory / Redis，可配置，默认缓存 1 小时。
- 新增（AspNetCore）：对外扩展方法收敛为两个入口：`AddPaScraperApi` 与 `AddPaScraperAuthorization`，简化接入。
- 优化（AspNetCore）：授权从 `AddPaScraperApi` 中拆出，按需显式启用，仍支持 ApiKey / Custom / Combined。
- 修复（AspNetCore）：Redis 注册与配置读取时序问题，避免 `configuration` 为空导致运行时报错。
- 优化（AspNetCore）：`CacheProvider != Redis` 时不注册 Redis，避免无意义连接与配置。
- 变更（AspNetCore）：多目标框架 `netstandard2.1;net6.0`，覆盖 .NET Core 3.0+ 与 .NET 5/6/7/8/9+。
- 改进：README.md 布局与说明优化（折叠长示例等）。

## 2.1.2 (2026-01-02)
- 新增：`AneiangGenericListResult`/`AneiangGenericResult` 统一返回 `UpdatedTime`（当前时间）。

## 2.1.1 (2025-12-31)
- 修复：若干细节问题以及稳定性提升。
- 优化：代码清理及文档微调。

## 2.1.0 (2025-12-29)
- 重大变更：项目架构调整，将爬虫分为 `News` (热榜) 和 `Sectors` (特定领域) 两大类，以提升模块化和可扩展性。
- 新增：新增 `Sectors` (特定领域) 爬虫模块。
- 新增：在 `Sectors` 模块下新增彩票数据爬虫 (`Aneiang.Pa.Lottery`)，支持福利彩票和体育彩票数据。
- 优化：核心库 (`Aneiang.Pa.Core`) 重构，优化了接口、模型和公共服务。
- 优化：调整了依赖注入的注册方式，提供更细粒度的服务注册选项（如 `AddPaScraper`, `AddNewsScraper`, `AddLotteryScraper`）。
- 改进：更新 `README.md` 文档以反映新的项目架构和使用方式。

## 1.1.4 (2025-12-24)
- 新增：ASP.NET Core Web API 集成支持（Aneiang.Pa.AspNetCore 包）
  - 提供开箱即用的 RESTful API 控制器（ScraperController）
  - 支持获取指定平台的新闻数据
  - 支持获取所有可用的爬虫源列表
  - 支持健康检查功能（检查所有爬虫或指定爬虫的健康状态）
- 新增：可选的授权功能
  - 支持 API Key 授权方式
  - 支持自定义授权验证函数
  - 支持组合授权方式（API Key 或自定义验证）
  - 支持排除特定路由不需要授权
- 优化：代理池功能完善
  - 支持轮询和随机两种代理选择策略
  - 支持带认证的代理服务器
- 改进：文档更新和完善
  - 更新 README.md，添加代理池功能详细说明
  - 更新 README.md，添加 ASP.NET Core Web API 集成说明
  - 更新 README.en.md，同步所有新功能说明

## 1.1.3 (2025-12-22)
- 新增：支持配置代理，提高爬取稳定性
- 优化：代码结构和性能改进

## 1.1.2 (2025-12-19)
- 优化：代码质量和性能提升
- 改进：README文档更新和完善

## 1.1.1 (2025-12-19)
- 新增：博客园热榜爬虫支持
- 新增：CSDN热榜爬虫支持
- 改进：各平台爬虫的稳定性和数据准确性

## 1.1.0 (2025-12-18)
- 新增：澎湃新闻热榜爬虫支持
- 新增：凤凰网热榜爬虫支持
- 新增：豆瓣热榜爬虫支持
- 改进：动态爬虫功能的易用性和稳定性
- 优化：HTTP请求处理和错误重试机制

## 1.0.0 (2025-12-17)
- 初始版本发布
- 支持以下平台的热榜爬虫：
  - 微博
  - 知乎
  - B站
  - 百度
  - 抖音
  - 虎扑
  - 今日头条
  - 腾讯新闻
  - 掘金
- 提供动态爬虫功能，支持自定义网站数据爬取
- 实现了完整的依赖注入和工厂模式支持
