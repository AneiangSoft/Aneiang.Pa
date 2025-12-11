using Aneiang.Pa.TouTiao.Models;
using Aneiang.Pa.TouTiao.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aneiang.Pa.TouTiao.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册爬虫
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddTouTiaoScraper(this IServiceCollection services, IConfiguration? configuration = null)
        {
            if (configuration != null)
            {
                services.Configure<TouTiaoScraperOptions>(configuration.GetSection("Scraper:TouTiao"));
            }
            services.AddHttpClient();
            services.AddSingleton<ITouTiaoNewScraper, TouTiaoNewScraper>();
        }
    }
}
