using System.Collections.Generic;

namespace Aneiang.Pa.TouTiao.Models
{
    public class TouTiaoOriginalResult
    {
        public List<TouTiaoOriginalItem> data { get; set; } = new List<TouTiaoOriginalItem>();

        public string status { get; set; }
    }

    public class TouTiaoOriginalItem
    {
        public long ClusterId { get; set; }
        public string Title { get; set; }
        public string LabelUrl { get; set; }
        public string Label { get; set; }
        public string Url { get; set; }
        public string HotValue { get; set; }
        public string Schema { get; set; }
        public Labeluri LabelUri { get; set; }
        public string ClusterIdStr { get; set; }
        public int ClusterType { get; set; }
        public string QueryWord { get; set; }
        public Image Image { get; set; }
    }

    public class Labeluri
    {
        public string uri { get; set; }
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public object url_list { get; set; }
        public int image_type { get; set; }
    }

    public class Image
    {
        public string uri { get; set; }
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public Url_List[] url_list { get; set; }
        public int image_type { get; set; }
    }

    public class Url_List
    {
        public string url { get; set; }
    }

}
