using Aneiang.Pa.Csdn.Models;
using Aneiang.Pa.Csdn.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aneiang.Pa.Csdn.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册爬虫
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddCsdnScraper(this IServiceCollection services, IConfiguration? configuration = null)
        {
            if (configuration != null)
            {
                services.Configure<CsdnScraperOptions>(configuration.GetSection("Scraper:Csdn"));
            }
            services.AddHttpClient();
            services.AddSingleton<ICsdnNewScraper, CsdnNewScraper>();
        }
    }
}
