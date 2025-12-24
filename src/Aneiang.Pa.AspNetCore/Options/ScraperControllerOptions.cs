namespace Aneiang.Pa.AspNetCore.Options
{
    /// <summary>
    /// 爬虫控制器配置选项
    /// </summary>
    public class ScraperControllerOptions
    {
        /// <summary>
        /// 路由前缀，默认为 "api/scraper"
        /// </summary>
        public string RoutePrefix { get; set; } = "api/scraper";

        /// <summary>
        /// 是否启用所有爬虫端点，默认为 true
        /// </summary>
        public bool EnableAllEndpoints { get; set; } = true;

        /// <summary>
        /// 是否在路由中使用小写，默认为 true（如：/api/scraper/weibo）
        /// </summary>
        public bool UseLowercaseInRoute { get; set; } = true;
    }
}

