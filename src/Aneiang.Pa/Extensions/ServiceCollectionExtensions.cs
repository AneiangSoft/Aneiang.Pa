using Aneiang.Pa.BaiDu.Models;
using Aneiang.Pa.BaiDu.News;
using Aneiang.Pa.Bilibili.Models;
using Aneiang.Pa.Bilibili.News;
using Aneiang.Pa.CnBlog.Models;
using Aneiang.Pa.CnBlog.News;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.Csdn.Models;
using Aneiang.Pa.Csdn.News;
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
using System;
using System.Net.Http;
using Aneiang.Pa.Core.Data;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aneiang.Pa.Extensions
{
    /// <summary>
    /// The service collection extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册新闻爬取器
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="httpConfigureHandler"></param>
        public static IServiceCollection AddNewsScraper(this IServiceCollection services, IConfiguration? configuration = null, Func<HttpMessageHandler>? httpConfigureHandler = null)
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
                services.Configure<CsdnScraperOptions>(configuration.GetSection("Scraper:Csdn"));
                services.Configure<CnBlogScraperOptions>(configuration.GetSection("Scraper:CnBlog"));
            }

            var httpClientBuilder = services.AddHttpClient(PaConsts.DefaultHttpClientName);
            if (httpConfigureHandler != null)
            {
                httpClientBuilder.ConfigurePrimaryHttpMessageHandler(httpConfigureHandler);
            }
            services.TryAddSingleton<IWeiBoNewScraper, WeiBoNewScraper>();
            services.TryAddSingleton<IZhiHuNewScraper, ZhiHuNewScraper>();
            services.TryAddSingleton<IBilibiliNewScraper, BilibiliNewScraper>();
            services.TryAddSingleton<IBaiDuNewScraper, BaiDuNewScraper>();
            services.TryAddSingleton<IDouYinNewScraper, DouYinNewScraper>();
            services.TryAddSingleton<ITouTiaoNewScraper, TouTiaoNewScraper>();
            services.TryAddSingleton<IHuPuNewScraper, HuPuNewScraper>();
            services.TryAddSingleton<ITencentNewScraper, TencentNewScraper>();
            services.TryAddSingleton<IJueJinNewScraper, JueJinNewScraper>();
            services.TryAddSingleton<IThePaperNewScraper, ThePaperNewScraper>();
            services.TryAddSingleton<IDouBanNewScraper, DouBanNewScraper>();
            services.TryAddSingleton<IIFengNewScraper, IFengNewScraper>();
            services.TryAddSingleton<ICsdnNewScraper, CsdnNewScraper>();
            services.TryAddSingleton<ICnBlogNewScraper, CnBlogNewScraper>();

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
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<ICsdnNewScraper>());
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<ICnBlogNewScraper>());

            services.AddSingleton<INewsScraperFactory, NewsScraperFactory>();

            return services;
        }
    }
}
