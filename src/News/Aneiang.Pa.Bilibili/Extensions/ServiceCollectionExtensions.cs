using Aneiang.Pa.Bilibili.Models;
using Aneiang.Pa.Bilibili.News;
using Aneiang.Pa.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Aneiang.Pa.Bilibili.Extensions
{
    /// <summary>
    /// The service collection extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册爬虫
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="httpConfigureHandler"></param>
        public static void AddBilibiliScraper(this IServiceCollection services, IConfiguration? configuration = null, Func<HttpMessageHandler>? httpConfigureHandler = null)
        {
            services.AddScraper<IBilibiliNewScraper, BilibiliNewScraper, BilibiliScraperOptions>("Scraper:Bilibili", configuration, httpConfigureHandler);
        }
    }
}
