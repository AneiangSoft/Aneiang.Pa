using System.Collections.Generic;

namespace Aneiang.Pa.DouBan.Models
{
    public class DouBanOriginalResult
    {
        public List<DouBanOriginalItem> items { get; set; } = new List<DouBanOriginalItem>();
    }


    public class DouBanOriginalItem
    {
        public Rating rating { get; set; }
        public string title { get; set; }
        public Pic pic { get; set; }
        public bool is_new { get; set; }
        public string uri { get; set; }
        public string episodes_info { get; set; }
        public string card_subtitle { get; set; }
        public string type { get; set; }
        public string id { get; set; }
    }

    public class Rating
    {
        public int count { get; set; }
        public int max { get; set; }
        public float star_count { get; set; }
        public float value { get; set; }
    }

    public class Pic
    {
        public string large { get; set; }
        public string normal { get; set; }
    }


}
