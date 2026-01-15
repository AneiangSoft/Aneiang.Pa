using Aneiang.Pa.Core.News;

namespace Aneiang.Pa._36kr.Models
{
    public class _36krScraperOptions: ScraperOptions
    {

        public _36krScraperOptions()
        {
            BaseUrl = "https://www.36kr.com";
            NewsUrl = "/hot-list/renqi/";
        }
    }
}
