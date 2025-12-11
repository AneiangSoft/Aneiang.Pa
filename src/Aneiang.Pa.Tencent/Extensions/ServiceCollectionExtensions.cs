using Aneiang.Pa.Tencent.Models;
using Aneiang.Pa.Tencent.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aneiang.Pa.Tencent.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册爬虫
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddTencentScraper(this IServiceCollection services, IConfiguration? configuration = null)
        {
            if (configuration != null)
            {
                services.Configure<TencentScraperOptions>(configuration.GetSection("Scraper:Tencent"));
            }
            services.AddHttpClient();
            services.AddSingleton<ITencentNewScraper, TencentNewScraper>();
        }
    }
}
