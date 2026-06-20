using Aneiang.Pa;
using Aneiang.Pa.Abstractions;
using FluentAssertions;
using Xunit;

namespace Aneiang.Pa.Tests;

public class RecipeBuilderTests
{
    [Fact]
    public void Build_should_produce_recipe_with_get_url()
    {
        var recipe = new RecipeBuilder("Test")
            .Category("Demo")
            .Get("https://example.com")
            .Header("X-Foo", "Bar")
            .ParseHtml(p => p
                .Container("article")
                .Field("title", "h2"))
            .Build();

        recipe.Name.Should().Be("Test");
        recipe.Category.Should().Be("Demo");
        recipe.Fetch.Method.Should().Be("GET");
        recipe.Fetch.Url.Should().Be("https://example.com");
        recipe.Fetch.Headers.Should().ContainKey("X-Foo");
        recipe.Parse.Should().BeOfType<HtmlParseSpec>();
        ((HtmlParseSpec)recipe.Parse).Container.Should().Be("article");
        ((HtmlParseSpec)recipe.Parse).Fields.Should().ContainKey("title");
    }

    [Fact]
    public void Build_should_support_post_with_body()
    {
        var recipe = new RecipeBuilder("Api")
            .Post("https://api/x", body: "{\"a\":1}")
            .ParseJson(p => p.Items("$.data").Field("name", "name"))
            .Build();

        recipe.Fetch.Method.Should().Be("POST");
        recipe.Fetch.Body.Should().Be("{\"a\":1}");
        recipe.Parse.Should().BeOfType<JsonParseSpec>();
    }
}
