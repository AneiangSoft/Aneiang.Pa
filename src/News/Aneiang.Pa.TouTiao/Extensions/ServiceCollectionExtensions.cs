using Aneiang.Pa.Core.Extensions;
using Aneiang.Pa.TouTiao.Models;
using Aneiang.Pa.TouTiao.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Aneiang.Pa.TouTiao.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddTouTiaoScraper(this IServiceCollection services, IConfiguration? configuration = null, Func<HttpMessageHandler>? httpConfigureHandler = null)
        {
            services.AddScraper<ITouTiaoNewScraper, TouTiaoNewScraper, TouTiaoScraperOptions>("Scraper:TouTiao", configuration, httpConfigureHandler);
        }
    }
}
