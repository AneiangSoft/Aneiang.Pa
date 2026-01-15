using System;
using System.Net.Http;
using Aneiang.Pa._36kr.Models;
using Aneiang.Pa._36kr.News;
using Aneiang.Pa.Core.Extensions;
using Aneiang.Pa.Dynamic.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aneiang.Pa._36kr.Extensions
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
        public static void Add36krScraper(this IServiceCollection services, IConfiguration? configuration = null, Func<HttpMessageHandler>? httpConfigureHandler = null)
        {
            // 首先，添加 DynamicScraper 依赖。这将通过通用的 AddScraper 注册 HttpClient。
            services.AddDynamicScraper(httpConfigureHandler);

            // 接着，添加自己的 Scraper，但明确告知通用的 AddScraper 不要再次注册 HttpClient。
            services.AddScraper<I36krNewScraper, _36krNewScraper, _36krScraperOptions>("Scraper:_36kr", configuration, httpConfigureHandler, addHttpClient: false);
        }
    }
}
