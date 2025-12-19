using System;

namespace Aneiang.Pa.Core.News
{
    /// <summary>
    /// 爬取器配置选项
    /// </summary>
    public class ScraperOptions
    {
        /// <summary>
        /// Base地址
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// 新闻/热门 地址
        /// </summary>
        public string NewsUrl { get; set; }

        /// <summary>
        /// Cookie
        /// </summary>
        public string Cookie { get; set; }

        /// <summary>
        /// UserAgent
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// 检查参数
        /// </summary>
        /// <exception cref="Exception"></exception>
        public virtual void Check()
        {
            if (string.IsNullOrWhiteSpace(NewsUrl) || string.IsNullOrWhiteSpace(BaseUrl))
            {
                throw new Exception("The configuration parameters are incomplete or missing!");
            }

            if (string.IsNullOrWhiteSpace(UserAgent))
            {
                UserAgent = UserAgentGenerator.GetRandomUserAgent();
            }
        }
    }
}
