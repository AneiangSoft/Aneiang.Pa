using Aneiang.Pa.Core.News;

namespace Aneiang.Pa.Tencent.Models
{
    public class TencentScraperOptions: ScraperOptions
    {

        public TencentScraperOptions()
        {
            BaseUrl = "https://i.news.qq.com";
            NewsUrl = "/web_backend/v2/getTagInfo?tagId=aEWqxLtdgmQ%3D";
        }
    }
}
