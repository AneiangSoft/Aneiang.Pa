using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Aneiang.Pa.Abstractions;

namespace Aneiang.Pa.Core.Parsing;

/// <summary>正则解析器</summary>
public sealed class RegexResponseParser : IResponseParser
{
    /// <inheritdoc />
    public bool CanParse(ParseSpec spec) => spec is RegexParseSpec;

    /// <inheritdoc />
    public async Task<ScrapeResult> ParseAsync(HttpResponseMessage response, ParseSpec spec, ScrapeContext context)
    {
        if (spec is not RegexParseSpec rs) return ScrapeResult.Fail("非 RegexParseSpec");
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var regex = new Regex(rs.Pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        var items = new List<IReadOnlyDictionary<string, string?>>();
        foreach (Match m in regex.Matches(content))
        {
            var dict = new Dictionary<string, string?>();
            // 命名组优先，否则使用 fields 映射
            foreach (var groupName in regex.GetGroupNames())
            {
                if (int.TryParse(groupName, out _)) continue;
                dict[groupName] = m.Groups[groupName].Value;
            }
            foreach (var kv in rs.Groups)
            {
                if (int.TryParse(kv.Value, out var idx) && idx < m.Groups.Count)
                    dict[kv.Key] = m.Groups[idx].Value;
                else
                    dict[kv.Key] = m.Groups[kv.Value].Value;
            }
            items.Add(dict);
        }
        return ScrapeResult.Ok(items);
    }
}
