using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aneiang.Pa.Abstractions;
using Aneiang.Pa.Core.Fetching;

namespace Aneiang.Pa.Core.Parsing;

/// <summary>终端节点：执行 Fetch + Parse</summary>
public sealed class FetchParseTerminal
{
    private readonly IFetcher _fetcher;
    private readonly IReadOnlyList<IResponseParser> _parsers;

    /// <summary>初始化</summary>
    public FetchParseTerminal(IFetcher fetcher, IEnumerable<IResponseParser> parsers)
    {
        _fetcher = fetcher;
        _parsers = parsers.ToArray();
    }

    /// <summary>把自身包装成 ScrapeDelegate</summary>
    public ScrapeDelegate AsDelegate() => InvokeAsync;

    private async Task<ScrapeResult> InvokeAsync(ScrapeContext ctx)
    {
        try
        {
            using var response = await _fetcher.FetchAsync(ctx).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return ScrapeResult.Fail($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");

            var parser = _parsers.FirstOrDefault(p => p.CanParse(ctx.Recipe.Parse));
            if (parser == null)
                return ScrapeResult.Fail($"找不到适合 {ctx.Recipe.Parse.Type} 的解析器");

            return await parser.ParseAsync(response, ctx.Recipe.Parse, ctx).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (ctx.CancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ScrapeResult.Fail(ex.Message);
        }
    }
}
