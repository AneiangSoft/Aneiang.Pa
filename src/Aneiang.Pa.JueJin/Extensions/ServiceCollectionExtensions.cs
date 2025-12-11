using Aneiang.Pa.JueJin.Models;
using Aneiang.Pa.JueJin.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aneiang.Pa.JueJin.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册爬虫
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddJueJinScraper(this IServiceCollection services, IConfiguration? configuration = null)
        {
            if (configuration != null)
            {
                services.Configure<JueJinScraperOptions>(configuration.GetSection("Scraper:JueJin"));
            }
            services.AddHttpClient();
            services.AddSingleton<IJueJinNewScraper, JueJinNewScraper>();
        }
    }
}
