using Aneiang.Pa.AspNetCore.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;

namespace Aneiang.Pa.AspNetCore.Conventions
{
    /// <summary>
    /// MVC 选项后配置，用于添加控制器约定
    /// </summary>
    internal class ScraperControllerMvcOptionsPostConfigure : IPostConfigureOptions<MvcOptions>
    {
        private readonly ScraperControllerOptions _options;

        /// <summary>
        /// 初始化后配置
        /// </summary>
        /// <param name="options">配置选项</param>
        public ScraperControllerMvcOptionsPostConfigure(IOptions<ScraperControllerOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// 后配置
        /// </summary>
        /// <param name="name">配置名称</param>
        /// <param name="options">MVC 选项</param>
        public void PostConfigure(string? name, MvcOptions options)
        {
            // 直接添加约定
            options.Conventions.Add(new ScraperControllerRouteConvention(_options));
        }
    }
}

