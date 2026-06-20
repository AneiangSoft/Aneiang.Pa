using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Aneiang.Pa.Abstractions;

namespace Aneiang.Pa.Core.Parsing;

/// <summary>嵌入 JSON 解析器（HTML 中正则抠 JSON，再按 JSON Path 解析）</summary>
public sealed class EmbeddedJsonResponseParser : IResponseParser
{
    /// <inheritdoc />
    public bool CanParse(ParseSpec spec) => spec is EmbeddedJsonParseSpec;

    /// <inheritdoc />
    public async Task<ScrapeResult> ParseAsync(HttpResponseMessage response, ParseSpec spec, ScrapeContext context)
    {
        if (spec is not EmbeddedJsonParseSpec ej) return ScrapeResult.Fail("非 EmbeddedJsonParseSpec");
        var raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var match = Regex.Match(raw, ej.ExtractPattern, RegexOptions.Singleline);
        if (!match.Success) return ScrapeResult.Fail($"未匹配到嵌入 JSON：{ej.ExtractPattern}");
        var jsonStr = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;

        using var doc = JsonDocument.Parse(jsonStr);
        var arr = JsonPath.Resolve(doc.RootElement, ej.ItemsPath);
        var items = new List<IReadOnlyDictionary<string, string?>>();
        if (arr is JsonElement node)
        {
            if (node.ValueKind == JsonValueKind.Array)
            {
                foreach (var elem in node.EnumerateArray())
                {
                    var dict = ExtractFields(elem, ej.Fields);
                    if (ShouldSkip(dict, ej)) continue;
                    items.Add(dict);
                }
            }
            else if (node.ValueKind == JsonValueKind.Object)
            {
                items.Add(ExtractFields(node, ej.Fields));
            }
        }
        return ScrapeResult.Ok(items);
    }

    private static bool ShouldSkip(IReadOnlyDictionary<string, string?> dict, EmbeddedJsonParseSpec ej)
    {
        if (string.IsNullOrEmpty(ej.FilterField)) return false;
        if (!dict.TryGetValue(ej.FilterField!, out var v)) return false;
        return string.Equals(v, ej.FilterEqualsValue, System.StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyDictionary<string, string?> ExtractFields(JsonElement context, IDictionary<string, FieldSpec> fields)
    {
        var dict = new Dictionary<string, string?>(fields.Count);
        foreach (var kv in fields)
        {
            string? str = null;
            if (!string.IsNullOrEmpty(kv.Value.Selector))
            {
                var sub = JsonPath.Resolve(context, kv.Value.Selector);
                if (sub is JsonElement e)
                {
                    str = e.ValueKind switch
                    {
                        JsonValueKind.String => e.GetString(),
                        JsonValueKind.Number => e.ToString(),
                        JsonValueKind.True or JsonValueKind.False => e.GetBoolean().ToString(),
                        JsonValueKind.Null or JsonValueKind.Undefined => null,
                        _ => e.GetRawText()
                    };
                }
            }
            dict[kv.Key] = FieldPostProcessor.Apply(str, kv.Value);
        }
        return dict;
    }
}
