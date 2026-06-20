using Aneiang.Pa.Core.News;
using System;

namespace Aneiang.Pa.Bilibili.Models
{
    public class BilibiliScraperOptions: ScraperOptions
    {
        public BilibiliScraperOptions()
        {
            BaseUrl = "https://s.search.bilibili.com";
            NewsUrl = "/main/hotword?limit=30";
        }
    }
}
