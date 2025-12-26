using System.Collections.Generic;

namespace Aneiang.Pa.BaiDu.Models
{
    public class BaiduOriginalResult
    {
        public BaiduOriginalData data { get; set; } = new BaiduOriginalData();
    }

    public class BaiduOriginalData
    {
        public List<BaiduOriginalCard> cards { get; set; } = new List<BaiduOriginalCard>();
    }

    public class BaiduOriginalCard
    {
        public List<BaiduOriginalContentItem> content { get; set; } = new List<BaiduOriginalContentItem>();
    }
    public class BaiduOriginalContentItem
    {
        public string appUrl { get; set; }
        public string desc { get; set; }
        public string hotChange { get; set; }
        public string hotScore { get; set; }
        public string hotTag { get; set; }
        public string img { get; set; }
        public int index { get; set; }
        public string indexUrl { get; set; }
        public string query { get; set; }
        public string rawUrl { get; set; }
        public object[] show { get; set; }
        public string url { get; set; }
        public string word { get; set; }
        public bool isTop { get; set; }
    }

}
