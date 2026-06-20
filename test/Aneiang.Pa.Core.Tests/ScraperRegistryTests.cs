using Aneiang.Pa.Core.Models;
using Aneiang.Pa.Core.Scraper;
using FluentAssertions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Aneiang.Pa.Core.Tests;

public class ScraperRegistryTests
{
    private class News1 : IScraper<TestPayload>
    {
        public ScraperDescriptor Descriptor { get; } = new() { Category = "News", Source = "A" };
        public Task<ScraperResult<TestPayload>> ScrapeAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(ScraperResult<TestPayload>.Success(new()));
    }

    private class Lottery1 : IScraper
    {
        public ScraperDescriptor Descriptor { get; } = new() { Category = "Lottery", Source = "SSQ" };
    }

    public class TestPayload { }

    [Fact]
    public void Registry_should_index_by_category_and_source_case_insensitive()
    {
        var registry = new ScraperRegistry(new IScraper[] { new News1(), new Lottery1() });
        registry.GetScraper("news", "a").Should().NotBeNull();
        registry.GetScraper("NEWS", "A").Should().NotBeNull();
        registry.GetScraper("Lottery", "SSQ").Should().NotBeNull();
        registry.GetScraper("missing", "x").Should().BeNull();
    }

    [Fact]
    public void Registry_should_filter_by_category()
    {
        var registry = new ScraperRegistry(new IScraper[] { new News1(), new Lottery1() });
        registry.GetDescriptors("News").Should().HaveCount(1);
        registry.GetDescriptors().Should().HaveCount(2);
    }
}
