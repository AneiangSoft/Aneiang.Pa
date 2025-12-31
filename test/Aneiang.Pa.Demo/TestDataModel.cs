using Aneiang.Pa.Dynamic.Attributes;

namespace Aneiang.Pa.Demo
{
    [HtmlHeader("https://www.de62.com/listinfo-16-0.html")]
    [HtmlContainer("div", htmlClass: "blogs-list", index: 1)]
    [HtmlItem("li")]
    public class TestDataSet
    {
        [HtmlValue("h2/a")]
        public string Title { get; set; }

        [HtmlValue("h2/a", attribute: "href")]
        public string Url { get; set; }

        [HtmlValue("i/a/img", attribute: "src")]
        public string Icon { get; set; }

        [HtmlValue("p")]
        public string Desc { get; set; }
    }


    [HtmlHeader("https://baike.baidu.com/item/%E5%8F%AF%E5%8F%A3%E5%8F%AF%E4%B9%90%E5%85%AC%E5%8F%B8/1612740")]
    public class TestData
    {
        [HtmlValue(htmlXPath: "//h1[@class=\"lemmaTitle_RF_t7 J-lemma-title\"]")]
        public string Title { get; set; }

        [HtmlValue(htmlXPath: "//div[@class=\"para_SkYG9 summary_Iw34L MARK_MODULE\"]")]
        public string Desc { get; set; }
    }
}
