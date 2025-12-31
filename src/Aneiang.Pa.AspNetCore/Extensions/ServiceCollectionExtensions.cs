using Aneiang.Pa.AspNetCore.Conventions;
using Aneiang.Pa.AspNetCore.Filters;
using Aneiang.Pa.AspNetCore.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Aneiang.Pa.AspNetCore.Extensions
{
    /// <summary>
    /// 服务集合扩展
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加爬虫控制器支持
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configure">配置选项</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddScraperController(
            this IServiceCollection services, 
            System.Action<ScraperControllerOptions>? configure = null)
        {
            // 先配置选项
            var optionsAction = configure ?? (_ => { });
            services.Configure(optionsAction);

            // 添加控制器约定以支持动态路由
            // 使用 PostConfigure 来确保选项已经配置完成
            services.AddTransient<IPostConfigureOptions<MvcOptions>, ScraperControllerMvcOptionsPostConfigure>();

            // 添加响应缓存服务
            services.AddResponseCaching();

            // 添加响应缓存约定
            // 使用 PostConfigure 来确保 ScraperControllerOptions 已经配置完成
            services.AddTransient<IPostConfigureOptions<MvcOptions>>(sp =>
            {
                var scraperOptions = sp.GetRequiredService<IOptions<ScraperControllerOptions>>();
                return new PostConfigureOptions<MvcOptions>(null, options =>
                {
                    options.Conventions.Add(new ResponseCacheConvention(scraperOptions));
                });
            });

            return services;
        }

        /// <summary>
        /// 配置授权选项
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configure">配置授权选项</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection ConfigureAuthorization(
            this IServiceCollection services,
            System.Action<AuthorizationOptions> configure)
        {
            if (configure == null)
            {
                throw new System.ArgumentNullException(nameof(configure));
            }

            // 配置授权选项
            services.Configure(configure);

            // 注册授权过滤器
            services.AddScoped<AuthorizationFilter>();

            // 添加控制器约定以应用授权过滤器
            services.AddTransient<IPostConfigureOptions<MvcOptions>, ScraperControllerAuthorizationPostConfigure>();

            return services;
        }

        /// <summary>
        /// 配置授权选项（通过配置节）
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置对象</param>
        /// <param name="sectionName">配置节名称，默认为 "Scraper:Authorization"</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection ConfigureAuthorization(
            this IServiceCollection services,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            string sectionName = "Scraper:Authorization")
        {
            if (configuration == null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            // 配置授权选项
            services.Configure<AuthorizationOptions>(configuration.GetSection(sectionName));

            // 注册授权过滤器
            services.AddScoped<AuthorizationFilter>();

            // 添加控制器约定以应用授权过滤器
            services.AddTransient<IPostConfigureOptions<MvcOptions>, ScraperControllerAuthorizationPostConfigure>();

            return services;
        }
    }
}

