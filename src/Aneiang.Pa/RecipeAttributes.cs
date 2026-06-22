using System;

namespace Aneiang.Pa;

/// <summary>声明此类型对应一个 Recipe</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class RecipeAttribute : Attribute
{
    /// <summary>Recipe 名</summary>
    public string Name { get; }

    /// <summary>分类（可选）</summary>
    public string? Category { get; set; }

    /// <summary>显示名（可选）</summary>
    public string? DisplayName { get; set; }

    /// <summary>初始化</summary>
    public RecipeAttribute(string name) { Name = name; }
}

/// <summary>声明 GET 请求</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class GetAttribute : Attribute
{
    /// <summary>URL 模板</summary>
    public string Url { get; }

    /// <summary>初始化</summary>
    public GetAttribute(string url) { Url = url; }
}

/// <summary>声明 POST 请求</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class PostAttribute : Attribute
{
    /// <summary>URL 模板</summary>
    public string Url { get; }

    /// <summary>请求体</summary>
    public string? Body { get; set; }

    /// <summary>初始化</summary>
    public PostAttribute(string url) { Url = url; }
}

/// <summary>声明 HTML 容器选择器</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ContainerAttribute : Attribute
{
    /// <summary>选择器（CSS / XPath，前缀 "xpath:"）</summary>
    public string Selector { get; }

    /// <summary>初始化</summary>
    public ContainerAttribute(string selector) { Selector = selector; }
}

/// <summary>声明属性的字段提取规则</summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class SelectorAttribute : Attribute
{
    /// <summary>选择器</summary>
    public string Selector { get; }

    /// <summary>属性名（HTML 用）</summary>
    public string? Attr { get; set; }

    /// <summary>是否 Trim 空白（默认 true）</summary>
    public bool Trim { get; set; } = true;

    /// <summary>是否合并连续空白（默认 true）</summary>
    public bool Collapse { get; set; } = true;

    /// <summary>相对 URL 基础地址</summary>
    public string? Base { get; set; }

    /// <summary>从字段值正则抽取</summary>
    public string? Regex { get; set; }

    /// <summary>初始化</summary>
    public SelectorAttribute(string selector) { Selector = selector; }
}
