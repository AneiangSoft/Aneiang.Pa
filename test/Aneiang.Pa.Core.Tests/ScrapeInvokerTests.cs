using Aneiang.Pa.Core.Models;
using Aneiang.Pa.Core.Pipeline;
using Aneiang.Pa.Core.Pipeline.Middlewares;
using Aneiang.Pa.Core.Scraper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Aneiang.Pa.Core.Tests;

public class ScrapeInvokerTests
{
    private class FakeScraper : IScraper<TestItem>
    {
        public ScraperDescriptor Descriptor { get; } = new()
        {
            Category = "Test", Source = "Fake", DisplayName = "Fake"
        };

        public int Calls { get; private set; }
        public CancellationToken LastToken { get; private set; }
        public Func<Task<ScraperResult<TestItem>>> Behavior { get; set; } = () => Task.FromResult(ScraperResult<TestItem>.Success(new()));

        public Task<ScraperResult<TestItem>> ScrapeAsync(CancellationToken cancellationToken = default)
        {
            Calls++;
            LastToken = cancellationToken;
            return Behavior();
        }
    }

    public class TestItem { }

    private static ScrapeInvoker CreateInvoker(params IScrapeMiddleware[] middlewares)
        => new(middlewares, new ScraperRegistry(System.Array.Empty<IScraper>()));

    [Fact]
    public async Task Pipeline_runs_middleware_in_order()
    {
        var order = new System.Collections.Generic.List<string>();
        var m1 = new TestMiddleware(0, order, "outer");
        var m2 = new TestMiddleware(50, order, "inner");
        var scraper = new FakeScraper();

        var invoker = CreateInvoker(m1, m2);
        var result = await invoker.InvokeAsync(scraper);

        result.IsSuccess.Should().BeTrue();
        order.Should().Equal("outer-pre", "inner-pre", "inner-post", "outer-post");
        scraper.Calls.Should().Be(1);
    }

    [Fact]
    public async Task RetryMiddleware_retries_on_failure()
    {
        var attempts = 0;
        var scraper = new FakeScraper
        {
            Behavior = () =>
            {
                attempts++;
                return attempts < 3
                    ? Task.FromResult(ScraperResult<TestItem>.Failure("transient"))
                    : Task.FromResult(ScraperResult<TestItem>.Success(new() { new TestItem() }));
            }
        };
        var options = OptionsMonitorFactory.Create(new ScrapeResilienceOptions
        {
            Default = new() { RetryCount = 3, RetryBackoffMs = new[] { 1, 1, 1 }, Timeout = TimeSpan.FromSeconds(5) }
        });
        var retry = new RetryMiddleware(options, NullLogger<RetryMiddleware>.Instance);

        var invoker = CreateInvoker(retry);
        var result = await invoker.InvokeAsync(scraper);

        result.IsSuccess.Should().BeTrue();
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task TimeoutMiddleware_returns_failure_on_timeout()
    {
        var scraper = new FakeScraper();
        scraper.Behavior = async () =>
        {
            await Task.Delay(2000, scraper.LastToken);
            return ScraperResult<TestItem>.Success(new());
        };
        var options = OptionsMonitorFactory.Create(new ScrapeResilienceOptions
        {
            Default = new() { Timeout = TimeSpan.FromMilliseconds(100) }
        });
        var timeout = new TimeoutMiddleware(options, NullLogger<TimeoutMiddleware>.Instance);

        var invoker = CreateInvoker(timeout);
        var result = await invoker.InvokeAsync(scraper);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("超时");
    }

    private class TestMiddleware : IScrapeMiddleware
    {
        private readonly System.Collections.Generic.List<string> _log;
        private readonly string _name;
        public int Order { get; }
        public TestMiddleware(int order, System.Collections.Generic.List<string> log, string name) { Order = order; _log = log; _name = name; }
        public async Task<ScraperResult<T>> InvokeAsync<T>(ScrapeContext ctx, ScrapeDelegate<T> next) where T : class
        {
            _log.Add($"{_name}-pre");
            var r = await next(ctx);
            _log.Add($"{_name}-post");
            return r;
        }
    }
}

internal static class OptionsMonitorFactory
{
    public static IOptionsMonitor<T> Create<T>(T value) where T : class, new()
        => new StaticOptionsMonitor<T>(value);

    private class StaticOptionsMonitor<T> : IOptionsMonitor<T> where T : class, new()
    {
        public T CurrentValue { get; }
        public StaticOptionsMonitor(T value) { CurrentValue = value; }
        public T Get(string? name) => CurrentValue;
        public IDisposable OnChange(System.Action<T, string?> listener) => new Empty();
        private class Empty : IDisposable { public void Dispose() { } }
    }
}
