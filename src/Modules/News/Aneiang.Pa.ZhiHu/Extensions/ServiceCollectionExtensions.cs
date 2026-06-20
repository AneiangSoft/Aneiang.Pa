using Aneiang.Pa.Core.Extensions;
using Aneiang.Pa.ZhiHu.Models;
using Aneiang.Pa.ZhiHu.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Aneiang.Pa.ZhiHu.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddZhiHuScraper(this IServiceCollection services, IConfiguration? configuration = null, Func<HttpMessageHandler>? httpConfigureHandler = null)
        {
            services.AddScraper<IZhiHuNewScraper, ZhiHuNewScraper, ZhiHuScraperOptions>("Scraper:ZhiHu", configuration, httpConfigureHandler);
        }
    }
}
