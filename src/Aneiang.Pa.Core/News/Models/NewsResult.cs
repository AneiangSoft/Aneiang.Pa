using System.Collections.Generic;

namespace Aneiang.Pa.Core.News.Models
{
    /// <summary>
    /// 新闻返回模型
    /// </summary>
    public class NewsResult
    {
        public NewsResult(bool isSuccessd = true,string? errorMessage = null)
        {
            IsSuccessd = isSuccessd;
            ErrorMessage = errorMessage;
        }
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccessd { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public List<NewsItem> Data { get; set; } = new List<NewsItem>();
    }
}
