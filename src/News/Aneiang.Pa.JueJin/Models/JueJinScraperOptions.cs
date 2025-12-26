using Aneiang.Pa.Core.News;

namespace Aneiang.Pa.JueJin.Models
{
    public class JueJinScraperOptions: ScraperOptions
    {

        public JueJinScraperOptions()
        {
            BaseUrl = "https://api.juejin.cn";
            NewsUrl = "/content_api/v1/content/article_rank?category_id=1&type=hot&spider=0";
        }
    }
}
