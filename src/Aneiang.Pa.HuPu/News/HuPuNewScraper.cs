using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.HuPu.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Aneiang.Pa.HuPu.News
{
    /// <summary>
    /// 虎扑热门爬虫
    /// </summary>
    public class HuPuNewScraper : IHuPuNewScraper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HuPuScraperOptions _options;
        /// <summary>
        /// 虎扑热门爬虫
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="options"></param>
        public HuPuNewScraper(IHttpClientFactory httpClientFactory, IOptions<HuPuScraperOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        /// <summary>
        /// 标识
        /// </summary>
        public string Source => "HuPu";


        /// <summary>
        /// 获取热门消息
        /// </summary>

        public async Task<NewsResult> GetNewsAsync()
        {
            try
            {
                _options.Check();
                var newsResult = new NewsResult();
                var client = _httpClientFactory.CreateClient(PaConsts.DefaultHttpClientName);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);
                client.DefaultRequestHeaders.Referrer = new Uri(_options.BaseUrl);
                var html = await client.GetStringAsync($"{_options.BaseUrl}{_options.NewsUrl}");
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);



                // 查找热榜项
                var items = htmlDocument.DocumentNode.SelectNodes("//li[contains(@class, 'bbs-sl-web-post-body')]");
                if (items == null || items.Count == 0)
                {
                    // 尝试备用选择器
                    items = htmlDocument.DocumentNode.SelectNodes("//li[@class='bbs-sl-web-post-body']");

                    if (items == null || items.Count == 0)
                    {
                        // 如果HtmlAgilityPack解析失败，回退到正则表达式
                        return ParseWithRegex(html);
                    }
                }

                // 4. 提取数据
                var hotItems = new List<NewsItem>();
                var urlBase = _options.BaseUrl.TrimEnd('/');

                foreach (var item in items)
                {
                    var link = item.SelectSingleNode(".//a[contains(@class, 'p-title')]");

                    if (link == null)
                    {
                        // 尝试其他可能的类名
                        link = item.SelectSingleNode(".//a[@class='p-title']");
                    }

                    if (link != null)
                    {
                        var href = link.GetAttributeValue("href", "");
                        var title = link.InnerText?.Trim();

                        if (!string.IsNullOrEmpty(href) && !string.IsNullOrEmpty(title))
                        {
                            // 确保href是完整的URL
                            if (!href.StartsWith("http"))
                            {
                                href = href.StartsWith("/") ? href : $"/{href}";
                                href = $"{urlBase}{href}";
                            }

                            hotItems.Add(new NewsItem
                            {
                                Id = href,
                                Title = title,
                                Url = href,
                                MobileUrl = href
                            });
                        }
                    }
                }

                if (hotItems.Count == 0)
                {
                    // 如果没有找到数据，回退到正则表达式
                    return ParseWithRegex(html);
                }

                newsResult.Data = hotItems;
                return newsResult;
            }
            catch (Exception e)
            {
                return new NewsResult(false, e.Message);
            }
        }

        private NewsResult ParseWithRegex(string html)
        {
            var result = new NewsResult();

            try
            {
                // 使用正则表达式作为备用解析方式
                var regex = new Regex(
                    @"<li class=""bbs-sl-web-post-body"">[\s\S]*?<a href=""(\/[^""]+?\.html)""[^>]*?class=""p-title""[^>]*>([^<]+)<\/a>",
                    RegexOptions.Compiled);

                var matches = regex.Matches(html);

                if (matches.Count == 0)
                {
                    return new NewsResult();
                }

                var hotItems = new List<NewsItem>();
                var urlBase = _options.BaseUrl.TrimEnd('/');

                foreach (Match match in matches)
                {
                    if (match.Groups.Count >= 3)
                    {
                        var path = match.Groups[1].Value;
                        var title = match.Groups[2].Value.Trim();
                        var fullUrl = $"{urlBase}{path}";
                        hotItems.Add(new NewsItem
                        {
                            Id = path,
                            Title = title,
                            Url = fullUrl,
                            MobileUrl = fullUrl
                        });
                    }
                }

                result.Data = hotItems;
            }
            catch (Exception ex)
            {
                result.IsSuccessd = false;
                result.ErrorMessage = $"正则表达式解析失败: {ex.Message}";
            }

            return result;
        }
    }
}
