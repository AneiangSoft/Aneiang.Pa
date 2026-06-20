using Aneiang.Pa.BaiDu.Models;
using Aneiang.Pa.BaiDu.News;
using Aneiang.Pa.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Aneiang.Pa.BaiDu.Extensions
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
        public static void AddBaiDuScraper(this IServiceCollection services, IConfiguration? configuration = null, Func<HttpMessageHandler>? httpConfigureHandler = null)
        {
            services.AddScraper<IBaiDuNewScraper, BaiDuNewScraper, BaiDuScraperOptions>("Scraper:BaiDu", configuration, httpConfigureHandler);
        }
    }
}
