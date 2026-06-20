using System;
using System.Threading.Tasks;
using Aneiang.Pa.Abstractions;
using Microsoft.Extensions.Logging;

namespace Aneiang.Pa.Core.Middlewares;

/// <summary>重试中间件：指数退避</summary>
public sealed class RetryMiddleware : IScrapeMiddleware
{
    private readonly PaOptions _options;
    private readonly ILogger<RetryMiddleware>? _logger;

    /// <inheritdoc />
    public int Order => 60;

    /// <inheritdoc />
    public string Name => "Retry";

    /// <summary>初始化</summary>
    public RetryMiddleware(PaOptions options, ILogger<RetryMiddleware>? logger = null)
    {
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ScrapeResult> InvokeAsync(ScrapeContext ctx, ScrapeDelegate next)
    {
        var max = Math.Max(1, _options.DefaultRetryCount + 1);
        ScrapeResult? last = null;
        Exception? lastEx = null;
        for (var attempt = 1; attempt <= max; attempt++)
        {
            try
            {
                var r = await next(ctx).ConfigureAwait(false);
                if (r.IsSuccess) return r;
                last = r;
            }
            catch (OperationCanceledException) when (ctx.CancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastEx = ex;
                _logger?.LogWarning(ex, "[Pa] Retry {Attempt}/{Max} {Name}", attempt, max, ctx.Recipe.Name);
            }

            if (attempt < max)
            {
                var delay = TimeSpan.FromMilliseconds(500 * Math.Pow(2, attempt - 1));
                await Task.Delay(delay, ctx.CancellationToken).ConfigureAwait(false);
            }
        }
        if (lastEx != null) return ScrapeResult.Fail($"重试 {max} 次仍失败: {lastEx.Message}");
        return last ?? ScrapeResult.Fail("未知错误");
    }
}
