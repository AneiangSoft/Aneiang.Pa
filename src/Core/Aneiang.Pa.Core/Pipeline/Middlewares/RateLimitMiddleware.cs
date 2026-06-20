using Aneiang.Pa.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aneiang.Pa.Core.Pipeline.Middlewares
{
    /// <summary>
    /// 限流中间件：滑动窗口令牌桶
    /// </summary>
    public class RateLimitMiddleware : IScrapeMiddleware
    {
        private readonly IOptionsMonitor<ScrapeRateLimitOptions> _options;
        private readonly ILogger<RateLimitMiddleware>? _logger;
        private static readonly ConcurrentDictionary<string, Bucket> Buckets = new ConcurrentDictionary<string, Bucket>();

        /// <inheritdoc />
        public int Order => 40;

        /// <summary>
        /// 初始化限流中间件
        /// </summary>
        public RateLimitMiddleware(IOptionsMonitor<ScrapeRateLimitOptions> options, ILogger<RateLimitMiddleware>? logger = null)
        {
            _options = options;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<ScraperResult<T>> InvokeAsync<T>(ScrapeContext context, ScrapeDelegate<T> next) where T : class
        {
            var policy = _options.CurrentValue.GetFor(context.Descriptor.Category, context.Descriptor.Source);
            if (policy.PermitsPerWindow <= 0 || policy.Window <= TimeSpan.Zero)
                return await next(context).ConfigureAwait(false);

            var key = $"{context.Descriptor.Category}:{context.Descriptor.Source}";
            var bucket = Buckets.GetOrAdd(key, _ => new Bucket());

            await bucket.WaitAsync(policy, context.CancellationToken, _logger, key).ConfigureAwait(false);
            return await next(context).ConfigureAwait(false);
        }

        private class Bucket
        {
            private readonly object _lock = new object();
            private readonly Queue<DateTime> _timestamps = new Queue<DateTime>();

            public async Task WaitAsync(ScrapeRateLimitPolicy policy, CancellationToken ct, ILogger? logger, string key)
            {
                while (true)
                {
                    TimeSpan? waitFor = null;
                    lock (_lock)
                    {
                        var now = DateTime.UtcNow;
                        var cutoff = now - policy.Window;
                        while (_timestamps.Count > 0 && _timestamps.Peek() < cutoff)
                            _timestamps.Dequeue();

                        if (_timestamps.Count < policy.PermitsPerWindow)
                        {
                            _timestamps.Enqueue(now);
                            return;
                        }
                        waitFor = _timestamps.Peek() - cutoff;
                    }

                    if (waitFor != null && waitFor.Value > TimeSpan.Zero)
                    {
                        logger?.LogDebug("[Scrape] RateLimit wait {Wait}ms for {Key}", waitFor.Value.TotalMilliseconds, key);
                        await Task.Delay(waitFor.Value, ct).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
