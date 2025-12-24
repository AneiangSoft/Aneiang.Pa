using Aneiang.Pa.AspNetCore.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;

namespace Aneiang.Pa.AspNetCore.Conventions
{
    /// <summary>
    /// 爬虫控制器路由约定
    /// </summary>
    public class ScraperControllerRouteConvention : IControllerModelConvention, IApplicationModelConvention
    {
        private readonly ScraperControllerOptions _options;

        /// <summary>
        /// 初始化路由约定
        /// </summary>
        /// <param name="options">配置选项</param>
        public ScraperControllerRouteConvention(ScraperControllerOptions options)
        {
            _options = options ?? new ScraperControllerOptions();
        }

        /// <summary>
        /// 应用约定
        /// </summary>
        /// <param name="controller">控制器模型</param>
        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerName == "Scraper")
            {
                var routePrefix = _options.RoutePrefix?.Trim('/') ?? "api/scraper";

                // 更新控制器的路由前缀（如果已存在路由特性，则更新它）
                if (controller.Selectors.Count == 0)
                {
                    controller.Selectors.Add(new SelectorModel());
                }

                foreach (var selector in controller.Selectors)
                {
                    if (selector.AttributeRouteModel == null)
                    {
                        selector.AttributeRouteModel = new AttributeRouteModel();
                    }
                    // 更新路由模板为配置的前缀
                    selector.AttributeRouteModel.Template = routePrefix;
                }

                // 更新所有 Action 的路由模板
                foreach (var action in controller.Actions)
                {
                    // 确保每个 Action 至少有一个选择器
                    if (action.Selectors.Count == 0)
                    {
                        action.Selectors.Add(new SelectorModel());
                    }

                    foreach (var selectorModel in action.Selectors)
                    {
                        if (selectorModel.AttributeRouteModel == null)
                        {
                            selectorModel.AttributeRouteModel = new AttributeRouteModel();
                        }

                        var actionTemplate = selectorModel.AttributeRouteModel.Template;
                        
                        // 如果 action 模板为空，根据 Action 名称设置默认模板
                        if (string.IsNullOrEmpty(actionTemplate))
                        {
                            // GetAvailableSources 使用空模板（根路径）
                            if (action.ActionName == "GetAvailableSources")
                            {
                                selectorModel.AttributeRouteModel.Template = string.Empty;
                            }
                            else
                            {
                                // 其他 Action 使用 action 名称
                                selectorModel.AttributeRouteModel.Template = action.ActionName;
                            }
                        }
                        // 如果 action 模板不是绝对路径，则保持不变（因为控制器已经有路由前缀了）
                        // 如果 action 模板以 / 开头，则使用绝对路径（保持不变）
                    }
                }
            }
        }

        /// <summary>
        /// 应用约定（IApplicationModelConvention 实现）
        /// </summary>
        /// <param name="application">应用程序模型</param>
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                if (controller.ControllerName == "Scraper")
                {
                    Apply(controller);
                }
            }
        }
    }
}

