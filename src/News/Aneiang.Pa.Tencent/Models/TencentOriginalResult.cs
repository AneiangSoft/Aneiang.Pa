using System.Collections.Generic;

namespace Aneiang.Pa.Tencent.Models
{
    public class TencentOriginalResult
    {
        public int ret { get; set; }
        public string msg { get; set; }
        public TencentOriginalData data { get; set; } = new TencentOriginalData();
    }

    public class TencentOriginalData
    {
        public List<TencentOriginalTab> tabs { get; set; } = new List<TencentOriginalTab>();
    }

    public class TencentOriginalTab
    {
        public string id { get; set; }
        public string channel_id { get; set; }
        public string name { get; set; }
        public string source { get; set; }
        public string type { get; set; }
        public List<TencentOriginalItem> articleList { get; set; } = new List<TencentOriginalItem>();
        public int article_count { get; set; }
        public string sub_tab { get; set; }
    }
    public class TencentOriginalItem
    {
        public string id { get; set; }
        public string articletype { get; set; }
        public string title { get; set; }
        public int pic_style { get; set; }
        public string publish_time { get; set; }
        public Pic_Info pic_info { get; set; } = new Pic_Info();
        public Link_Info link_info { get; set; } = new Link_Info();
        public Doc_Info doc_info { get; set; } = new Doc_Info();
        public string update_time { get; set; }
        public string desc { get; set; }
    }

    public class Pic_Info
    {
        public string[] big_img { get; set; }
        public string[] small_img { get; set; }
        public string[] three_img { get; set; }
        public string ls_img_exp_type { get; set; }
        public string share_img { get; set; }
    }

    public class Link_Info
    {
        public string share_url { get; set; }
        public string url { get; set; }
        public string short_url { get; set; }
        public string org_url { get; set; }
    }
    public class Doc_Info
    {
        public string first_cate_id { get; set; }
        public string first_cate_name { get; set; }
        public string sec_cate_id { get; set; }
        public string sec_cate_name { get; set; }
    }
}
