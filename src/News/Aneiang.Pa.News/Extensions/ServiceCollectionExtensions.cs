using Aneiang.Pa._36kr.News;
using Aneiang.Pa.BaiDu.Models;
using Aneiang.Pa.BaiDu.News;
using Aneiang.Pa.Bilibili.Models;
using Aneiang.Pa.Bilibili.News;
using Aneiang.Pa.CnBlog.Models;
using Aneiang.Pa.CnBlog.News;
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
using Aneiang.Pa.ItHome.Models;
using Aneiang.Pa.ItHome.News;
using Aneiang.Pa.JueJin.Models;
using Aneiang.Pa.JueJin.News;
using Aneiang.Pa.News.News;
using Aneiang.Pa.Tencent.Models;
using Aneiang.Pa.Tencent.News;
using Aneiang.Pa.ThePaper.Models;
using Aneiang.Pa.ThePaper.News;
using Aneiang.Pa.TouTiao.Models;
using Aneiang.Pa.TouTiao.News;
using Aneiang.Pa.WeiBo.News;
using Aneiang.Pa.ZhiHu.Models;
using Aneiang.Pa.ZhiHu.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Net.Http;
using Aneiang.Pa._36kr.Models;

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
            //var addHttpClient = false;
            //if (!addHttpClient)
            //{
            //    // 统一注册 HTTP 客户端，设置默认超时时间
            //    var httpClientBuilder = services.AddHttpClient(PaConsts.DefaultHttpClientName)
            //        .ConfigureHttpClient(client =>
            //        {
            //            client.Timeout = TimeSpan.FromSeconds(PaConsts.DefaultHttpTimeoutSeconds);
            //        });

            //    if (httpConfigureHandler != null)
            //    {
            //        httpClientBuilder.ConfigurePrimaryHttpMessageHandler(httpConfigureHandler);
            //    }

            //    configureHttpClient?.Invoke(httpClientBuilder);
            //    addHttpClient = true;
            //}

            services.AddScraper<IWeiBoNewScraper, WeiBoNewScraper, WeiBoNewScraper>("Scraper:WeiBo", configuration, httpConfigureHandler);
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IWeiBoNewScraper>());

            services.AddScraper<IZhiHuNewScraper, ZhiHuNewScraper, ZhiHuScraperOptions>("Scraper:ZhiHu", configuration, httpConfigureHandler,false);
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IZhiHuNewScraper>());

            services.AddScraper<IBilibiliNewScraper, BilibiliNewScraper, BilibiliScraperOptions>("Scraper:Bilibili", configuration, httpConfigureHandler, false);
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IBilibiliNewScraper>());

            services.AddScraper<IBaiDuNewScraper, BaiDuNewScraper, BaiDuScraperOptions>("Scraper:BaiDu", configuration, httpConfigureHandler, false);
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IBaiDuNewScraper>());

            services.AddScraper<IDouYinNewScraper, DouYinNewScraper, DouYinScraperOptions>("Scraper:DouYin", configuration, httpConfigureHandler, false);
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IDouYinNewScraper>());

            services.AddScraper<ITouTiaoNewScraper, TouTiaoNewScraper, TouTiaoScraperOptions>("Scraper:TouTiao", configuration, httpConfigureHandler, false);
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<ITouTiaoNewScraper>());

            services.AddScraper<IHuPuNewScraper, HuPuNewScraper, HuPuScraperOptions>("Scraper:HuPu", configuration, httpConfigureHandler, false);
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IHuPuNewScraper>());

            services.AddScraper<ITencentNewScraper, TencentNewScraper, TencentScraperOptions>("Scraper:Tencent", configuration, httpConfigureHandler, false);
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<ITencentNewScraper>());

            services.AddScraper<IJueJinNewScraper, JueJinNewScraper, JueJinScraperOptions>("Scraper:JueJin", configuration, httpConfigureHandler, false);
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IJueJinNewScraper>());

            services.AddScraper<IThePaperNewScraper, ThePaperNewScraper, ThePaperScraperOptions>("Scraper:ThePaper", configuration, httpConfigureHandler, false);
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IThePaperNewScraper>());

            services.AddScraper<IDouBanNewScraper, DouBanNewScraper, DouBanScraperOptions>("Scraper:DouBan", configuration, httpConfigureHandler, false);
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IDouBanNewScraper>());

            services.AddScraper<IIFengNewScraper, IFengNewScraper, IFengScraperOptions>("Scraper:IFeng", configuration, httpConfigureHandler, false);
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IIFengNewScraper>());

            services.AddScraper<ICsdnNewScraper, CsdnNewScraper, CsdnScraperOptions>("Scraper:Csdn", configuration, httpConfigureHandler, false);
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<ICsdnNewScraper>());

            services.AddScraper<ICnBlogNewScraper, CnBlogNewScraper, CnBlogScraperOptions>("Scraper:CnBlog", configuration, httpConfigureHandler, false);
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<ICnBlogNewScraper>());

            services.AddScraper<IItHomeNewScraper, ItHomeNewScraper, ItHomeScraperOptions>("Scraper:ItHome", configuration, httpConfigureHandler, false);
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<IItHomeNewScraper>());

            services.AddScraper<I36krNewScraper, _36krNewScraper, _36krScraperOptions > ("Scraper:_36kr", configuration, httpConfigureHandler, false);
            services.AddSingleton<INewsScraper>(provider => provider.GetRequiredService<I36krNewScraper>());


            services.AddDynamicScraper();
            services.AddSingleton<INewsScraperFactory, NewsScraperFactory>();
            // 注册健康检查服务
            services.TryAddSingleton<IScraperHealthCheckService, NewsScraperHealthCheckService>();
            return services;
        }
    }
}
