using Aneiang.Pa.Core.Data;

namespace Aneiang.Pa.Core.News.Models
{
    /// <summary>
    /// 新闻
    /// </summary>
    public class NewsItem : IExtendableObject
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 手机地址
        /// </summary>
        public string MobileUrl { get; set; }

        /// <summary>
        /// 扩展参数
        /// </summary>
        public string ExtensionData { get; set; }
    }
}
