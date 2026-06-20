using Aneiang.Pa.Core.Models;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.Core.Scraper;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Aneiang.Pa.Client
{
    /// <summary>
    /// Aneiang.Pa Web API 强类型客户端
    /// </summary>
    public class PaClient : IDisposable
    {
        private readonly HttpClient _http;
        private readonly bool _ownsHttpClient;
        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// News 端点客户端
        /// </summary>
        public NewsApi News { get; }

        /// <summary>
        /// Lottery 端点客户端
        /// </summary>
        public LotteryApi Lottery { get; }

        /// <summary>
        /// Registry 端点客户端（按 Category/Source 调用）
        /// </summary>
        public RegistryApi Registry { get; }

        /// <summary>
        /// 通过 BaseAddress 初始化客户端
        /// </summary>
        public PaClient(string baseAddress, string? apiKey = null, string apiKeyHeader = "X-API-Key")
        {
            if (string.IsNullOrWhiteSpace(baseAddress)) throw new ArgumentException("baseAddress 不能为空", nameof(baseAddress));
            _http = new HttpClient { BaseAddress = new Uri(baseAddress.TrimEnd('/') + "/") };
            if (!string.IsNullOrWhiteSpace(apiKey))
                _http.DefaultRequestHeaders.Add(apiKeyHeader, apiKey);
            _ownsHttpClient = true;
            News = new NewsApi(this);
            Lottery = new LotteryApi(this);
            Registry = new RegistryApi(this);
        }

        /// <summary>
        /// 通过外部 HttpClient 初始化（不会被释放）
        /// </summary>
        public PaClient(HttpClient httpClient)
        {
            _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _ownsHttpClient = false;
            News = new NewsApi(this);
            Lottery = new LotteryApi(this);
            Registry = new RegistryApi(this);
        }

        internal async Task<T?> GetAsync<T>(string url, CancellationToken ct)
        {
            var resp = await _http.GetAsync(url, ct).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            var stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOpts, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_ownsHttpClient) _http.Dispose();
        }

        /// <summary>
        /// News 端点客户端
        /// </summary>
        public class NewsApi
        {
            private readonly PaClient _owner;
            internal NewsApi(PaClient owner) { _owner = owner; }

            /// <summary>
            /// 获取指定平台的热榜
            /// </summary>
            public Task<AneiangGenericListResult<NewsItem>?> GetAsync(string source, CancellationToken ct = default)
                => _owner.GetAsync<AneiangGenericListResult<NewsItem>>($"api/scraper/news/{Uri.EscapeDataString(source)}", ct);

            /// <summary>
            /// 列出所有支持的源
            /// </summary>
            public Task<NewsSourcesResponse?> ListSourcesAsync(CancellationToken ct = default)
                => _owner.GetAsync<NewsSourcesResponse>("api/scraper/news/sources", ct);
        }

        /// <summary>
        /// Lottery 端点客户端
        /// </summary>
        public class LotteryApi
        {
            private readonly PaClient _owner;
            internal LotteryApi(PaClient owner) { _owner = owner; }

            /// <summary>
            /// 获取福利彩票开奖
            /// </summary>
            public Task<JsonElement> GetWelfareAsync(string type, int pageNo = 1, int pageSize = 30, CancellationToken ct = default)
                => _owner.GetAsync<JsonElement>($"api/scraper/lottery/welfare/{Uri.EscapeDataString(type)}?pageNo={pageNo}&pageSize={pageSize}", ct);

            /// <summary>
            /// 获取体育彩票开奖
            /// </summary>
            public Task<JsonElement> GetSportAsync(string type, int pageNo = 1, int pageSize = 30, CancellationToken ct = default)
                => _owner.GetAsync<JsonElement>($"api/scraper/lottery/sport/{Uri.EscapeDataString(type)}?pageNo={pageNo}&pageSize={pageSize}", ct);
        }

        /// <summary>
        /// Registry 端点客户端
        /// </summary>
        public class RegistryApi
        {
            private readonly PaClient _owner;
            internal RegistryApi(PaClient owner) { _owner = owner; }

            /// <summary>
            /// 列出所有已注册分类
            /// </summary>
            public Task<CategoriesResponse?> ListCategoriesAsync(CancellationToken ct = default)
                => _owner.GetAsync<CategoriesResponse>("api/scraper/registry/categories", ct);

            /// <summary>
            /// 列出所有爬虫描述符
            /// </summary>
            public Task<DescriptorsResponse?> ListDescriptorsAsync(string? category = null, CancellationToken ct = default)
            {
                var url = string.IsNullOrWhiteSpace(category)
                    ? "api/scraper/registry/descriptors"
                    : $"api/scraper/registry/descriptors?category={Uri.EscapeDataString(category)}";
                return _owner.GetAsync<DescriptorsResponse>(url, ct);
            }

            /// <summary>
            /// 通过 Category + Source 调用爬取
            /// </summary>
            public Task<JsonElement> FetchAsync(string category, string source, CancellationToken ct = default)
                => _owner.GetAsync<JsonElement>($"api/scraper/fetch/{Uri.EscapeDataString(category)}/{Uri.EscapeDataString(source)}", ct);
        }
    }

    /// <summary>列源响应</summary>
    public class NewsSourcesResponse
    {
        /// <summary>源标识列表</summary>
        public string[] Sources { get; set; } = Array.Empty<string>();
        /// <summary>数量</summary>
        public int Count { get; set; }
    }

    /// <summary>分类响应</summary>
    public class CategoriesResponse
    {
        /// <summary>分类列表</summary>
        public string[] Categories { get; set; } = Array.Empty<string>();
    }

    /// <summary>描述符响应</summary>
    public class DescriptorsResponse
    {
        /// <summary>描述符</summary>
        public List<ScraperDescriptor> Scrapers { get; set; } = new List<ScraperDescriptor>();
        /// <summary>数量</summary>
        public int Count { get; set; }
        /// <summary>分类列表</summary>
        public string[] Categories { get; set; } = Array.Empty<string>();
    }
}
