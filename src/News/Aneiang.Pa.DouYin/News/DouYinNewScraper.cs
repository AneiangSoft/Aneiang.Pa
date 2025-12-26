using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.DouYin.Models;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Aneiang.Pa.DouYin.News
{
    /// <summary>
    /// 抖音热门爬虫
    /// </summary>
    public class DouYinNewScraper : IDouYinNewScraper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DouYinScraperOptions _options;

        /// <summary>
        /// 抖音热门爬虫
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="options"></param>
        public DouYinNewScraper(IHttpClientFactory httpClientFactory, IOptions<DouYinScraperOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        /// <summary>
        /// 标识
        /// </summary>
        public string Source => "DouYin";


        /// <summary>
        /// 获取热门消息
        /// </summary>
        /// <returns>新闻结果</returns>
        public async Task<AneiangGenericListResult<NewsItem>> GetNewsAsync()
        {
            try
            {
                _options.Check();
                var cookie = await GetDouYinCookieAsync();
                if (string.IsNullOrEmpty(cookie))
                {
                    return AneiangGenericListResult<NewsItem>.Failure("获取抖音Cookie失败！");
                }
                
                var client = ScraperHttpClientHelper.CreateConfiguredClient(
                    _httpClientFactory,
                    _options.BaseUrl,
                    _options.UserAgent,
                    cookie);
                
                var newsResult = new AneiangGenericListResult<NewsItem>();
                var response = await ScraperHttpClientHelper.GetAsync(
                    client,
                    $"{_options.BaseUrl}{_options.NewsUrl}");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var douyinData = JsonSerializer.Deserialize<DouYinOriginalResult>(jsonString);
                    if (douyinData == null) return newsResult;
                    
                    newsResult.Data = douyinData.data.word_list
                        .Select(k =>
                        {
                            var newsItem = new NewsItem
                            {
                                Id = k.sentence_id,
                                Title = k.word,
                                Url = $"https://www.douyin.com/hot/{k.sentence_id}",
                            };
                            newsItem.SetOriginal(k);
                            return newsItem;
                        })
                        .ToList();
                }
                else
                {
                    return AneiangGenericListResult<NewsItem>.Failure($"HTTP 请求失败，状态码: {response.StatusCode}");
                }
                
                return newsResult;
            }
            catch (Exception e)
            {
                return ScraperHttpClientHelper.CreateNewsErrorResult(e, Source);
            }
        }

        /// <summary>
        /// 获取抖音Cookie
        /// </summary>
        /// <returns>Cookie 字符串，如果获取失败则返回 null</returns>
        private async Task<string?> GetDouYinCookieAsync()
        {
            try
            {
                var client = ScraperHttpClientHelper.CreateConfiguredClient(
                    _httpClientFactory,
                    "https://login.douyin.com/",
                    _options.UserAgent);

                const string loginUrl = "https://login.douyin.com/";
                var request = new HttpRequestMessage(HttpMethod.Get, loginUrl);

                request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");

                var response = await client.SendAsync(request).ConfigureAwait(false);

                if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
                {
                    return string.Join("; ", cookieValues);
                }

                return client.DefaultRequestHeaders.Contains("Cookie")
                    ? client.DefaultRequestHeaders.GetValues("Cookie").FirstOrDefault()
                    : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
