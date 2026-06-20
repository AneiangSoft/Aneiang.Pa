using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aneiang.Pa.Abstractions;
using Aneiang.Pa.Core;
using Aneiang.Pa.Core.Mapping;

namespace Aneiang.Pa;

/// <summary>
/// 一次爬取的 Fluent 句柄
/// </summary>
public sealed class PaSource
{
    private readonly PaContainer _container;
    private readonly string _name;
    private readonly Dictionary<string, object?> _vars = new();
    private bool _bypassCache;
    private string? _proxyOverride;
    private TimeSpan? _timeoutOverride;
    private readonly HashSet<string> _skipMiddlewares = new(StringComparer.OrdinalIgnoreCase);

    internal PaSource(PaContainer container, string name)
    {
        _container = container;
        _name = name;
    }

    /// <summary>设置请求时变量（替换 URL/Header 模板用）</summary>
    public PaSource With(string key, object? value)
    {
        _vars[key] = value;
        return this;
    }

    /// <summary>分页参数</summary>
    public PaSource WithPaging(int pageNo, int pageSize)
    {
        _vars["pageNo"] = pageNo;
        _vars["pageSize"] = pageSize;
        return this;
    }

    /// <summary>本次跳过缓存</summary>
    public PaSource NoCache()
    {
        _bypassCache = true;
        _skipMiddlewares.Add("Cache");
        return this;
    }

    /// <summary>本次禁用重试</summary>
    public PaSource NoRetry()
    {
        _skipMiddlewares.Add("Retry");
        return this;
    }

    /// <summary>本次使用指定代理</summary>
    public PaSource WithProxy(string proxyUrl)
    {
        _proxyOverride = proxyUrl;
        return this;
    }

    /// <summary>覆盖单次超时</summary>
    public PaSource WithTimeout(TimeSpan timeout)
    {
        _timeoutOverride = timeout;
        return this;
    }

    /// <summary>执行爬取，返回字段字典列表</summary>
    public async Task<ScrapeResult> GetAsync(CancellationToken cancellationToken = default)
    {
        var recipe = _container.Registry.Find(_name)
            ?? throw new InvalidOperationException($"未找到 Recipe '{_name}'。请检查名称或先 Pa.Define / UseRecipesFolder。");
        var ctx = BuildContext(recipe, cancellationToken);
        return await _container.Runner.ExecuteAsync(ctx).ConfigureAwait(false);
    }

    /// <summary>执行爬取并映射为强类型 T</summary>
    public async Task<ScrapeResult<T>> GetAsync<T>(CancellationToken cancellationToken = default) where T : class, new()
    {
        var raw = await GetAsync(cancellationToken).ConfigureAwait(false);
        if (!raw.IsSuccess) return ScrapeResult<T>.Fail(raw.ErrorMessage ?? "未知错误", raw.Metadata);
        var data = raw.Data.Select(FieldMapper.Map<T>).ToArray();
        return ScrapeResult<T>.Ok(data, raw.Metadata);
    }

    /// <summary>异步流式枚举（M0 阶段为简单包装，M3 引入分页）</summary>
    public async IAsyncEnumerable<IReadOnlyDictionary<string, string?>> StreamAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var result = await GetAsync(ct).ConfigureAwait(false);
        foreach (var item in result.Data) yield return item;
    }

    private ScrapeContext BuildContext(ScraperRecipe recipe, CancellationToken ct)
    {
        var ctx = new ScrapeContext(recipe, ct)
        {
            BypassCache = _bypassCache
        };
        foreach (var kv in _vars) ctx.Variables[kv.Key] = kv.Value;
        foreach (var name in _skipMiddlewares) ctx.SkipMiddlewares.Add(name);
        if (_proxyOverride != null) ctx.Items["__proxy_override__"] = _proxyOverride;
        if (_timeoutOverride != null) ctx.Items["__timeout_override__"] = _timeoutOverride;
        return ctx;
    }
}
