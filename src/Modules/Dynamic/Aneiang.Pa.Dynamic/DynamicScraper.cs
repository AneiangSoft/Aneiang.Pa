using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.Core.Scraper;
using Aneiang.Pa.Dynamic.Extensions;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Aneiang.Pa.Dynamic
{
    /// <summary>
    /// 动态爬虫
    /// </summary>
    public class DynamicScraper : IDynamicScraper
    {
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// 动态爬虫
        /// </summary>
        public DynamicScraper(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// 爬虫元数据
        /// </summary>
        public ScraperDescriptor Descriptor { get; } = new ScraperDescriptor
        {
            Category = ScraperCategories.Dynamic,
            Source = "Dynamic",
            DisplayName = "动态爬虫"
        };

        /// <summary>
        /// 通用数据抓取，需要配合 HtmlHeaderAttribute 使用
        /// </summary>
        public async Task<T> DataScraperAsync<T>() where T : new()
        {
            var meta = DynamicScraperMetadata.For(typeof(T));
            if (meta.Header == null || string.IsNullOrWhiteSpace(meta.Header.Url))
                throw new InvalidOperationException("Url is not set in HtmlHeader Attribute");
            return await DataScraperAsync<T>(meta.Header.Url, meta.Header.Referrer, meta.Header.UserAgent).ConfigureAwait(false);
        }

        /// <summary>
        /// 通用数据集抓取，需要配合 HtmlHeaderAttribute 使用
        /// </summary>
        public async Task<List<T>> DatasetScraperAsync<T>() where T : new()
        {
            var meta = DynamicScraperMetadata.For(typeof(T));
            if (meta.Header == null || string.IsNullOrWhiteSpace(meta.Header.Url))
                throw new InvalidOperationException("Url is not set in HtmlHeader Attribute");
            return await DatasetScraperAsync<T>(meta.Header.Url, meta.Header.Referrer, meta.Header.UserAgent).ConfigureAwait(false);
        }

        /// <summary>
        /// 通用数据抓取
        /// </summary>
        public async Task<T> DataScraperAsync<T>(string url, string? referer = null, string? userAgent = null)
            where T : new()
        {
            try
            {
                var meta = DynamicScraperMetadata.For(typeof(T));
                var htmlDocument = await LoadHtmlAsync(url, referer, userAgent).ConfigureAwait(false);

                var instance = new T();
                foreach (var binding in meta.Values)
                {
                    var node = htmlDocument.DocumentNode.SelectSingleNode(binding.XPath);
                    var value = ExtractValue(node, binding.Attribute);
                    binding.Property.SetPropertyValue(instance, value ?? string.Empty);
                }
                return instance;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("DataScraperAsync Error: " + e.Message, e);
            }
        }

        /// <summary>
        /// 通用数据集抓取
        /// </summary>
        public async Task<List<T>> DatasetScraperAsync<T>(string url, string? referer = null, string? userAgent = null)
            where T : new()
        {
            try
            {
                var meta = DynamicScraperMetadata.For(typeof(T));
                if (string.IsNullOrWhiteSpace(url))
                {
                    if (meta.Header == null || string.IsNullOrWhiteSpace(meta.Header.Url))
                        throw new InvalidOperationException("Url is not set in HtmlHeader Attribute");
                    url = meta.Header.Url;
                    userAgent = meta.Header.UserAgent;
                    referer = meta.Header.Referrer;
                }
                if (meta.Container == null || string.IsNullOrWhiteSpace(meta.ContainerXPath))
                    throw new InvalidOperationException("HtmlContainer Attribute is not set");
                if (meta.Item == null || string.IsNullOrWhiteSpace(meta.ItemXPath))
                    throw new InvalidOperationException("HtmlItem Attribute is not set");

                var htmlDocument = await LoadHtmlAsync(url, referer, userAgent).ConfigureAwait(false);

                var containerNode = htmlDocument.DocumentNode.SelectSingleNode(meta.ContainerXPath);
                if (containerNode == null) return new List<T>();

                var itemNodes = containerNode.SelectNodes(meta.ItemXPath);
                if (itemNodes == null) return new List<T>();

                var list = new List<T>(itemNodes.Count);
                foreach (var itemNode in itemNodes)
                {
                    var instance = new T();
                    foreach (var binding in meta.Values)
                    {
                        HtmlNode? valueNode;
                        if (binding.Attribute.HtmlTag == ".")
                        {
                            valueNode = itemNode;
                        }
                        else
                        {
                            valueNode = itemNode.SelectSingleNode(binding.XPath);
                        }
                        var value = ExtractValue(valueNode, binding.Attribute);
                        binding.Property.SetPropertyValue(instance, value ?? string.Empty);
                    }
                    list.Add(instance);
                }
                return list;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("DatasetScraperAsync Error: " + e.Message, e);
            }
        }

        private async Task<HtmlDocument> LoadHtmlAsync(string url, string? referer, string? userAgent)
        {
            var client = _httpClientFactory.CreateClient(PaConsts.DefaultHttpClientName);
            client.DefaultRequestHeaders.UserAgent.Clear();
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent ?? UserAgentGenerator.GetRandomUserAgent());
            if (referer != null) client.DefaultRequestHeaders.Referrer = new Uri(referer);
            var html = await client.GetStringAsync(url).ConfigureAwait(false);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc;
        }

        private static string? ExtractValue(HtmlNode? node, Attributes.HtmlValueAttribute attr)
        {
            if (node == null) return null;
            var value = string.IsNullOrWhiteSpace(attr.HtmlAttribute)
                ? node.InnerText
                : node.GetAttributeValue<string>(attr.HtmlAttribute, "");
            if (value != null && attr.IsTrim) value = value.Trim();
            return value;
        }
    }
}
