using Aneiang.Pa.Core.Models;
using System.Threading.Tasks;

namespace Aneiang.Pa.Core.Pipeline
{
    /// <summary>
    /// 管道中下一节点的委托
    /// </summary>
    public delegate Task<ScraperResult<T>> ScrapeDelegate<T>(ScrapeContext context) where T : class;

    /// <summary>
    /// 爬取中间件接口
    /// </summary>
    public interface IScrapeMiddleware
    {
        /// <summary>
        /// 执行顺序，数值越小越靠外层（先执行）
        /// </summary>
        int Order { get; }

        /// <summary>
        /// 执行管道节点
        /// </summary>
        Task<ScraperResult<T>> InvokeAsync<T>(ScrapeContext context, ScrapeDelegate<T> next) where T : class;
    }
}
