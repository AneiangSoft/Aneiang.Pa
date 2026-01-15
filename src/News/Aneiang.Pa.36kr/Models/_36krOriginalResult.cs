using Aneiang.Pa.Dynamic.Attributes;

namespace Aneiang.Pa._36kr.Models
{
    [HtmlContainer("div", htmlClass: "article-list", index: 1)]
    [HtmlItem(htmlXPath: ".//div[contains(@class, 'article-wrapper')]")]
    //[HtmlItem("div", htmlClass: "article-wrapper")]
    public class _36krOriginalResult
    {
        [HtmlValue("a",htmlClass: "article-item-title weight-bold")]
        public string Title { get; set; }

        [HtmlValue("a", htmlClass: "article-item-title weight-bold", attribute: "href")]
        public string Id { get; set; }

        [HtmlValue("a", htmlClass: "article-item-title weight-bold", attribute: "href")]
        public string Url { get; set; }

        [HtmlValue("a", htmlClass: "article-item-description ellipsis-2")]
        public string Desc { get; set; }

        [HtmlValue(htmlXPath: ".//span[@class=\"kr-flow-bar-hot\"]/span[1]")]
        public string Hot { get; set; }
        [HtmlValue("a", htmlClass: "kr-flow-bar-author")]
        public string Author { get; set; }
    }
}
