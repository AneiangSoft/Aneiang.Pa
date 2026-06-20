using Aneiang.Pa.Dynamic.Attributes;

namespace Aneiang.Pa.CnBlog.Models
{
    [HtmlContainer("div", htmlClass: "post-list",htmlId: "post_list", index: 1)]
    [HtmlItem("article",htmlClass: "post-item")]
    public class CnBlogOriginalResult
    {
        [HtmlValue("a",htmlClass: "post-item-title")]
        public string Title { get; set; }

        [HtmlValue(".",attribute: "data-post-id")]
        public string Id { get; set; }

        [HtmlValue("a", htmlClass: "post-item-title",attribute: "href")]
        public string Url { get; set; }

        [HtmlValue(htmlXPath:".//a[@class=\"post-item-author\"]/span")]
        public string AuthorName { get; set; }

        [HtmlValue("a", htmlClass: "post-item-author", attribute: "href")]
        public string AuthorUrl { get; set; }

        [HtmlValue("p", htmlClass: "post-item-summary")]
        public string Desc { get; set; }

        [HtmlValue(htmlXPath: ".//footer[@class=\"post-item-foot\"]/span[1]")]
        public string CreateTime { get; set; }

        [HtmlValue(htmlXPath: ".//footer[@class=\"post-item-foot\"]/a[2]")]
        public string CommentCount { get; set; }

        [HtmlValue(htmlXPath: ".//footer[@class=\"post-item-foot\"]/a[3]")]
        public string LikeCount { get; set; }

        [HtmlValue(htmlXPath: ".//footer[@class=\"post-item-foot\"]/a[4]")]
        public string ReadCount { get; set; }
    }


}
