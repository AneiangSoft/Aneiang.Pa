using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aneiang.Pa.Abstractions;

namespace Aneiang.Pa.Core;

/// <summary>
/// 核心执行器：把 Recipe 通过中间件管道跑完
/// </summary>
public interface IScrapeRunner
{
    /// <summary>执行单次爬取（返回字段字典）</summary>
    Task<ScrapeResult> ExecuteAsync(ScrapeContext context);
}

/// <summary>默认实现</summary>
public sealed class ScrapeRunner : IScrapeRunner
{
    private readonly IReadOnlyList<IScrapeMiddleware> _middlewares;
    private readonly ScrapeDelegate _terminal;

    /// <summary>初始化执行器</summary>
    public ScrapeRunner(IEnumerable<IScrapeMiddleware> middlewares, ScrapeDelegate terminal)
    {
        _middlewares = middlewares.OrderBy(m => m.Order).ToArray();
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
    }

    /// <inheritdoc />
    public Task<ScrapeResult> ExecuteAsync(ScrapeContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        ScrapeDelegate next = _terminal;
        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            var mw = _middlewares[i];
            if (context.SkipMiddlewares.Contains(mw.Name)) continue;
            var inner = next;
            next = ctx => mw.InvokeAsync(ctx, inner);
        }
        return next(context);
    }
}
