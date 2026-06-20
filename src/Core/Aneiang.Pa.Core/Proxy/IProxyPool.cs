using System;
using System.Collections.Generic;

namespace Aneiang.Pa.Core.Proxy
{
    /// <summary>
    /// 代理池接口（带健康跟踪）
    /// </summary>
    public interface IProxyPool
    {
        /// <summary>
        /// 获取下一个可用代理 URI（兼容旧接口）。无可用时返回 null。
        /// </summary>
        Uri? GetNextProxy();

        /// <summary>
        /// 获取下一个可用代理项；无可用时返回 null
        /// </summary>
        ProxyEntry? GetNextEntry();

        /// <summary>
        /// 上报某代理调用成功
        /// </summary>
        void ReportSuccess(ProxyEntry entry);

        /// <summary>
        /// 上报某代理调用失败
        /// </summary>
        void ReportFailure(ProxyEntry entry);

        /// <summary>
        /// 获取所有代理健康快照
        /// </summary>
        IReadOnlyList<ProxyHealthInfo> GetHealthSnapshot();
    }
}
