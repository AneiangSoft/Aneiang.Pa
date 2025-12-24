using System;
using System.Linq;
using System.Threading.Tasks;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.Models;
using Aneiang.Pa.News;
using Aneiang.Pa.AspNetCore.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        private static readonly string[] AvailableSources = System.Enum.GetNames(typeof(ScraperSource));

        /// <summary>
        /// 初始化爬虫控制器
        /// </summary>
        /// <param name="scraperFactory">爬虫工厂</param>
        /// <param name="logger">日志记录器</param>
        /// <param name="options">配置选项</param>
        /// <param name="healthCheckService">健康检查服务（可选）</param>
        public ScraperController(
            INewsScraperFactory scraperFactory, 
            ILogger<ScraperController> logger,
            IOptions<ScraperControllerOptions> options,
            IScraperHealthCheckService? healthCheckService = null)
        {
            _scraperFactory = scraperFactory ?? throw new ArgumentNullException(nameof(scraperFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        [HttpGet("{source}")]
        [ProducesResponseType(typeof(NewsResult), 200)]
        [ProducesResponseType(typeof(NewsResult), 400)]
        [ProducesResponseType(typeof(NewsResult), 404)]
        [ProducesResponseType(typeof(NewsResult), 500)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult<NewsResult>> GetNews([FromRoute] string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                _logger.LogWarning("GetNews called with empty source parameter");
                return BadRequest(new NewsResult(false, "源参数不能为空"));
            }

            try
            {
                // 尝试解析枚举值（支持大小写不敏感）
                if (!System.Enum.TryParse<ScraperSource>(source, true, out var scraperSource))
                {
                    _logger.LogWarning("Unsupported scraper source: {Source}", source);
                    return NotFound(new NewsResult(false, $"不支持的爬虫源: {source}"));
                }

                _logger.LogInformation("Fetching news from source: {Source}", scraperSource);
                var scraper = _scraperFactory.GetScraper(scraperSource);
                var result = await scraper.GetNewsAsync();
                
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
                return NotFound(new NewsResult(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when fetching news from source: {Source}", source);
                return StatusCode(500, new NewsResult(false, $"获取新闻失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取所有支持的爬虫源列表
        /// </summary>
        /// <returns>支持的爬虫源列表</returns>
        /// <response code="200">成功获取爬虫源列表</response>
        [HttpGet("available-sources")]
        [ProducesResponseType(typeof(object), 200)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public ActionResult GetAvailableSources()
        {
            return Ok(new 
            { 
                sources = AvailableSources,
                count = AvailableSources.Length
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
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
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
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
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
    }
}

