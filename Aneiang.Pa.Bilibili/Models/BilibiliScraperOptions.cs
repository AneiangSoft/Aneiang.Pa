using System;

namespace Aneiang.Pa.Bilibili.Models
{
    public class BilibiliScraperOptions
    {
        public string SearchUrl { get; set; } = "https://s.search.bilibili.com/main/hotword?limit=30";

        public void Check()
        {
            if (string.IsNullOrWhiteSpace(SearchUrl))
            {
                throw new Exception("The Zhihu configuration parameters are incomplete or missing!");
            }
        }
    }
}
