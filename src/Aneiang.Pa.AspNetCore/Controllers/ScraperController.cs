using System;
using System.Threading.Tasks;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.Models;
using Aneiang.Pa.News;
using Microsoft.AspNetCore.Mvc;

namespace Aneiang.Pa.AspNetCore.Controllers
{
    /// <summary>
    /// 爬虫控制器
    /// </summary>
    [ApiController]
    [Route("api/scraper")]
    public class ScraperController : ControllerBase
    {
        private readonly INewsScraperFactory _scraperFactory;

        /// <summary>
        /// 初始化爬虫控制器
        /// </summary>
        /// <param name="scraperFactory">爬虫工厂</param>
        public ScraperController(INewsScraperFactory scraperFactory)
        {
            _scraperFactory = scraperFactory ?? throw new ArgumentNullException(nameof(scraperFactory));
        }

        /// <summary>
        /// 获取指定平台的新闻
        /// </summary>
        /// <param name="source">爬虫源</param>
        /// <returns>新闻结果</returns>
        [HttpGet("{source}")]
        public async Task<ActionResult<NewsResult>> GetNews([FromRoute] string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return BadRequest(new NewsResult(false, "源参数不能为空"));
            }

            try
            {
                // 尝试解析枚举值（支持大小写不敏感）
                if (!System.Enum.TryParse<ScraperSource>(source, true, out var scraperSource))
                {
                    return NotFound(new NewsResult(false, $"不支持的爬虫源: {source}"));
                }

                var scraper = _scraperFactory.GetScraper(scraperSource);
                var result = await scraper.GetNewsAsync();
                
                if (!result.IsSuccessd)
                {
                    return StatusCode(500, result);
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new NewsResult(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new NewsResult(false, $"获取新闻失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取所有支持的爬虫源列表
        /// </summary>
        /// <returns>支持的爬虫源列表</returns>
        [HttpGet]
        public ActionResult GetAvailableSources()
        {
            var sources = System.Enum.GetNames(typeof(ScraperSource));
            return Ok(new { sources });
        }
    }
}

