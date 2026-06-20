using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Aneiang.Pa.ZhiHu.Models
{
    public class ZhiHuOriginalResult
    {
        public List<ZhiHuOriginalItem> data { get; set; } = new List<ZhiHuOriginalItem>();
    }

    public class ZhiHuOriginalItem
    {
        public string type { get; set; }
        public string style_type { get; set; }
        public string id { get; set; }
        public string card_id { get; set; }
        public FeedSpecific feed_specific { get; set; }
        public Target target { get; set; }
        public string attached_info { get; set; }

    }

    public class FeedSpecific
    {
        public int answer_count { get; set; }
    }

    public class Target
    {
        public Title_Area title_area { get; set; }
        public Excerpt_Area excerpt_area { get; set; }
        public Image_Area image_area { get; set; }
        public Metrics_Area metrics_area { get; set; }
        public Label_Area label_area { get; set; }
        public Link link { get; set; }
    }

    public class Title_Area
    {
        public string text { get; set; }
    }

    public class Excerpt_Area
    {
        public string text { get; set; }
    }

    public class Image_Area
    {
        public string url { get; set; }
    }

    public class Metrics_Area
    {
        public string text { get; set; }
        public string font_color { get; set; }
        public string background { get; set; }
        public string weight { get; set; }
    }

    public class Label_Area
    {
        public string type { get; set; }
        public int trend { get; set; }
        public string night_color { get; set; }
        public string normal_color { get; set; }
    }

    public class Link
    {
        public string url { get; set; }
    }
}
