using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Aneiang.Pa.Core.Proxy
{
    /// <summary>
    /// 默认代理池实现（带健康跟踪 + 临时禁用）
    /// </summary>
    public class DefaultProxyPool : IProxyPool
    {
        private readonly ProxyPoolOptions _options;
        private readonly List<ProxyEntry> _entries;
        private int _index = -1;
        private readonly Random _random = new Random();
        private readonly object _lock = new object();

        /// <summary>
        /// 连续失败达到此阈值时临时禁用
        /// </summary>
        public int ConsecutiveFailureThreshold { get; set; } = 3;

        /// <summary>
        /// 临时禁用时长
        /// </summary>
        public TimeSpan BanDuration { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// 初始化代理池
        /// </summary>
        public DefaultProxyPool(IOptions<ProxyPoolOptions> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _entries = (_options.Proxies ?? new List<string>())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => new ProxyEntry(new Uri(p)))
                .ToList();
        }

        /// <inheritdoc />
        public Uri? GetNextProxy() => GetNextEntry()?.Uri;

        /// <inheritdoc />
        public ProxyEntry? GetNextEntry()
        {
            if (!_options.Enabled || _entries.Count == 0) return null;

            // 收集当前可用代理
            List<ProxyEntry> available;
            lock (_lock)
            {
                available = _entries.Where(e => e.IsAvailable).ToList();
            }
            if (available.Count == 0) return null;

            if (_options.Strategy == ProxySelectionStrategy.Random)
            {
                lock (_random)
                {
                    return available[_random.Next(0, available.Count)];
                }
            }

            // 轮询：在可用列表上轮询
            var next = Interlocked.Increment(ref _index);
            var idx = next % available.Count;
            if (idx < 0) idx += available.Count;
            return available[idx];
        }

        /// <inheritdoc />
        public void ReportSuccess(ProxyEntry entry)
        {
            if (entry == null) return;
            lock (_lock)
            {
                entry.SuccessCount++;
                entry.ConsecutiveFailures = 0;
                entry.BannedUntilUtc = null;
            }
        }

        /// <inheritdoc />
        public void ReportFailure(ProxyEntry entry)
        {
            if (entry == null) return;
            lock (_lock)
            {
                entry.FailureCount++;
                entry.ConsecutiveFailures++;
                if (entry.ConsecutiveFailures >= ConsecutiveFailureThreshold)
                {
                    entry.BannedUntilUtc = DateTime.UtcNow.Add(BanDuration);
                }
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<ProxyHealthInfo> GetHealthSnapshot()
        {
            lock (_lock)
            {
                return _entries.Select(e => new ProxyHealthInfo
                {
                    Uri = e.Uri.ToString(),
                    IsAvailable = e.IsAvailable,
                    SuccessCount = e.SuccessCount,
                    FailureCount = e.FailureCount,
                    ConsecutiveFailures = e.ConsecutiveFailures,
                    BannedUntilUtc = e.BannedUntilUtc,
                    SuccessRate = e.SuccessRate
                }).ToList();
            }
        }
    }
}
