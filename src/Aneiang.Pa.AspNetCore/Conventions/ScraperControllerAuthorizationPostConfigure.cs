using Aneiang.Pa.AspNetCore.Constants;
using Aneiang.Pa.AspNetCore.Filters;
using Aneiang.Pa.AspNetCore.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;

namespace Aneiang.Pa.AspNetCore.Conventions
{
    /// <summary>
    /// 授权后配置，用于添加授权过滤器到控制器
    /// </summary>
    internal class ScraperControllerAuthorizationPostConfigure : IPostConfigureOptions<MvcOptions>
    {
        private readonly IOptions<AuthorizationOptions> _options;

        /// <summary>
        /// 初始化后配置
        /// </summary>
        /// <param name="options">授权选项</param>
        public ScraperControllerAuthorizationPostConfigure(IOptions<AuthorizationOptions> options)
        {
            _options = options ?? throw new System.ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// 后配置
        /// </summary>
        /// <param name="name">配置名称</param>
        /// <param name="mvcOptions">MVC 选项</param>
        public void PostConfigure(string? name, MvcOptions mvcOptions)
        {
            // 检查授权是否启用
            var optionsValue = _options?.Value;
            if (optionsValue == null || !optionsValue.Enabled)
            {
                return;
            }

            // 添加控制器约定，只对 ScraperController 应用授权过滤器
            // 这样其他控制器不会受到影响
            mvcOptions.Conventions.Add(new ScraperControllerAuthorizationConvention());
        }
    }

    /// <summary>
    /// ScraperController 授权约定，只对 ScraperController 应用授权过滤器
    /// </summary>
    internal class ScraperControllerAuthorizationConvention : IApplicationModelConvention
    {
        /// <summary>
        /// 应用约定
        /// </summary>
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                // 只对 ScraperController 应用授权过滤器
                if (controller.ControllerName == ScraperControllerConstants.ControllerName ||
                    controller.ControllerType.Name == "ScraperController")
                {
                    // 为 ScraperController 添加授权过滤器
                    // 使用 ServiceFilterAttribute 通过依赖注入创建过滤器实例
                    controller.Filters.Add(new ServiceFilterAttribute(typeof(AuthorizationFilter)));
                }
            }
        }
    }
}
