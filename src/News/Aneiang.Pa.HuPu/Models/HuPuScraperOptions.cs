using Aneiang.Pa.Core.News;

namespace Aneiang.Pa.HuPu.Models
{
    public class HuPuScraperOptions: ScraperOptions
    {

        public HuPuScraperOptions()
        {
            BaseUrl = "https://bbs.hupu.com";
            NewsUrl = "/topic-daily-hot";
        }
    }
}
