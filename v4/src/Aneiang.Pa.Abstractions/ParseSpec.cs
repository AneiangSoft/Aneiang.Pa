using System.Collections.Generic;

namespace Aneiang.Pa.Abstractions;

/// <summary>解析规格基类</summary>
public abstract class ParseSpec
{
    /// <summary>解析器类型标识（"html" / "json" / "regex" / "xml" / "custom"）</summary>
    public abstract string Type { get; }
}

/// <summary>HTML 解析规格（基于 CSS 选择器或 XPath）</summary>
public sealed class HtmlParseSpec : ParseSpec
{
    /// <inheritdoc />
    public override string Type => "html";

    /// <summary>容器选择器（CSS 或 XPath，前缀 "xpath:" 表示 XPath）</summary>
    public string? Container { get; init; }

    /// <summary>字段选择器映射</summary>
    public IDictionary<string, FieldSpec> Fields { get; init; } = new Dictionary<string, FieldSpec>();
}

/// <summary>JSON 解析规格（基于 JSONPath）</summary>
public sealed class JsonParseSpec : ParseSpec
{
    /// <inheritdoc />
    public override string Type => "json";

    /// <summary>条目数组路径，如 "$.data.cards[0].content"</summary>
    public string ItemsPath { get; init; } = "$";

    /// <summary>字段路径映射</summary>
    public IDictionary<string, FieldSpec> Fields { get; init; } = new Dictionary<string, FieldSpec>();
}

/// <summary>正则解析规格</summary>
public sealed class RegexParseSpec : ParseSpec
{
    /// <inheritdoc />
    public override string Type => "regex";

    /// <summary>正则表达式</summary>
    public string Pattern { get; init; } = "";

    /// <summary>命名捕获组到字段名映射（默认按命名组直接映射）</summary>
    public IDictionary<string, string> Groups { get; init; } = new Dictionary<string, string>();
}

/// <summary>嵌入 JSON 解析规格（HTML 响应中正则抠出 JSON 字符串，再按 JSON 解析）</summary>
public sealed class EmbeddedJsonParseSpec : ParseSpec
{
    /// <inheritdoc />
    public override string Type => "embedded";

    /// <summary>从响应文本提取 JSON 的正则（取第一个捕获组作为 JSON 字符串）</summary>
    public string ExtractPattern { get; init; } = "";

    /// <summary>提取后的 JSON 中条目数组路径</summary>
    public string ItemsPath { get; init; } = "$";

    /// <summary>字段映射</summary>
    public IDictionary<string, FieldSpec> Fields { get; init; } = new Dictionary<string, FieldSpec>();

    /// <summary>过滤：当指定字段值等于 FilterEqualsValue 时跳过该项（用于排除置顶等）</summary>
    public string? FilterField { get; init; }

    /// <summary>跳过条件值</summary>
    public string? FilterEqualsValue { get; init; }
}

/// <summary>字段提取规格</summary>
public sealed class FieldSpec
{
    /// <summary>选择器（CSS / XPath / JSONPath）</summary>
    public string Selector { get; init; } = "";

    /// <summary>属性名（HTML 用，如 "href"）</summary>
    public string? Attr { get; init; }

    /// <summary>是否 Trim 空白</summary>
    public bool Trim { get; init; } = true;

    /// <summary>是否合并连续空白为单空格</summary>
    public bool Collapse { get; init; } = true;

    /// <summary>对相对 URL 的基础地址（用于自动补全）</summary>
    public string? Base { get; init; }

    /// <summary>正则提取：从字段值中按 Pattern 抽取（可选）</summary>
    public string? Regex { get; init; }

    /// <summary>是否对最终值进行 URL 编码（用于参与拼装 URL）</summary>
    public bool UrlEncode { get; init; }

    /// <summary>格式模板：包含 {value} 占位符，提取后用最终值替换</summary>
    public string? Format { get; init; }

    /// <summary>映射表：当字段值匹配时，替换为指定值（用于 flag 翻译等）</summary>
    public IDictionary<string, string>? Map { get; init; }

    /// <summary>静态默认值（当 Selector 为空或无匹配时返回）</summary>
    public string? Default { get; init; }
}
