using Aneiang.Pa.Core.Data;
using Aneiang.Pa.ThePaper.Models;
using Aneiang.Pa.ThePaper.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Net.Http;

namespace Aneiang.Pa.ThePaper.Extensions
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
        public static void AddThePaperScraper(this IServiceCollection services, IConfiguration? configuration = null, Func<HttpMessageHandler>? httpConfigureHandler = null)
        {
            if (configuration != null)
            {
                services.Configure<ThePaperScraperOptions>(configuration.GetSection("Scraper:ThePaper"));
            }
            var httpClientBuilder = services.AddHttpClient(PaConsts.DefaultHttpClientName);
            if (httpConfigureHandler != null)
            {
                httpClientBuilder.ConfigurePrimaryHttpMessageHandler(httpConfigureHandler);
            }
            services.TryAddSingleton<IThePaperNewScraper, ThePaperNewScraper>();
        }
    }
}
