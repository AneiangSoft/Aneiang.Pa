using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Aneiang.Pa.ZhiHu.Models
{
    public class ZhiHuScraperOptions
    {
        public string NewsUrl { get; set; } = "https://www.zhihu.com/api/v3/feed/topstory/hot-list-web?limit=20&desktop=true";

        public void Check()
        {
            if (string.IsNullOrWhiteSpace(NewsUrl))
            {
                throw new Exception("The Zhihu configuration parameters are incomplete or missing!");
            }
        }
    }
}
