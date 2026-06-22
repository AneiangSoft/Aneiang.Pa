using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Aneiang.Pa.Abstractions;

namespace Aneiang.Pa.Core.Recipes;

/// <summary>JSON Recipe Provider</summary>
public sealed class JsonRecipeProvider : IRecipeProvider
{
    private readonly Func<IEnumerable<(string Source, string Json)>> _source;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>初始化</summary>
    public JsonRecipeProvider(string name, Func<IEnumerable<(string, string)>> source)
    {
        Name = name;
        _source = source;
    }

    /// <inheritdoc />
    public IEnumerable<ScraperRecipe> GetRecipes()
    {
        foreach (var (origin, json) in _source())
        {
            ScraperRecipe? recipe = null;
            try
            {
                var dto = JsonSerializer.Deserialize<RecipeDto>(json, JsonOpts);
                recipe = dto?.ToRecipe();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[JsonRecipeProvider] 解析失败 {origin}: {ex.Message}");
            }
            if (recipe != null) yield return recipe;
        }
    }

    /// <summary>从文件夹加载</summary>
    public static JsonRecipeProvider FromFolder(string folder)
        => new($"folder:{folder}", () =>
        {
            if (!Directory.Exists(folder)) return Enumerable.Empty<(string, string)>();
            return Directory.EnumerateFiles(folder, "*.json", SearchOption.AllDirectories)
                .Select(f => (f, File.ReadAllText(f)));
        });
}
