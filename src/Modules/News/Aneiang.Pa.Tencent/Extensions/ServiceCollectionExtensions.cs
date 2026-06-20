using Aneiang.Pa.Core.Extensions;
using Aneiang.Pa.Tencent.Models;
using Aneiang.Pa.Tencent.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Aneiang.Pa.Tencent.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddTencentScraper(this IServiceCollection services, IConfiguration? configuration = null, Func<HttpMessageHandler>? httpConfigureHandler = null)
        {
            services.AddScraper<ITencentNewScraper, TencentNewScraper, TencentScraperOptions>("Scraper:Tencent", configuration, httpConfigureHandler);
        }
    }
}
