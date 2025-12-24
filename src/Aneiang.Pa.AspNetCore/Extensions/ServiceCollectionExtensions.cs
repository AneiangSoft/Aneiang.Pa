using Aneiang.Pa.AspNetCore.Conventions;
using Aneiang.Pa.AspNetCore.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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
        public static IServiceCollection AddScraperController(this IServiceCollection services, System.Action<ScraperControllerOptions>? configure = null)
        {
            // 先配置选项
            var optionsAction = configure ?? (_ => { });
            services.Configure(optionsAction);

            // 添加控制器约定以支持动态路由
            // 使用 PostConfigure 来确保选项已经配置完成
            services.AddTransient<IPostConfigureOptions<MvcOptions>, ScraperControllerMvcOptionsPostConfigure>();

            return services;
        }
    }
}

