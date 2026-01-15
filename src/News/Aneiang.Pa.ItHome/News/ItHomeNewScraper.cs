using System;
using System.Linq;
using System.Threading.Tasks;
using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.Dynamic;
using Aneiang.Pa.ItHome.Models;
using Microsoft.Extensions.Options;

namespace Aneiang.Pa.ItHome.News
{
    /// <summary>
    /// 博客园热门爬虫
    /// </summary>
    public class ItHomeNewScraper : IItHomeNewScraper
    {
        private readonly ItHomeScraperOptions _options;
        private readonly IDynamicScraper _dynamicScraper;

        /// <summary>
        /// 博客园热门爬虫
        /// </summary>
        /// <param name="options"></param>
        /// <param name="dynamicScraper"></param>
        public ItHomeNewScraper(IOptions<ItHomeScraperOptions> options, IDynamicScraper dynamicScraper)
        {
            _dynamicScraper = dynamicScraper;
            _options = options.Value;
        }

        /// <summary>
        /// 标识
        /// </summary>
        public string Source => "ItHome";

        /// <summary>
        /// 获取热门消息
        /// </summary>
        public async Task<AneiangGenericListResult<NewsItem>> GetNewsAsync()
        {
            try
            {
                _options.Check();
                var data = await _dynamicScraper.DatasetScraperAsync<ItHomeOriginalResult>($"{_options.BaseUrl}{_options.NewsUrl}",
                    _options.BaseUrl,
                    _options.UserAgent);

                data = data.Where(a => !string.IsNullOrWhiteSpace(a.Tags)).ToList();

                var newsResult = new AneiangGenericListResult<NewsItem>();
                foreach (var item in data)
                {
                    var newsItem = new NewsItem
                    {
                        Id = item.Id,
                        Title = item.Title,
                        Url = item.Url,
                        MobileUrl = item.Url,
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
