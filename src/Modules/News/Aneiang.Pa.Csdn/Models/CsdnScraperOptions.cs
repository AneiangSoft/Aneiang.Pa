using Aneiang.Pa.Core.News;

namespace Aneiang.Pa.Csdn.Models
{
    public class CsdnScraperOptions: ScraperOptions
    {

        public CsdnScraperOptions()
        {
            BaseUrl = "https://blog.csdn.net";
            NewsUrl = "/phoenix/web/blog/hot-rank?page=0&pageSize=25&type=";
        }
    }
}
