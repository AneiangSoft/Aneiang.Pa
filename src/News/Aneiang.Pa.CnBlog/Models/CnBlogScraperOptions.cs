using Aneiang.Pa.Core.News;

namespace Aneiang.Pa.CnBlog.Models
{
    public class CnBlogScraperOptions: ScraperOptions
    {

        public CnBlogScraperOptions()
        {
            BaseUrl = "https://www.cnblogs.com";
            NewsUrl = "/pick/";
        }
    }
}
