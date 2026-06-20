using System.Threading;
using System.Threading.Tasks;
using Aneiang.Pa.Core.Models;

namespace Aneiang.Pa.Core.Scraper
{
    /// <summary>
    /// 泛型爬虫接口，统一爬取入口
    /// </summary>
    /// <typeparam name="TResult">结果条目类型</typeparam>
    public interface IScraper<TResult> : IScraper where TResult : class
    {
        /// <summary>
        /// 执行爬取
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>爬取结果</returns>
        Task<ScraperResult<TResult>> ScrapeAsync(CancellationToken cancellationToken = default);
    }
}
