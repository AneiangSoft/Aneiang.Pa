using Aneiang.Pa.Core.Extensions;
using Aneiang.Pa.HuPu.Models;
using Aneiang.Pa.HuPu.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Aneiang.Pa.HuPu.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddHuPuScraper(this IServiceCollection services, IConfiguration? configuration = null, Func<HttpMessageHandler>? httpConfigureHandler = null)
        {
            services.AddScraper<IHuPuNewScraper, HuPuNewScraper, HuPuScraperOptions>("Scraper:HuPu", configuration, httpConfigureHandler);
        }
    }
}
