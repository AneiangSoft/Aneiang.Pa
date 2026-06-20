using System;
using System.Net.Http;
using Aneiang.Pa.Core.Extensions;
using Aneiang.Pa.Core.Pipeline;
using Aneiang.Pa.Core.Scraper;
using Aneiang.Pa.Dynamic.Extensions;
using Aneiang.Pa.Lottery.Extensions;
using Aneiang.Pa.News.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aneiang.Pa.Extensions
{
    /// <summary>
    ///     The service collection extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     注册爬取器（含统一管道）
        /// </summary>
        public static IServiceCollection AddPaScraper(
            this IServiceCollection services,
            IConfiguration? configuration = null,
            Func<HttpMessageHandler>? httpConfigureHandler = null,
            Action<IHttpClientBuilder>? configureHttpClient = null)
        {
            services.AddNewsScraper(configuration, httpConfigureHandler, configureHttpClient);
            services.AddLotteryScraper(httpConfigureHandler, false);
            services.AddScraperRegistry();
            services.AddPaScrapePipeline(configuration);
            return services;
        }
    }
}
