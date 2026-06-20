using Aneiang.Pa.Core.Extensions;
using Aneiang.Pa.JueJin.Models;
using Aneiang.Pa.JueJin.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Aneiang.Pa.JueJin.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddJueJinScraper(this IServiceCollection services, IConfiguration? configuration = null, Func<HttpMessageHandler>? httpConfigureHandler = null)
        {
            services.AddScraper<IJueJinNewScraper, JueJinNewScraper, JueJinScraperOptions>("Scraper:JueJin", configuration, httpConfigureHandler);
        }
    }
}
