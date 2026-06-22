using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Aneiang.Pa.Abstractions;

/// <summary>
/// 抓取规格
/// </summary>
public sealed class FetchSpec
{
    /// <summary>HTTP 方法（默认 GET）</summary>
    public string Method { get; init; } = "GET";

    /// <summary>URL 模板，支持 {pageNo}/{pageSize}/{var:xxx} 占位</summary>
    public string Url { get; init; } = "";

    /// <summary>请求头</summary>
    public IDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();

    /// <summary>请求体（POST/PUT 用）</summary>
    public string? Body { get; init; }

    /// <summary>请求体 ContentType（默认 application/json）</summary>
    public string ContentType { get; init; } = "application/json";

    /// <summary>单次超时；null 表示走全局默认</summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>是否使用浏览器渲染（需 Aneiang.Pa.Browser 包）</summary>
    public bool UseBrowser { get; init; }
}
