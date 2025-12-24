using Aneiang.Pa.AspNetCore.Options;
using Aneiang.Pa.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Aneiang.Pa.AspNetCore.Extensions
{
    /// <summary>
    /// 应用程序构建器扩展
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// 映射爬虫控制器路由（使用 IApplicationBuilder）
        /// 注意：控制器已通过约定自动注册，此方法主要用于兼容性
        /// </summary>
        /// <param name="app">应用程序构建器</param>
        /// <param name="configure">可选的自定义路由配置</param>
        /// <returns>应用程序构建器</returns>
        public static IApplicationBuilder MapScraperController(this IApplicationBuilder app, System.Action<ScraperControllerOptions>? configure = null)
        {
            var options = app.ApplicationServices.GetService<IOptions<ScraperControllerOptions>>()?.Value 
                ?? new ScraperControllerOptions();

            // 如果提供了自定义配置，则应用它
            if (configure != null)
            {
                configure(options);
            }

            // 控制器已经通过约定自动注册路由，这里主要用于兼容性
            // 实际路由配置在 ScraperControllerRouteConvention 中完成
            return app;
        }
    }
}

