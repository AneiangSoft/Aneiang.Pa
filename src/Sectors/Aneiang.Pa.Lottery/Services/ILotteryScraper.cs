using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.Core.Scraper;
using Aneiang.Pa.Lottery.Data;
using System.Threading.Tasks;

namespace Aneiang.Pa.Lottery.Services
{
    /// <summary>
    /// 彩票爬取器
    /// </summary>
    public interface ILotteryScraper : IScraper
    {
        /// <summary>
        /// 获取福利彩票最新开奖结果
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<AneiangGenericResult<WelfareLotteryData>> GetWelfareLotteryAsync(LotteryType type, int pageNo = 1,
            int pageSize = 30);

        /// <summary>
        /// 获取体育彩票最新开奖结果
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<AneiangGenericResult<SportLotteryResult>> GetSportLotteryAsync(LotteryType type, int pageNo = 1,
            int pageSize = 30);
    }
}
