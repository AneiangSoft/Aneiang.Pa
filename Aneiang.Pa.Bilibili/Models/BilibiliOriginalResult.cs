using System.Collections.Generic;

namespace Aneiang.Pa.Bilibili.Models
{
    public class BilibiliRankOriginalResult
    {
        public int code { get; set; }
        public string message { get; set; }
        public int ttl { get; set; }
        public BilibiliRankOriginalData data { get; set; } = new BilibiliRankOriginalData();
    }

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


    public class BilibiliRankOriginalData
    {
        public string note { get; set; }
        public List<BilibiliRankOriginalItem> list { get; set; } = new List<BilibiliRankOriginalItem>();
    }

    public class BilibiliRankOriginalItem
    {
        public long aid { get; set; }
        public int videos { get; set; }
        public int tid { get; set; }
        public string tname { get; set; }
        public int copyright { get; set; }
        public string pic { get; set; }
        public string title { get; set; }
        public int pubdate { get; set; }
        public int ctime { get; set; }
        public string desc { get; set; }
        public int state { get; set; }
        public int duration { get; set; }
        public int mission_id { get; set; }
        public Rights rights { get; set; }
        public Owner owner { get; set; }
        public Stat stat { get; set; }
        public string dynamic { get; set; }
        public long cid { get; set; }
        public Dimension dimension { get; set; }
        public string short_link_v2 { get; set; }
        public int up_from_v2 { get; set; }
        public string first_frame { get; set; }
        public string pub_location { get; set; }
        public string cover43 { get; set; }
        public int tidv2 { get; set; }
        public string tnamev2 { get; set; }
        public int pid_v2 { get; set; }
        public string pid_name_v2 { get; set; }
        public string bvid { get; set; }
        public int score { get; set; }
        public int enable_vt { get; set; }
    }

    public class Rights
    {
        public int bp { get; set; }
        public int elec { get; set; }
        public int download { get; set; }
        public int movie { get; set; }
        public int pay { get; set; }
        public int hd5 { get; set; }
        public int no_reprint { get; set; }
        public int autoplay { get; set; }
        public int ugc_pay { get; set; }
        public int is_cooperation { get; set; }
        public int ugc_pay_preview { get; set; }
        public int no_background { get; set; }
        public int arc_pay { get; set; }
        public int pay_free_watch { get; set; }
    }

    public class Owner
    {
        public long mid { get; set; }
        public string name { get; set; }
        public string face { get; set; }
    }

    public class Stat
    {
        public long aid { get; set; }
        public int view { get; set; }
        public int danmaku { get; set; }
        public int reply { get; set; }
        public int favorite { get; set; }
        public int coin { get; set; }
        public int share { get; set; }
        public int now_rank { get; set; }
        public int his_rank { get; set; }
        public int like { get; set; }
        public int dislike { get; set; }
        public int vt { get; set; }
        public int vv { get; set; }
        public int fav_g { get; set; }
        public int like_g { get; set; }
    }

    public class Dimension
    {
        public int width { get; set; }
        public int height { get; set; }
        public int rotate { get; set; }
    }

}
