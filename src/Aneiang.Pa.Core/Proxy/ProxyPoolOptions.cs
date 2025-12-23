using System;
using System.Collections.Generic;

namespace Aneiang.Pa.Core.Proxy
{
    /// <summary>
    /// 代理池配置选项。
    /// </summary>
    public class ProxyPoolOptions
    {
        /// <summary>
        /// 是否启用代理池。
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 代理地址列表（例如：http://user:pwd@host:port）。
        /// </summary>
        public List<string> Proxies { get; set; } = new List<string>();

        /// <summary>
        /// 选择策略，默认轮询。
        /// </summary>
        public ProxySelectionStrategy Strategy { get; set; } = ProxySelectionStrategy.RoundRobin;

        /// <summary>
        /// 简单校验配置。
        /// </summary>
        public void Check()
        {
            if (!Enabled)
            {
                return;
            }

            if (Proxies == null || Proxies.Count == 0)
            {
                throw new InvalidOperationException("ProxyPool is enabled, but no proxy is configured.");
            }
        }
    }

    /// <summary>
    /// 代理选择策略。
    /// </summary>
    public enum ProxySelectionStrategy
    {
        /// <summary>
        /// 轮询。
        /// </summary>
        RoundRobin = 0,

        /// <summary>
        /// 随机。
        /// </summary>
        Random = 1
    }
}


