using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Aneiang.Pa.Abstractions;
using Microsoft.Extensions.Logging;

namespace Aneiang.Pa.Core.Middlewares;

/// <summary>日志中间件：记录开始/结束/耗时</summary>
public sealed class LoggingMiddleware : IScrapeMiddleware
{
    private readonly ILogger<LoggingMiddleware>? _logger;

    /// <inheritdoc />
    public int Order => 0;

    /// <inheritdoc />
    public string Name => "Logging";

    /// <summary>初始化</summary>
    public LoggingMiddleware(ILogger<LoggingMiddleware>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ScrapeResult> InvokeAsync(ScrapeContext ctx, ScrapeDelegate next)
    {
        var sw = Stopwatch.StartNew();
        _logger?.LogInformation("[Pa] Start {Name} CorrelationId={Cid}", ctx.Recipe.Name, ctx.CorrelationId);
        try
        {
            var result = await next(ctx).ConfigureAwait(false);
            sw.Stop();
            if (result.IsSuccess)
                _logger?.LogInformation("[Pa] OK    {Name} Items={Count} Elapsed={Ms}ms", ctx.Recipe.Name, result.Data.Count, sw.ElapsedMilliseconds);
            else
                _logger?.LogWarning("[Pa] FAIL  {Name} Reason={Err} Elapsed={Ms}ms", ctx.Recipe.Name, result.ErrorMessage, sw.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger?.LogError(ex, "[Pa] EX    {Name} Elapsed={Ms}ms", ctx.Recipe.Name, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
