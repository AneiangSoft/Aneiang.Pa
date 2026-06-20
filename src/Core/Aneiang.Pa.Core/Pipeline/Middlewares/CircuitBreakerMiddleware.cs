using Aneiang.Pa.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Aneiang.Pa.Core.Pipeline.Middlewares
{
    /// <summary>
    /// 熔断中间件：连续失败超过阈值时打开熔断，期间快速失败
    /// </summary>
    public class CircuitBreakerMiddleware : IScrapeMiddleware
    {
        private readonly IOptionsMonitor<ScrapeResilienceOptions> _options;
        private readonly ILogger<CircuitBreakerMiddleware>? _logger;
        private static readonly ConcurrentDictionary<string, CircuitState> States = new ConcurrentDictionary<string, CircuitState>();

        /// <inheritdoc />
        public int Order => 50;

        /// <summary>
        /// 初始化熔断中间件
        /// </summary>
        public CircuitBreakerMiddleware(IOptionsMonitor<ScrapeResilienceOptions> options, ILogger<CircuitBreakerMiddleware>? logger = null)
        {
            _options = options;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<ScraperResult<T>> InvokeAsync<T>(ScrapeContext context, ScrapeDelegate<T> next) where T : class
        {
            var policy = _options.CurrentValue.GetFor(context.Descriptor);
            if (policy.CircuitBreakerFailureThreshold <= 0)
                return await next(context).ConfigureAwait(false);

            var key = $"{context.Descriptor.Category}:{context.Descriptor.Source}";
            var state = States.GetOrAdd(key, _ => new CircuitState());

            // Open 状态：检查是否到了恢复时间
            if (state.IsOpen)
            {
                if (DateTime.UtcNow < state.OpenUntilUtc)
                {
                    _logger?.LogWarning("[Scrape] CircuitBreaker open, fast fail {Key}", key);
                    return ScraperResult<T>.Failure($"熔断器已打开（{key}），快速失败");
                }
                // 进入半开
                state.IsOpen = false;
                state.HalfOpen = true;
            }

            try
            {
                var result = await next(context).ConfigureAwait(false);
                if (result.IsSuccess)
                {
                    state.FailureCount = 0;
                    state.HalfOpen = false;
                }
                else
                {
                    OnFailure(state, policy, key);
                }
                return result;
            }
            catch
            {
                OnFailure(state, policy, key);
                throw;
            }
        }

        private void OnFailure(CircuitState state, ScrapeResiliencePolicy policy, string key)
        {
            state.FailureCount++;
            if (state.FailureCount >= policy.CircuitBreakerFailureThreshold)
            {
                state.IsOpen = true;
                state.HalfOpen = false;
                state.OpenUntilUtc = DateTime.UtcNow.Add(policy.CircuitBreakerBreakDuration);
                _logger?.LogError("[Scrape] CircuitBreaker tripped for {Key} until {Until:o}", key, state.OpenUntilUtc);
            }
        }

        private class CircuitState
        {
            public int FailureCount;
            public bool IsOpen;
            public bool HalfOpen;
            public DateTime OpenUntilUtc;
        }
    }
}
