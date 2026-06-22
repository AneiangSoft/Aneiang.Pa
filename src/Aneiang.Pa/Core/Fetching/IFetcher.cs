using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Aneiang.Pa.Abstractions;

namespace Aneiang.Pa.Core.Fetching;

/// <summary>HTTP 抓取器：根据 FetchSpec 发起请求</summary>
public interface IFetcher
{
    /// <summary>执行抓取，返回原始响应</summary>
    Task<HttpResponseMessage> FetchAsync(ScrapeContext context);
}

/// <summary>默认 HttpClient 实现</summary>
public sealed class HttpFetcher : IFetcher
{
    private readonly IHttpClientFactory _factory;

    /// <summary>初始化</summary>
    public HttpFetcher(IHttpClientFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <inheritdoc />
    public async Task<HttpResponseMessage> FetchAsync(ScrapeContext ctx)
    {
        var spec = ctx.Recipe.Fetch;
        var client = _factory.CreateClient(PaConsts.HttpClientName);
        var url = TemplateRenderer.Render(spec.Url, ctx.Variables);

        var method = new HttpMethod(string.IsNullOrWhiteSpace(spec.Method) ? "GET" : spec.Method);
        using var request = new HttpRequestMessage(method, url);

        // 默认 UA
        if (!spec.Headers.ContainsKey("User-Agent"))
            request.Headers.UserAgent.ParseAdd(UserAgents.Random());

        foreach (var kv in spec.Headers)
        {
            var value = TemplateRenderer.Render(kv.Value, ctx.Variables);
            if (string.IsNullOrWhiteSpace(value)) continue;
            if (!request.Headers.TryAddWithoutValidation(kv.Key, value))
            {
                // 内容头延后处理
            }
        }

        if (!string.IsNullOrEmpty(spec.Body))
        {
            request.Content = new StringContent(
                TemplateRenderer.Render(spec.Body!, ctx.Variables),
                Encoding.UTF8,
                spec.ContentType);
        }

        return await client.SendAsync(request, ctx.CancellationToken).ConfigureAwait(false);
    }
}

/// <summary>简单的 URL/Header 模板渲染：{name} → variables[name]，{name:url} 进行 URL 编码</summary>
internal static class TemplateRenderer
{
    public static string Render(string template, IDictionary<string, object?> vars)
    {
        if (string.IsNullOrEmpty(template) || !template.Contains('{')) return template;
        var sb = new StringBuilder(template.Length);
        var i = 0;
        while (i < template.Length)
        {
            var c = template[i];
            if (c == '{' && i + 1 < template.Length && template[i + 1] != '{')
            {
                var end = template.IndexOf('}', i + 1);
                if (end > i)
                {
                    var token = template.Substring(i + 1, end - i - 1);
                    string key; string? modifier = null;
                    var colon = token.IndexOf(':');
                    if (colon > 0)
                    {
                        key = token.Substring(0, colon);
                        modifier = token.Substring(colon + 1);
                    }
                    else
                    {
                        key = token;
                    }
                    if (vars.TryGetValue(key, out var v) && v != null)
                    {
                        var val = v.ToString() ?? "";
                        sb.Append(modifier?.ToLowerInvariant() switch
                        {
                            "url" => Uri.EscapeDataString(val),
                            _ => val
                        });
                    }
                    i = end + 1;
                    continue;
                }
            }
            sb.Append(c);
            i++;
        }
        return sb.ToString();
    }
}
