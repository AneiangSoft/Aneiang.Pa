using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Aneiang.Pa.AspNetCore.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aneiang.Pa.AspNetCore.Filters
{
    /// <summary>
    /// 授权过滤器
    /// </summary>
    public class AuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly AuthorizationOptions _options;
        private readonly ILogger<AuthorizationFilter>? _logger;

        /// <summary>
        /// 初始化授权过滤器
        /// </summary>
        /// <param name="options">授权选项</param>
        /// <param name="logger">日志记录器（可选）</param>
        public AuthorizationFilter(IOptions<AuthorizationOptions> options, ILogger<AuthorizationFilter>? logger = null)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
        }

        /// <summary>
        /// 执行授权检查
        /// </summary>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // 如果未启用授权，直接通过
            if (!_options.Enabled)
            {
                _logger?.LogDebug("授权未启用，跳过授权检查");
                return;
            }

            var httpContext = context.HttpContext;
            var route = httpContext.Request.Path.Value ?? "";
            
            _logger?.LogDebug("执行授权检查，路由：{Route}", route);

            // 检查是否在排除列表中
            if (IsRouteExcluded(route))
            {
                _logger?.LogDebug("路由 {Route} 在排除列表中，跳过授权检查", route);
                return;
            }

            bool isAuthorized = false;
            ClaimsPrincipal? principal = null;

            try
            {
                switch (_options.Scheme)
                {
                    case AuthorizationScheme.ApiKey:
                        isAuthorized = CheckApiKey(httpContext);
                        break;

                    case AuthorizationScheme.Custom:
                        if (_options.CustomAuthorizationFunc != null)
                        {
                            var (authorized, claimsPrincipal) = _options.CustomAuthorizationFunc(httpContext);
                            isAuthorized = authorized;
                            principal = claimsPrincipal;
                        }
                        else
                        {
                            _logger?.LogWarning("自定义授权方式已启用，但未设置 CustomAuthorizationFunc");
                            isAuthorized = false;
                        }
                        break;

                    case AuthorizationScheme.Combined:
                        // API Key 检查
                        if (CheckApiKey(httpContext))
                        {
                            isAuthorized = true;
                        }
                        // 自定义验证
                        else if (_options.CustomAuthorizationFunc != null)
                        {
                            var (authorized, claimsPrincipal) = _options.CustomAuthorizationFunc(httpContext);
                            isAuthorized = authorized;
                            principal = claimsPrincipal;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "授权检查过程中发生异常");
                isAuthorized = false;
            }

            if (!isAuthorized)
            {
                _logger?.LogWarning("未授权访问尝试：{Route} from {RemoteIpAddress}", route, httpContext.Connection.RemoteIpAddress);
                context.Result = new UnauthorizedObjectResult(new { error = _options.UnauthorizedMessage });
                return;
            }

            // 如果提供了 ClaimsPrincipal，设置到 HttpContext
            if (principal != null)
            {
                httpContext.User = principal;
            }

            _logger?.LogDebug("授权检查通过：{Route}", route);
        }

        /// <summary>
        /// 检查 API Key
        /// </summary>
        private bool CheckApiKey(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            if (_options.ApiKeys == null || _options.ApiKeys.Count == 0)
            {
                _logger?.LogWarning("API Key 列表为空");
                return false;
            }

            // 从请求头获取 API Key
            string? apiKey = null;
            if (httpContext.Request.Headers.TryGetValue(_options.ApiKeyHeaderName, out var headerValues))
            {
                apiKey = headerValues.FirstOrDefault();
            }

            // 如果请求头中没有，尝试从查询参数获取
            if (string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(_options.ApiKeyQueryParameterName))
            {
                apiKey = httpContext.Request.Query[_options.ApiKeyQueryParameterName].FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return false;
            }

            // 检查 API Key 是否在允许的列表中
            var isValid = _options.ApiKeys.Any(key => string.Equals(key, apiKey, StringComparison.Ordinal));
            return isValid;
        }

        /// <summary>
        /// 检查路由是否在排除列表中
        /// </summary>
        private bool IsRouteExcluded(string route)
        {
            if (_options.ExcludedRoutes == null || _options.ExcludedRoutes.Count == 0)
            {
                return false;
            }

            // 规范化路由（移除尾随斜杠）
            route = route.TrimEnd('/');

            foreach (var pattern in _options.ExcludedRoutes)
            {
                if (string.IsNullOrWhiteSpace(pattern))
                {
                    continue;
                }

                var normalizedPattern = pattern.TrimEnd('/');

                // 精确匹配
                if (string.Equals(route, normalizedPattern, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // 通配符匹配（简单实现）
                if (normalizedPattern.EndsWith("/*", StringComparison.Ordinal))
                {
                    var prefix = normalizedPattern.Substring(0, normalizedPattern.Length - 2);
                    if (route.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
