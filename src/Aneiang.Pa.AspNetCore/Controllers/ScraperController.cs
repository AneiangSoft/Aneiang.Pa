using Aneiang.Pa.AspNetCore.Caching;
using Aneiang.Pa.AspNetCore.Options;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.Core.Pipeline;
using Aneiang.Pa.Core.Scraper;
using Aneiang.Pa.Lottery.Services;
using Aneiang.Pa.News.Models;
using Aneiang.Pa.News.News;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aneiang.Pa.Lottery.Data;

namespace Aneiang.Pa.AspNetCore.Controllers
{
    /// <summary>
    /// 爬虫控制器
    /// </summary>
    [ApiController]
    [Route("api/scraper")]
    [Produces("application/json")]
    public class ScraperController : ControllerBase
    {
        private readonly INewsScraperFactory _scraperFactory;
        private readonly ILogger<ScraperController> _logger;
        private readonly ScraperControllerOptions _options;
        private readonly IScraperHealthCheckService? _healthCheckService;
        private readonly ILotteryScraper _lotteryScraper;
        private readonly ICacheService _cache;
        private readonly IScraperRegistry _registry;
        private readonly IScrapeInvoker? _invoker;

        private static readonly string[] AvailableSources = Enum.GetNames(typeof(ScraperSource));
        private static readonly string[] AvailableLotteryTypes = Enum.GetNames(typeof(LotteryType));

