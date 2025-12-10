using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Aneiang.Pa.Bilibili.Models;
using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Core.News.Models;
using Microsoft.Extensions.Options;

namespace Aneiang.Pa.Bilibili.News
{
    /// <summary>
    /// 微博热门
    /// </summary>
    public class BilibiliNewScraper : IBilibiliNewScraper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly BilibiliScraperOptions _options;
        /// <summary>
        /// 微博热门
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="options"></param>
        public BilibiliNewScraper(IHttpClientFactory httpClientFactory, IOptions<BilibiliScraperOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        /// <summary>
        /// 标识
        /// </summary>
        public string Source => "Bilibili";


        /// <summary>
        /// 获取热门消息
        /// </summary>

        public async Task<NewsResult> GetNewsAsync()
        {
            try
            {
                _options.Check();
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Referrer = new Uri(_options.SearchUrl);
                var newsResult = new NewsResult();
                var response = await client.GetAsync(_options.SearchUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<BilibiliSearchOriginalResult>(jsonString);
                    if (result == null) return newsResult;
                    foreach (var item in result.list)
                    {
                        var newsItem = new NewsItem
                        {
                            Id = item.keyword,
                            Title = item.show_name,
                            Url = $"https://search.bilibili.com/all?keyword={Uri.EscapeUriString(item.keyword)}",
                            MobileUrl = $"https://search.bilibili.com/all?keyword={Uri.EscapeUriString(item.keyword)}"
                        };
                        newsItem.SetOriginal(item);
                        newsResult.Data.Add(newsItem);
                    }
                }
                return newsResult;
            }
            catch (Exception e)
            {
                return new NewsResult(false, e.Message);
            }
        }
    }
}
