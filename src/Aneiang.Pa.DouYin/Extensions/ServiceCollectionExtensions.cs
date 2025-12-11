using Aneiang.Pa.DouYin.Models;
using Aneiang.Pa.DouYin.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aneiang.Pa.DouYin.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册爬虫
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddDouYinScraper(this IServiceCollection services, IConfiguration? configuration = null)
        {
            if (configuration != null)
            {
                services.Configure<DouYinScraperOptions>(configuration.GetSection("Scraper:DouYin"));
            }
            services.AddHttpClient();
            services.AddSingleton<IDouYinNewScraper, DouYinNewScraper>();
        }
    }
}
