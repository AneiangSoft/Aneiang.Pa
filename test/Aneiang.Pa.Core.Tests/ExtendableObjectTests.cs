using Aneiang.Pa.Core.Data;
using FluentAssertions;
using Xunit;

namespace Aneiang.Pa.Core.Tests;

public class ExtendableObjectTests
{
    private class Foo : IExtendableObject
    {
        public string ExtensionData { get; set; } = string.Empty;
    }

    [Fact]
    public void SetProperty_then_GetProperty_should_round_trip()
    {
        var foo = new Foo();
        foo.SetProperty("Name", "alice");
        foo.SetProperty("Age", 30);

        foo.GetProperty<string>("Name").Should().Be("alice");
        foo.GetProperty<int>("Age").Should().Be(30);
    }

    [Fact]
    public void SetProperty_should_update_existing_key_without_loss()
    {
        var foo = new Foo();
        foo.SetProperty("A", 1);
        foo.SetProperty("B", 2);
        foo.SetProperty("A", 100);

        foo.GetProperty<int>("A").Should().Be(100);
        foo.GetProperty<int>("B").Should().Be(2);
    }

    [Fact]
    public void GetProperty_for_missing_key_returns_default()
    {
        var foo = new Foo();
        foo.SetProperty("A", 1);
        foo.GetProperty<string>("NotExists").Should().BeNull();
        foo.GetProperty<int>("NotExists").Should().Be(0);
    }

    [Fact]
    public void SetOriginal_and_GetOriginal_should_round_trip()
    {
        var foo = new Foo();
        foo.SetOriginal(new { A = 1, B = "hi" });
        var raw = foo.GetProperty<System.Text.Json.JsonElement>("Original");
        raw.GetProperty("A").GetInt32().Should().Be(1);
        raw.GetProperty("B").GetString().Should().Be("hi");
    }
}
