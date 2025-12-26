using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Aneiang.Pa.Bilibili.Models;
using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.Core.News.Models;
using Microsoft.Extensions.Options;

namespace Aneiang.Pa.Bilibili.News
{
    /// <summary>
    /// B站热门搜索爬虫
    /// </summary>
    public class BilibiliNewScraper : IBilibiliNewScraper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly BilibiliScraperOptions _options;
        /// <summary>
        /// B站热门搜索爬虫
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
                
                var newsResult = new AneiangGenericListResult<NewsItem>();
                var response = await ScraperHttpClientHelper.GetAsync(
                    client,
                    $"{_options.BaseUrl}{_options.NewsUrl}");
                
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
