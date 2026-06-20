using Aneiang.Pa.Core.News;

namespace Aneiang.Pa.TouTiao.Models
{
    public class TouTiaoScraperOptions: ScraperOptions
    {

        public TouTiaoScraperOptions()
        {
            BaseUrl = "https://www.toutiao.com";
            NewsUrl = "/hot-event/hot-board/?origin=toutiao_pc";
        }
    }
}
