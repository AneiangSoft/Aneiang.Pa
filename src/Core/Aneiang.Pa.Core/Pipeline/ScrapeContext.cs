using System.Collections.Generic;
using System.Threading;

namespace Aneiang.Pa.Core.Pipeline
{
    /// <summary>
    /// 爬取上下文，在管道中流转，可携带中间件之间的共享数据
    /// </summary>
    public class ScrapeContext
    {
        /// <summary>
        /// 关联的爬虫描述符
        /// </summary>
        public Scraper.ScraperDescriptor Descriptor { get; }

        /// <summary>
        /// 中间件之间共享的键值集合
        /// </summary>
        public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>();

        /// <summary>
        /// 关联 ID（用于日志/追踪）
        /// </summary>
        public string CorrelationId { get; }

        /// <summary>
        /// 取消令牌
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// 初始化爬取上下文
        /// </summary>
        public ScrapeContext(Scraper.ScraperDescriptor descriptor, CancellationToken cancellationToken, string? correlationId = null)
        {
            Descriptor = descriptor;
            CancellationToken = cancellationToken;
            CorrelationId = correlationId ?? System.Guid.NewGuid().ToString("N");
        }
    }
}
