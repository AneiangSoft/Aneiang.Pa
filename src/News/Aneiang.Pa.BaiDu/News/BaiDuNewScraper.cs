using Aneiang.Pa.BaiDu.Models;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.Core.News.Models;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Aneiang.Pa.Core.Data;

namespace Aneiang.Pa.BaiDu.News
{
    /// <summary>
    /// 百度热门爬虫
    /// </summary>
    public class BaiDuNewScraper : IBaiDuNewScraper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly BaiDuScraperOptions _options;
        /// <summary>
        /// 百度热门爬虫
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="options"></param>
        public BaiDuNewScraper(IHttpClientFactory httpClientFactory, IOptions<BaiDuScraperOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        /// <summary>
        /// 标识
        /// </summary>
        public string Source => "BaiDu";


        /// <summary>
        /// 获取热门消息
        /// </summary>

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
                var jsonMatch = Regex.Match(response, @"<!--s-data:(.*?)-->", RegexOptions.Singleline);

                if (!jsonMatch.Success || jsonMatch.Groups.Count < 2)
                {
                    return newsResult;
                }

                var jsonStr = jsonMatch.Groups[1].Value;

                // 解析JSON
                var baiduData = JsonSerializer.Deserialize<BaiduOriginalResult>(jsonStr);

                if (baiduData?.data.cards == null || baiduData.data.cards.Count == 0)
                {
                    return newsResult;
                }

                // 过滤和转换数据
                var contentItems = baiduData.data.cards[0].content;

                // 过滤掉置顶项，并转换为NewsItem
                newsResult.Data = contentItems
                    .Where(k => k.isTop != true)  // 过滤掉isTop为true的项
                    .Select(k =>
                    {
                        var newsItem = new NewsItem
                        {
                            Id = k.rawUrl,
                            Title = k.word,
                            Url = k.rawUrl,
                        };
                        newsItem.SetOriginal(k);
                        return newsItem;
                    })
                    .ToList();
                return newsResult;
            }
            catch (Exception e)
            {
                return ScraperHttpClientHelper.CreateNewsErrorResult(e, Source);
            }
        }
    }
}
