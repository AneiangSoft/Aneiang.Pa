using System;
using System.Linq;
using System.Threading.Tasks;
using Aneiang.Pa._36kr.Models;
using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.Dynamic;
using Microsoft.Extensions.Options;

namespace Aneiang.Pa._36kr.News
{
    /// <summary>
    /// 博客园热门爬虫
    /// </summary>
    public class _36krNewScraper : I36krNewScraper
    {
        private readonly _36krScraperOptions _options;
        private readonly IDynamicScraper _dynamicScraper;

        /// <summary>
        /// 博客园热门爬虫
        /// </summary>
        /// <param name="options"></param>
        /// <param name="dynamicScraper"></param>
        public _36krNewScraper(IOptions<_36krScraperOptions> options, IDynamicScraper dynamicScraper)
        {
            _dynamicScraper = dynamicScraper;
            _options = options.Value;
        }

        /// <summary>
        /// 标识
        /// </summary>
        public string Source => "_36kr";

        /// <summary>
        /// 获取热门消息
        /// </summary>
        public async Task<AneiangGenericListResult<NewsItem>> GetNewsAsync()
        {
            try
            {
                _options.Check();
                var data = await _dynamicScraper.DatasetScraperAsync<_36krOriginalResult>($"{_options.BaseUrl}{_options.NewsUrl}{DateTime.Now:yyyy-MM-dd}/1",
                    _options.BaseUrl,
                    _options.UserAgent);

                var newsResult = new AneiangGenericListResult<NewsItem>();
                foreach (var item in data)
                {
                    var newsItem = new NewsItem
                    {
                        Id = item.Id,
                        Title = item.Title,
                        Url = $"{_options.BaseUrl}{item.Url}",
                        MobileUrl = $"{_options.BaseUrl}{item.Url}",
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
