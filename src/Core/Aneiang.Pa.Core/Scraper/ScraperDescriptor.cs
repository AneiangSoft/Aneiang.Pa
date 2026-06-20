using System;

namespace Aneiang.Pa.Core.Scraper
{
    /// <summary>
    /// 爬虫元数据描述符
    /// </summary>
    public class ScraperDescriptor
    {
        /// <summary>
        /// 分类标识（如 News / Lottery / Dynamic）
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 源标识（如 BaiDu / WeiBo / SSQ），替代 ScraperSource 枚举
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// 显示名称（如 "百度热榜"）
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 结果类型（用于序列化/反序列化）
        /// </summary>
        public Type? ResultType { get; set; }

        /// <summary>
        /// 是否支持分页
        /// </summary>
        public bool SupportsPaging { get; set; }

        /// <summary>
        /// 生成缓存键
        /// </summary>
        public string ToCacheKey(params object[] extra)
        {
            var suffix = extra.Length > 0 ? ":" + string.Join(":", extra) : "";
            return $"scraper:{Category}:{Source}{suffix}";
        }

        /// <summary>
        /// 返回 "Category/Source" 格式的路由标识
        /// </summary>
        public override string ToString() => $"{Category}/{Source}";
    }
}
