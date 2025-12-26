using Aneiang.Pa.BaiDu.Models;
using Aneiang.Pa.BaiDu.News;
using Aneiang.Pa.Bilibili.Models;
using Aneiang.Pa.Bilibili.News;
using Aneiang.Pa.CnBlog.Models;
using Aneiang.Pa.CnBlog.News;
using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Core.Extensions;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.Csdn.Models;
using Aneiang.Pa.Csdn.News;
using Aneiang.Pa.DouBan.Models;
using Aneiang.Pa.DouBan.News;
using Aneiang.Pa.DouYin.Models;
using Aneiang.Pa.DouYin.News;
using Aneiang.Pa.Dynamic.Extensions;
using Aneiang.Pa.HuPu.Models;
using Aneiang.Pa.HuPu.News;
using Aneiang.Pa.IFeng.Models;
using Aneiang.Pa.IFeng.News;
using Aneiang.Pa.JueJin.Models;
using Aneiang.Pa.JueJin.News;
using Aneiang.Pa.News.News;
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
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Net.Http;

namespace Aneiang.Pa.News.Extensions
{
    /// <summary>
    /// The service collection extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册新闻爬取器
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置对象</param>
        /// <param name="httpConfigureHandler">HTTP 消息处理器工厂</param>
        /// <param name="configureHttpClient">HTTP 客户端配置操作</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddNewsScraper(
            this IServiceCollection services, 
            IConfiguration? configuration = null, 
            Func<HttpMessageHandler>? httpConfigureHandler = null,
            Action<IHttpClientBuilder>? configureHttpClient = null)
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

            // 统一注册 HTTP 客户端，设置默认超时时间
            var httpClientBuilder = services.AddHttpClient(PaConsts.DefaultHttpClientName)
                .ConfigureHttpClient(client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(PaConsts.DefaultHttpTimeoutSeconds);
                });

            if (httpConfigureHandler != null)
            {
                httpClientBuilder.ConfigurePrimaryHttpMessageHandler(httpConfigureHandler);
            }

            configureHttpClient?.Invoke(httpClientBuilder);

            services.AddDynamicScraper();

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
            
            // 注册健康检查服务
            services.TryAddSingleton<IScraperHealthCheckService, NewsScraperHealthCheckService>();

            return services;
        }
    }
}
