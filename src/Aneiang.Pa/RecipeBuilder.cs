using System;
using System.Collections.Generic;
using Aneiang.Pa.Abstractions;

namespace Aneiang.Pa;

/// <summary>Recipe Fluent 构建器</summary>
public sealed class RecipeBuilder
{
    private readonly string _name;
    private string? _category;
    private string? _displayName;
    private FetchBuilder _fetch = new();
    private ParseSpec _parse = new HtmlParseSpec();

    /// <summary>初始化</summary>
    public RecipeBuilder(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name");
        _name = name;
    }

    /// <summary>设置分类</summary>
    public RecipeBuilder Category(string category) { _category = category; return this; }

    /// <summary>设置显示名</summary>
    public RecipeBuilder DisplayName(string name) { _displayName = name; return this; }

    /// <summary>设置 GET URL</summary>
    public RecipeBuilder Get(string url) { _fetch.Method = "GET"; _fetch.Url = url; return this; }

    /// <summary>设置 POST URL</summary>
    public RecipeBuilder Post(string url, string? body = null) { _fetch.Method = "POST"; _fetch.Url = url; _fetch.Body = body; return this; }

    /// <summary>追加请求头</summary>
    public RecipeBuilder Header(string name, string value) { _fetch.Headers[name] = value; return this; }

    /// <summary>设置 UA</summary>
    public RecipeBuilder WithUserAgent(string ua) => Header("User-Agent", ua);

    /// <summary>HTML 解析配置</summary>
    public RecipeBuilder ParseHtml(Action<HtmlParseBuilder> configure)
    {
        var builder = new HtmlParseBuilder();
        configure(builder);
        _parse = builder.Build();
        return this;
    }

    /// <summary>JSON 解析配置</summary>
    public RecipeBuilder ParseJson(Action<JsonParseBuilder> configure)
    {
        var builder = new JsonParseBuilder();
        configure(builder);
        _parse = builder.Build();
        return this;
    }

    /// <summary>构建</summary>
    public ScraperRecipe Build() => new()
    {
        Name = _name,
        Category = _category,
        DisplayName = _displayName,
        Fetch = _fetch.Build(),
        Parse = _parse
    };

    private sealed class FetchBuilder
    {
        public string Method = "GET";
        public string Url = "";
        public Dictionary<string, string> Headers = new();
        public string? Body;
        public TimeSpan? Timeout;

        public FetchSpec Build() => new()
        {
            Method = Method,
            Url = Url,
            Headers = Headers,
            Body = Body,
            Timeout = Timeout
        };
    }
}

/// <summary>HTML 解析构建器</summary>
public sealed class HtmlParseBuilder
{
    private string? _container;
    private readonly Dictionary<string, FieldSpec> _fields = new();

    /// <summary>设置容器选择器</summary>
    public HtmlParseBuilder Container(string selector) { _container = selector; return this; }

    /// <summary>添加字段</summary>
    public HtmlParseBuilder Field(string name, string selector, Action<FieldBuilder>? configure = null)
    {
        var fb = new FieldBuilder { Selector = selector };
        configure?.Invoke(fb);
        _fields[name] = fb.Build();
        return this;
    }

    internal HtmlParseSpec Build() => new() { Container = _container, Fields = _fields };
}

/// <summary>JSON 解析构建器</summary>
public sealed class JsonParseBuilder
{
    private string _itemsPath = "$";
    private readonly Dictionary<string, FieldSpec> _fields = new();

    /// <summary>设置条目路径</summary>
    public JsonParseBuilder Items(string path) { _itemsPath = path; return this; }

    /// <summary>添加字段</summary>
    public JsonParseBuilder Field(string name, string selector)
    {
        _fields[name] = new FieldSpec { Selector = selector };
        return this;
    }

    internal JsonParseSpec Build() => new() { ItemsPath = _itemsPath, Fields = _fields };
}

/// <summary>字段构建器</summary>
public sealed class FieldBuilder
{
    /// <summary>选择器</summary>
    public string Selector { get; set; } = "";
    /// <summary>属性名</summary>
    public string? Attr { get; set; }
    /// <summary>是否 Trim</summary>
    public bool Trim { get; set; } = true;
    /// <summary>是否合并空白</summary>
    public bool Collapse { get; set; } = true;
    /// <summary>基础 URL</summary>
    public string? Base { get; set; }
    /// <summary>正则</summary>
    public string? Regex { get; set; }

    /// <summary>属性</summary>
    public FieldBuilder WithAttr(string attr) { Attr = attr; return this; }

    /// <summary>基础 URL</summary>
    public FieldBuilder WithBase(string baseUrl) { Base = baseUrl; return this; }

    internal FieldSpec Build() => new()
    {
        Selector = Selector,
        Attr = Attr,
        Trim = Trim,
        Collapse = Collapse,
        Base = Base,
        Regex = Regex
    };
}
