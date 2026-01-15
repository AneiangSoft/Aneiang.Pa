using Aneiang.Pa.Core.News;

namespace Aneiang.Pa.ItHome.Models
{
    public class ItHomeScraperOptions: ScraperOptions
    {

        public ItHomeScraperOptions()
        {
            BaseUrl = "https://www.ithome.com";
            NewsUrl = "/blog/";
        }
    }
}
