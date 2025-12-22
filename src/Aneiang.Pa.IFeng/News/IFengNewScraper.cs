using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.IFeng.Models;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
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
        /// 获取热门消息
        /// </summary>

        public async Task<NewsResult> GetNewsAsync()
        {
            try
            {
                _options.Check();
                var client = _httpClientFactory.CreateClient(PaConsts.DefaultHttpClientName);
                client.DefaultRequestHeaders.Referrer = new Uri(_options.BaseUrl);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);
                var response = await client.GetStringAsync($"{_options.BaseUrl}{_options.NewsUrl}");
                var newsResult = new NewsResult();
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
                return new NewsResult(false, e.Message);
            }
        }
    }
}
