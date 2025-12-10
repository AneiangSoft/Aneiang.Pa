using System.Collections.Generic;

namespace Aneiang.Pa.Bilibili.Models
{
    public class BilibiliSearchOriginalResult
    {
        public int code { get; set; }
        public string exp_str { get; set; }
        public List<BilibiliSearchOriginalItem> list { get; set; } = new List<BilibiliSearchOriginalItem>();
    }
    public class BilibiliSearchOriginalItem
    {
        public int hot_id { get; set; }
        public string keyword { get; set; }
        public string show_name { get; set; }
        public double score { get; set; }
        public int word_type { get; set; }
        public int goto_type { get; set; }
        public string goto_value { get; set; }
        public string icon { get; set; }
        public object[] live_id { get; set; }
        public int call_reason { get; set; }
        public string heat_layer { get; set; }
        public int pos { get; set; }
        public int id { get; set; }
        public string status { get; set; }
        public string name_type { get; set; }
        public int resource_id { get; set; }
        public int set_gray { get; set; }
        public object[] card_values { get; set; }
        public int heat_score { get; set; }
        public Stat_Datas stat_datas { get; set; }
    }
    public class Stat_Datas
    {
        public string is_commercial { get; set; }
        public string stime { get; set; }
        public string etime { get; set; }
    }
}
