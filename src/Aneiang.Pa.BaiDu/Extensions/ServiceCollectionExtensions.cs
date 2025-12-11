using Aneiang.Pa.BaiDu.Models;
using Aneiang.Pa.BaiDu.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aneiang.Pa.BaiDu.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册爬虫
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddBaiDuScraper(this IServiceCollection services, IConfiguration? configuration = null)
        {
            if (configuration != null)
            {
                services.Configure<BaiDuScraperOptions>(configuration.GetSection("Scraper:BaiDu"));
            }
            services.AddHttpClient();
            services.AddSingleton<IBaiDuNewScraper, BaiDuNewScraper>();
        }
    }
}
