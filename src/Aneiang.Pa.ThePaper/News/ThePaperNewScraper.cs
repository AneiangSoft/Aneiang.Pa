using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.ThePaper.Models;
using Microsoft.Extensions.Options;

namespace Aneiang.Pa.ThePaper.News
{
    /// <summary>
    /// 澎湃新闻爬虫
    /// </summary>
    public class ThePaperNewScraper : IThePaperNewScraper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ThePaperScraperOptions _options;
        /// <summary>
        /// 澎湃新闻爬虫
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="options"></param>
        public ThePaperNewScraper(IHttpClientFactory httpClientFactory, IOptions<ThePaperScraperOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        /// <summary>
        /// 标识
        /// </summary>
        public string Source => "ThePaper";


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
                var newsResult = new NewsResult();
                var response = await client.GetAsync($"{_options.BaseUrl}{_options.NewsUrl}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ThePaperOriginalResult>(jsonString);
                    if (result == null || result.resultCode != 1) return newsResult;
                    foreach (var item in result.data.hotNews)
                    {
                        var newsItem = new NewsItem
                        {
                            Id = item.contId,
                            Title = item.name,
                            Url = $"https://www.thepaper.cn/newsDetail_forward_{item.contId}",
                            MobileUrl = $"https://www.thepaper.cn/newsDetail_forward_{item.contId}"
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
