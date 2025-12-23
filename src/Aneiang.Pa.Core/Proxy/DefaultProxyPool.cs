using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Options;

namespace Aneiang.Pa.Core.Proxy
{
    /// <summary>
    /// 默认简单实现的代理池。
    /// </summary>
    public class DefaultProxyPool : IProxyPool
    {
        private readonly ProxyPoolOptions _options;
        private readonly List<Uri> _proxyUris;
        private int _index = -1;
        private readonly Random _random = new Random();

        public DefaultProxyPool(IOptions<ProxyPoolOptions> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));

            _proxyUris = (_options.Proxies ?? new List<string>())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => new Uri(p))
                .ToList();
        }

        /// <inheritdoc />
        public Uri? GetNextProxy()
        {
            if (!_options.Enabled || _proxyUris.Count == 0)
            {
                return null;
            }

            if (_options.Strategy == ProxySelectionStrategy.Random)
            {
                lock (_random)
                {
                    var idx = _random.Next(0, _proxyUris.Count);
                    return _proxyUris[idx];
                }
            }

            // 默认轮询
            var next = Interlocked.Increment(ref _index);
            var index = next % _proxyUris.Count;
            if (index < 0)
            {
                index += _proxyUris.Count;
            }
            return _proxyUris[index];
        }
    }
}


