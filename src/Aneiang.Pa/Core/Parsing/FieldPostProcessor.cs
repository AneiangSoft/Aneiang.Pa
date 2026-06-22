using System;
using System.Text.RegularExpressions;
using Aneiang.Pa.Abstractions;

namespace Aneiang.Pa.Core.Parsing;

/// <summary>字段后处理：trim/collapse/regex/base/map/format/url-encode/default</summary>
internal static class FieldPostProcessor
{
    public static string? Apply(string? value, FieldSpec field)
    {
        if (value == null && !string.IsNullOrEmpty(field.Default)) value = field.Default;
        if (value == null) return null;

        if (field.Collapse) value = Regex.Replace(value, @"\s+", " ");
        if (field.Trim) value = value.Trim();

        if (!string.IsNullOrEmpty(field.Regex) && value != null)
        {
            var m = Regex.Match(value, field.Regex!);
            value = m.Success ? (m.Groups.Count > 1 ? m.Groups[1].Value : m.Value) : null;
        }

        if (field.Map != null && value != null && field.Map.TryGetValue(value, out var mapped))
            value = mapped;

        if (!string.IsNullOrEmpty(field.Base) && !string.IsNullOrEmpty(value)
            && Uri.TryCreate(field.Base, UriKind.Absolute, out var baseUri)
            && Uri.TryCreate(baseUri, value, out var abs))
        {
            value = abs.ToString();
        }

        if (field.UrlEncode && !string.IsNullOrEmpty(value))
            value = Uri.EscapeDataString(value);

        if (!string.IsNullOrEmpty(field.Format) && value != null)
            value = field.Format!.Replace("{value}", value);

        return value;
    }
}
