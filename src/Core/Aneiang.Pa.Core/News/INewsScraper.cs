using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.Core.Scraper;
using System.Threading;
using System.Threading.Tasks;

namespace Aneiang.Pa.Core.News
{
    /// <summary>
    /// 新闻爬取器
    /// </summary>
    public interface INewsScraper : IScraper<NewsItem>
    {
        /// <summary>
        /// 标识（保留以向后兼容，等价于 Descriptor.Source）
        /// </summary>
        string Source { get; }

        /// <summary>
        /// 获取新闻（保留以向后兼容）
        /// </summary>
        /// <returns>新闻结果</returns>
        Task<AneiangGenericListResult<NewsItem>> GetNewsAsync();
    }
}
