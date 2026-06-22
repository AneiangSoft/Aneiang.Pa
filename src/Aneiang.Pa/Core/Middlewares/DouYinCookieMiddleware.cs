using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Aneiang.Pa.Abstractions;
using Microsoft.Extensions.Logging;

namespace Aneiang.Pa.Core.Middlewares;

/// <summary>
/// 抖音 Cookie 中间件：自动从 login.douyin.com 获取 Cookie 并注入到请求。
/// 抖音热搜接口需要先访问登录页拿到 Set-Cookie，否则会被反爬拦截。
/// 仅对 Recipe.Name == "DouYin" 生效。
/// </summary>
public sealed class DouYinCookieMiddleware : IScrapeMiddleware
{
    private readonly IHttpClientFactory _factory;
    private readonly ILogger<DouYinCookieMiddleware>? _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private string? _cachedCookie;
    private DateTime _cookieExpiry = DateTime.MinValue;

    /// <inheritdoc />
    public int Order => 5;

    /// <inheritdoc />
    public string Name => "DouYinCookie";

    /// <summary>Cookie 缓存时长，默认 10 分钟</summary>
    public TimeSpan CookieTtl { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>获取 Cookie 时的 User-Agent</summary>
    public string UserAgent { get; set; } =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36";

    /// <summary>初始化</summary>
    public DouYinCookieMiddleware(IHttpClientFactory factory, ILogger<DouYinCookieMiddleware>? logger = null)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ScrapeResult> InvokeAsync(ScrapeContext ctx, ScrapeDelegate next)
    {
        // 仅对 DouYin Recipe 生效
        if (!string.Equals(ctx.Recipe.Name, "DouYin", StringComparison.OrdinalIgnoreCase))
            return await next(ctx).ConfigureAwait(false);

        var cookie = await GetCookieAsync(ctx.CancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(cookie))
            ctx.Variables["douyin_cookie"] = cookie;

        return await next(ctx).ConfigureAwait(false);
    }

    private async Task<string?> GetCookieAsync(CancellationToken ct)
    {
        if (_cachedCookie != null && DateTime.UtcNow < _cookieExpiry)
            return _cachedCookie;

        await _lock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_cachedCookie != null && DateTime.UtcNow < _cookieExpiry)
                return _cachedCookie;

            try
            {
                var client = _factory.CreateClient(PaConsts.HttpClientName);
                using var request = new HttpRequestMessage(HttpMethod.Get, "https://login.douyin.com/");
                request.Headers.UserAgent.ParseAdd(UserAgent);
                request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");

                using var response = await client.SendAsync(request, ct).ConfigureAwait(false);

                if (response.Headers.TryGetValues("Set-Cookie", out var values))
                {
                    var cookie = string.Join("; ", System.Linq.Enumerable.Select(values, StripAttributes));
                    if (!string.IsNullOrEmpty(cookie))
                    {
                        _cachedCookie = cookie;
                        _cookieExpiry = DateTime.UtcNow.Add(CookieTtl);
                        _logger?.LogInformation("[DouYinCookie] 已获取 Cookie（{Length} 字节，{Ttl} 分钟内复用）",
                            cookie.Length, CookieTtl.TotalMinutes);
                        return cookie;
                    }
                }

                _logger?.LogWarning("[DouYinCookie] login.douyin.com 未返回 Set-Cookie 头");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "[DouYinCookie] 获取 Cookie 失败");
            }
        }
        finally
        {
            _lock.Release();
        }
        return _cachedCookie;
    }

    /// <summary>从 "name=value; Path=/; HttpOnly" 中只保留 "name=value" 部分</summary>
    private static string StripAttributes(string setCookie)
    {
        if (string.IsNullOrEmpty(setCookie)) return string.Empty;
        var semi = setCookie.IndexOf(';');
        return semi > 0 ? setCookie.Substring(0, semi) : setCookie;
    }
}
