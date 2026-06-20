using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Aneiang.Pa.Abstractions;

namespace Aneiang.Pa.Core.Middlewares;

/// <summary>追踪中间件：创建 Activity（OpenTelemetry 兼容）</summary>
public sealed class TracingMiddleware : IScrapeMiddleware
{
    /// <summary>ActivitySource 名</summary>
    public const string ActivitySourceName = "Aneiang.Pa";

    private static readonly ActivitySource ActivitySource = new(ActivitySourceName, "4.0.0");

    /// <inheritdoc />
    public int Order => 20;

    /// <inheritdoc />
    public string Name => "Tracing";

    /// <inheritdoc />
    public async Task<ScrapeResult> InvokeAsync(ScrapeContext ctx, ScrapeDelegate next)
    {
        using var activity = ActivitySource.StartActivity($"Pa {ctx.Recipe.Name}", ActivityKind.Client);
        activity?.SetTag("pa.name", ctx.Recipe.Name);
        activity?.SetTag("pa.category", ctx.Recipe.Category);
        activity?.SetTag("pa.correlation_id", ctx.CorrelationId);

        try
        {
            var result = await next(ctx).ConfigureAwait(false);
            activity?.SetTag("pa.success", result.IsSuccess);
            activity?.SetTag("pa.items_count", result.Data.Count);
            if (!result.IsSuccess)
                activity?.SetStatus(ActivityStatusCode.Error, result.ErrorMessage);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
