using Aneiang.Pa.Core.Extensions;
using Aneiang.Pa.WeiBo.Models;
using Aneiang.Pa.WeiBo.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Aneiang.Pa.WeiBo.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddWeiBoScraper(this IServiceCollection services, IConfiguration? configuration = null, Func<HttpMessageHandler>? httpConfigureHandler = null)
        {
            services.AddScraper<IWeiBoNewScraper, WeiBoNewScraper, WeiBoScraperOptions>("Scraper:WeiBo", configuration, httpConfigureHandler);
        }
    }
}
