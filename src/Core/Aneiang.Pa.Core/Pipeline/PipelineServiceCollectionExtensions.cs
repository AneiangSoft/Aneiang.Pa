using Aneiang.Pa.Core.Pipeline.Middlewares;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aneiang.Pa.Core.Pipeline
{
    /// <summary>
    /// 管道注册扩展
    /// </summary>
    public static class PipelineServiceCollectionExtensions
    {
        /// <summary>
        /// 注册爬取管道（IScrapeInvoker + 内置中间件）。可结合 AddPaScraperOptions 使用。
        /// 重复注册不会重复添加中间件。
        /// </summary>
        public static IServiceCollection AddPaScrapePipeline(this IServiceCollection services, IConfiguration? configuration = null)
        {
            if (configuration != null)
            {
                services.Configure<ScrapeResilienceOptions>(configuration.GetSection("Scraper:Resilience"));
                services.Configure<ScrapeRateLimitOptions>(configuration.GetSection("Scraper:RateLimit"));
                services.Configure<ScrapeCacheOptions>(configuration.GetSection("Scraper:CachePipeline"));
            }
            else
            {
                services.AddOptions<ScrapeResilienceOptions>();
                services.AddOptions<ScrapeRateLimitOptions>();
                services.AddOptions<ScrapeCacheOptions>();
            }

            services.TryAddSingleton<IScrapeInvoker, ScrapeInvoker>();

            // 默认中间件（按需启用，使用 TryAddEnumerable 防止重复）
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IScrapeMiddleware, LoggingMiddleware>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IScrapeMiddleware, MetricsMiddleware>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IScrapeMiddleware, TracingMiddleware>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IScrapeMiddleware, CacheMiddleware>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IScrapeMiddleware, RateLimitMiddleware>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IScrapeMiddleware, CircuitBreakerMiddleware>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IScrapeMiddleware, RetryMiddleware>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IScrapeMiddleware, TimeoutMiddleware>());

            return services;
        }

        /// <summary>
        /// 显式添加自定义中间件
        /// </summary>
        public static IServiceCollection AddPaScrapeMiddleware<TMiddleware>(this IServiceCollection services)
            where TMiddleware : class, IScrapeMiddleware
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IScrapeMiddleware, TMiddleware>());
            return services;
        }
    }
}
