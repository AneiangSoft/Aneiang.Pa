using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.Core.Scraper;
using Aneiang.Pa.Core.Models;
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
        /// 爬虫元数据
        /// </summary>
        public ScraperDescriptor Descriptor { get; } = new ScraperDescriptor
        {
            Category = ScraperCategories.News,
            Source = "ThePaper",
            DisplayName = "澎湃热榜",
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
