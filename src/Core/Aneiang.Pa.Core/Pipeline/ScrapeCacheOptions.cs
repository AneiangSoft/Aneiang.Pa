using Aneiang.Pa.Core.Models;
using System;
using System.Collections.Generic;

namespace Aneiang.Pa.Core.Pipeline
{
    /// <summary>
    /// 管道层缓存配置
    /// </summary>
    public class ScrapeCacheOptions
    {
        /// <summary>
        /// 默认缓存时长（&lt;=Zero 表示禁用缓存）
        /// </summary>
        public TimeSpan DefaultDuration { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// 按 "Category:Source" 配置缓存时长（支持通配符 "Category:*"）
        /// </summary>
        public Dictionary<string, TimeSpan> PerSource { get; set; } = new Dictionary<string, TimeSpan>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 获取生效缓存时长
        /// </summary>
        public TimeSpan GetFor(string category, string source)
        {
            var key = $"{category}:{source}";
            if (PerSource.TryGetValue(key, out var d)) return d;
            var wildcard = $"{category}:*";
            if (PerSource.TryGetValue(wildcard, out var wd)) return wd;
            return DefaultDuration;
        }
    }
}
