using Aneiang.Pa.BaiDu.Models;
using Aneiang.Pa.BaiDu.News;
using Aneiang.Pa.Bilibili.Models;
using Aneiang.Pa.Bilibili.News;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.DouBan.Models;
using Aneiang.Pa.DouBan.News;
using Aneiang.Pa.DouYin.Models;
using Aneiang.Pa.DouYin.News;
using Aneiang.Pa.HuPu.Models;
using Aneiang.Pa.HuPu.News;
using Aneiang.Pa.IFeng.Models;
using Aneiang.Pa.IFeng.News;
using Aneiang.Pa.JueJin.Models;
using Aneiang.Pa.JueJin.News;
using Aneiang.Pa.News;
using Aneiang.Pa.Tencent.Models;
using Aneiang.Pa.Tencent.News;
using Aneiang.Pa.ThePaper.Models;
using Aneiang.Pa.ThePaper.News;
using Aneiang.Pa.TouTiao.Models;
using Aneiang.Pa.TouTiao.News;
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
                services.Configure<DouYinScraperOptions>(configuration.GetSection("Scraper:DouYin"));
                services.Configure<TouTiaoScraperOptions>(configuration.GetSection("Scraper:TouTiao"));
                services.Configure<HuPuScraperOptions>(configuration.GetSection("Scraper:HuPu"));
                services.Configure<TencentScraperOptions>(configuration.GetSection("Scraper:Tencent"));
                services.Configure<JueJinScraperOptions>(configuration.GetSection("Scraper:JueJin"));
                services.Configure<ThePaperScraperOptions>(configuration.GetSection("Scraper:ThePaper"));
                services.Configure<DouBanScraperOptions>(configuration.GetSection("Scraper:DouBan"));
                services.Configure<IFengScraperOptions>(configuration.GetSection("Scraper:IFeng"));
            }

            services.AddHttpClient();
            services.AddSingleton<IWeiBoNewScraper, WeiBoNewScraper>();
            services.AddSingleton<IZhiHuNewScraper, ZhiHuNewScraper>();
            services.AddSingleton<IBilibiliNewScraper, BilibiliNewScraper>();
            services.AddSingleton<IBaiDuNewScraper, BaiDuNewScraper>();
            services.AddSingleton<IDouYinNewScraper, DouYinNewScraper>();
            services.AddSingleton<ITouTiaoNewScraper, TouTiaoNewScraper>();
            services.AddSingleton<IHuPuNewScraper, HuPuNewScraper>();
            services.AddSingleton<ITencentNewScraper, TencentNewScraper>();
            services.AddSingleton<IJueJinNewScraper, JueJinNewScraper>();
            services.AddSingleton<IThePaperNewScraper, ThePaperNewScraper>();
            services.AddSingleton<IDouBanNewScraper, DouBanNewScraper>();
            services.AddSingleton<IIFengNewScraper, IFengNewScraper>();

            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IWeiBoNewScraper>());
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IZhiHuNewScraper>());
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IBilibiliNewScraper>());
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IBaiDuNewScraper>());
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IDouYinNewScraper>());
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<ITouTiaoNewScraper>());
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IHuPuNewScraper>());
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<ITencentNewScraper>());
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IJueJinNewScraper>());
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IThePaperNewScraper>());
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IDouBanNewScraper>()); 
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IIFengNewScraper>());

            services.AddSingleton<INewsScraperFactory, NewsScraperFactory>();
        }
    }
}
