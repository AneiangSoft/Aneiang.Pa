using Aneiang.Pa.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Aneiang.Pa.Core.Pipeline.Middlewares
{
    /// <summary>
    /// 重试中间件：失败时按指数退避重试
    /// </summary>
    public class RetryMiddleware : IScrapeMiddleware
    {
        private readonly IOptionsMonitor<ScrapeResilienceOptions> _options;
        private readonly ILogger<RetryMiddleware>? _logger;

        /// <inheritdoc />
        public int Order => 60;

        /// <summary>
        /// 初始化重试中间件
        /// </summary>
        public RetryMiddleware(IOptionsMonitor<ScrapeResilienceOptions> options, ILogger<RetryMiddleware>? logger = null)
        {
            _options = options;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<ScraperResult<T>> InvokeAsync<T>(ScrapeContext context, ScrapeDelegate<T> next) where T : class
        {
            var policy = _options.CurrentValue.GetFor(context.Descriptor);
            var maxAttempts = System.Math.Max(1, policy.RetryCount + 1);
            var backoffs = policy.RetryBackoffMs ?? new[] { 500, 1500, 3000 };

            ScraperResult<T>? last = null;
            Exception? lastEx = null;
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var result = await next(context).ConfigureAwait(false);
                    if (result.IsSuccess) return result;
                    last = result;
                }
                catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    _logger?.LogWarning(ex, "[Scrape] Retry attempt {Attempt}/{Max} {Category}/{Source}",
                        attempt, maxAttempts, context.Descriptor.Category, context.Descriptor.Source);
                }

                if (attempt < maxAttempts)
                {
                    var idx = System.Math.Min(attempt - 1, backoffs.Length - 1);
                    await Task.Delay(backoffs[idx], context.CancellationToken).ConfigureAwait(false);
                }
            }

            if (lastEx != null)
                return ScraperResult<T>.Failure($"重试 {maxAttempts} 次后仍失败: {lastEx.Message}");
            return last ?? ScraperResult<T>.Failure("未知错误");
        }
    }
}
