using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.DouBan.Models;
using Microsoft.Extensions.Options;

namespace Aneiang.Pa.DouBan.News
{
    /// <summary>
    /// 豆瓣新闻爬虫
    /// </summary>
    public class DouBanNewScraper : IDouBanNewScraper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DouBanScraperOptions _options;
        /// <summary>
        /// 豆瓣新闻爬虫
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="options"></param>
        public DouBanNewScraper(IHttpClientFactory httpClientFactory, IOptions<DouBanScraperOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        /// <summary>
        /// 标识
        /// </summary>
        public string Source => "DouBan";


        /// <summary>
        /// 获取热门消息
        /// </summary>

        public async Task<NewsResult> GetNewsAsync()
        {
            try
            {
                _options.Check();
                var client = _httpClientFactory.CreateClient(PaConsts.DefaultHttpClientName);
                client.DefaultRequestHeaders.Referrer = new Uri(_options.BaseUrl);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                var newsResult = new NewsResult();
                var response = await client.GetAsync($"{_options.BaseUrl}{_options.NewsUrl}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<DouBanOriginalResult>(jsonString);
                    if (result == null) return newsResult;
                    foreach (var item in result.items)
                    {
                        var newsItem = new NewsItem
                        {
                            Id = item.id,
                            Title = item.title,
                            Url = $"https://movie.douban.com/subject/{item.id}",
                            MobileUrl = $"https://movie.douban.com/subject/{item.id}"
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
