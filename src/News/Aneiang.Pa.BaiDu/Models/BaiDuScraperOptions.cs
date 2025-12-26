using Aneiang.Pa.Core.News;
using System;

namespace Aneiang.Pa.BaiDu.Models
{
    public class BaiDuScraperOptions: ScraperOptions
    {
        public BaiDuScraperOptions()
        {
            BaseUrl = "https://top.baidu.com";
            NewsUrl = "/board?tab=realtime";
        }
    }
}
