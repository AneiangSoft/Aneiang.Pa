using System;
using System.Threading.Tasks;
using Aneiang.Pa.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Aneiang.Pa.Core.Middlewares;

/// <summary>缓存中间件（默认 Memory，可注入分布式实现）</summary>
public sealed class CacheMiddleware : IScrapeMiddleware
{
    private readonly IMemoryCache _cache;
    private readonly PaOptions _options;
    private readonly ILogger<CacheMiddleware>? _logger;

    /// <inheritdoc />
    public int Order => 30;

    /// <inheritdoc />
    public string Name => "Cache";

    /// <summary>初始化</summary>
    public CacheMiddleware(IMemoryCache cache, PaOptions options, ILogger<CacheMiddleware>? logger = null)
    {
        _cache = cache;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ScrapeResult> InvokeAsync(ScrapeContext ctx, ScrapeDelegate next)
    {
        if (ctx.BypassCache || _options.DefaultCacheDuration <= TimeSpan.Zero)
            return await next(ctx).ConfigureAwait(false);

        var key = "pa:" + ctx.Recipe.Name;
        if (_cache.TryGetValue(key, out ScrapeResult? cached) && cached != null)
        {
            _logger?.LogDebug("[Pa] Cache HIT  {Name}", ctx.Recipe.Name);
            return cached;
        }

        var result = await next(ctx).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            _cache.Set(key, result, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _options.DefaultCacheDuration
            });
        }
        return result;
    }
}
