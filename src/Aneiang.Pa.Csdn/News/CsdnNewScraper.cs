using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.Csdn.Models;
using Microsoft.Extensions.Options;

namespace Aneiang.Pa.Csdn.News
{
    /// <summary>
    /// Csdn热门爬虫
    /// </summary>
    public class CsdnNewScraper : ICsdnNewScraper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CsdnScraperOptions _options;

        /// <summary>
        /// Csdn热门爬虫
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="options"></param>
        public CsdnNewScraper(IHttpClientFactory httpClientFactory, IOptions<CsdnScraperOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        /// <summary>
        /// 标识
        /// </summary>
        public string Source => "Csdn";


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
                    var result = JsonSerializer.Deserialize<CsdnOriginalResult>(jsonString);
                    if (result == null) return newsResult;
                    foreach (var item in result.data)
                    {
                        var newsItem = new NewsItem
                        {
                            Id = item.productId,
                            Title = item.articleTitle,
                            Url = item.articleDetailUrl,
                            MobileUrl = item.articleDetailUrl,
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
