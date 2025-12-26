using Aneiang.Pa.Core.News;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.Lottery.Data;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Aneiang.Pa.Lottery.Services
{
    /// <summary>
    /// 彩票爬虫
    /// </summary>
    public class LotteryScraper : ILotteryScraper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// 知乎热门爬虫
        /// </summary>
        /// <param name="httpClientFactory"></param>
        public LotteryScraper(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// 获取福利彩票最新开奖结果
        /// </summary>
        public async Task<AneiangGenericResult<WelfareLotteryData>> GetWelfareLotteryAsync(LotteryType type, int pageNo = 1, int pageSize = 30)
        {
            if (type != LotteryType.SSQ
                && type != LotteryType.KL8
                && type != LotteryType.FC3D
                && type != LotteryType.QLC)
            {
                return AneiangGenericResult<WelfareLotteryData>.Failure("不支持的福利彩票类型");
            }

            try
            {
                var welfareLotteryResult = new AneiangGenericResult<WelfareLotteryData>(false);
                var apiBaseUrl = $"https://www.cwl.gov.cn/cwl_admin/front/cwlkj/search/kjxx/findDrawNotice";
                var queryUrl = $"name={type.ToString().ToLower()}&issueCount=&issueStart=&issueEnd=&dayStart=&dayEnd=&pageNo={pageNo}&pageSize={pageSize}&week=&systemType=PC";
                var client = ScraperHttpClientHelper.CreateConfiguredClient(_httpClientFactory, "https://www.cwl.gov.cn", UserAgentGenerator.GetRandomUserAgent());
                var response = await ScraperHttpClientHelper.GetAsync(client, $"{apiBaseUrl}?{queryUrl}");
                if (!response.IsSuccessStatusCode)
                    return AneiangGenericResult<WelfareLotteryData>.Failure($"HTTP 请求失败，状态码: {response.StatusCode}");
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<WelfareLotteryData>(jsonString, _jsonSerializerOptions);
                if (result == null) return welfareLotteryResult;
                welfareLotteryResult.IsSuccessd = true;
                welfareLotteryResult.Data = result;
                return welfareLotteryResult;
            }
            catch (Exception e)
            {
                return AneiangGenericResult<WelfareLotteryData>.Failure(e.Message);
            }
        }


        /// <summary>
        /// 获取体育彩票最新开奖结果
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<AneiangGenericResult<SportLotteryResult>> GetSportLotteryAsync(LotteryType type, int pageNo = 1, int pageSize = 30)
        {
            if (type != LotteryType.DLT
                && type != LotteryType.PL3
                && type != LotteryType.PL5
                && type != LotteryType.QXC)
            {
                return AneiangGenericResult<SportLotteryResult>.Failure("不支持的体育彩票类型");
            }

            try
            {
                var sportLotteryResult = AneiangGenericResult<SportLotteryResult>.Success();
                var apiBaseUrl = $"https://webapi.sporttery.cn/gateway/lottery/getHistoryPageListV1.qry";
                var queryUrl = $"gameNo={((int)type):D2}&provinceId=0&pageSize={pageSize}&isVerify=1&pageNo={pageNo}";
                var client = ScraperHttpClientHelper.CreateConfiguredClient(_httpClientFactory, "https://webapi.sporttery.cn", UserAgentGenerator.GetRandomUserAgent());
                var response = await ScraperHttpClientHelper.GetAsync(client, $"{apiBaseUrl}?{queryUrl}");
                if (!response.IsSuccessStatusCode)
                    return AneiangGenericResult<SportLotteryResult>.Failure($"HTTP 请求失败，状态码: {response.StatusCode}");
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<SportLotteryResult>(jsonString, _jsonSerializerOptions);
                if (result == null) return sportLotteryResult;
                sportLotteryResult.IsSuccessd = true;
                sportLotteryResult.Data = result;
                return sportLotteryResult;
            }
            catch (Exception e)
            {
                return AneiangGenericResult<SportLotteryResult>.Failure(e.Message);
            }
        }
    }
}
