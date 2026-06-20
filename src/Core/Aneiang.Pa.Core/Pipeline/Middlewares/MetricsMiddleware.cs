using Aneiang.Pa.Core.Models;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;

namespace Aneiang.Pa.Core.Pipeline.Middlewares
{
    /// <summary>
    /// 指标中间件：通过 System.Diagnostics.Metrics 暴露指标，
    /// 可被 OpenTelemetry / prometheus-net 等采集器导出
    /// </summary>
    public class MetricsMiddleware : IScrapeMiddleware
    {
        /// <summary>
        /// Meter 名称（用于 OpenTelemetry MeterProviderBuilder.AddMeter）
        /// </summary>
        public const string MeterName = "Aneiang.Pa";

        private static readonly Meter Meter = new Meter(MeterName, "1.0.0");
        private static readonly Counter<long> ScrapeTotal = Meter.CreateCounter<long>("pa_scrape_total", description: "累计爬取调用数");
        private static readonly Histogram<double> ScrapeDuration = Meter.CreateHistogram<double>("pa_scrape_duration_seconds", unit: "s", description: "爬取耗时分布");
        private static readonly Counter<long> ScrapeItems = Meter.CreateCounter<long>("pa_scrape_items_total", description: "累计抓取条目数");

        /// <inheritdoc />
        public int Order => 10;

        /// <inheritdoc />
        public async Task<ScraperResult<T>> InvokeAsync<T>(ScrapeContext context, ScrapeDelegate<T> next) where T : class
        {
            var sw = Stopwatch.StartNew();
            var status = "ok";
            ScraperResult<T>? result = null;
            try
            {
                result = await next(context).ConfigureAwait(false);
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
                    { "category", context.Descriptor.Category },
                    { "source", context.Descriptor.Source },
                    { "status", status }
                };
                ScrapeTotal.Add(1, tags);
                ScrapeDuration.Record(sw.Elapsed.TotalSeconds, tags);
                if (result != null && result.IsSuccess)
                {
                    var itemTags = new TagList
                    {
                        { "category", context.Descriptor.Category },
                        { "source", context.Descriptor.Source }
                    };
                    ScrapeItems.Add(result.Data.Count, itemTags);
                }
            }
        }
    }
}
