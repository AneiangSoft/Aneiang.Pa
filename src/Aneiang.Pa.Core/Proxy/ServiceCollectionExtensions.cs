using Aneiang.Pa.Core.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;

namespace Aneiang.Pa.Core.Proxy
{
    /// <summary>
    /// 代理池及默认 HttpClient 注册扩展。
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册代理池相关服务。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration">代理池配置节点（如 configuration.GetSection("Scraper:ProxyPool")），可为 null。</param>
        /// <param name="configure">通过代码动态配置代理池，可为 null。</param>
        public static IServiceCollection AddPaProxyPool(
            this IServiceCollection services,
            IConfiguration? configuration = null,
            Action<ProxyPoolOptions>? configure = null)
        {
            if (configuration != null)
            {
                services.Configure<ProxyPoolOptions>(configuration);
            }

            if (configure != null)
            {
                services.Configure(configure);
            }

            services.TryAddSingleton<IProxyPool, DefaultProxyPool>();

            return services;
        }

        /// <summary>
        /// 注册带代理池支持的默认 HttpClient（名称为 <see cref="PaConsts.DefaultHttpClientName"/>）。
        /// 如未启用代理池，则退化为普通 HttpClient。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="proxyConfiguration">代理池配置节点，可为 null。</param>
        /// <param name="proxyConfigure">通过代码配置代理池，可为 null。</param>
        /// <param name="overrideHandlerFactory">
        /// 可选的自定义 HttpMessageHandler 工厂。如果不为 null，会在内部包裹代理逻辑，
        /// 否则使用默认的 <see cref="HttpClientHandler"/>。
        /// </param>
        public static IHttpClientBuilder AddPaDefaultHttpClientWithProxy(
            this IServiceCollection services,
            IConfiguration? proxyConfiguration = null,
            Action<ProxyPoolOptions>? proxyConfigure = null,
            Func<HttpMessageHandler>? overrideHandlerFactory = null)
        {
            services.AddPaProxyPool(proxyConfiguration, proxyConfigure);

            // 注意：这里使用带 IServiceProvider 的重载，方便读取代理配置。
            return services.AddHttpClient(PaConsts.DefaultHttpClientName)
                .ConfigurePrimaryHttpMessageHandler(sp =>
                {
                    var options = sp.GetService<IOptions<ProxyPoolOptions>>()?.Value ?? new ProxyPoolOptions();
                    var pool = sp.GetService<IProxyPool>();

                    HttpMessageHandler innerHandler;
                    if (overrideHandlerFactory != null)
                    {
                        innerHandler = overrideHandlerFactory();
                    }
                    else
                    {
                        innerHandler = new HttpClientHandler();
                    }

                    // 未启用或没有代理时，直接返回原始 handler。
                    if (!options.Enabled || pool == null)
                    {
                        return innerHandler;
                    }

                    var proxyUri = pool.GetNextProxy();
                    if (proxyUri == null)
                    {
                        return innerHandler;
                    }

                    if (innerHandler is HttpClientHandler httpClientHandler)
                    {
                        httpClientHandler.Proxy = new WebProxy(proxyUri);
                        httpClientHandler.UseProxy = true;
                        return httpClientHandler;
                    }

                    // 如果用户传入的是自定义 DelegatingHandler 等，这里只能原样返回，
                    // 由用户自行在 overrideHandlerFactory 内部处理代理。
                    return innerHandler;
                });
        }
    }
}


