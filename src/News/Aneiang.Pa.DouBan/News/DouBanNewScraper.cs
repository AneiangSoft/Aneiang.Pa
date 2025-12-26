using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Core.News;
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
        /// <returns>新闻结果</returns>
        public async Task<AneiangGenericListResult<NewsItem>> GetNewsAsync()
        {
            try
            {
                _options.Check();
                var client = ScraperHttpClientHelper.CreateConfiguredClient(
                    _httpClientFactory,
                    _options.BaseUrl,
                    _options.UserAgent);
                
                // 添加 Accept header
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                
                var newsResult = new AneiangGenericListResult<NewsItem>();
                var response = await ScraperHttpClientHelper.GetAsync(
                    client,
                    $"{_options.BaseUrl}{_options.NewsUrl}");
                
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
    }
}
