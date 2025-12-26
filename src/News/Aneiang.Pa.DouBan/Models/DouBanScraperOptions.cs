using Aneiang.Pa.Core.News;

namespace Aneiang.Pa.DouBan.Models
{
    public class DouBanScraperOptions: ScraperOptions
    {

        public DouBanScraperOptions()
        {
            BaseUrl = "https://m.douban.com";
            NewsUrl = "/rexxar/api/v2/subject/recent_hot/movie";
        }
    }
}
