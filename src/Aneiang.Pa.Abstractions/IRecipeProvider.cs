using System.Collections.Generic;

namespace Aneiang.Pa.Abstractions;

/// <summary>Recipe 提供器：负责从 YAML/JSON/特性/Builder 等来源加载 Recipe</summary>
public interface IRecipeProvider
{
    /// <summary>提供器名称</summary>
    string Name { get; }

    /// <summary>枚举所有可加载的 Recipe</summary>
    IEnumerable<ScraperRecipe> GetRecipes();
}
