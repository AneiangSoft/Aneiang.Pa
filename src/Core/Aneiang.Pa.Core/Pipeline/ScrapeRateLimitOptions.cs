using System;
using System.Collections.Generic;

namespace Aneiang.Pa.Core.Pipeline
{
    /// <summary>
    /// 限流策略
    /// </summary>
    public class ScrapeRateLimitPolicy
    {
        /// <summary>
        /// 单个时间窗口内允许的请求数
        /// </summary>
        public int PermitsPerWindow { get; set; } = 0;

        /// <summary>
        /// 时间窗口
        /// </summary>
        public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);
    }

    /// <summary>
    /// 限流配置（全局 + 按 "Category:Source" 覆盖）
    /// </summary>
    public class ScrapeRateLimitOptions
    {
        /// <summary>
        /// 全局策略；PermitsPerWindow=0 表示不启用
        /// </summary>
        public ScrapeRateLimitPolicy Global { get; set; } = new ScrapeRateLimitPolicy();

        /// <summary>
        /// 按 "Category:Source" 覆盖（支持通配符 "Category:*"）
        /// </summary>
        public Dictionary<string, ScrapeRateLimitPolicy> PerSource { get; set; } = new Dictionary<string, ScrapeRateLimitPolicy>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 获取生效策略
        /// </summary>
        public ScrapeRateLimitPolicy GetFor(string category, string source)
        {
            var key = $"{category}:{source}";
            if (PerSource.TryGetValue(key, out var p)) return p;
            var wildcard = $"{category}:*";
            if (PerSource.TryGetValue(wildcard, out var wp)) return wp;
            return Global;
        }
    }
}
