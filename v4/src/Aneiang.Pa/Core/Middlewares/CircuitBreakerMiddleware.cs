using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Aneiang.Pa.Abstractions;
using Microsoft.Extensions.Logging;

namespace Aneiang.Pa.Core.Middlewares;

/// <summary>熔断中间件：连续失败超阈值则快速失败</summary>
public sealed class CircuitBreakerMiddleware : IScrapeMiddleware
{
    private static readonly ConcurrentDictionary<string, State> States = new();
    private readonly ILogger<CircuitBreakerMiddleware>? _logger;

    /// <summary>失败阈值</summary>
    public int FailureThreshold { get; set; } = 5;

    /// <summary>熔断时长</summary>
    public TimeSpan BreakDuration { get; set; } = TimeSpan.FromMinutes(1);

    /// <inheritdoc />
    public int Order => 50;

    /// <inheritdoc />
    public string Name => "CircuitBreaker";

    /// <summary>初始化</summary>
    public CircuitBreakerMiddleware(ILogger<CircuitBreakerMiddleware>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ScrapeResult> InvokeAsync(ScrapeContext ctx, ScrapeDelegate next)
    {
        var state = States.GetOrAdd(ctx.Recipe.Name, _ => new State());
        if (state.IsOpen && DateTime.UtcNow < state.OpenUntil)
        {
            _logger?.LogWarning("[Pa] CircuitBreaker open, fast fail {Name}", ctx.Recipe.Name);
            return ScrapeResult.Fail($"熔断器已打开（{ctx.Recipe.Name}）");
        }
        if (state.IsOpen) state.IsOpen = false; // 转半开

        try
        {
            var r = await next(ctx).ConfigureAwait(false);
            if (r.IsSuccess) state.FailureCount = 0;
            else IncreaseFailure(state, ctx.Recipe.Name);
            return r;
        }
        catch
        {
            IncreaseFailure(state, ctx.Recipe.Name);
            throw;
        }
    }

    private void IncreaseFailure(State state, string name)
    {
        state.FailureCount++;
        if (state.FailureCount >= FailureThreshold)
        {
            state.IsOpen = true;
            state.OpenUntil = DateTime.UtcNow.Add(BreakDuration);
            _logger?.LogError("[Pa] CircuitBreaker tripped {Name} until {Until:o}", name, state.OpenUntil);
        }
    }

    private class State
    {
        public int FailureCount;
        public bool IsOpen;
        public DateTime OpenUntil;
    }
}
