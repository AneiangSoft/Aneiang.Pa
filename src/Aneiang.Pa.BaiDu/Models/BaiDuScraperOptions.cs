using System;

namespace Aneiang.Pa.BaiDu.Models
{
    public class BaiDuScraperOptions
    {
        public string NewsUrl { get; set; } = "https://top.baidu.com/board?tab=realtime";

        public void Check()
        {
            if (string.IsNullOrWhiteSpace(NewsUrl))
            {
                throw new Exception("The Baidu configuration parameters are incomplete or missing!");
            }
        }
    }
}
