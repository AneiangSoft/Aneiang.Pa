using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.Core.Scraper;
using Aneiang.Pa.Core.Models;
using Aneiang.Pa.IFeng.Models;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Aneiang.Pa.IFeng.News
{
    /// <summary>
    /// 凤凰网热门爬虫
    /// </summary>
    public class IFengNewScraper : IIFengNewScraper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IFengScraperOptions _options;
        /// <summary>
        /// 凤凰网热门爬虫
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="options"></param>
        public IFengNewScraper(IHttpClientFactory httpClientFactory, IOptions<IFengScraperOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        /// <summary>
        /// 标识
        /// </summary>
        public string Source => "IFeng";

        /// <summary>
        /// 爬虫元数据
        /// </summary>
        public ScraperDescriptor Descriptor { get; } = new ScraperDescriptor
        {
            Category = ScraperCategories.News,
            Source = "IFeng",
            DisplayName = "凤凰网热榜",
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
                
                var response = await ScraperHttpClientHelper.GetStringAsync(
                    client,
                    $"{_options.BaseUrl}{_options.NewsUrl}");
                
                var newsResult = new AneiangGenericListResult<NewsItem>();
                var jsonMatch = Regex.Match(response, @"var\s+allData\s*=\s*(\{[\s\S]*?\});", RegexOptions.Singleline);
                if (!jsonMatch.Success || jsonMatch.Groups.Count < 2)
                {
                    return newsResult;
                }
                
                var jsonStr = jsonMatch.Groups[1].Value;
                var result = JsonSerializer.Deserialize<IFengOriginalResult>(jsonStr);
                if (result == null) return newsResult;
                
                foreach (var item in result.hotNews1)
                {
                    var newsItem = new NewsItem
                    {
                        Id = item.url,
                        Title = item.title,
                        Url = item.url,
                        MobileUrl = item.url
                    };
                    newsItem.SetOriginal(item);
                    newsResult.Data.Add(newsItem);
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
