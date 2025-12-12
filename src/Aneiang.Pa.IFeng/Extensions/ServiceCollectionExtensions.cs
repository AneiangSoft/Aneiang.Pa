using Aneiang.Pa.IFeng.Models;
using Aneiang.Pa.IFeng.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aneiang.Pa.IFeng.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册爬虫
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddIFengScraper(this IServiceCollection services, IConfiguration? configuration = null)
        {
            if (configuration != null)
            {
                services.Configure<IFengScraperOptions>(configuration.GetSection("Scraper:IFeng"));
            }
            services.AddHttpClient();
            services.AddSingleton<IIFengNewScraper, IFengNewScraper>();
        }
    }
}
