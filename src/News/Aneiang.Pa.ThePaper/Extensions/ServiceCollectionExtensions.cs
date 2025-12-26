using Aneiang.Pa.Core.Extensions;
using Aneiang.Pa.ThePaper.Models;
using Aneiang.Pa.ThePaper.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Aneiang.Pa.ThePaper.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddThePaperScraper(this IServiceCollection services, IConfiguration? configuration = null, Func<HttpMessageHandler>? httpConfigureHandler = null)
        {
            services.AddScraper<IThePaperNewScraper, ThePaperNewScraper, ThePaperScraperOptions>("Scraper:ThePaper", configuration, httpConfigureHandler);
        }
    }
}
