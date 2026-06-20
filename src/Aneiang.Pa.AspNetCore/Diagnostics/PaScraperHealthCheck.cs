using Aneiang.Pa.Core.Scraper;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aneiang.Pa.AspNetCore.Diagnostics
{
    /// <summary>
    /// 标准 ASP.NET Core 健康检查：基于 IScraperRegistry 探测所有/部分爬虫
    /// </summary>
    public class PaScraperHealthCheck : IHealthCheck
    {
        private readonly IScraperRegistry _registry;
        private readonly PaScraperHealthCheckOptions _options;

        /// <summary>
        /// 初始化健康检查
        /// </summary>
        public PaScraperHealthCheck(IScraperRegistry registry, PaScraperHealthCheckOptions options)
        {
            _registry = registry;
            _options = options ?? new PaScraperHealthCheckOptions();
        }

        /// <inheritdoc />
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var scrapers = _registry.GetScrapers().ToList();
            if (_options.Categories != null && _options.Categories.Length > 0)
            {
                var allowed = new HashSet<string>(_options.Categories, StringComparer.OrdinalIgnoreCase);
                scrapers = scrapers.Where(s => allowed.Contains(s.Descriptor.Category)).ToList();
            }
            if (scrapers.Count == 0)
            {
                return HealthCheckResult.Healthy("无注册爬虫", new Dictionary<string, object> { ["count"] = 0 });
            }

            var data = new Dictionary<string, object>();
            var failures = new List<string>();
            var sw = Stopwatch.StartNew();

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_options.Timeout);

            var tasks = scrapers.Select(async s =>
            {
                try
                {
                    var typed = s.GetType();
                    var iface = typed.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IScraper<>));
                    if (iface == null) return (s.Descriptor, true, "skipped");

                    var method = iface.GetMethod("ScrapeAsync");
                    var task = (Task)method!.Invoke(s, new object?[] { cts.Token })!;
                    await task.ConfigureAwait(false);
                    var resultProp = task.GetType().GetProperty("Result")!;
                    var result = resultProp.GetValue(task);
                    var isSuccess = (bool)(result!.GetType().GetProperty("IsSuccess")!.GetValue(result) ?? false);
                    var msg = (string?)result.GetType().GetProperty("ErrorMessage")!.GetValue(result);
                    return (s.Descriptor, isSuccess, msg);
                }
                catch (Exception ex)
                {
                    return (s.Descriptor, false, ex.Message);
                }
            });

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            sw.Stop();

            foreach (var (descriptor, healthy, message) in results)
            {
                var key = $"{descriptor.Category}/{descriptor.Source}";
                data[key] = new { healthy, message };
                if (!healthy) failures.Add(key);
            }
            data["elapsedMs"] = sw.ElapsedMilliseconds;
            data["total"] = scrapers.Count;
            data["unhealthy"] = failures.Count;

            if (failures.Count == 0)
                return HealthCheckResult.Healthy($"全部 {scrapers.Count} 个爬虫健康", data);
            if (failures.Count < scrapers.Count)
                return HealthCheckResult.Degraded($"{failures.Count}/{scrapers.Count} 个爬虫不健康", null, data);
            return HealthCheckResult.Unhealthy("所有爬虫均不健康", null, data);
        }
    }

    /// <summary>
    /// PaScraperHealthCheck 配置
    /// </summary>
    public class PaScraperHealthCheckOptions
    {
        /// <summary>
        /// 检查范围（仅检查指定分类，null 表示全部）
        /// </summary>
        public string[]? Categories { get; set; }

        /// <summary>
        /// 超时
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
    }
}
