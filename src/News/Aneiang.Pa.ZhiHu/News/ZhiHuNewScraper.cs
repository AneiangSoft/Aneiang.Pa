using Aneiang.Pa.Core.News;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.ZhiHu.Models;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Aneiang.Pa.Core.Data;

namespace Aneiang.Pa.ZhiHu.News
{
    /// <summary>
    /// 知乎热门爬虫
    /// </summary>
    public class ZhiHuNewScraper : IZhiHuNewScraper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ZhiHuScraperOptions _options;

        /// <summary>
        /// 知乎热门爬虫
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="options"></param>
        public ZhiHuNewScraper(IHttpClientFactory httpClientFactory, IOptions<ZhiHuScraperOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        /// <summary>
        /// 标识
        /// </summary>
        public string Source => "ZhiHu";


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
                    var result = JsonSerializer.Deserialize<ZhiHuOriginalResult>(jsonString);
                    if (result == null) return newsResult;
                    
                    foreach (var item in result.data)
                    {
                        var newsItem = new NewsItem
                        {
                            Id = item.card_id,
                            Title = item.target.title_area.text,
                            Url = item.target.link.url,
                            MobileUrl = item.target.link.url,
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
