using Aneiang.Pa.Core.News;

namespace Aneiang.Pa.IFeng.Models
{
    public class IFengScraperOptions: ScraperOptions
    {
        public IFengScraperOptions()
        {
            BaseUrl = "https://www.ifeng.com";
            NewsUrl = "/";
        }
    }
}
