using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Aneiang.Pa.AspNetCore.Options
{
    /// <summary>
    /// 授权配置选项
    /// </summary>
    public class AuthorizationOptions
    {
        /// <summary>
        /// 是否启用授权，默认为 false
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// API Key 列表（用于 API Key 授权方式）
        /// </summary>
        public List<string> ApiKeys { get; set; } = new List<string>();

        /// <summary>
        /// API Key 请求头名称，默认为 "X-API-Key"
        /// </summary>
        public string ApiKeyHeaderName { get; set; } = "X-API-Key";

        /// <summary>
        /// API Key 查询参数名称，默认为 "apiKey"（如果设置了，将从查询字符串中读取）
        /// </summary>
        public string? ApiKeyQueryParameterName { get; set; } = "apiKey";

        /// <summary>
        /// 自定义授权验证函数
        /// 参数：HttpContext，返回：是否授权成功，以及可选的 Claims
        /// 如果返回 true，请求将被授权；如果返回 false，请求将被拒绝
        /// </summary>
        public Func<Microsoft.AspNetCore.Http.HttpContext, (bool Authorized, ClaimsPrincipal? Principal)>? CustomAuthorizationFunc { get; set; }

        /// <summary>
        /// 未授权时的错误消息，默认为 "未授权访问"
        /// </summary>
        public string UnauthorizedMessage { get; set; } = "未授权访问";

        /// <summary>
        /// 授权方式
        /// </summary>
        public AuthorizationScheme Scheme { get; set; } = AuthorizationScheme.ApiKey;

        /// <summary>
        /// 排除的路由模式（这些路由不需要授权）
        /// 支持通配符模式，如 "/api/scraper/health" 或 "/api/scraper/*/health"
        /// </summary>
        public List<string> ExcludedRoutes { get; set; } = new List<string>();
    }

    /// <summary>
    /// 授权方式
    /// </summary>
    public enum AuthorizationScheme
    {
        /// <summary>
        /// API Key 授权（通过请求头或查询参数）
        /// </summary>
        ApiKey,

        /// <summary>
        /// 自定义授权验证函数
        /// </summary>
        Custom,

        /// <summary>
        /// 组合方式（API Key 或自定义验证函数，满足任一即可）
        /// </summary>
        Combined
    }
}
