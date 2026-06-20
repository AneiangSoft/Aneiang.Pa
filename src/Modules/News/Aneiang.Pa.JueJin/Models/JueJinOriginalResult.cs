using System.Collections.Generic;

namespace Aneiang.Pa.JueJin.Models
{
    public class JueJinOriginalResult
    {
        public int err_no { get; set; }
        public string err_msg { get; set; }
        public List<JueJinOriginalItem> data { get; set; } = new List<JueJinOriginalItem>();
    }



    public class JueJinOriginalItem
    {
        public Content content { get; set; }
        public Content_Counter content_counter { get; set; }
        public Author author { get; set; }
        public Author_Counter author_counter { get; set; }
        public User_Interact user_interact { get; set; }
    }

    public class Content
    {
        public string content_id { get; set; }
        public int item_type { get; set; }
        public string format { get; set; }
        public string author_id { get; set; }
        public string title { get; set; }
        public string brief { get; set; }
        public int status { get; set; }
        public int ctime { get; set; }
        public int mtime { get; set; }
        public string category_id { get; set; }
        public string[] tag_ids { get; set; }
    }

    public class Content_Counter
    {
        public int view { get; set; }
        public int like { get; set; }
        public int collect { get; set; }
        public int hot_rank { get; set; }
        public int comment_count { get; set; }
        public int interact_count { get; set; }
    }

    public class Author
    {
        public string user_id { get; set; }
        public string name { get; set; }
        public string avatar { get; set; }
        public bool is_followed { get; set; }
    }

    public class Author_Counter
    {
        public int level { get; set; }
        public int power { get; set; }
        public int follower { get; set; }
        public int followee { get; set; }
        public int publish { get; set; }
        public int view { get; set; }
        public int like { get; set; }
        public int hot_rank { get; set; }
    }

    public class User_Interact
    {
        public bool is_user_like { get; set; }
        public bool is_user_collect { get; set; }
        public bool is_follow { get; set; }
    }


}
