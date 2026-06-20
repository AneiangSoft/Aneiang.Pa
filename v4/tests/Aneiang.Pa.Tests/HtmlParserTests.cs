using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Aneiang.Pa.Abstractions;
using Aneiang.Pa.Core.Parsing;
using FluentAssertions;
using Xunit;

namespace Aneiang.Pa.Tests;

public class HtmlParserTests
{
    [Fact]
    public async Task Parse_should_extract_fields_from_container()
    {
        var html = @"<html><body>
            <article class='item'>
                <h2><a href='/a'>Hello</a></h2>
                <p>Desc A</p>
            </article>
            <article class='item'>
                <h2><a href='/b'>World</a></h2>
                <p>Desc B</p>
            </article>
        </body></html>";

        var spec = new HtmlParseSpec
        {
            Container = "article.item",
            Fields = new Dictionary<string, FieldSpec>
            {
                ["title"] = new FieldSpec { Selector = "h2 a", Trim = true, Collapse = true },
                ["url"]   = new FieldSpec { Selector = "h2 a", Attr = "href", Base = "https://example.com" },
                ["desc"]  = new FieldSpec { Selector = "p", Trim = true }
            }
        };

        var resp = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(html, Encoding.UTF8, "text/html")
        };

        var parser = new HtmlResponseParser();
        var result = await parser.ParseAsync(resp, spec, BuildCtx());

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data[0]["title"].Should().Be("Hello");
        result.Data[0]["url"].Should().Be("https://example.com/a");
        result.Data[0]["desc"].Should().Be("Desc A");
        result.Data[1]["title"].Should().Be("World");
    }

    [Fact]
    public async Task Parse_should_support_xpath_prefix()
    {
        var html = @"<root><x>1</x><x>2</x></root>";
        var spec = new HtmlParseSpec
        {
            Container = "xpath://x",
            Fields = new Dictionary<string, FieldSpec>
            {
                ["v"] = new FieldSpec { Selector = "." }
            }
        };
        var resp = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(html, Encoding.UTF8, "text/html") };
        var result = await new HtmlResponseParser().ParseAsync(resp, spec, BuildCtx());
        result.Data.Should().HaveCount(2);
        result.Data[0]["v"].Should().Be("1");
    }

    private static ScrapeContext BuildCtx() => new(new ScraperRecipe { Name = "T" }, default);
}
