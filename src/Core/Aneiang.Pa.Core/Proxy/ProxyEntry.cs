using System;

namespace Aneiang.Pa.Core.Proxy
{
    /// <summary>
    /// 代理项（含健康跟踪信息）
    /// </summary>
    public class ProxyEntry
    {
        /// <summary>
        /// 代理 URI
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// 累计成功次数
        /// </summary>
        public long SuccessCount { get; internal set; }

        /// <summary>
        /// 累计失败次数
        /// </summary>
        public long FailureCount { get; internal set; }

        /// <summary>
        /// 连续失败计数
        /// </summary>
        public int ConsecutiveFailures { get; internal set; }

        /// <summary>
        /// 临时禁用截止时间（UTC），null 表示未禁用
        /// </summary>
        public DateTime? BannedUntilUtc { get; internal set; }

        /// <summary>
        /// 是否当前可用
        /// </summary>
        public bool IsAvailable => BannedUntilUtc == null || DateTime.UtcNow >= BannedUntilUtc.Value;

        /// <summary>
        /// 成功率
        /// </summary>
        public double SuccessRate
        {
            get
            {
                var total = SuccessCount + FailureCount;
                return total == 0 ? 1.0 : (double)SuccessCount / total;
            }
        }

        /// <summary>
        /// 初始化代理项
        /// </summary>
        public ProxyEntry(Uri uri)
        {
            Uri = uri;
        }
    }

    /// <summary>
    /// 代理健康快照
    /// </summary>
    public class ProxyHealthInfo
    {
        /// <summary>代理 URI</summary>
        public string Uri { get; set; } = string.Empty;
        /// <summary>是否可用</summary>
        public bool IsAvailable { get; set; }
        /// <summary>成功次数</summary>
        public long SuccessCount { get; set; }
        /// <summary>失败次数</summary>
        public long FailureCount { get; set; }
        /// <summary>连续失败</summary>
        public int ConsecutiveFailures { get; set; }
        /// <summary>禁用截止</summary>
        public DateTime? BannedUntilUtc { get; set; }
        /// <summary>成功率</summary>
        public double SuccessRate { get; set; }
    }
}
