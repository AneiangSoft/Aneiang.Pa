using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using Aneiang.Pa.Abstractions;

namespace Aneiang.Pa.Core.Middlewares;

/// <summary>指标中间件：暴露 Meter "Aneiang.Pa"，可被 OpenTelemetry/prometheus-net 采集</summary>
public sealed class MetricsMiddleware : IScrapeMiddleware
{
    /// <summary>Meter 名</summary>
    public const string MeterName = "Aneiang.Pa";

    private static readonly Meter Meter = new(MeterName, "4.0.0");
    private static readonly Counter<long> TotalCounter = Meter.CreateCounter<long>("pa_scrape_total");
    private static readonly Histogram<double> DurationHist = Meter.CreateHistogram<double>("pa_scrape_duration_seconds", "s");
    private static readonly Counter<long> ItemsCounter = Meter.CreateCounter<long>("pa_scrape_items_total");

    /// <inheritdoc />
    public int Order => 10;

    /// <inheritdoc />
    public string Name => "Metrics";

    /// <inheritdoc />
    public async Task<ScrapeResult> InvokeAsync(ScrapeContext ctx, ScrapeDelegate next)
    {
        var sw = Stopwatch.StartNew();
        var status = "ok";
        ScrapeResult? result = null;
        try
        {
            result = await next(ctx).ConfigureAwait(false);
            if (!result.IsSuccess) status = "fail";
            return result;
        }
        catch
        {
            status = "exception";
            throw;
        }
        finally
        {
            sw.Stop();
            var tags = new TagList
            {
                { "name", ctx.Recipe.Name },
                { "category", ctx.Recipe.Category ?? "" },
                { "status", status }
            };
            TotalCounter.Add(1, tags);
            DurationHist.Record(sw.Elapsed.TotalSeconds, tags);
            if (result?.IsSuccess == true)
                ItemsCounter.Add(result.Data.Count, new TagList
                {
                    { "name", ctx.Recipe.Name },
                    { "category", ctx.Recipe.Category ?? "" }
                });
        }
    }
}
