using Aneiang.Pa.Dynamic.Attributes;

namespace Aneiang.Pa._36kr.Models
{
    [HtmlContainer("div", htmlClass: "newsflash-catalog-flow-list", index: 1)]
    [HtmlItem(htmlXPath: ".//div[contains(@class, 'flow-item')]")]
    public class _36krOriginalResult
    {
        [HtmlValue("a",htmlClass: "item-title")]
        public string Title { get; set; }

        [HtmlValue("a", htmlClass: "item-title", attribute: "href")]
        public string Id { get; set; }

        [HtmlValue("a", htmlClass: "item-title", attribute: "href")]
        public string Url { get; set; }

        [HtmlValue(htmlXPath: ".//div[@class=\"item-desc\"]/span[1]")]
        public string Desc { get; set; }
    }
}
