using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Aneiang.Pa.Abstractions;

namespace Aneiang.Pa.Core.Parsing;

/// <summary>JSON 解析器（System.Text.Json + 简易 JSONPath）</summary>
public sealed class JsonResponseParser : IResponseParser
{
    /// <inheritdoc />
    public bool CanParse(ParseSpec spec) => spec is JsonParseSpec;

    /// <inheritdoc />
    public async Task<ScrapeResult> ParseAsync(HttpResponseMessage response, ParseSpec spec, ScrapeContext context)
    {
        if (spec is not JsonParseSpec json) return ScrapeResult.Fail("ParseSpec 不是 JsonParseSpec");

        var raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        using var doc = JsonDocument.Parse(raw);
        var items = new List<IReadOnlyDictionary<string, string?>>();

        var rootArray = JsonPath.Resolve(doc.RootElement, json.ItemsPath);
        if (rootArray is JsonElement arr)
        {
            if (arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var elem in arr.EnumerateArray())
                    items.Add(ExtractFields(elem, json.Fields));
            }
            else if (arr.ValueKind == JsonValueKind.Object)
            {
                items.Add(ExtractFields(arr, json.Fields));
            }
        }

        return ScrapeResult.Ok(items);
    }

    private static IReadOnlyDictionary<string, string?> ExtractFields(JsonElement context, IDictionary<string, FieldSpec> fields)
    {
        var dict = new Dictionary<string, string?>(fields.Count);
        foreach (var kv in fields)
        {
            string? str = null;
            if (string.IsNullOrEmpty(kv.Value.Selector))
            {
                // 无 selector：仅靠 default + format
                str = null;
            }
            else
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

/// <summary>简易 JSONPath（支持 $.a.b、$.arr[0]、$.arr[*].x 子集）</summary>
internal static class JsonPath
{
    public static object? Resolve(JsonElement root, string path)
    {
        if (string.IsNullOrEmpty(path) || path == "$") return root;
        var p = path.StartsWith("$") ? path.Substring(1).TrimStart('.') : path.TrimStart('.');
        var current = (object)root;
        var i = 0;
        while (i < p.Length)
        {
            // 读取 token
            var dot = p.IndexOf('.', i);
            var bracket = p.IndexOf('[', i);
            int end;
            if (dot < 0 && bracket < 0) end = p.Length;
            else if (dot < 0) end = bracket;
            else if (bracket < 0) end = dot;
            else end = Math.Min(dot, bracket);

            var token = p.Substring(i, end - i);
            if (!string.IsNullOrEmpty(token) && current is JsonElement ce && ce.ValueKind == JsonValueKind.Object)
            {
                if (!ce.TryGetProperty(token, out var next)) return null;
                current = next;
            }
            i = end;
            if (i < p.Length && p[i] == '[')
            {
                var close = p.IndexOf(']', i);
                if (close < 0) return null;
                var idxStr = p.Substring(i + 1, close - i - 1);
                if (current is not JsonElement arr || arr.ValueKind != JsonValueKind.Array) return null;
                if (idxStr == "*") return arr; // 此简易版本不展开 *，让调用方处理
                if (!int.TryParse(idxStr, out var idx)) return null;
                if (idx < 0 || idx >= arr.GetArrayLength()) return null;
                current = arr[idx];
                i = close + 1;
            }
            if (i < p.Length && p[i] == '.') i++;
        }
        return current;
    }
}
