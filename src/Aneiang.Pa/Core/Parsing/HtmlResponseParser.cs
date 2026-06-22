using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.XPath;
using Aneiang.Pa.Abstractions;

namespace Aneiang.Pa.Core.Parsing;

/// <summary>HTML 解析器（AngleSharp 实现）</summary>
public sealed class HtmlResponseParser : IResponseParser
{
    /// <inheritdoc />
    public bool CanParse(ParseSpec spec) => spec is HtmlParseSpec;

    /// <inheritdoc />
    public async Task<ScrapeResult> ParseAsync(HttpResponseMessage response, ParseSpec spec, ScrapeContext context)
    {
        if (spec is not HtmlParseSpec html) return ScrapeResult.Fail("ParseSpec 不是 HtmlParseSpec");

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var parser = new HtmlParser();
        using var doc = await parser.ParseDocumentAsync(content).ConfigureAwait(false);

        var items = new List<IReadOnlyDictionary<string, string?>>();

        if (string.IsNullOrEmpty(html.Container))
        {
            // 单条数据：在整个文档下提取一次
            items.Add(ExtractFields(doc.DocumentElement, html.Fields));
        }
        else
        {
            var nodes = QueryAll(doc, html.Container!);
            foreach (var node in nodes)
                items.Add(ExtractFields(node, html.Fields));
        }

        return ScrapeResult.Ok(items);
    }

    private static IEnumerable<IElement> QueryAll(IParentNode root, string selector)
    {
        if (selector.StartsWith("xpath:", StringComparison.OrdinalIgnoreCase))
        {
            var xpath = selector.Substring("xpath:".Length);
            // 优先把 root 视作 IElement；若是 Document 则用 DocumentElement
            IElement? el = root as IElement
                ?? (root as AngleSharp.Dom.IDocument)?.DocumentElement;
            if (el != null)
            {
                var nodes = el.SelectNodes(xpath);
                foreach (var n in nodes)
                    if (n is IElement e) yield return e;
            }
            yield break;
        }
        foreach (var e in root.QuerySelectorAll(selector))
            yield return e;
    }

    private static IElement? QueryOne(IParentNode root, string selector)
    {
        if (selector == ".") return root as IElement;
        if (selector.StartsWith("xpath:", StringComparison.OrdinalIgnoreCase))
        {
            if (root is IElement el)
                return el.SelectSingleNode(selector.Substring("xpath:".Length)) as IElement;
            return null;
        }
        return root.QuerySelector(selector);
    }

    private static IReadOnlyDictionary<string, string?> ExtractFields(IElement context, IDictionary<string, FieldSpec> fields)
    {
        var dict = new Dictionary<string, string?>(fields.Count);
        foreach (var kv in fields)
        {
            var value = ExtractField(context, kv.Value);
            dict[kv.Key] = value;
        }
        return dict;
    }

    private static string? ExtractField(IElement context, FieldSpec field)
    {
        IElement? node = string.IsNullOrEmpty(field.Selector) ? context : QueryOne(context, field.Selector);
        string? value = null;
        if (node != null)
        {
            value = string.IsNullOrEmpty(field.Attr)
                ? node.TextContent
                : node.GetAttribute(field.Attr);
        }
        return FieldPostProcessor.Apply(value, field);
    }
}
