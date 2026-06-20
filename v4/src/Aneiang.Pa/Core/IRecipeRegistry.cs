using System;
using System.Collections.Generic;
using System.Linq;
using Aneiang.Pa.Abstractions;

namespace Aneiang.Pa.Core;

/// <summary>
/// Recipe 注册表（线程安全）
/// </summary>
public interface IRecipeRegistry
{
    /// <summary>添加或覆盖 Recipe</summary>
    void Register(ScraperRecipe recipe);

    /// <summary>按名称查找（不区分大小写）</summary>
    ScraperRecipe? Find(string name);

    /// <summary>所有 Recipe</summary>
    IReadOnlyCollection<ScraperRecipe> All();
}

/// <summary>默认实现（基于 Dictionary）</summary>
public sealed class RecipeRegistry : IRecipeRegistry
{
    private readonly Dictionary<string, ScraperRecipe> _recipes
        = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();

    /// <inheritdoc />
    public void Register(ScraperRecipe recipe)
    {
        if (recipe == null) throw new ArgumentNullException(nameof(recipe));
        if (string.IsNullOrWhiteSpace(recipe.Name)) throw new ArgumentException("Recipe.Name 不能为空");
        lock (_lock) _recipes[recipe.Name] = recipe;
    }

    /// <inheritdoc />
    public ScraperRecipe? Find(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        lock (_lock) return _recipes.TryGetValue(name, out var r) ? r : null;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<ScraperRecipe> All()
    {
        lock (_lock) return _recipes.Values.ToArray();
    }
}
