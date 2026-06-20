using Aneiang.Pa.Core.Proxy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Aneiang.Pa.Core.Tests;

public class ProxyPoolTests
{
    private static DefaultProxyPool CreatePool(params string[] proxies)
    {
        var opt = Options.Create(new ProxyPoolOptions
        {
            Enabled = true,
            Proxies = new System.Collections.Generic.List<string>(proxies),
            Strategy = ProxySelectionStrategy.RoundRobin
        });
        return new DefaultProxyPool(opt);
    }

    [Fact]
    public void RoundRobin_iterates_through_proxies()
    {
        var pool = CreatePool("http://a:1", "http://b:2", "http://c:3");
        var first = pool.GetNextEntry();
        var second = pool.GetNextEntry();
        var third = pool.GetNextEntry();
        var fourth = pool.GetNextEntry();
        new[] { first!.Uri.Authority, second!.Uri.Authority, third!.Uri.Authority }
            .Should().BeEquivalentTo(new[] { "a:1", "b:2", "c:3" });
        fourth!.Uri.Authority.Should().Be(first.Uri.Authority);
    }

    [Fact]
    public void Failure_threshold_should_ban_proxy_temporarily()
    {
        var pool = CreatePool("http://a:1", "http://b:2");
        pool.ConsecutiveFailureThreshold = 2;
        var entryA = pool.GetNextEntry()!;
        pool.ReportFailure(entryA);
        pool.ReportFailure(entryA);

        var snapshot = pool.GetHealthSnapshot();
        snapshot.Should().Contain(s => s.Uri.Contains("a:1") && !s.IsAvailable);
    }

    [Fact]
    public void ReportSuccess_resets_consecutive_failures()
    {
        var pool = CreatePool("http://a:1");
        pool.ConsecutiveFailureThreshold = 3;
        var entry = pool.GetNextEntry()!;
        pool.ReportFailure(entry);
        pool.ReportSuccess(entry);
        entry.ConsecutiveFailures.Should().Be(0);
    }
}
