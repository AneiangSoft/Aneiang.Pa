using System.Collections.Generic;

namespace Aneiang.Pa.Core.Scraper
{
    /// <summary>
    /// 爬虫注册表接口，统一管理所有已注册的爬虫实例
    /// </summary>
    public interface IScraperRegistry
    {
        /// <summary>
        /// 按 Category + Source 获取爬虫（非泛型）
        /// </summary>
        IScraper? GetScraper(string category, string source);

        /// <summary>
        /// 获取指定分类下所有爬虫
        /// </summary>
        IEnumerable<IScraper> GetScrapers(string? category = null);

        /// <summary>
        /// 获取所有已注册爬虫的描述符
        /// </summary>
        IEnumerable<ScraperDescriptor> GetDescriptors(string? category = null);

        /// <summary>
        /// 获取所有已注册分类
        /// </summary>
        IEnumerable<string> GetCategories();

        /// <summary>
        /// 判断指定爬虫是否存在
        /// </summary>
        bool Contains(string category, string source);
    }
}
