namespace Aneiang.Pa.Core.Scraper
{
    /// <summary>
    /// 所有爬虫的基础接口
    /// </summary>
    public interface IScraper
    {
        /// <summary>
        /// 爬虫元数据描述符
        /// </summary>
        ScraperDescriptor Descriptor { get; }
    }
}
