using Aneiang.Pa.Core.News;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Aneiang.Pa.ZhiHu.Models
{
    public class ZhiHuScraperOptions: ScraperOptions
    {

        public ZhiHuScraperOptions()
        {
            BaseUrl = "https://www.zhihu.com";
            NewsUrl = "/api/v3/feed/topstory/hot-list-web?limit=20&desktop=true";
        }
    }
}
