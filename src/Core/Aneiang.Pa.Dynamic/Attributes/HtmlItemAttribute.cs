using System;

namespace Aneiang.Pa.Dynamic.Attributes
{
    /// <summary>
    /// 数据项特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class HtmlItemAttribute : Attribute
    {
        /// <summary>
        /// HTML XPath
        /// </summary>
        public string? HtmlXPath { get; set; }

        /// <summary>
        /// HTML标签
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
        /// 数据项特性
        /// </summary>
        /// <param name="htmlTag">HTML标签</param>
        /// <param name="htmlXPath">HTML XPath</param>
        /// <param name="htmlId">HTML Id</param>
        /// <param name="htmlClass">HTML Class</param>
        /// <param name="index">存在多个，需要指定索引</param>
        public HtmlItemAttribute(string? htmlTag = null, string? htmlXPath = null, string? htmlId = null, string? htmlClass = null, int index = 0)
        {
            HtmlXPath = htmlXPath;
            HtmlTag = htmlTag;
            HtmlId = htmlId;
            HtmlClass = htmlClass;
            Index = index;
        }
    }
}
