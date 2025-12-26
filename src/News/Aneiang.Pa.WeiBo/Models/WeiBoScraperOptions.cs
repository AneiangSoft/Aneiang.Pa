using Aneiang.Pa.Core.News;
using System;

namespace Aneiang.Pa.WeiBo.Models
{
    public class WeiBoScraperOptions: ScraperOptions
    {
        public WeiBoScraperOptions()
        {
            BaseUrl = "https://s.weibo.com";
            NewsUrl = "/top/summary?cate=realtimehot";
            Cookie =
                "SUB=_2AkMWIuNSf8NxqwJRmP8dy2rhaoV2ygrEieKgfhKJJRMxHRl-yT9jqk86tRB6PaLNvQZR6zYUcYVT1zSjoSreQHidcUq7";
            UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36";
        }

        public override void Check()
        {
            base.Check();
            if (string.IsNullOrWhiteSpace(Cookie))
            {
                throw new Exception("The Weibo configuration parameters are incomplete or missing!");
            }
        }
    }
}
