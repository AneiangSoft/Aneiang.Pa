using Aneiang.Pa.Core.Models;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.Core.Pipeline;
using Aneiang.Pa.Core.Proxy;
using Aneiang.Pa.Core.Scraper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aneiang.Pa.AspNetCore.Controllers
{
    /// <summary>
    /// V2 统一爬虫控制器（基于管道 + 注册表）
    /// </summary>
    [ApiController]
    [Route("api/scraper/v2")]
    [Produces("application/json")]
    public class ScraperV2Controller : ControllerBase
    {
        private readonly IScraperRegistry _registry;
        private readonly IScrapeInvoker _invoker;
        private readonly IProxyPool? _proxyPool;
        private readonly ILogger<ScraperV2Controller> _logger;

        /// <summary>
        /// 初始化 v2 控制器
        /// </summary>
        public ScraperV2Controller(
            IScraperRegistry registry,
            IScrapeInvoker invoker,
            ILogger<ScraperV2Controller> logger,
            IProxyPool? proxyPool = null)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _invoker = invoker ?? throw new ArgumentNullException(nameof(invoker));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _proxyPool = proxyPool;
        }

        /// <summary>
        /// 列出所有已注册分类
        /// </summary>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(object), 200)]
        public ActionResult ListCategories()
            => Ok(new { categories = _registry.GetCategories() });

        /// <summary>
        /// 列出指定分类下的所有源（可选）
        /// </summary>
        [HttpGet("sources")]
        [ProducesResponseType(typeof(object), 200)]
        public ActionResult ListSources([FromQuery] string? category)
        {
            var descriptors = _registry.GetDescriptors(category).ToList();
            return Ok(new
            {
                count = descriptors.Count,
                sources = descriptors.Select(d => new
                {
                    category = d.Category,
                    source = d.Source,
                    displayName = d.DisplayName,
                    supportsPaging = d.SupportsPaging
                })
            });
        }

        /// <summary>
        /// 统一爬取入口（POST 形式，便于扩展请求参数）
        /// </summary>
        [HttpPost("scrape")]
        [ProducesResponseType(typeof(ScraperResult<NewsItem>), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<ActionResult> Scrape(
            [FromBody] ScrapeInvocationDto dto,
            CancellationToken cancellationToken)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Category) || string.IsNullOrWhiteSpace(dto.Source))
                return BadRequest(new { error = "category 和 source 必填" });

            var scraper = _registry.GetScraper(dto.Category, dto.Source);
            if (scraper == null)
                return NotFound(new { error = $"未找到爬虫: {dto.Category}/{dto.Source}" });

            try
            {
                var result = await _invoker.InvokeAsync<NewsItem>(dto.Category, dto.Source, cancellationToken).ConfigureAwait(false);
                if (!result.IsSuccess)
                    return StatusCode(500, result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "v2/scrape 异常 {Category}/{Source}", dto.Category, dto.Source);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 简易 GET 入口（不支持 ScrapeRequest 参数）
        /// </summary>
        [HttpGet("fetch/{category}/{source}")]
        [ProducesResponseType(typeof(ScraperResult<NewsItem>), 200)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<ActionResult> Fetch(string category, string source, CancellationToken cancellationToken)
        {
            var scraper = _registry.GetScraper(category, source);
            if (scraper == null)
                return NotFound(new { error = $"未找到爬虫: {category}/{source}" });

            try
            {
                var result = await _invoker.InvokeAsync<NewsItem>(category, source, cancellationToken).ConfigureAwait(false);
                return result.IsSuccess ? Ok(result) : StatusCode(500, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "v2/fetch 异常 {Category}/{Source}", category, source);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 获取代理池健康快照
        /// </summary>
        [HttpGet("proxy-pool/health")]
        [ProducesResponseType(typeof(object), 200)]
        public ActionResult GetProxyHealth()
        {
            if (_proxyPool == null)
                return Ok(new { enabled = false });
            var snapshot = _proxyPool.GetHealthSnapshot();
            return Ok(new { enabled = true, count = snapshot.Count, proxies = snapshot });
        }
    }

    /// <summary>
    /// v2/scrape 请求体
    /// </summary>
    public class ScrapeInvocationDto
    {
        /// <summary>分类</summary>
        public string Category { get; set; } = string.Empty;
        /// <summary>源标识</summary>
        public string Source { get; set; } = string.Empty;
        /// <summary>请求参数</summary>
        public ScrapeRequest? Request { get; set; }
    }
}
