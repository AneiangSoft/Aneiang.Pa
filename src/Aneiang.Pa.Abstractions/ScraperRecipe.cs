using System;
using System.Collections.Generic;

namespace Aneiang.Pa.Abstractions;

/// <summary>
/// 爬虫配方：声明"如何抓"和"如何解析"，而非"怎么实现"。
/// 所有 Recipe 来源（YAML / Builder / Attribute）最终都转为此结构。
/// </summary>
public sealed class ScraperRecipe
{
    /// <summary>唯一标识（如 "WeiBo" / "News.BaiDu"）</summary>
    public string Name { get; init; } = "";

    /// <summary>分组（可选，如 "News" / "Lottery"）</summary>
    public string? Category { get; init; }

    /// <summary>显示名称（可选）</summary>
    public string? DisplayName { get; init; }

    /// <summary>抓取规格</summary>
    public FetchSpec Fetch { get; init; } = new();

    /// <summary>解析规格</summary>
    public ParseSpec Parse { get; init; } = new HtmlParseSpec();

    /// <summary>各中间件配置（重试/缓存/限流等），缺省即用全局默认</summary>
    public IDictionary<string, object?> Middlewares { get; init; } = new Dictionary<string, object?>();
}
