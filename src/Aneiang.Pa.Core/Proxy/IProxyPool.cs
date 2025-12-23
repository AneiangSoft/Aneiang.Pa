using System;

namespace Aneiang.Pa.Core.Proxy
{
    /// <summary>
    /// 代理池抽象。
    /// </summary>
    public interface IProxyPool
    {
        /// <summary>
        /// 获取下一个可用代理地址。
        /// </summary>
        /// <returns>代理 Uri；如果未启用或无可用代理，则返回 null。</returns>
        Uri? GetNextProxy();
    }
}


