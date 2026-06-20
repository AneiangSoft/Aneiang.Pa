using Aneiang.Pa.Core.Scraper;
using System;
using System.Collections.Generic;

namespace Aneiang.Pa.Core.Pipeline
{
    /// <summary>
    /// 单条弹性策略
    /// </summary>
    public class ScrapeResiliencePolicy
    {
        /// <summary>
        /// 重试次数（不含首次调用）
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// 退避毫秒序列（不足时取最后一个值）
        /// </summary>
        public int[]? RetryBackoffMs { get; set; }

        /// <summary>
        /// 单次执行超时；&lt;=0 表示不限制
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 熔断器：连续失败阈值；&lt;=0 表示不启用熔断
        /// </summary>
        public int CircuitBreakerFailureThreshold { get; set; } = 0;

        /// <summary>
        /// 熔断打开后的休眠时长
        /// </summary>
        public TimeSpan CircuitBreakerBreakDuration { get; set; } = TimeSpan.FromMinutes(1);
    }

    /// <summary>
    /// 弹性策略集合（默认策略 + 按 "Category:Source" 覆盖）
    /// </summary>
    public class ScrapeResilienceOptions
    {
        /// <summary>
        /// 默认策略
        /// </summary>
        public ScrapeResiliencePolicy Default { get; set; } = new ScrapeResiliencePolicy();

        /// <summary>
        /// 按 "Category:Source" 覆盖（支持通配符 "Category:*"）
        /// </summary>
        public Dictionary<string, ScrapeResiliencePolicy> Overrides { get; set; } = new Dictionary<string, ScrapeResiliencePolicy>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 根据描述符获取生效策略
        /// </summary>
        public ScrapeResiliencePolicy GetFor(ScraperDescriptor descriptor)
        {
            var key = $"{descriptor.Category}:{descriptor.Source}";
            if (Overrides.TryGetValue(key, out var policy)) return policy;
            var wildcard = $"{descriptor.Category}:*";
            if (Overrides.TryGetValue(wildcard, out var wp)) return wp;
            return Default;
        }
    }
}
