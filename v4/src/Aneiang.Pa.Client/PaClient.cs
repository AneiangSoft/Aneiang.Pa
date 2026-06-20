using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Aneiang.Pa.Abstractions;

namespace Aneiang.Pa.Client;

/// <summary>Aneiang.Pa 4.0 HTTP 客户端</summary>
public sealed class PaClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly bool _ownsHttp;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>初始化（自带 HttpClient）</summary>
    public PaClient(string baseAddress, string? apiKey = null)
    {
        if (string.IsNullOrWhiteSpace(baseAddress)) throw new ArgumentException(nameof(baseAddress));
        _http = new HttpClient { BaseAddress = new Uri(baseAddress.TrimEnd('/') + "/") };
        if (!string.IsNullOrWhiteSpace(apiKey))
            _http.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        _ownsHttp = true;
    }

    /// <summary>使用外部 HttpClient</summary>
    public PaClient(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _ownsHttp = false;
    }

    /// <summary>调用某个 source 的爬取</summary>
    public async Task<ScrapeResult?> FetchAsync(string name, bool noCache = false, CancellationToken ct = default)
    {
        var url = $"api/pa/source/{Uri.EscapeDataString(name)}" + (noCache ? "?noCache=1" : "");
        var resp = await _http.GetAsync(url, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<ScrapeResult>(stream, JsonOpts, ct).ConfigureAwait(false);
    }

    /// <summary>列出所有可用 source</summary>
    public async Task<SourcesResponse?> ListSourcesAsync(CancellationToken ct = default)
    {
        var resp = await _http.GetAsync("api/pa/sources", ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<SourcesResponse>(stream, JsonOpts, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Dispose() { if (_ownsHttp) _http.Dispose(); }
}

/// <summary>列出 source 的响应</summary>
public class SourcesResponse
{
    /// <summary>总数</summary>
    public int Count { get; set; }
    /// <summary>条目</summary>
    public SourceItem[] Items { get; set; } = Array.Empty<SourceItem>();
}

/// <summary>单条 source</summary>
public class SourceItem
{
    /// <summary>名称</summary>
    public string Name { get; set; } = "";
    /// <summary>分类</summary>
    public string? Category { get; set; }
    /// <summary>显示名</summary>
    public string? DisplayName { get; set; }
}
