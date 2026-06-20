using Aneiang.Pa.Core.Models;
using Aneiang.Pa.Core.Scraper;
using System.Threading;
using System.Threading.Tasks;

namespace Aneiang.Pa.Core.Pipeline
{
    /// <summary>
    /// 爬取调用器：在管道中执行 IScraper&lt;T&gt;，自动应用所有中间件
    /// </summary>
    public interface IScrapeInvoker
    {
        /// <summary>
        /// 通过爬虫实例调用
        /// </summary>
        Task<ScraperResult<T>> InvokeAsync<T>(IScraper<T> scraper, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// 通过分类 + 源标识调用
        /// </summary>
        Task<ScraperResult<T>> InvokeAsync<T>(string category, string source, CancellationToken cancellationToken = default) where T : class;
    }
}
