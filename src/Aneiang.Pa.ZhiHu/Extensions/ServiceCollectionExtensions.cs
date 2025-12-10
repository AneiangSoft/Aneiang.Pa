using Aneiang.Pa.ZhiHu.Models;
using Aneiang.Pa.ZhiHu.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aneiang.Pa.ZhiHu.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册微博爬取器
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddZhiHuScraper(this IServiceCollection services, IConfiguration? configuration = null)
        {
            if (configuration != null)
            {
                services.Configure<ZhiHuScraperOptions>(configuration.GetSection("Scraper:ZhiHu"));
            }
            services.AddHttpClient();
            services.AddSingleton<IZhiHuNewScraper, ZhiHuNewScraper>();
        }
    }
}
