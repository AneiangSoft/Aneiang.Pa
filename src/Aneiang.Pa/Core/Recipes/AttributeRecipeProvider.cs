using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aneiang.Pa.Abstractions;

namespace Aneiang.Pa.Core.Recipes;

/// <summary>从特性标注的类型自动构造 Recipe</summary>
public sealed class AttributeRecipeProvider : IRecipeProvider
{
    private readonly IEnumerable<Assembly> _assemblies;

    /// <inheritdoc />
    public string Name => "Attributes";

    /// <summary>从程序集集合加载</summary>
    public AttributeRecipeProvider(IEnumerable<Assembly> assemblies)
    {
        _assemblies = assemblies;
    }

    /// <summary>从当前 AppDomain 加载所有已加载程序集</summary>
    public static AttributeRecipeProvider FromLoadedAssemblies()
        => new(AppDomain.CurrentDomain.GetAssemblies());

    /// <inheritdoc />
    public IEnumerable<ScraperRecipe> GetRecipes()
    {
        foreach (var asm in _assemblies)
        {
            Type[] types;
            try { types = asm.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray()!; }
            catch { continue; }

            foreach (var type in types)
            {
                var recipeAttr = type.GetCustomAttribute<RecipeAttribute>();
                if (recipeAttr == null) continue;

                var recipe = BuildRecipe(type, recipeAttr);
                if (recipe != null) yield return recipe;
            }
        }
    }

    private static ScraperRecipe? BuildRecipe(Type type, RecipeAttribute recipeAttr)
    {
        FetchSpec fetch;
        var get = type.GetCustomAttribute<GetAttribute>();
        var post = type.GetCustomAttribute<PostAttribute>();
        if (get != null)
        {
            fetch = new FetchSpec { Method = "GET", Url = get.Url };
        }
        else if (post != null)
        {
            fetch = new FetchSpec { Method = "POST", Url = post.Url, Body = post.Body };
        }
        else
        {
            return null;
        }

        var container = type.GetCustomAttribute<ContainerAttribute>();

        var fields = new Dictionary<string, FieldSpec>();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var sel = prop.GetCustomAttribute<SelectorAttribute>();
            if (sel == null) continue;
            fields[prop.Name] = new FieldSpec
            {
                Selector = sel.Selector,
                Attr = sel.Attr,
                Trim = sel.Trim,
                Collapse = sel.Collapse,
                Base = sel.Base,
                Regex = sel.Regex
            };
        }

        return new ScraperRecipe
        {
            Name = recipeAttr.Name,
            Category = recipeAttr.Category,
            DisplayName = recipeAttr.DisplayName,
            Fetch = fetch,
            Parse = new HtmlParseSpec { Container = container?.Selector, Fields = fields }
        };
    }
}
