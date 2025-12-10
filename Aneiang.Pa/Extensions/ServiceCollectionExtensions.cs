using Aneiang.Pa.Core.News;
using Aneiang.Pa.News;
using Aneiang.Pa.WeiBo.Models;
using Aneiang.Pa.WeiBo.News;
using Aneiang.Pa.ZhiHu.Models;
using Aneiang.Pa.ZhiHu.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aneiang.Pa.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册微博爬取器
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddNewsScraper(this IServiceCollection services, IConfiguration? configuration = null)
        {
            if (configuration != null)
            {
                services.Configure<WeiBoScraperOptions>(configuration.GetSection("Scraper:WeiBo"));
                services.Configure<ZhiHuScraperOptions>(configuration.GetSection("Scraper:ZhiHu"));
            }

            services.AddHttpClient();
            services.AddSingleton<IWeiBoNewScraper, WeiBoNewScraper>();
            services.AddSingleton<IZhiHuNewScraper, ZhiHuNewScraper>();

            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IWeiBoNewScraper>());
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IZhiHuNewScraper>());
            services.AddSingleton<INewsScraperFactory, NewsScraperFactory>();
        }
    }
}
