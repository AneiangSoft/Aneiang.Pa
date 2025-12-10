using System;
using System.Collections.Generic;
using System.Linq;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.Data;

namespace Aneiang.Pa.News
{
    /// <summary>
    ///     新闻爬取器工厂
    /// </summary>
    public class NewsScraperFactory : INewsScraperFactory
    {
        private readonly Dictionary<string, INewsScraper> _scraperMap;

        /// <summary>
        ///     新闻爬取器工厂
        /// </summary>
        public NewsScraperFactory(IEnumerable<INewsScraper> scrapers)
        {
            _scraperMap = scrapers.ToDictionary(s => s.Source, s => s);
        }

        /// <summary>
        ///     获取指定爬取器
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public INewsScraper GetScraper(ScraperSource source)
        {
            if (_scraperMap.TryGetValue(source.ToString(), out var scraper)) return scraper;
            throw new ArgumentException($"No scraper registered for source: {source}");
        }
    }

    /// <summary>
    ///     新闻爬取器工厂
    /// </summary>
    public interface INewsScraperFactory
    {
        /// <summary>
        ///     获取指定爬取器
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        INewsScraper GetScraper(ScraperSource source);
    }
}