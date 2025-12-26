using Aneiang.Pa.Core.News;

namespace Aneiang.Pa.ThePaper.Models
{
    public class ThePaperScraperOptions: ScraperOptions
    {

        public ThePaperScraperOptions()
        {
            BaseUrl = "https://cache.thepaper.cn";
            NewsUrl = "/contentapi/wwwIndex/rightSidebar";
        }
    }
}
