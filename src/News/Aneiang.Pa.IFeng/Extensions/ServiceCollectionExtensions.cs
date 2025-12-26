using Aneiang.Pa.Core.Extensions;
using Aneiang.Pa.IFeng.Models;
using Aneiang.Pa.IFeng.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Aneiang.Pa.IFeng.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddIFengScraper(this IServiceCollection services, IConfiguration? configuration = null, Func<HttpMessageHandler>? httpConfigureHandler = null)
        {
            services.AddScraper<IIFengNewScraper, IFengNewScraper, IFengScraperOptions>("Scraper:IFeng", configuration, httpConfigureHandler);
        }
    }
}
