using System.Threading.Tasks;
using Aneiang.Pa.Core.News.Models;

namespace Aneiang.Pa.Core.News
{
    /// <summary>
    /// 新闻爬取器
    /// </summary>
    public interface INewsScraper
    {
        /// <summary>
        /// 标识
        /// </summary>
        string Source { get; }

        /// <summary>
        /// 获取新闻
        /// </summary>
        /// <returns></returns>
        Task<NewsResult> GetNewsAsync();
    }
}
