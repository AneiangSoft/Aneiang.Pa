using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.WeiBo.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Aneiang.Pa.WeiBo.News
{
    /// <summary>
    /// 微博热门爬虫
    /// </summary>
    public class WeiBoNewScraper : IWeiBoNewScraper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly WeiBoScraperOptions _options;

        /// <summary>
        /// 微博热门爬虫
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="options"></param>
        public WeiBoNewScraper(IHttpClientFactory httpClientFactory, IOptions<WeiBoScraperOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        /// <summary>
        /// 标识
        /// </summary>
        public string Source => "WeiBo";

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
                    _options.UserAgent,
                    _options.Cookie);
                
                var html = await ScraperHttpClientHelper.GetStringAsync(
                    client,
                    $"{_options.BaseUrl}{_options.NewsUrl}");
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                var rows = htmlDocument.DocumentNode.SelectNodes("//*[@id='pl_top_realtimehot']//table/tbody/tr");
                var newsResult = new AneiangGenericListResult<NewsItem>();
                if (rows == null) return newsResult;

                // 跳过第一行（可能是表头）
                for (var i = 1; i < rows.Count; i++)
                {
                    var row = rows[i];
                    var linkNode =
                        row.SelectSingleNode(".//td[@class='td-02']/a[not(contains(@href, 'javascript:void(0);'))]");

                    if (linkNode != null)
                    {
                        var title = linkNode.InnerText?.Trim();
                        var href = linkNode.GetAttributeValue("href", "");

                        if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(href))
                        {
                            var flagNode = row.SelectSingleNode(".//td[@class='td-03']");
                            var flag = flagNode?.InnerText?.Trim();

                            string flagUrl = null;
                            if (flag == "新")
                            {
                                flagUrl = "https://simg.s.weibo.com/moter/flags/1_0.png";
                            }
                            else if (flag == "热")
                            {
                                flagUrl = "https://simg.s.weibo.com/moter/flags/2_0.png";
                            }

                            var newsItem = new NewsItem
                            {
                                Id = title,
                                Title = title,
                                Url = $"{_options.BaseUrl}{href}",
                                MobileUrl = $"{_options.BaseUrl}{href}"
                            };
                            newsItem.SetProperty("Icon", flagUrl);
                            newsResult.Data.Add(newsItem);
                        }
                    }
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
