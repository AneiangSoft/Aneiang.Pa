using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Core.Scraper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Net.Http;

namespace Aneiang.Pa.Core.Extensions
{
    /// <summary>
    /// Provides extension methods for IServiceCollection to add scrapers.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a generic scraper to the service collection for scrapers that do not require options.
        /// </summary>
        /// <typeparam name="TScraper">The interface of the scraper.</typeparam>
        /// <typeparam name="TScraperImpl">The implementation of the scraper.</typeparam>
        /// <param name="services">The IServiceCollection to add the services to.</param>
        /// <param name="httpConfigureHandler">Optional HTTP message handler configuration.</param>
        /// <param name="addHttpClient">Whether to add the HttpClient. Defaults to true.</param>
        public static void AddScraper<TScraper, TScraperImpl>(this IServiceCollection services, Func<HttpMessageHandler>? httpConfigureHandler = null, bool addHttpClient = true)
            where TScraper : class, IScraper
            where TScraperImpl : class, TScraper
        {
            if (addHttpClient)
            {
                var httpClientBuilder = services.AddHttpClient(PaConsts.DefaultHttpClientName)
                    .ConfigureHttpClient(client =>
                    {
                        client.Timeout = TimeSpan.FromSeconds(PaConsts.DefaultHttpTimeoutSeconds);
                    });
                if (httpConfigureHandler != null)
                {
                    httpClientBuilder.ConfigurePrimaryHttpMessageHandler(httpConfigureHandler);
                }
            }

            services.TryAddSingleton<TScraper, TScraperImpl>();
            services.AddSingleton<IScraper>(sp => sp.GetRequiredService<TScraper>());
        }

        /// <summary>
        /// Adds a generic scraper to the service collection.
        /// </summary>
        /// <typeparam name="TScraper">The interface of the scraper.</typeparam>
        /// <typeparam name="TScraperImpl">The implementation of the scraper.</typeparam>
        /// <typeparam name="TOptions">The options for the scraper.</typeparam>
        /// <param name="services">The IServiceCollection to add the services to.</param>
        /// <param name="configSectionName">The name of the configuration section.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="httpConfigureHandler">Optional HTTP message handler configuration.</param>
        /// <param name="addHttpClient">Whether to add the HttpClient. Defaults to true.</param>
        public static void AddScraper<TScraper, TScraperImpl, TOptions>(this IServiceCollection services, string configSectionName, IConfiguration? configuration = null, Func<HttpMessageHandler>? httpConfigureHandler = null, bool addHttpClient = true)
            where TScraper : class, IScraper
            where TScraperImpl : class, TScraper
            where TOptions : class
        {
            if (configuration != null)
            {
                services.Configure<TOptions>(configuration.GetSection(configSectionName));
            }

            AddScraper<TScraper, TScraperImpl>(services, httpConfigureHandler, addHttpClient);
        }

        /// <summary>
        /// 注册爬虫注册表（IScraperRegistry），应在所有爬虫注册完成后调用。
        /// </summary>
        public static IServiceCollection AddScraperRegistry(this IServiceCollection services)
        {
            services.TryAddSingleton<IScraperRegistry, ScraperRegistry>();
            return services;
        }
    }
}
