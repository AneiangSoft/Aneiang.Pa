using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Aneiang.Pa.Abstractions;
using Aneiang.Pa.Core.Parsing;
using FluentAssertions;
using Xunit;

namespace Aneiang.Pa.Tests;

public class JsonParserTests
{
    [Fact]
    public async Task Parse_should_extract_fields_from_array()
    {
        var json = @"{""data"":{""list"":[
            {""title"":""A"",""views"":100},
            {""title"":""B"",""views"":200}
        ]}}";
        var spec = new JsonParseSpec
        {
            ItemsPath = "$.data.list",
            Fields = new Dictionary<string, FieldSpec>
            {
                ["title"] = new FieldSpec { Selector = "title" },
                ["views"] = new FieldSpec { Selector = "views" }
            }
        };
        var resp = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        var result = await new JsonResponseParser().ParseAsync(resp, spec, BuildCtx());

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data[0]["title"].Should().Be("A");
        result.Data[0]["views"].Should().Be("100");
        result.Data[1]["title"].Should().Be("B");
    }

    [Fact]
    public async Task Parse_should_support_root_object_when_path_resolves_to_object()
    {
        var json = @"{""name"":""Test"",""count"":42}";
        var spec = new JsonParseSpec
        {
            ItemsPath = "$",
            Fields = new Dictionary<string, FieldSpec>
            {
                ["name"] = new FieldSpec { Selector = "name" },
                ["count"] = new FieldSpec { Selector = "count" }
            }
        };
        var resp = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        var result = await new JsonResponseParser().ParseAsync(resp, spec, BuildCtx());

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data[0]["name"].Should().Be("Test");
        result.Data[0]["count"].Should().Be("42");
    }

    private static ScrapeContext BuildCtx() => new(new ScraperRecipe { Name = "T" }, default);
}
