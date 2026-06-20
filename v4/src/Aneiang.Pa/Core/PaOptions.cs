using System;

namespace Aneiang.Pa.Core;

/// <summary>
/// Pa 全局选项
/// </summary>
public sealed class PaOptions
{
    /// <summary>是否启用结构化日志（默认 true）</summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>是否启用 Metrics（默认 true）</summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>是否启用 Tracing（默认 true）</summary>
    public bool EnableTracing { get; set; } = true;

    /// <summary>默认请求超时</summary>
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>默认重试次数</summary>
    public int DefaultRetryCount { get; set; } = 3;

    /// <summary>默认缓存时长（设为 Zero 即不缓存）</summary>
    public TimeSpan DefaultCacheDuration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>额外加载的 Recipe 目录（YAML/JSON）</summary>
    public string? RecipesFolder { get; set; }

    /// <summary>是否加载内置 Recipe（默认 true）</summary>
    public bool LoadBuiltInRecipes { get; set; } = true;
}
