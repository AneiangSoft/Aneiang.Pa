using System;
using System.Threading;
using System.Threading.Tasks;
using Aneiang.Pa.Abstractions;
using Microsoft.Extensions.Logging;

namespace Aneiang.Pa.Core.Middlewares;

/// <summary>超时中间件：单次硬超时</summary>
public sealed class TimeoutMiddleware : IScrapeMiddleware
{
    private readonly PaOptions _options;
    private readonly ILogger<TimeoutMiddleware>? _logger;

    /// <inheritdoc />
    public int Order => 70;

    /// <inheritdoc />
    public string Name => "Timeout";

    /// <summary>初始化</summary>
    public TimeoutMiddleware(PaOptions options, ILogger<TimeoutMiddleware>? logger = null)
    {
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ScrapeResult> InvokeAsync(ScrapeContext ctx, ScrapeDelegate next)
    {
        var timeout = ctx.Items.TryGetValue("__timeout_override__", out var v) && v is TimeSpan t
            ? t : (ctx.Recipe.Fetch.Timeout ?? _options.DefaultTimeout);
        if (timeout <= TimeSpan.Zero) return await next(ctx).ConfigureAwait(false);

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ctx.CancellationToken);
        linked.CancelAfter(timeout);

        var inner = new ScrapeContext(ctx.Recipe, linked.Token, ctx.CorrelationId)
        {
            BypassCache = ctx.BypassCache
        };
        foreach (var kv in ctx.Variables) inner.Variables[kv.Key] = kv.Value;
        foreach (var kv in ctx.Items) inner.Items[kv.Key] = kv.Value;
        foreach (var s in ctx.SkipMiddlewares) inner.SkipMiddlewares.Add(s);

        try
        {
            return await next(inner).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (linked.IsCancellationRequested && !ctx.CancellationToken.IsCancellationRequested)
        {
            _logger?.LogWarning("[Pa] Timeout {Name} after {Ms}ms", ctx.Recipe.Name, timeout.TotalMilliseconds);
            return ScrapeResult.Fail($"操作超时（{timeout.TotalMilliseconds}ms）");
        }
    }
}
