using System;
using System.Collections.Generic;
using System.Linq;

namespace Aneiang.Pa.Core.Scraper
{
    /// <summary>
    /// 爬虫注册表默认实现
    /// </summary>
    public class ScraperRegistry : IScraperRegistry
    {
        private readonly Dictionary<(string Category, string Source), IScraper> _scrapers;
        private readonly HashSet<string> _categories;

        /// <summary>
        /// 初始化爬虫注册表
        /// </summary>
        public ScraperRegistry(IEnumerable<IScraper> scrapers)
        {
            _scrapers = new Dictionary<(string, string), IScraper>();
            _categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var scraper in scrapers)
            {
                var descriptor = scraper.Descriptor;
                if (descriptor == null || string.IsNullOrWhiteSpace(descriptor.Category) || string.IsNullOrWhiteSpace(descriptor.Source))
                    continue;

                var key = (NormalizeKey(descriptor.Category), NormalizeKey(descriptor.Source));
                _scrapers[key] = scraper;
                _categories.Add(descriptor.Category);
            }
        }

        /// <inheritdoc />
        public IScraper? GetScraper(string category, string source)
        {
            if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(source))
                return null;

            _scrapers.TryGetValue((NormalizeKey(category), NormalizeKey(source)), out var scraper);
            return scraper;
        }

        /// <inheritdoc />
        public IEnumerable<IScraper> GetScrapers(string? category = null)
        {
            if (string.IsNullOrWhiteSpace(category))
                return _scrapers.Values;

            return _scrapers.Values
                .Where(s => string.Equals(s.Descriptor.Category, category, StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc />
        public IEnumerable<ScraperDescriptor> GetDescriptors(string? category = null)
        {
            return GetScrapers(category).Select(s => s.Descriptor);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetCategories()
        {
            return _categories;
        }

        /// <inheritdoc />
        public bool Contains(string category, string source)
        {
            return GetScraper(category, source) != null;
        }

        private static string NormalizeKey(string value) => value.ToLowerInvariant();
    }
}
