using System.Collections.Generic;

namespace Aneiang.Pa.Csdn.Models
{
    public class CsdnOriginalResult
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<CsdnOriginalItem> data { get; set; } = new List<CsdnOriginalItem>();
    }

    public class CsdnOriginalItem
    {
        public string period { get; set; }
        public string hotRankScore { get; set; }
        public string pcHotRankScore { get; set; }
        public bool loginUserIsFollow { get; set; }
        public string nickName { get; set; }
        public string avatarUrl { get; set; }
        public string userName { get; set; }
        public string articleTitle { get; set; }
        public string articleDetailUrl { get; set; }
        public string commentCount { get; set; }
        public string favorCount { get; set; }
        public string viewCount { get; set; }
        public object hotComment { get; set; }
        public string[] picList { get; set; }
        public object isNew { get; set; }
        public string productId { get; set; }
        public string productType { get; set; }
        public string recommendType { get; set; }
        public object report_data { get; set; }
    }

}
