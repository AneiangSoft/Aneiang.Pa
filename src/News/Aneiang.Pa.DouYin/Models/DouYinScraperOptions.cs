using System;
using Aneiang.Pa.Core.News;

namespace Aneiang.Pa.DouYin.Models
{
    /// <summary>
    /// 爬取器配置选项
    /// </summary>
    public class DouYinScraperOptions : ScraperOptions
    {
        public DouYinScraperOptions()
        {
            BaseUrl = "https://www.douyin.com";
            NewsUrl = "/aweme/v1/web/hot/search/list/?device_platform=webapp&aid=6383&channel=channel_pc_web&detail_list=1";
        }
    }
}
