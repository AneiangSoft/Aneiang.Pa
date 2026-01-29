using System;
using System.Collections.Generic;
using System.Text;

namespace Aneiang.Pa.Lottery.Data
{
    #region 福利彩票
    [Serializable]
    public class WelfareLotteryData
    {
        public int State { get; set; }
        public string Message { get; set; }
        public int Total { get; set; }
        public int PageNum { get; set; }
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public int Tflag { get; set; }
        public WelfareLottery[] Result { get; set; }
    }

    [Serializable]
    public class WelfareLottery
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string DetailsLink { get; set; }
        public string VideoLink { get; set; }
        public string Date { get; set; }
        public string Week { get; set; }
        public string Red { get; set; }
        public string Blue { get; set; }
        public string Blue2 { get; set; }
        public string Sales { get; set; }
        public string Poolmoney { get; set; }
        public string Content { get; set; }
        public string Addmoney { get; set; }
        public string Addmoney2 { get; set; }
        public string Msg { get; set; }
        public string Z2add { get; set; }
        public string M2add { get; set; }
        public Prizegrade[] Prizegrades { get; set; }
    }

    [Serializable]
    public class Prizegrade
    {
        public int Type { get; set; }
        public string Typenum { get; set; }
        public string Typemoney { get; set; }
    }
    #endregion

    #region 体育彩票
    public class SportLotteryResult
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public bool Success { get; set; }
        public SportLotteryData Value { get; set; }
    }

    public class SportLotteryData
    {
        public LastPoolDraw LastPoolDraw { get; set; }
        public SportLotteryList[] List { get; set; }
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public int Pages { get; set; }
        public int Total { get; set; }
    }

    public class LastPoolDraw
    {
        public string LotteryDrawNum { get; set; }
        public string LotteryDrawResult { get; set; }
        public string LotteryDrawTime { get; set; }
        public string LotteryGameName { get; set; }
        public string LotteryGameNum { get; set; }
        public string PoolBalanceAfterdraw { get; set; }
        public PrizeLevelList[] PrizeLevelList { get; set; }
    }

    public class PrizeLevelList
    {
        public int AwardType { get; set; }
        public string Group { get; set; }
        public string LotteryCondition { get; set; }
        public string PrizeLevel { get; set; }
        public int Sort { get; set; }
        public string StakeAmount { get; set; }
        public string StakeAmountFormat { get; set; }
        public string StakeCount { get; set; }
        public string TotalPrizeamount { get; set; }
    }

    public class SportLotteryList
    {
        public string LotteryGameName { get; set; }
        public string LotteryGameNum { get; set; }
        public string LotteryDrawNum { get; set; }
        public string LotteryDrawResult { get; set; }
        public int LotterySuspendedFlag { get; set; }
        public int LotteryDrawStatus { get; set; }
        public string LotterySaleEndtime { get; set; }
        public string LotterySaleBeginTime { get; set; }
        public string LotteryDrawTime { get; set; }
        public string LotteryPaidBeginTime { get; set; }
        public string LotteryPaidEndTime { get; set; }
        public string EstimateDrawTime { get; set; }
        public int Verify { get; set; }
        public int LotteryPromotionFlag { get; set; }
        public int IsGetKjpdf { get; set; }
        public int IsGetXlpdf { get; set; }
        public int PdfType { get; set; }
        public string LotteryUnsortDrawresult { get; set; }
        public string PoolBalanceAfterdraw { get; set; }
        public string PoolBalanceAfterdrawRj { get; set; }
        public string DrawFlowFund { get; set; }
        public string DrawFlowFundRj { get; set; }
        public string TotalSaleAmount { get; set; }
        public string TotalSaleAmountRj { get; set; }
        public int LotteryEquipmentCount { get; set; }
        public int LotteryGamePronum { get; set; }
        public string DrawPdfUrl { get; set; }
        public PrizeLevelList[] PrizeLevelList { get; set; }
    }
    #endregion
}
