using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Aneiang.Pa.Abstractions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Aneiang.Pa.Core.Recipes;

/// <summary>YAML Recipe Provider（从文件夹/嵌入资源加载）</summary>
public sealed class YamlRecipeProvider : IRecipeProvider
{
    private readonly Func<IEnumerable<(string Source, string Yaml)>> _source;
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>初始化</summary>
    public YamlRecipeProvider(string name, Func<IEnumerable<(string, string)>> source)
    {
        Name = name;
        _source = source;
    }

    /// <inheritdoc />
    public IEnumerable<ScraperRecipe> GetRecipes()
    {
        foreach (var (origin, yaml) in _source())
        {
            ScraperRecipe? recipe = null;
            try
            {
                var dto = Deserializer.Deserialize<RecipeDto>(yaml);
                recipe = dto?.ToRecipe();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[YamlRecipeProvider] 解析失败 {origin}: {ex.Message}");
            }
            if (recipe != null) yield return recipe;
        }
    }

    /// <summary>从文件夹加载（递归 *.yaml/*.yml）</summary>
    public static YamlRecipeProvider FromFolder(string folder)
        => new($"folder:{folder}", () =>
        {
            if (!Directory.Exists(folder)) return Enumerable.Empty<(string, string)>();
            return Directory.EnumerateFiles(folder, "*.y*ml", SearchOption.AllDirectories)
                .Select(f => (f, File.ReadAllText(f)));
        });

    /// <summary>从指定程序集嵌入资源加载（资源名以 .yaml 结尾）</summary>
    public static YamlRecipeProvider FromEmbeddedResources(Assembly assembly)
        => new($"embedded:{assembly.GetName().Name}", () =>
        {
            var list = new List<(string, string)>();
            foreach (var name in assembly.GetManifestResourceNames())
            {
                if (!(name.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase)
                    || name.EndsWith(".yml", StringComparison.OrdinalIgnoreCase))) continue;
                using var s = assembly.GetManifestResourceStream(name);
                if (s == null) continue;
                using var r = new StreamReader(s);
                list.Add((name, r.ReadToEnd()));
            }
            return list;
        });
}

/// <summary>YAML 反序列化 DTO</summary>
internal sealed class RecipeDto
{
    public string? Name { get; set; }
    public string? Category { get; set; }
    public string? DisplayName { get; set; }
    public FetchDto? Fetch { get; set; }
    public ParseDto? Parse { get; set; }

    public ScraperRecipe? ToRecipe()
    {
        if (string.IsNullOrWhiteSpace(Name)) return null;
        return new ScraperRecipe
        {
            Name = Name!,
            Category = Category,
            DisplayName = DisplayName,
            Fetch = Fetch?.ToFetch() ?? new FetchSpec(),
            Parse = Parse?.ToParse() ?? new HtmlParseSpec()
        };
    }
}

internal sealed class FetchDto
{
    public string? Method { get; set; }
    public string? Url { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public string? Body { get; set; }
    public string? ContentType { get; set; }
    public TimeSpan? Timeout { get; set; }
    public bool? UseBrowser { get; set; }

    public FetchSpec ToFetch() => new()
    {
        Method = Method ?? "GET",
        Url = Url ?? "",
        Headers = Headers ?? new Dictionary<string, string>(),
        Body = Body,
        ContentType = ContentType ?? "application/json",
        Timeout = Timeout,
        UseBrowser = UseBrowser ?? false
    };
}

internal sealed class ParseDto
{
    public string? Type { get; set; }
    public string? Container { get; set; }
    public string? ItemsPath { get; set; }
    public string? Pattern { get; set; }
    public string? ExtractPattern { get; set; }
    public string? FilterField { get; set; }
    public string? FilterEqualsValue { get; set; }
    public Dictionary<string, FieldDto>? Fields { get; set; }
    public Dictionary<string, string>? Groups { get; set; }

    public ParseSpec ToParse()
    {
        var fields = Fields?.ToDictionary(kv => kv.Key, kv => kv.Value.ToField())
                     ?? new Dictionary<string, FieldSpec>();
        return Type?.ToLowerInvariant() switch
        {
            "json" => new JsonParseSpec { ItemsPath = ItemsPath ?? "$", Fields = fields },
            "regex" => new RegexParseSpec { Pattern = Pattern ?? "", Groups = Groups ?? new() },
            "embedded" => new EmbeddedJsonParseSpec
            {
                ExtractPattern = ExtractPattern ?? "",
                ItemsPath = ItemsPath ?? "$",
                Fields = fields,
                FilterField = FilterField,
                FilterEqualsValue = FilterEqualsValue
            },
            _ => new HtmlParseSpec { Container = Container, Fields = fields }
        };
    }
}

internal sealed class FieldDto
{
    public string? Selector { get; set; }
    public string? Attr { get; set; }
    public bool? Trim { get; set; }
    public bool? Collapse { get; set; }
    public string? Base { get; set; }
    public string? Regex { get; set; }
    public bool? UrlEncode { get; set; }
    public string? Format { get; set; }
    public Dictionary<string, string>? Map { get; set; }
    public string? Default { get; set; }

    public FieldSpec ToField() => new()
    {
        Selector = Selector ?? "",
        Attr = Attr,
        Trim = Trim ?? true,
        Collapse = Collapse ?? true,
        Base = Base,
        Regex = Regex,
        UrlEncode = UrlEncode ?? false,
        Format = Format,
        Map = Map,
        Default = Default
    };
}
