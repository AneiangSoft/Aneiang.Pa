using System.Collections.Generic;

namespace Aneiang.Pa.Core.Models
{
    /// <summary>
    /// 统一爬取请求参数模型
    /// </summary>
    public class ScrapeRequest
    {
        /// <summary>
        /// 页码（从 1 开始）
        /// </summary>
        public int? PageNo { get; set; }

        /// <summary>
        /// 每页数量
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// 自定义筛选条件
        /// </summary>
        public IDictionary<string, string>? Filters { get; set; }
    }
}
