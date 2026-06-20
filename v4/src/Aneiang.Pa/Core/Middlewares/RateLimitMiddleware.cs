using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aneiang.Pa.Abstractions;
using Microsoft.Extensions.Logging;

namespace Aneiang.Pa.Core.Middlewares;

/// <summary>限流中间件：滑动窗口（每个 Recipe 独立桶）</summary>
public sealed class RateLimitMiddleware : IScrapeMiddleware
{
    private static readonly ConcurrentDictionary<string, Bucket> Buckets = new();
    private readonly ILogger<RateLimitMiddleware>? _logger;

    /// <summary>每个时间窗口允许的请求数；&lt;=0 表示不限流</summary>
    public int PermitsPerWindow { get; set; } = 0;

    /// <summary>时间窗口</summary>
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);

    /// <inheritdoc />
    public int Order => 40;

    /// <inheritdoc />
    public string Name => "RateLimit";

    /// <summary>初始化</summary>
    public RateLimitMiddleware(ILogger<RateLimitMiddleware>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ScrapeResult> InvokeAsync(ScrapeContext ctx, ScrapeDelegate next)
    {
        if (PermitsPerWindow <= 0) return await next(ctx).ConfigureAwait(false);
        var bucket = Buckets.GetOrAdd(ctx.Recipe.Name, _ => new Bucket());
        await bucket.WaitAsync(PermitsPerWindow, Window, ctx.CancellationToken, _logger, ctx.Recipe.Name).ConfigureAwait(false);
        return await next(ctx).ConfigureAwait(false);
    }

    private sealed class Bucket
    {
        private readonly object _lock = new();
        private readonly Queue<DateTime> _ts = new();

        public async Task WaitAsync(int permits, TimeSpan window, System.Threading.CancellationToken ct, ILogger? logger, string name)
        {
            while (true)
            {
                TimeSpan? wait = null;
                lock (_lock)
                {
                    var now = DateTime.UtcNow;
                    var cutoff = now - window;
                    while (_ts.Count > 0 && _ts.Peek() < cutoff) _ts.Dequeue();
                    if (_ts.Count < permits)
                    {
                        _ts.Enqueue(now);
                        return;
                    }
                    wait = _ts.Peek() - cutoff;
                }
                if (wait != null && wait.Value > TimeSpan.Zero)
                {
                    logger?.LogDebug("[Pa] RateLimit wait {Ms}ms for {Name}", wait.Value.TotalMilliseconds, name);
                    await Task.Delay(wait.Value, ct).ConfigureAwait(false);
                }
            }
        }
    }
}
