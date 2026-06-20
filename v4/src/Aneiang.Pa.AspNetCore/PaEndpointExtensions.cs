using System;
using System.Linq;
using Aneiang.Pa.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Aneiang.Pa.AspNetCore;

/// <summary>Pa ASP.NET Core 集成扩展</summary>
public static class PaEndpointExtensions
{
    /// <summary>注册 Pa（在 builder.Services 阶段调用）</summary>
    public static IServiceCollection AddPa(this IServiceCollection services, Action<PaOptions>? configure = null)
    {
        var opts = new PaOptions();
        configure?.Invoke(opts);
        Pa.Configure(c =>
        {
            c.Options.EnableLogging = opts.EnableLogging;
            c.Options.EnableMetrics = opts.EnableMetrics;
            c.Options.EnableTracing = opts.EnableTracing;
            c.Options.DefaultTimeout = opts.DefaultTimeout;
            c.Options.DefaultRetryCount = opts.DefaultRetryCount;
            c.Options.DefaultCacheDuration = opts.DefaultCacheDuration;
            c.Options.RecipesFolder = opts.RecipesFolder;
            c.Options.LoadBuiltInRecipes = opts.LoadBuiltInRecipes;
        });
        services.AddSingleton(opts);
        return services;
    }

    /// <summary>映射所有 Pa API 端点（GET /pa/sources / /pa/source/{name} / /pa/health）</summary>
    public static IEndpointRouteBuilder MapPaApi(this IEndpointRouteBuilder app, string prefix = "/api/pa")
    {
        var p = prefix.TrimEnd('/');

        // 列出所有 Recipe
        app.MapGet($"{p}/sources", () =>
        {
            var sources = Pa.Sources();
            return Results.Ok(new
            {
                count = sources.Count,
                items = sources.Select(s => new
                {
                    name = s.Name,
                    category = s.Category,
                    displayName = s.DisplayName
                })
            });
        });

        // 抓取
        app.MapGet($"{p}/source/{{name}}", async (string name, HttpContext http) =>
        {
            var noCache = http.Request.Query.ContainsKey("noCache");
            var src = Pa.Source(name);
            if (noCache) src = src.NoCache();
            var result = await src.GetAsync(http.RequestAborted);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.Json(result, statusCode: 500);
        });

        // 健康检查
        app.MapGet($"{p}/health", () =>
        {
            var sources = Pa.Sources();
            return Results.Ok(new { ok = true, recipeCount = sources.Count });
        });

        return app;
    }
}
