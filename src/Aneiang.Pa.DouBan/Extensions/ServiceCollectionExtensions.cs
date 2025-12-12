using Aneiang.Pa.DouBan.Models;
using Aneiang.Pa.DouBan.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aneiang.Pa.DouBan.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册爬虫
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddDouBanScraper(this IServiceCollection services, IConfiguration? configuration = null)
        {
            if (configuration != null)
            {
                services.Configure<DouBanScraperOptions>(configuration.GetSection("Scraper:DouBan"));
            }
            services.AddHttpClient();
            services.AddSingleton<IDouBanNewScraper, DouBanNewScraper>();
        }
    }
}
