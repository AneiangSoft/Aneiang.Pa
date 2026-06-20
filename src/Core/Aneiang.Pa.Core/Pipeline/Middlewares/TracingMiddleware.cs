using Aneiang.Pa.Core.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Aneiang.Pa.Core.Pipeline.Middlewares
{
    /// <summary>
    /// 追踪中间件：创建 Activity（OpenTelemetry 兼容）
    /// </summary>
    public class TracingMiddleware : IScrapeMiddleware
    {
        /// <summary>
        /// ActivitySource 名称（用于 OpenTelemetry TracerProviderBuilder.AddSource）
        /// </summary>
        public const string ActivitySourceName = "Aneiang.Pa";

        private static readonly ActivitySource ActivitySource = new ActivitySource(ActivitySourceName, "1.0.0");

        /// <inheritdoc />
        public int Order => 20;

        /// <inheritdoc />
        public async Task<ScraperResult<T>> InvokeAsync<T>(ScrapeContext context, ScrapeDelegate<T> next) where T : class
        {
            using var activity = ActivitySource.StartActivity($"Scrape {context.Descriptor.Category}/{context.Descriptor.Source}", ActivityKind.Client);
            if (activity != null)
            {
                activity.SetTag("scraper.category", context.Descriptor.Category);
                activity.SetTag("scraper.source", context.Descriptor.Source);
                activity.SetTag("scraper.correlation_id", context.CorrelationId);
            }

            try
            {
                var result = await next(context).ConfigureAwait(false);
                if (activity != null)
                {
                    activity.SetTag("scraper.success", result.IsSuccess);
                    activity.SetTag("scraper.items_count", result.Data.Count);
                    if (!result.IsSuccess)
                    {
                        activity.SetStatus(ActivityStatusCode.Error, result.ErrorMessage);
                    }
                }
                return result;
            }
            catch (System.Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddEvent(new ActivityEvent("exception", tags: new ActivityTagsCollection
                {
                    { "exception.type", ex.GetType().FullName },
                    { "exception.message", ex.Message }
                }));
                throw;
            }
        }
    }
}
