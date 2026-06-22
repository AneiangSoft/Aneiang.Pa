using Aneiang.Pa;
using FluentAssertions;
using Xunit;

namespace Aneiang.Pa.Tests;

public class PaFacadeTests
{
    [Fact]
    public void Sources_should_include_built_in_recipes()
    {
        // 注：默认容器，包含嵌入的 BuiltInRecipes
        var sources = Pa.Sources();
        sources.Should().NotBeEmpty();
        sources.Select(r => r.Name).Should().Contain("WeiBo");
    }

    [Fact]
    public void Define_with_builder_should_register_recipe()
    {
        Pa.Define("Test.Builder", b => b
            .Category("Demo")
            .Get("https://example.com")
            .ParseHtml(p => p.Container("body")));

        Pa.Sources().Select(r => r.Name).Should().Contain("Test.Builder");
        var src = Pa.Source("Test.Builder");
        src.Should().NotBeNull();
    }

    [Fact]
    public void Source_with_unknown_name_should_throw_when_executed()
    {
        var act = async () => await Pa.Source("__nonexistent__").GetAsync();
        act.Should().ThrowAsync<InvalidOperationException>();
    }
}
