using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.TouTiao.Models;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Aneiang.Pa.TouTiao.News
{
    /// <summary>
    /// 头条热门爬虫
    /// </summary>
    public class TouTiaoNewScraper : ITouTiaoNewScraper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TouTiaoScraperOptions _options;
        /// <summary>
        /// 头条热门爬虫
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="options"></param>
        public TouTiaoNewScraper(IHttpClientFactory httpClientFactory, IOptions<TouTiaoScraperOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        /// <summary>
        /// 标识
        /// </summary>
        public string Source => "TouTiao";


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
                    var result = JsonSerializer.Deserialize<TouTiaoOriginalResult>(jsonString);
                    if (result == null || result.status != "success") return newsResult;
                    
                    foreach (var item in result.data)
                    {
                        var newsItem = new NewsItem
                        {
                            Id = item.ClusterIdStr,
                            Title = item.Title,
                            Url = $"https://www.toutiao.com/trending/{item.ClusterIdStr}/",
                            MobileUrl = $"https://www.toutiao.com/trending/{item.ClusterIdStr}/"
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
