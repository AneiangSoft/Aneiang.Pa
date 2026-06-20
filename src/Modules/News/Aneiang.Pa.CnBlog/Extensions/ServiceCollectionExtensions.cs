using Aneiang.Pa.CnBlog.Models;
using Aneiang.Pa.CnBlog.News;
using Aneiang.Pa.Core.Extensions;
using Aneiang.Pa.Dynamic.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Aneiang.Pa.CnBlog.Extensions
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
        public static void AddCnBlogScraper(this IServiceCollection services, IConfiguration? configuration = null, Func<HttpMessageHandler>? httpConfigureHandler = null)
        {
            // 首先，添加 DynamicScraper 依赖。这将通过通用的 AddScraper 注册 HttpClient。
            services.AddDynamicScraper(httpConfigureHandler);

            // 接着，添加 CnBlog 自己的 Scraper，但明确告知通用的 AddScraper 不要再次注册 HttpClient。
            services.AddScraper<ICnBlogNewScraper, CnBlogNewScraper, CnBlogScraperOptions>("Scraper:CnBlog", configuration, httpConfigureHandler, addHttpClient: false);
        }
    }
}
