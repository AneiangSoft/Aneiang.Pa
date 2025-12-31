using Aneiang.Pa.AspNetCore.Controllers;
using Aneiang.Pa.AspNetCore.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;
using System.Linq;

namespace Aneiang.Pa.AspNetCore.Conventions
{
    /// <summary>
    /// 应用响应缓存的约定
    /// </summary>
    public class ResponseCacheConvention : IApplicationModelConvention
    {
        private readonly ScraperControllerOptions _scraperOptions;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="scraperOptions"></param>
        public ResponseCacheConvention(IOptions<ScraperControllerOptions> scraperOptions)
        {
            _scraperOptions = scraperOptions.Value;
        }

        /// <summary>
        /// Apply
        /// </summary>
        /// <param name="application"></param>
        public void Apply(ApplicationModel application)
        {
            if (!_scraperOptions.EnableResponseCaching || _scraperOptions.CacheDurationSeconds <= 0)
            {
                return;
            }

            foreach (var controller in application.Controllers)
            {
                if (controller.ControllerType == typeof(ScraperController))
                {
                    foreach (var action in controller.Actions)
                    {
                        // 检查操作是否已有 ResponseCache 属性，如果有，则跳过
                        bool hasResponseCacheAttr = action.Selectors.Any(s => s.EndpointMetadata.Any(m => m is ResponseCacheAttribute));

                        if (!hasResponseCacheAttr)
                        {
                            var cacheAttribute = new ResponseCacheAttribute
                            {
                                Duration = _scraperOptions.CacheDurationSeconds,
                                Location = ResponseCacheLocation.Any,
                                VaryByQueryKeys = new[] { "*" } // 根据所有查询参数进行缓存
                            };
                            action.Filters.Add(cacheAttribute);
                        }
                    }
                }
            }
        }
    }
}