        /// <summary>
        /// 初始化爬虫控制器
        /// </summary>
        public ScraperController(
            INewsScraperFactory scraperFactory,
            ILogger<ScraperController> logger,
            IOptions<ScraperControllerOptions> options,
            ILotteryScraper lotteryScraper,
            ICacheService cache,
            IScraperRegistry registry,
            IScrapeInvoker? invoker = null,
            IScraperHealthCheckService? healthCheckService = null)
        {
            _scraperFactory = scraperFactory ?? throw new ArgumentNullException(nameof(scraperFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _lotteryScraper = lotteryScraper;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _invoker = invoker;
            _options = options?.Value ?? new ScraperControllerOptions();
            _healthCheckService = healthCheckService;
        }

        /// <summary>
        /// 获取指定平台的新闻
        /// </summary>
        /// <param name="source">爬虫源（支持大小写不敏感）</param>
        /// <returns>新闻结果</returns>
        /// <response code="200">成功获取新闻</response>
        /// <response code="400">请求参数无效</response>
        /// <response code="404">不支持的爬虫源</response>
        /// <response code="500">服务器内部错误</response>
        [HttpGet("news/{source}")]
        [ProducesResponseType(typeof(AneiangGenericListResult<NewsItem>), 200)]
        [ProducesResponseType(typeof(AneiangGenericListResult<NewsItem>), 400)]
        [ProducesResponseType(typeof(AneiangGenericListResult<NewsItem>), 404)]
        [ProducesResponseType(typeof(AneiangGenericListResult<NewsItem>), 500)]

        public async Task<ActionResult<AneiangGenericListResult<NewsItem>>> GetNews([FromRoute] string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                _logger.LogWarning("GetNews called with empty source parameter");
                return BadRequest(new AneiangGenericListResult<NewsItem>(false, "源参数不能为空"));
            }

            try
            {
                // 尝试解析枚举值（支持大小写不敏感）
                if (!System.Enum.TryParse<ScraperSource>(source, true, out var scraperSource))
                {
                    _logger.LogWarning("Unsupported scraper source: {Source}", source);
                    return NotFound(new AneiangGenericListResult<NewsItem>(false, $"不支持的爬虫源: {source}"));
                }

                var cacheKey = $"scraper:news:{scraperSource}";

                var result = await _cache.GetOrCreateAsync(cacheKey, async () =>
                {
                    _logger.LogInformation("Fetching news from source: {Source}", scraperSource);
                    var scraper = _scraperFactory.GetScraper(scraperSource);
                    return await scraper.GetNewsAsync();
                }, _options.CacheDuration);


                if (!result.IsSuccessd)
                {
                    _logger.LogError("Failed to fetch news from {Source}: {ErrorMessage}", scraperSource, result.ErrorMessage);
                    return StatusCode(500, result);
                }

                _logger.LogInformation("Successfully fetched {Count} news items from {Source}", result.Data.Count, scraperSource);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "ArgumentException when fetching news from source: {Source}", source);
                return NotFound(new AneiangGenericListResult<NewsItem>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when fetching news from source: {Source}", source);
                return StatusCode(500, new AneiangGenericListResult<NewsItem>(false, $"获取新闻失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取所有支持的爬虫源列表
        /// </summary>
        /// <returns>支持的爬虫源列表</returns>
        /// <response code="200">成功获取爬虫源列表</response>
        [HttpGet("news/sources")]
        [ProducesResponseType(typeof(object), 200)]

        public ActionResult GetNewsSources()
        {
            return Ok(new
            {
                sources = AvailableSources,
                count = AvailableSources.Length
            });
        }

        /// <summary>
        /// 获取福利彩票开奖信息
        /// </summary>
        /// <param name="type">福利彩票类型（支持大小写不敏感）</param>
        /// <param name="pageNo">当前页数（默认为1）</param>
        /// <param name="pageSize">每页请求数量（默认为30）</param>
        /// <returns>彩票开奖信息</returns>
        /// <response code="200">成功获取彩票开奖信息</response>
        /// <response code="400">请求参数无效</response>
        /// <response code="404">不支持的爬虫源</response>
        /// <response code="500">服务器内部错误</response>
        [HttpGet("lottery/welfare/{type}")]
        [ProducesResponseType(typeof(AneiangGenericResult<WelfareLotteryData>), 200)]
        [ProducesResponseType(typeof(AneiangGenericResult<WelfareLotteryData>), 400)]
        [ProducesResponseType(typeof(AneiangGenericResult<WelfareLotteryData>), 404)]
        [ProducesResponseType(typeof(AneiangGenericResult<WelfareLotteryData>), 500)]

        public async Task<ActionResult<AneiangGenericResult<WelfareLotteryData>>> GetWelfareLottery([FromRoute] string type, int? pageNo, int? pageSize)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                _logger.LogWarning("GetWelfareLottery called with empty type parameter");
                return BadRequest(new AneiangGenericResult<WelfareLotteryData>(false, "福利彩票类型参数不能为空"));
            }

            try
            {
                if (!Enum.TryParse<LotteryType>(type, true, out var lotteryType))
                {
                    _logger.LogWarning("Unsupported welfare lottery type: {Source}", lotteryType);
                    return NotFound(new AneiangGenericResult<WelfareLotteryData>(false, $"不支持的福利彩票类型: {type}"));
                }

                var pn = pageNo ?? 1;
                var ps = pageSize ?? 30;
                var cacheKey = $"scraper:lottery:welfare:{lotteryType}:{pn}:{ps}";

                var result = await _cache.GetOrCreateAsync(cacheKey,
                    () => _lotteryScraper.GetWelfareLotteryAsync(lotteryType, pn, ps),
                    _options.CacheDuration);

                if (!result.IsSuccessd)
                {
                    _logger.LogError("Failed to fetch welfare lottery from {Source}: {ErrorMessage}", lotteryType, result.ErrorMessage);
                    return StatusCode(500, result);
                }

                _logger.LogInformation("Successfully fetched {Count} welfare lottery items from {Source}", result.Data?.Total, lotteryType);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "ArgumentException when fetching welfare lottery from type: {Source}", type);
                return NotFound(new AneiangGenericResult<WelfareLotteryData>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when fetching welfare lottery from type: {Source}", type);
                return StatusCode(500, new AneiangGenericResult<WelfareLotteryData>(false, $"获取福利彩票开奖信息失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取体育彩票开奖信息
        /// </summary>
        /// <param name="type">体育彩票类型（支持大小写不敏感）</param>
        /// <param name="pageNo">当前页数（默认为1）</param>
        /// <param name="pageSize">每页请求数量（默认为30）</param>
        /// <returns>彩票开奖信息</returns>
        /// <response code="200">成功获取彩票开奖信息</response>
        /// <response code="400">请求参数无效</response>
        /// <response code="404">不支持的爬虫源</response>
        /// <response code="500">服务器内部错误</response>
        [HttpGet("lottery/sport/{type}")]
        [ProducesResponseType(typeof(AneiangGenericResult<SportLotteryResult>), 200)]
        [ProducesResponseType(typeof(AneiangGenericResult<SportLotteryResult>), 400)]
        [ProducesResponseType(typeof(AneiangGenericResult<SportLotteryResult>), 404)]
        [ProducesResponseType(typeof(AneiangGenericResult<SportLotteryResult>), 500)]

        public async Task<ActionResult<AneiangGenericResult<SportLotteryResult>>> GetSportLottery([FromRoute] string type, int? pageNo, int? pageSize)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                _logger.LogWarning("GetSportLottery called with empty type parameter");
                return BadRequest(new AneiangGenericResult<SportLotteryResult>(false, "体育彩票类型参数不能为空"));
            }

            try
            {
                if (!Enum.TryParse<LotteryType>(type, true, out var lotteryType))
                {
                    _logger.LogWarning("Unsupported sport lottery type: {Source}", lotteryType);
                    return NotFound(new AneiangGenericResult<SportLotteryResult>(false, $"不支持的体育彩票类型: {type}"));
                }

                var pn = pageNo ?? 1;
                var ps = pageSize ?? 30;
                var cacheKey = $"scraper:lottery:sport:{lotteryType}:{pn}:{ps}";

                var result = await _cache.GetOrCreateAsync(cacheKey,
                    () => _lotteryScraper.GetSportLotteryAsync(lotteryType, pn, ps),
                    _options.CacheDuration);

                if (!result.IsSuccessd)
                {
                    _logger.LogError("Failed to fetch sport lottery from {Source}: {ErrorMessage}", lotteryType, result.ErrorMessage);
                    return StatusCode(500, result);
                }

                _logger.LogInformation("Successfully fetched {Count} sport lottery items from {Source}", result.Data?.Value.Total, lotteryType);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "ArgumentException when fetching sport lottery from type: {Source}", type);
                return NotFound(new AneiangGenericResult<SportLotteryResult>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when fetching sport lottery from type: {Source}", type);
                return StatusCode(500, new AneiangGenericResult<SportLotteryResult>(false, $"获取体育彩票开奖信息失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取所有支持的彩票类型列表
        /// </summary>
        /// <returns>支持的彩票类型列表</returns>
        /// <response code="200">成功获取支持的彩票类型列表</response>
        [HttpGet("lottery/types")]
        [ProducesResponseType(typeof(object), 200)]

        public ActionResult GetLotteryTypes()
        {
            return Ok(new
            {
                sources = AvailableLotteryTypes,
                count = AvailableLotteryTypes.Length
            });
        }

        /// <summary>
        /// 检查所有爬虫的健康状态
        /// </summary>
        /// <param name="timeoutMs">超时时间（毫秒），默认5000</param>
        /// <returns>健康检查结果</returns>
        /// <response code="200">成功完成健康检查</response>
        /// <response code="503">健康检查服务不可用</response>
        [HttpGet("health")]
        [ProducesResponseType(typeof(HealthCheckResult), 200)]
        [ProducesResponseType(typeof(object), 503)]

        public async Task<ActionResult<HealthCheckResult>> CheckHealth([FromQuery] int? timeoutMs = null)
        {
            if (_healthCheckService == null)
            {
                _logger.LogWarning("健康检查服务未注册");
                return StatusCode(503, new { error = "健康检查服务不可用" });
            }

            try
            {
                var timeout = timeoutMs ?? 5000;
                if (timeout <= 0 || timeout > 60000)
                {
                    return BadRequest(new { error = "超时时间必须在 1-60000 毫秒之间" });
                }

                _logger.LogInformation("开始执行爬虫健康检查，超时时间：{Timeout}ms", timeout);
                var result = await _healthCheckService.CheckAllAsync(timeout);

                if (result.IsHealthy)
                {
                    _logger.LogInformation("所有爬虫健康检查通过");
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning(
                        "部分爬虫健康检查失败：总计 {Total}，健康 {Healthy}，不健康 {Unhealthy}",
                        result.TotalCount,
                        result.HealthyCount,
                        result.UnhealthyCount);
                    return StatusCode(503, result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "健康检查过程中发生异常");
                return StatusCode(500, new { error = $"健康检查失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 检查指定爬虫的健康状态
        /// </summary>
        /// <param name="source">爬虫源（支持大小写不敏感）</param>
        /// <param name="timeoutMs">超时时间（毫秒），默认5000</param>
        /// <returns>爬虫健康状态</returns>
        /// <response code="200">成功完成健康检查</response>
        /// <response code="400">请求参数无效</response>
        /// <response code="404">不支持的爬虫源</response>
        /// <response code="503">健康检查服务不可用</response>
        [HttpGet("{source}/health")]
        [ProducesResponseType(typeof(ScraperHealthStatus), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 503)]

        public async Task<ActionResult<ScraperHealthStatus>> CheckScraperHealth(
            [FromRoute] string source,
            [FromQuery] int? timeoutMs = null)
        {
            if (_healthCheckService == null)
            {
                _logger.LogWarning("健康检查服务未注册");
                return StatusCode(503, new { error = "健康检查服务不可用" });
            }

            if (string.IsNullOrWhiteSpace(source))
            {
                return BadRequest(new { error = "源参数不能为空" });
            }

            try
            {
                // 尝试解析枚举值（支持大小写不敏感）
                if (!System.Enum.TryParse<ScraperSource>(source, true, out var scraperSource))
                {
                    _logger.LogWarning("不支持的爬虫源: {Source}", source);
                    return NotFound(new { error = $"不支持的爬虫源: {source}" });
                }

                var timeout = timeoutMs ?? 5000;
                if (timeout <= 0 || timeout > 60000)
                {
                    return BadRequest(new { error = "超时时间必须在 1-60000 毫秒之间" });
                }

                _logger.LogInformation("开始检查爬虫 {Source} 的健康状态，超时时间：{Timeout}ms", scraperSource, timeout);
                var scraper = _scraperFactory.GetScraper(scraperSource);
                var status = await _healthCheckService.CheckAsync(scraper, timeout);

                if (status.IsHealthy)
                {
                    _logger.LogInformation("爬虫 {Source} 健康检查通过", scraperSource);
                    return Ok(status);
                }
                else
                {
                    _logger.LogWarning("爬虫 {Source} 健康检查失败: {Error}", scraperSource, status.ErrorMessage);
                    return StatusCode(503, status);
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "ArgumentException when checking health for source: {Source}", source);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "健康检查过程中发生异常: {Source}", source);
                return StatusCode(500, new { error = $"健康检查失败: {ex.Message}" });
            }
        }

        // ========================= 统一入口（基于 IScraperRegistry） =========================

        /// <summary>
        /// 获取所有已注册爬虫的描述符列表（支持按分类筛选）
        /// </summary>
        /// <param name="category">可选分类筛选</param>
        /// <returns>爬虫描述符列表</returns>
        [HttpGet("registry/descriptors")]
        [ProducesResponseType(typeof(object), 200)]
        public ActionResult GetAllScrapers([FromQuery] string? category = null)
        {
            var descriptors = _registry.GetDescriptors(category).ToList();
            return Ok(new
            {
                scrapers = descriptors,
                count = descriptors.Count,
                categories = _registry.GetCategories()
            });
        }

        /// <summary>
        /// 获取所有已注册分类
        /// </summary>
        /// <returns>分类列表</returns>
        [HttpGet("registry/categories")]
        [ProducesResponseType(typeof(object), 200)]
        public ActionResult GetCategories()
        {
            return Ok(new
            {
                categories = _registry.GetCategories()
            });
        }

        /// <summary>
        /// 统一爬取入口：按 Category + Source 获取数据
        /// </summary>
        /// <param name="category">分类（如 News / Lottery）</param>
        /// <param name="source">源标识（如 BaiDu / SSQ）</param>
        /// <returns>爬取结果</returns>
        [HttpGet("fetch/{category}/{source}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<ActionResult> FetchScraper(string category, string source)
        {
            if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(source))
            {
                return BadRequest(new { error = "category 和 source 参数不能为空" });
            }

            try
            {
                var scraper = _registry.GetScraper(category, source);
                if (scraper == null)
                {
                    _logger.LogWarning("未找到爬虫: {Category}/{Source}", category, source);
                    return NotFound(new { error = $"未找到爬虫: {category}/{source}" });
                }

                var cacheKey = scraper.Descriptor.ToCacheKey();
                var descriptor = scraper.Descriptor;

                object result;
                if (descriptor.Category == ScraperCategories.News && scraper is INewsScraper newsScraper)
                {
                    result = await _cache.GetOrCreateAsync(cacheKey,
                        () => newsScraper.GetNewsAsync(),
                        _options.CacheDuration);
                }
                else
                {
                    _logger.LogWarning("FetchScraper 暂不支持该爬虫类型的直接调用: {Category}/{Source}", category, source);
                    return StatusCode(501, new { error = $"暂不支持直接调用 {category}/{source}，请使用专用端点" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FetchScraper 异常: {Category}/{Source}", category, source);
                return StatusCode(500, new { error = $"爬取失败: {ex.Message}" });
            }
        }
    }
}

