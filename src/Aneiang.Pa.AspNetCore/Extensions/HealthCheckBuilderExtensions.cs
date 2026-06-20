using Aneiang.Pa.AspNetCore.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Aneiang.Pa.AspNetCore.Extensions
{
    /// <summary>
    /// HealthChecks 注册扩展
    /// </summary>
    public static class HealthCheckBuilderExtensions
    {
        /// <summary>
        /// 注册 Aneiang.Pa 爬虫健康检查到标准 ASP.NET Core HealthChecks 框架
        /// </summary>
        public static IHealthChecksBuilder AddPaScrapers(
            this IHealthChecksBuilder builder,
            string name = "pa-scrapers",
            HealthStatus failureStatus = HealthStatus.Degraded,
            Action<PaScraperHealthCheckOptions>? configure = null)
        {
            var options = new PaScraperHealthCheckOptions();
            configure?.Invoke(options);
            builder.Services.AddSingleton(options);

            return builder.AddCheck<PaScraperHealthCheck>(name, failureStatus);
        }
    }
}
