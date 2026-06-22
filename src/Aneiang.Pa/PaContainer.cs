using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Aneiang.Pa.Abstractions;
using Aneiang.Pa.Core;
using Aneiang.Pa.Core.Fetching;
using Aneiang.Pa.Core.Middlewares;
using Aneiang.Pa.Core.Parsing;
using Aneiang.Pa.Core.Recipes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aneiang.Pa;

/// <summary>
/// Pa 内部容器：把所有依赖打包好。可以基于内嵌默认实现或外部 ServiceProvider 构造。
/// </summary>
public sealed class PaContainer
{
    /// <summary>选项</summary>
    public PaOptions Options { get; }
    /// <summary>注册表</summary>
    public IRecipeRegistry Registry { get; }
    /// <summary>执行器</summary>
    public IScrapeRunner Runner { get; }

    private PaContainer(PaOptions opts, IRecipeRegistry registry, IScrapeRunner runner)
    {
        Options = opts;
        Registry = registry;
        Runner = runner;
    }

    /// <summary>创建默认容器（自带 HttpClient、内置 Recipe）</summary>
    public static PaContainer CreateDefault() => CreateCustom(new PaOptions(), null, null, null);

    /// <summary>使用自定义配置构造</summary>
    internal static PaContainer CreateCustom(
        PaOptions options,
        IEnumerable<IRecipeProvider>? extraProviders,
        IEnumerable<IScrapeMiddleware>? extraMiddlewares,
        Func<HttpMessageHandler>? httpHandlerFactory)
    {
        var services = new ServiceCollection();
        services.AddSingleton(options);

        // HttpClient
        var httpBuilder = services.AddHttpClient(PaConsts.HttpClientName)
            .ConfigureHttpClient(c => c.Timeout = options.DefaultTimeout);
        if (httpHandlerFactory != null)
            httpBuilder.ConfigurePrimaryHttpMessageHandler(httpHandlerFactory);

        services.AddSingleton<IFetcher, HttpFetcher>();
        services.AddSingleton<IResponseParser, HtmlResponseParser>();
        services.AddSingleton<IResponseParser, JsonResponseParser>();
        services.AddSingleton<IResponseParser, RegexResponseParser>();
        services.AddSingleton<IResponseParser, EmbeddedJsonResponseParser>();
        services.AddSingleton<FetchParseTerminal>();

        services.AddSingleton<IRecipeRegistry, RecipeRegistry>();

        // 默认日志：若未注入则用 Null
        services.AddLogging();

        // 默认中间件
        services.AddMemoryCache();
        services.AddSingleton<IScrapeMiddleware, LoggingMiddleware>();
        if (options.EnableMetrics)
            services.AddSingleton<IScrapeMiddleware, MetricsMiddleware>();
        if (options.EnableTracing)
            services.AddSingleton<IScrapeMiddleware, TracingMiddleware>();
        services.AddSingleton<IScrapeMiddleware, CacheMiddleware>();
        services.AddSingleton<IScrapeMiddleware, CircuitBreakerMiddleware>();
        services.AddSingleton<IScrapeMiddleware, RetryMiddleware>();
        services.AddSingleton<IScrapeMiddleware, TimeoutMiddleware>();

        // 站点特化中间件（对特定 Recipe 透明生效）
        services.AddSingleton<IScrapeMiddleware, DouYinCookieMiddleware>();

        // 用户额外中间件
        if (extraMiddlewares != null)
            foreach (var mw in extraMiddlewares)
                services.AddSingleton(mw);

        var sp = services.BuildServiceProvider();

        // 加载 Recipe Providers
        var registry = sp.GetRequiredService<IRecipeRegistry>();
        var providers = new List<IRecipeProvider>();
        if (options.LoadBuiltInRecipes)
            providers.Add(YamlRecipeProvider.FromEmbeddedResources(typeof(PaContainer).Assembly));
        if (!string.IsNullOrWhiteSpace(options.RecipesFolder))
            providers.Add(YamlRecipeProvider.FromFolder(options.RecipesFolder!));
        if (extraProviders != null) providers.AddRange(extraProviders);
        foreach (var p in providers)
            foreach (var r in p.GetRecipes())
                registry.Register(r);

        // 终端
        var terminal = sp.GetRequiredService<FetchParseTerminal>().AsDelegate();

        // Runner
        var middlewares = sp.GetServices<IScrapeMiddleware>();
        IScrapeRunner runner = new ScrapeRunner(middlewares, terminal);

        return new PaContainer(options, registry, runner);
    }

    /// <summary>从外部 IServiceProvider 接管</summary>
    public static PaContainer FromServiceProvider(IServiceProvider sp)
    {
        var options = sp.GetService<IOptions<PaOptions>>()?.Value
                     ?? sp.GetService<PaOptions>()
                     ?? new PaOptions();

        var registry = sp.GetRequiredService<IRecipeRegistry>();
        var terminal = sp.GetRequiredService<FetchParseTerminal>().AsDelegate();
        var middlewares = sp.GetServices<IScrapeMiddleware>();
        IScrapeRunner runner = new ScrapeRunner(middlewares, terminal);
        return new PaContainer(options, registry, runner);
    }
}
