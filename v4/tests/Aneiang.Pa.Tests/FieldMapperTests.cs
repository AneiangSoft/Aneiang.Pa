using Aneiang.Pa.Core.Mapping;
using FluentAssertions;
using Xunit;

namespace Aneiang.Pa.Tests;

public class FieldMapperTests
{
    public class Item
    {
        public string? Title { get; set; }
        public int Count { get; set; }
        public bool Active { get; set; }
    }

    [Fact]
    public void Map_should_set_string_int_bool_props()
    {
        var dict = new Dictionary<string, string?>
        {
            ["Title"] = "Hello",
            ["Count"] = "42",
            ["Active"] = "true"
        };
        var item = FieldMapper.Map<Item>(dict);
        item.Title.Should().Be("Hello");
        item.Count.Should().Be(42);
        item.Active.Should().BeTrue();
    }

    [Fact]
    public void Map_should_be_case_insensitive_for_keys()
    {
        var dict = new Dictionary<string, string?>
        {
            ["title"] = "lower",
            ["COUNT"] = "1"
        };
        var item = FieldMapper.Map<Item>(dict);
        item.Title.Should().Be("lower");
        item.Count.Should().Be(1);
    }

    [Fact]
    public void Map_should_skip_unknown_keys_silently()
    {
        var dict = new Dictionary<string, string?>
        {
            ["Title"] = "OK",
            ["Unknown"] = "ignored"
        };
        var item = FieldMapper.Map<Item>(dict);
        item.Title.Should().Be("OK");
    }
}
