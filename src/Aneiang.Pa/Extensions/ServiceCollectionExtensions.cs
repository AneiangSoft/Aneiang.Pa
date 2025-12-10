using Aneiang.Pa.BaiDu.Models;
using Aneiang.Pa.BaiDu.News;
using Aneiang.Pa.Bilibili.Models;
using Aneiang.Pa.Bilibili.News;
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
                services.Configure<BilibiliScraperOptions>(configuration.GetSection("Scraper:Bilibili"));
                services.Configure<BaiDuScraperOptions>(configuration.GetSection("Scraper:BaiDu"));
            }

            services.AddHttpClient();
            services.AddSingleton<IWeiBoNewScraper, WeiBoNewScraper>();
            services.AddSingleton<IZhiHuNewScraper, ZhiHuNewScraper>();
            services.AddSingleton<IBilibiliNewScraper, BilibiliNewScraper>();
            services.AddSingleton<IBaiDuNewScraper, BaiDuNewScraper>();

            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IWeiBoNewScraper>());
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IZhiHuNewScraper>());
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IBilibiliNewScraper>());
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IBaiDuNewScraper>());
            services.AddSingleton<INewsScraperFactory, NewsScraperFactory>();
        }
    }
}
