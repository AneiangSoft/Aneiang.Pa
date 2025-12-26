using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.Tencent.Models;
using Microsoft.Extensions.Options;

namespace Aneiang.Pa.Tencent.News
{
    /// <summary>
    /// 腾讯综合早报爬虫
    /// </summary>
    public class TencentNewScraper : ITencentNewScraper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TencentScraperOptions _options;
        /// <summary>
        /// 腾讯综合早报爬虫
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="options"></param>
        public TencentNewScraper(IHttpClientFactory httpClientFactory, IOptions<TencentScraperOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        /// <summary>
        /// 标识
        /// </summary>
        public string Source => "Tencent";


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
                    var result = JsonSerializer.Deserialize<TencentOriginalResult>(jsonString);
                    if (result == null) return newsResult;
                    if (result.data.tabs.Count == 0) return newsResult;
                    
                    foreach (var item in result.data.tabs[0].articleList)
                    {
                        var newsItem = new NewsItem
                        {
                            Id = item.id,
                            Title = item.title,
                            Url = item.link_info.url,
                            MobileUrl = item.link_info.url,
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
