using Aneiang.Pa.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aneiang.Pa.Core.Pipeline.Middlewares
{
    /// <summary>
    /// 超时中间件：为单次爬取设置硬性超时
    /// </summary>
    public class TimeoutMiddleware : IScrapeMiddleware
    {
        private readonly IOptionsMonitor<ScrapeResilienceOptions> _options;
        private readonly ILogger<TimeoutMiddleware>? _logger;

        /// <inheritdoc />
        public int Order => 70;

        /// <summary>
        /// 初始化超时中间件
        /// </summary>
        public TimeoutMiddleware(IOptionsMonitor<ScrapeResilienceOptions> options, ILogger<TimeoutMiddleware>? logger = null)
        {
            _options = options;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<ScraperResult<T>> InvokeAsync<T>(ScrapeContext context, ScrapeDelegate<T> next) where T : class
        {
            var timeout = _options.CurrentValue.GetFor(context.Descriptor).Timeout;
            if (timeout <= TimeSpan.Zero)
                return await next(context).ConfigureAwait(false);

            using var linked = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
            linked.CancelAfter(timeout);

            var newContext = new ScrapeContext(context.Descriptor, linked.Token, context.CorrelationId);
            foreach (var kv in context.Items) newContext.Items[kv.Key] = kv.Value;

            try
            {
                return await next(newContext).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (linked.IsCancellationRequested && !context.CancellationToken.IsCancellationRequested)
            {
                _logger?.LogWarning("[Scrape] Timeout {Category}/{Source} after {Timeout}",
                    context.Descriptor.Category, context.Descriptor.Source, timeout);
                return ScraperResult<T>.Failure($"操作超时（{timeout.TotalMilliseconds}ms）");
            }
        }
    }
}
