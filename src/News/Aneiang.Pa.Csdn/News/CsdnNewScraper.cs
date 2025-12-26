using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Core.News;
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
