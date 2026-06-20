using Aneiang.Pa.Core.Extensions;
using Aneiang.Pa.DouYin.Models;
using Aneiang.Pa.DouYin.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Aneiang.Pa.DouYin.Extensions
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
        public static void AddDouYinScraper(this IServiceCollection services, IConfiguration? configuration = null, Func<HttpMessageHandler>? httpConfigureHandler = null)
        {
            services.AddScraper<IDouYinNewScraper, DouYinNewScraper, DouYinScraperOptions>("Scraper:DouYin", configuration, httpConfigureHandler);
        }
    }
}
