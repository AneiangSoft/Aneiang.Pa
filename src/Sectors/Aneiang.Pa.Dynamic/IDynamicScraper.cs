using Aneiang.Pa.Core.Scraper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aneiang.Pa.Dynamic
{
    /// <summary>
    /// 动态爬虫接口
    /// </summary>
    public interface IDynamicScraper : IScraper
    {

        /// <summary>
        /// 通用数据抓取，需要配合HtmlHeaderAttribute使用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> DataScraperAsync<T>() where T : new();

        /// <summary>
        /// 通用数据集抓取，需要配合HtmlHeaderAttribute使用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<List<T>> DatasetScraperAsync<T>() where T : new();

        /// <summary>
        /// 通用数据抓取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="referer"></param>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        Task<T> DataScraperAsync<T>(string url, string? referer = null, string? userAgent = null) where T : new();

        /// <summary>
        /// 通用数据集抓取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="referer"></param>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        Task<List<T>> DatasetScraperAsync<T>(string url, string? referer = null, string? userAgent = null) where T : new();
    }
}
