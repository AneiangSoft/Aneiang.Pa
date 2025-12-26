using Aneiang.Pa.Core.Extensions;
using Aneiang.Pa.DouBan.Models;
using Aneiang.Pa.DouBan.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Aneiang.Pa.DouBan.Extensions
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
        public static void AddDouBanScraper(this IServiceCollection services, IConfiguration? configuration = null, Func<HttpMessageHandler>? httpConfigureHandler = null)
        {
            services.AddScraper<IDouBanNewScraper, DouBanNewScraper, DouBanScraperOptions>("Scraper:DouBan", configuration, httpConfigureHandler);
        }
    }
}
