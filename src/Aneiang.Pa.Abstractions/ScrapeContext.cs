using System;
using System.Collections.Generic;
using System.Threading;

namespace Aneiang.Pa.Abstractions;

/// <summary>
/// 单次爬取的执行上下文，在管道中流转
/// </summary>
public sealed class ScrapeContext
{
    /// <summary>当前 Recipe</summary>
    public ScraperRecipe Recipe { get; }

    /// <summary>关联 ID（用于日志/追踪）</summary>
    public string CorrelationId { get; }

    /// <summary>取消令牌</summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>请求时变量（替换 URL 模板用）</summary>
    public IDictionary<string, object?> Variables { get; } = new Dictionary<string, object?>();

    /// <summary>中间件之间共享数据</summary>
    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>();

    /// <summary>临时覆盖：禁用缓存</summary>
    public bool BypassCache { get; set; }

    /// <summary>临时覆盖：跳过指定中间件</summary>
    public ISet<string> SkipMiddlewares { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>初始化上下文</summary>
    public ScrapeContext(ScraperRecipe recipe, CancellationToken ct, string? correlationId = null)
    {
        Recipe = recipe ?? throw new ArgumentNullException(nameof(recipe));
        CancellationToken = ct;
        CorrelationId = correlationId ?? Guid.NewGuid().ToString("N");
    }
}
