using Aneiang.Pa.Core.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Net.Http;

namespace Aneiang.Pa.Dynamic.Extensions
{
    /// <summary>
    /// The service collection extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册爬取器
        /// </summary>
        /// <param name="services"></param>
        /// <param name="httpConfigureHandler"></param>
        public static IServiceCollection AddDynamicScraper(this IServiceCollection services, Func<HttpMessageHandler>? httpConfigureHandler = null)
        {
            var httpClientBuilder = services.AddHttpClient(PaConsts.DefaultHttpClientName);
            if (httpConfigureHandler != null)
            {
                httpClientBuilder.ConfigurePrimaryHttpMessageHandler(httpConfigureHandler);
            }
            services.TryAddSingleton<IDynamicScraper, DynamicScraper>();
            return services;
        }
    }
}
