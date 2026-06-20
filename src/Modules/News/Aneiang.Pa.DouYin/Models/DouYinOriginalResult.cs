using System.Collections.Generic;

namespace Aneiang.Pa.DouYin.Models
{
    public class DouYinOriginalResult
    {
        public DouyinOriginalData data { get; set; } = new DouyinOriginalData();
    }

    public class DouyinOriginalData
    {
        public List<DouyinOriginalItem> word_list { get; set; } = new List<DouyinOriginalItem>();

        public List<DouyinOriginalItem> trending_list { get; set; } = new List<DouyinOriginalItem>();
    }

    public class DouyinOriginalItem
    {
        public int article_detail_count { get; set; }
        public bool can_extend_detail { get; set; }
        public int discuss_video_count { get; set; }
        public int display_style { get; set; }
        public long event_time { get; set; }
        public string group_id { get; set; }
        public int hot_value { get; set; }
        public string hotlist_param { get; set; }
        public int label { get; set; }
        public string label_url { get; set; }
        public int max_rank { get; set; }
        public int position { get; set; }
        public string sentence_id { get; set; }
        public int sentence_tag { get; set; }
        public int video_count { get; set; }
        public string word { get; set; }
        public Word_Cover word_cover { get; set; }
        public int word_type { get; set; }
    }

    public class Word_Cover
    {
        public string uri { get; set; }
        public string[] url_list { get; set; }
    }

}
