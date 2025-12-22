using Aneiang.Pa.Core.Data;
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

        public async Task<NewsResult> GetNewsAsync()
        {
            try
            {
                _options.Check();
                var newsResult = new NewsResult();
                var client = _httpClientFactory.CreateClient(PaConsts.DefaultHttpClientName);
                var cookie = await GetDouYinCookieAsync();
                if (string.IsNullOrEmpty(cookie)) return new NewsResult(false, "获取抖音Cookie失败！");
                client.DefaultRequestHeaders.Referrer = new Uri(_options.BaseUrl);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);
                client.DefaultRequestHeaders.Add("Cookie", cookie);
                var response = await client.GetAsync($"{_options.BaseUrl}{_options.NewsUrl}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var douyinData = JsonSerializer.Deserialize<DouYinOriginalResult>(jsonString);
                    if (douyinData == null) return new NewsResult();
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
                return newsResult;
            }
            catch (Exception e)
            {
                return new NewsResult(false, e.Message);
            }
        }

        /// <summary>
        /// 获取抖音Cookie
        /// </summary>
        /// <returns></returns>
        private async Task<string?> GetDouYinCookieAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient(PaConsts.DefaultHttpClientName);

                const string loginUrl = "https://login.douyin.com/";
                var request = new HttpRequestMessage(HttpMethod.Get, loginUrl);

                request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");

                var response = await client.SendAsync(request);

                if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
                {
                    return string.Join("; ", cookieValues);
                }

                return client.DefaultRequestHeaders.Contains("Cookie") ? client.DefaultRequestHeaders.GetValues("Cookie").FirstOrDefault() : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
