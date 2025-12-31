using Aneiang.Pa.Core.Data;
using Aneiang.Pa.Dynamic.Attributes;
using Aneiang.Pa.Dynamic.Extensions;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Aneiang.Pa.Core.News;

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
        /// <param name="httpClientFactory"></param>
        public DynamicScraper(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// 通用数据抓取，需要配合HtmlHeaderAttribute使用
        /// </summary>
        public async Task<T> DataScraperAsync<T>()
            where T : new()
        {
            try
            {
                var type = typeof(T);
                var htmlHeaderAttribute = type.GetCustomAttribute<HtmlHeaderAttribute>();
                if (htmlHeaderAttribute == null || string.IsNullOrWhiteSpace(htmlHeaderAttribute.Url))
                    throw new Exception("Url is not set in HtmlHeader Attribute");
                var url = htmlHeaderAttribute.Url;
                var userAgent = htmlHeaderAttribute.UserAgent;
                var referer = htmlHeaderAttribute.Referrer;
                return await DataScraperAsync<T>(url, referer, userAgent);
            }
            catch (Exception e)
            {
                throw new Exception("DataScraperAsync Error: " + e.Message);
            }
        }

        /// <summary>
        /// 通用数据集抓取，需要配合HtmlHeaderAttribute使用
        /// </summary>
        public async Task<List<T>> DatasetScraperAsync<T>()
            where T : new()
        {
            try
            {
                var type = typeof(T);
                var htmlHeaderAttribute = type.GetCustomAttribute<HtmlHeaderAttribute>();
                if (htmlHeaderAttribute == null || string.IsNullOrWhiteSpace(htmlHeaderAttribute.Url))
                    throw new Exception("Url is not set in HtmlHeader Attribute");
                var url = htmlHeaderAttribute.Url;
                var userAgent = htmlHeaderAttribute.UserAgent;
                var referer = htmlHeaderAttribute.Referrer;
                return await DatasetScraperAsync<T>(url, referer, userAgent);
            }
            catch (Exception e)
            {
                throw new Exception("DataScraperAsync Error: " + e.Message);
            }
        }

        /// <summary>
        /// 通用数据抓取
        /// </summary>
        public async Task<T> DataScraperAsync<T>(string url, string? referer = null, string? userAgent = null)
            where T : new()
        {
            try
            {
                var type = typeof(T);
                var client = _httpClientFactory.CreateClient(PaConsts.DefaultHttpClientName);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent ?? UserAgentGenerator.GetRandomUserAgent());
                if (referer != null) client.DefaultRequestHeaders.Referrer = new Uri(referer);
                var html = await client.GetStringAsync(url);
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                var instance = new T();
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var valueAttribute = property.GetCustomAttribute<HtmlValueAttribute>();
                    if (valueAttribute == null || (string.IsNullOrWhiteSpace(valueAttribute.HtmlTag) && string.IsNullOrWhiteSpace(valueAttribute.HtmlXPath)))
                        throw new Exception("HtmlValue Attribute is not set");

                    var valueXpath = BuildXPath(valueAttribute);
                    var valueNode = htmlDocument.DocumentNode.SelectSingleNode(valueXpath);
                    var value = string.IsNullOrWhiteSpace(valueAttribute.HtmlAttribute)
                        ? valueNode?.InnerText
                        : valueNode?.GetAttributeValue<string>(valueAttribute.HtmlAttribute, "");
                    if (value != null && valueAttribute.IsTrim) value = value.Trim();
                    property.SetPropertyValue(instance, value ?? "");
                }

                return instance;
            }
            catch (Exception e)
            {
                throw new Exception("DataScraperAsync Error: " + e.Message);
            }
        }

        /// <summary>
        /// 通用数据集抓取
        /// </summary>
        public async Task<List<T>> DatasetScraperAsync<T>(string url, string? referer = null, string? userAgent = null) where T : new()
        {
            try
            {
                var type = typeof(T);
                if (string.IsNullOrWhiteSpace(url))
                {
                    var htmlHeaderAttribute = type.GetCustomAttribute<HtmlHeaderAttribute>();
                    if (htmlHeaderAttribute == null || string.IsNullOrWhiteSpace(htmlHeaderAttribute.Url))
                        throw new Exception("Url is not set in HtmlHeader Attribute");
                    url = htmlHeaderAttribute.Url;
                    userAgent = htmlHeaderAttribute.UserAgent;
                    referer = htmlHeaderAttribute.Referrer;
                }

                var client = _httpClientFactory.CreateClient(PaConsts.DefaultHttpClientName);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent ?? UserAgentGenerator.GetRandomUserAgent());
                if (referer != null) client.DefaultRequestHeaders.Referrer = new Uri(referer);
                var html = await client.GetStringAsync(url);
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                var htmlContainerAttribute = type.GetCustomAttribute<HtmlContainerAttribute>();
                if (htmlContainerAttribute == null || (string.IsNullOrWhiteSpace(htmlContainerAttribute.HtmlTag) && string.IsNullOrWhiteSpace(htmlContainerAttribute.HtmlXPath)))
                    throw new Exception("HtmlContainer Attribute is not set");
                var htmlItemAttribute = type.GetCustomAttribute<HtmlItemAttribute>();
                if (htmlItemAttribute == null || (string.IsNullOrWhiteSpace(htmlItemAttribute.HtmlTag) && string.IsNullOrWhiteSpace(htmlItemAttribute.HtmlXPath)))
                    throw new Exception("HtmlItem Attribute is not set");

                var containerXpath = BuildXPath(htmlContainerAttribute);
                var containerNode = htmlDocument.DocumentNode.SelectSingleNode(containerXpath);

                var itemsXpath = BuildXPath(htmlItemAttribute);
                var itemNodes = containerNode.SelectNodes(itemsXpath);

                var properties = new Dictionary<PropertyInfo, HtmlValueAttribute>();
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var attribute = property.GetCustomAttribute<HtmlValueAttribute>();
                    properties.Add(property, attribute);
                }
                var list = new List<T>();

                foreach (var itemNode in itemNodes)
                {
                    var instance = new T();
                    foreach (var propertyAttr in properties)
                    {
                        HtmlNode valueNode;
                        var valueAttribute = propertyAttr.Value;
                        if (valueAttribute == null || (string.IsNullOrWhiteSpace(valueAttribute.HtmlTag) && string.IsNullOrWhiteSpace(valueAttribute.HtmlXPath)))
                            throw new Exception("HtmlValue Attribute is not set");

                        if (valueAttribute.HtmlTag == ".")
                        {
                            valueNode = itemNode;
                        }
                        else
                        {
                            var valueXpath = BuildXPath(valueAttribute);
                            valueNode =
                                itemNode.SelectSingleNode(valueXpath);
                        }
                        var value = string.IsNullOrWhiteSpace(valueAttribute.HtmlAttribute)
                            ? valueNode?.InnerText
                            : valueNode?.GetAttributeValue<string>(valueAttribute.HtmlAttribute, "");
                        if (value != null && valueAttribute.IsTrim) value = value.Trim();
                        propertyAttr.Key.SetPropertyValue(instance, value ?? "");
                    }
                    list.Add(instance);
                }
                return list;
            }
            catch (Exception e)
            {
                throw new Exception("DatasetScraperAsync Error: " + e.Message);
            }
        }

        /// <summary>
        /// 生成XPath
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        private static string BuildXPath(Attribute attribute)
        {
            var xpath = "";
            var containsXpath = "";
            var htmlId = "";
            var htmlClass = "";
            var index = 0;

            switch (attribute)
            {
                case HtmlContainerAttribute htmlContainerAttribute:
                    {
                        if (!string.IsNullOrWhiteSpace(htmlContainerAttribute.HtmlXPath))
                        {
                            return htmlContainerAttribute.HtmlXPath;
                        }
                        xpath = $"//{htmlContainerAttribute.HtmlTag}";
                        htmlId = htmlContainerAttribute.HtmlId;
                        htmlClass = htmlContainerAttribute.HtmlClass;
                        index = htmlContainerAttribute.Index;
                        break;
                    }
                case HtmlItemAttribute htmlItemAttribute:
                    {
                        if (!string.IsNullOrWhiteSpace(htmlItemAttribute.HtmlXPath))
                        {
                            return htmlItemAttribute.HtmlXPath;
                        }
                        xpath = $".//{htmlItemAttribute.HtmlTag}";
                        htmlId = htmlItemAttribute.HtmlId;
                        htmlClass = htmlItemAttribute.HtmlClass;
                        index = htmlItemAttribute.Index;
                        break;
                    }
                case HtmlValueAttribute htmlValueAttribute:
                    {
                        if (!string.IsNullOrWhiteSpace(htmlValueAttribute.HtmlXPath))
                        {
                            return htmlValueAttribute.HtmlXPath;
                        }
                        xpath = $".//{htmlValueAttribute.HtmlTag}";
                        htmlId = htmlValueAttribute.HtmlId;
                        htmlClass = htmlValueAttribute.HtmlClass;
                        index = htmlValueAttribute.Index;
                        break;
                    }
            }

            if (!string.IsNullOrWhiteSpace(htmlId))
            {
                containsXpath = $"@id='{htmlId}' and";
            }
            if (!string.IsNullOrWhiteSpace(htmlClass))
            {
                var classList = htmlClass.Trim().Split(" ");
                foreach (var c in classList)
                {
                    containsXpath += $"@class='{c}' and";
                }
            }

            if (containsXpath.EndsWith("and"))
            {
                containsXpath = containsXpath.Remove(containsXpath.Length - 4, 4);
            }

            if (!string.IsNullOrWhiteSpace(containsXpath))
            {
                xpath = $"{xpath}[{containsXpath}]";
            }

            if (index > 0)
            {
                xpath = $"({xpath})[{index}]";
            }
            return xpath;
        }

    }
}
