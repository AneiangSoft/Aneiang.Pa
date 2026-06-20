using Aneiang.Pa.Dynamic.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aneiang.Pa.Dynamic
{
    /// <summary>
    /// 动态爬虫类型元数据（含缓存的反射结果与预编译 XPath）
    /// </summary>
    internal sealed class DynamicScraperMetadata
    {
        private static readonly ConcurrentDictionary<Type, DynamicScraperMetadata> Cache = new ConcurrentDictionary<Type, DynamicScraperMetadata>();

        /// <summary>
        /// HTML 头部特性
        /// </summary>
        public HtmlHeaderAttribute? Header { get; }

        /// <summary>
        /// 容器特性
        /// </summary>
        public HtmlContainerAttribute? Container { get; }

        /// <summary>
        /// 容器 XPath（预编译）
        /// </summary>
        public string? ContainerXPath { get; }

        /// <summary>
        /// 数据项特性
        /// </summary>
        public HtmlItemAttribute? Item { get; }

        /// <summary>
        /// 数据项 XPath（预编译）
        /// </summary>
        public string? ItemXPath { get; }

        /// <summary>
        /// 字段绑定列表
        /// </summary>
        public IReadOnlyList<DynamicValueBinding> Values { get; }

        private DynamicScraperMetadata(Type type)
        {
            Header = type.GetCustomAttribute<HtmlHeaderAttribute>();
            Container = type.GetCustomAttribute<HtmlContainerAttribute>();
            Item = type.GetCustomAttribute<HtmlItemAttribute>();

            if (Container != null) ContainerXPath = XPathBuilder.Build(Container);
            if (Item != null) ItemXPath = XPathBuilder.Build(Item);

            var bindings = new List<DynamicValueBinding>();
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var attr = prop.GetCustomAttribute<HtmlValueAttribute>();
                if (attr == null) continue;
                bindings.Add(new DynamicValueBinding(prop, attr, XPathBuilder.Build(attr)));
            }
            Values = bindings;
        }

        public static DynamicScraperMetadata For(Type type)
            => Cache.GetOrAdd(type, t => new DynamicScraperMetadata(t));
    }

    internal sealed class DynamicValueBinding
    {
        public PropertyInfo Property { get; }
        public HtmlValueAttribute Attribute { get; }
        public string XPath { get; }

        public DynamicValueBinding(PropertyInfo property, HtmlValueAttribute attribute, string xpath)
        {
            Property = property;
            Attribute = attribute;
            XPath = xpath;
        }
    }

    internal static class XPathBuilder
    {
        public static string Build(Attribute attribute)
        {
            string xpath;
            string containsXpath = "";
            string htmlId = "";
            string htmlClass = "";
            int index = 0;

            switch (attribute)
            {
                case HtmlContainerAttribute c:
                    if (!string.IsNullOrWhiteSpace(c.HtmlXPath)) return c.HtmlXPath;
                    xpath = $"//{c.HtmlTag}";
                    htmlId = c.HtmlId;
                    htmlClass = c.HtmlClass;
                    index = c.Index;
                    break;
                case HtmlItemAttribute i:
                    if (!string.IsNullOrWhiteSpace(i.HtmlXPath)) return i.HtmlXPath;
                    xpath = $".//{i.HtmlTag}";
                    htmlId = i.HtmlId;
                    htmlClass = i.HtmlClass;
                    index = i.Index;
                    break;
                case HtmlValueAttribute v:
                    if (!string.IsNullOrWhiteSpace(v.HtmlXPath)) return v.HtmlXPath;
                    xpath = $".//{v.HtmlTag}";
                    htmlId = v.HtmlId;
                    htmlClass = v.HtmlClass;
                    index = v.Index;
                    break;
                default:
                    return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(htmlId))
                containsXpath = $"@id='{htmlId}' and";
            if (!string.IsNullOrWhiteSpace(htmlClass))
                containsXpath += $"@class='{htmlClass}'";

            if (containsXpath.EndsWith("and"))
                containsXpath = containsXpath.Remove(containsXpath.Length - 4, 4);

            if (!string.IsNullOrWhiteSpace(containsXpath))
                xpath = $"{xpath}[{containsXpath}]";

            if (index > 0)
                xpath = $"({xpath})[{index}]";

            return xpath;
        }
    }
}
