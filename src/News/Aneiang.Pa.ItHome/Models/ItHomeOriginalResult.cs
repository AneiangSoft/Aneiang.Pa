using Aneiang.Pa.Dynamic.Attributes;
using System;

namespace Aneiang.Pa.ItHome.Models
{
    [HtmlContainer("ul", htmlClass: "bl", index: 1)]
    [HtmlItem("li",htmlClass: "")]
    public class ItHomeOriginalResult
    {
        [HtmlValue("a",htmlClass: "title")]
        public string Title { get; set; }

        [HtmlValue("a", htmlClass: "title", attribute: "href")]
        public string Id { get; set; }

        [HtmlValue("a", htmlClass: "title", attribute: "href")]
        public string Url { get; set; }

        [HtmlValue("div", htmlClass: "m")]
        public string Desc { get; set; }

        [HtmlValue("div", htmlClass: "c", attribute: "data-ot")]
        public string CreateTime { get; set; }
        [HtmlValue(htmlXPath: ".//div[@class=\"tags\"]/a[1]")]
        public string Tags { get; set; }
    }
}
