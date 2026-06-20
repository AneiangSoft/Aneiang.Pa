using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Aneiang.Pa.Bilibili.Models;
using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.Core.Scraper;
using Aneiang.Pa.Core.Models;
using Microsoft.Extensions.Options;
using System.Threading;

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
        /// 爬虫元数据
        /// </summary>
        public ScraperDescriptor Descriptor { get; } = new ScraperDescriptor
        {
            Category = ScraperCategories.News,
            Source = "Bilibili",
            DisplayName = "B站热搜",
            ResultType = typeof(NewsItem)
        };

        /// <summary>
        /// 执行爬取（统一入口）
        /// </summary>
        public async Task<ScraperResult<NewsItem>> ScrapeAsync(CancellationToken cancellationToken = default)
        {
            var result = await GetNewsAsync();
            return new ScraperResult<NewsItem>
            {
                IsSuccess = result.IsSuccessd,
                ErrorMessage = result.ErrorMessage,
                UpdatedTime = result.UpdatedTime,
                Data = result.Data
            };
        }


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
