using System;
using Aneiang.Pa.AspNetCore.Caching;
using Aneiang.Pa.AspNetCore.Conventions;
using Aneiang.Pa.AspNetCore.Filters;
using Aneiang.Pa.AspNetCore.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Aneiang.Pa.AspNetCore.Extensions
{
    /// <summary>
    /// 服务集合扩展（对外仅暴露：AddPaScraperApi / AddPaScraperAuthorization）
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 一站式注册爬虫 API（ScraperController + 数据缓存(内存/Redis/不启用)）。
        /// 
        /// 注意：授权不在此方法中默认开启（避免使用者无意中开启鉴权导致不可访问）。
        /// 如需授权，请显式调用 <see cref="AddPaScraperAuthorization(IServiceCollection, IConfiguration, string, Action{AuthorizationOptions}?)"/>。
        /// </summary>
        /// <param name="services">DI 容器</param>
        /// <param name="configuration">配置</param>
        /// <param name="scraperSectionName">Scraper 配置节名称，默认 "Scraper"</param>
        public static IServiceCollection AddPaScraperApi(
            this IServiceCollection services,
            IConfiguration configuration,
            string scraperSectionName = "Scraper")
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var scraperSection = configuration.GetSection(scraperSectionName);

            // 1) 绑定 ScraperControllerOptions（控制器/缓存）
            services.Configure<ScraperControllerOptions>(scraperSection);

            // 2) 动态路由约定
            services.AddTransient<IPostConfigureOptions<MvcOptions>, ScraperControllerMvcOptionsPostConfigure>();

            // 3) 数据缓存注册（按配置决定是否注册 Redis）
            AddScraperDataCacheCore(services, scraperSection);

            return services;
        }

        /// <summary>
        /// 单独启用 Scraper 授权（从配置文件读取 + 可选代码覆盖）。
        /// 
        /// 支持的授权策略：ApiKey / Custom / Combined（CustomAuthorizationFunc）。
        /// </summary>
        /// <param name="services">DI 容器</param>
        /// <param name="configuration">配置</param>
        /// <param name="authorizationSectionName">授权配置节名称，默认 "Scraper:Authorization"</param>
        /// <param name="configure">可选：代码方式进一步覆盖/补充授权配置（比如 CustomAuthorizationFunc）</param>
        public static IServiceCollection AddPaScraperAuthorization(
            this IServiceCollection services,
            IConfiguration configuration,
            string authorizationSectionName = "Scraper:Authorization",
            Action<AuthorizationOptions>? configure = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            // 1) 从配置文件绑定
            services.Configure<AuthorizationOptions>(configuration.GetSection(authorizationSectionName));

            // 2) 可选：代码覆盖（用于自定义策略等）
            if (configure != null)
            {
                services.Configure(configure);
            }

            // 3) 注册授权过滤器与约定
            services.AddScoped<AuthorizationFilter>();
            services.AddTransient<IPostConfigureOptions<MvcOptions>, ScraperControllerAuthorizationPostConfigure>();

            return services;
        }

        // =========================
        // 内部实现（不对外暴露）
        // =========================

        private static void AddScraperDataCacheCore(IServiceCollection services, IConfigurationSection scraperSection)
        {
            // 先直接从配置读取缓存提供方（避免 DI 尚未构建时无法获取 Options）
            var cacheProvider = scraperSection.GetValue("CacheProvider", ScraperCacheProvider.None);

            // 只有当 CacheProvider=Redis 时才注册 Redis 分布式缓存（避免无意义连接/配置）
            // 同时才注册 RedisCacheService，避免未注册 IDistributedCache 时启动报错
            if (cacheProvider == ScraperCacheProvider.Redis)
            {
                var redisSection = scraperSection.GetSection("Redis");
                var redisConfiguration = redisSection.GetValue<string>("Configuration");
                var instanceName = redisSection.GetValue<string>("InstanceName");

                if (string.IsNullOrWhiteSpace(redisConfiguration))
                {
                    throw new InvalidOperationException("当 CacheProvider=Redis 时，必须配置 Scraper:Redis:Configuration");
                }

                services.AddStackExchangeRedisCache(o =>
                {
                    o.Configuration = redisConfiguration;
                    o.InstanceName = instanceName;
                });

                services.TryAddSingleton<RedisCacheService>();
            }

            // 依赖项注册（按需）
            services.TryAddSingleton<NoCacheService>();
            services.TryAddSingleton<MemoryCacheService>();

            // 内存缓存（总是安全可注册）
            services.TryAddSingleton<IMemoryCache, MemoryCache>();

            // ICacheService：按配置选择实现
            services.TryAddSingleton<ICacheService>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<ScraperControllerOptions>>().Value;

                switch (options.CacheProvider)
                {
                    case ScraperCacheProvider.Memory:
                        return sp.GetRequiredService<MemoryCacheService>();

                    case ScraperCacheProvider.Redis:
                        // 走 Redis 必须确保 IDistributedCache 已注册（在上面已按配置注册过）
                        sp.GetRequiredService<IDistributedCache>();
                        return sp.GetRequiredService<RedisCacheService>();

                    case ScraperCacheProvider.None:
                    default:
                        return sp.GetRequiredService<NoCacheService>();
                }
            });
        }
    }
}
