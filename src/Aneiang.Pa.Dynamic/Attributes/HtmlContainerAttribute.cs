using System;

namespace Aneiang.Pa.Dynamic.Attributes
{
    /// <summary>
    /// 容器特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class HtmlContainerAttribute : Attribute
    {
        /// <summary>
        /// HTML XPath
        /// </summary>
        public string? HtmlXPath { get; set; }

        /// <summary>
        /// HTML容器标签
        /// </summary>
        public string? HtmlTag { get; set; }

        /// <summary>
        /// HTML Id
        /// </summary>
        public string? HtmlId { get; set; }

        /// <summary>
        /// HTML Class
        /// </summary>
        public string? HtmlClass { get; set; }

        /// <summary>
        /// 存在多个，需要指定索引
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 容器特性
        /// </summary>
        /// <param name="htmlTag">HTML标签</param>
        /// <param name="htmlXPath">Html XPath</param>
        /// <param name="htmlId">HTML Id</param>
        /// <param name="htmlClass">HTML Class</param>
        /// <param name="index">存在多个，需要指定索引,从1开始</param>
        public HtmlContainerAttribute(string? htmlTag = null, string? htmlXPath = null, string? htmlId = null, string? htmlClass = null, int index = 0)
        {
            HtmlXPath = htmlXPath;
            HtmlTag = htmlTag;
            HtmlId = htmlId;
            HtmlClass = htmlClass;
            Index = index;
        }
    }
}
