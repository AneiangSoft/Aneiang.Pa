using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Aneiang.Pa.Abstractions;
using Aneiang.Pa.Core;
using Aneiang.Pa.Core.Fetching;
using Aneiang.Pa.Core.Parsing;
using Aneiang.Pa.Core.Recipes;

namespace Aneiang.Pa;

/// <summary>
/// Aneiang.Pa 4.0 静态门面：极简使用入口
/// <code>
/// var data = await Pa.Source("WeiBo").GetAsync();
/// </code>
/// </summary>
public static class Pa
{
    private static PaContainer _container = PaContainer.CreateDefault();
    private static readonly object _lock = new();

    /// <summary>
    /// 重新初始化（一般用于配置外部依赖）。
    /// 不调用此方法时，使用默认容器（自带 HttpClient + 内置 Recipe）。
    /// </summary>
    public static void Configure(Action<PaConfigurationBuilder> configure)
    {
        var builder = new PaConfigurationBuilder();
        configure(builder);
        lock (_lock) _container = builder.Build();
    }

    /// <summary>从指定服务提供器接管</summary>
    public static void UseServices(IServiceProvider sp)
    {
        lock (_lock) _container = PaContainer.FromServiceProvider(sp);
    }

    /// <summary>
    /// 通过 Recipe 名获取一个可执行的 Source 句柄
    /// </summary>
    public static PaSource Source(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name 不能为空", nameof(name));
        return new PaSource(_container, name);
    }

    /// <summary>动态注册一个 Recipe</summary>
    public static void Define(ScraperRecipe recipe) => _container.Registry.Register(recipe);

    /// <summary>使用 Builder DSL 内联定义 Recipe</summary>
    public static void Define(string name, Action<RecipeBuilder> configure)
    {
        var builder = new RecipeBuilder(name);
        configure(builder);
        Define(builder.Build());
    }

    /// <summary>从已加载程序集自动发现带 [Recipe] 特性的类型</summary>
    public static void DiscoverFromLoadedAssemblies()
    {
        var provider = Aneiang.Pa.Core.Recipes.AttributeRecipeProvider.FromLoadedAssemblies();
        foreach (var r in provider.GetRecipes()) Define(r);
    }

    /// <summary>列出所有已注册 Recipe</summary>
    public static IReadOnlyCollection<ScraperRecipe> Sources()
        => _container.Registry.All();

    /// <summary>获取选项</summary>
    public static PaOptions Options => _container.Options;
}

/// <summary>Pa 配置构建器</summary>
public sealed class PaConfigurationBuilder
{
    /// <summary>选项</summary>
    public PaOptions Options { get; } = new();
    private readonly List<IRecipeProvider> _providers = new();
    private readonly List<IScrapeMiddleware> _middlewares = new();
    private Func<HttpMessageHandler>? _httpHandlerFactory;

    /// <summary>追加 Recipe 提供器</summary>
    public PaConfigurationBuilder UseRecipeProvider(IRecipeProvider provider)
    {
        _providers.Add(provider);
        return this;
    }

    /// <summary>从文件夹加载 YAML</summary>
    public PaConfigurationBuilder UseRecipesFolder(string folder)
        => UseRecipeProvider(YamlRecipeProvider.FromFolder(folder));

    /// <summary>追加中间件</summary>
    public PaConfigurationBuilder UseMiddleware(IScrapeMiddleware middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }

    /// <summary>自定义 HttpClientHandler 工厂</summary>
    public PaConfigurationBuilder UseHttpHandler(Func<HttpMessageHandler> factory)
    {
        _httpHandlerFactory = factory;
        return this;
    }

    internal PaContainer Build()
    {
        return PaContainer.CreateCustom(Options, _providers, _middlewares, _httpHandlerFactory);
    }
}
