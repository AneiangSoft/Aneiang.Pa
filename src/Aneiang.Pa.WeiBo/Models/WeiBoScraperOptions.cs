using System;

namespace Aneiang.Pa.WeiBo.Models
{
    public class WeiBoScraperOptions
    {
        public string Cookie { get; set; } = "SUB=_2AkMWIuNSf8NxqwJRmP8dy2rhaoV2ygrEieKgfhKJJRMxHRl-yT9jqk86tRB6PaLNvQZR6zYUcYVT1zSjoSreQHidcUq7";
        public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36";
        public string NewsUrl { get; set; } = "/top/summary?cate=realtimehot";
        public string BaseUrl { get; set; } = "https://s.weibo.com";

        public void Check()
        {
            if (string.IsNullOrWhiteSpace(Cookie)
                || string.IsNullOrWhiteSpace(NewsUrl)
                || string.IsNullOrWhiteSpace(BaseUrl))
            {
                throw new Exception("The Weibo configuration parameters are incomplete or missing!");
            }
        }
    }
}
