using Aneiang.Pa.Abstractions;
using Aneiang.Pa.Core;
using FluentAssertions;
using Xunit;

namespace Aneiang.Pa.Tests;

public class RecipeRegistryTests
{
    [Fact]
    public void Register_and_Find_should_be_case_insensitive()
    {
        var reg = new RecipeRegistry();
        reg.Register(new ScraperRecipe { Name = "WeiBo" });
        reg.Find("weibo").Should().NotBeNull();
        reg.Find("WEIBO").Should().NotBeNull();
        reg.Find("MISSING").Should().BeNull();
    }

    [Fact]
    public void Register_should_overwrite_same_name()
    {
        var reg = new RecipeRegistry();
        reg.Register(new ScraperRecipe { Name = "X", Category = "v1" });
        reg.Register(new ScraperRecipe { Name = "X", Category = "v2" });
        reg.Find("X")!.Category.Should().Be("v2");
    }

    [Fact]
    public void All_returns_snapshot()
    {
        var reg = new RecipeRegistry();
        reg.Register(new ScraperRecipe { Name = "A" });
        reg.Register(new ScraperRecipe { Name = "B" });
        reg.All().Should().HaveCount(2);
    }
}
