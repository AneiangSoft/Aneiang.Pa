using System;
using System.Collections.Generic;

namespace Aneiang.Pa.Abstractions;

/// <summary>统一爬取结果</summary>
public sealed class ScrapeResult<T> where T : class
{
    /// <summary>是否成功</summary>
    public bool IsSuccess { get; init; }

    /// <summary>错误消息</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedTime { get; init; } = DateTime.Now;

    /// <summary>数据列表</summary>
    public IReadOnlyList<T> Data { get; init; } = Array.Empty<T>();

    /// <summary>元数据（如来源 URL、耗时、命中缓存等）</summary>
    public IDictionary<string, object?>? Metadata { get; init; }

    /// <summary>创建成功结果</summary>
    public static ScrapeResult<T> Ok(IReadOnlyList<T> data, IDictionary<string, object?>? meta = null)
        => new() { IsSuccess = true, Data = data, Metadata = meta, UpdatedTime = DateTime.Now };

    /// <summary>创建失败结果</summary>
    public static ScrapeResult<T> Fail(string message, IDictionary<string, object?>? meta = null)
        => new() { IsSuccess = false, ErrorMessage = message, Metadata = meta, UpdatedTime = DateTime.Now };
}

/// <summary>非泛型结果（保存为字典数据）</summary>
public sealed class ScrapeResult
{
    /// <summary>是否成功</summary>
    public bool IsSuccess { get; init; }

    /// <summary>错误消息</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>更新时间</summary>
    public DateTime UpdatedTime { get; init; } = DateTime.Now;

    /// <summary>原始字段字典列表（解析后未映射）</summary>
    public IReadOnlyList<IReadOnlyDictionary<string, string?>> Data { get; init; } =
        Array.Empty<IReadOnlyDictionary<string, string?>>();

    /// <summary>元数据</summary>
    public IDictionary<string, object?>? Metadata { get; init; }

    /// <summary>创建成功结果</summary>
    public static ScrapeResult Ok(IReadOnlyList<IReadOnlyDictionary<string, string?>> data, IDictionary<string, object?>? meta = null)
        => new() { IsSuccess = true, Data = data, Metadata = meta };

    /// <summary>创建失败结果</summary>
    public static ScrapeResult Fail(string message, IDictionary<string, object?>? meta = null)
        => new() { IsSuccess = false, ErrorMessage = message, Metadata = meta };
}
