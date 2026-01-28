using System.ComponentModel;
using Aneiang.Pa.Extensions;
using Aneiang.Pa.Lottery.Data;
using Aneiang.Pa.Lottery.Services;
using Aneiang.Pa.News.Models;
using Aneiang.Pa.News.News;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Warning);
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Warning;
});

builder.Services.AddPaScraper(builder.Configuration);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

app.MapMcp();

await app.RunAsync();

[McpServerToolType]
public static class AneiangPaTools
{
    private static readonly IReadOnlyDictionary<string, LotteryType> LotteryAliases =
        new Dictionary<string, LotteryType>(StringComparer.OrdinalIgnoreCase)
        {
            ["SSQ"] = LotteryType.SSQ,
            ["双色球"] = LotteryType.SSQ,

            ["KL8"] = LotteryType.KL8,
            ["快乐8"] = LotteryType.KL8,

            ["FC3D"] = LotteryType.FC3D,
            ["福彩3D"] = LotteryType.FC3D,
            ["3D"] = LotteryType.FC3D,

            ["QLC"] = LotteryType.QLC,
            ["七乐彩"] = LotteryType.QLC,

            ["DLT"] = LotteryType.DLT,
            ["大乐透"] = LotteryType.DLT,

            ["PL3"] = LotteryType.PL3,
            ["排列三"] = LotteryType.PL3,

            ["PL5"] = LotteryType.PL5,
            ["排列五"] = LotteryType.PL5,

            ["QXC"] = LotteryType.QXC,
            ["7星彩"] = LotteryType.QXC,
            ["七星彩"] = LotteryType.QXC,
        };

    [McpServerTool(Name = "news.get"), Description("按指定 source 抓取新闻热榜/列表（source 大小写不敏感）")]
    public static async Task<object> GetNews(
        INewsScraperFactory scraperFactory,
        [Description("例如 WeiBo/ZhiHu/Bilibili/BaiDu/DouYin/TouTiao/Tencent/JueJin/ThePaper/DouBan/IFeng/Csdn/CnBlog/ItHome/36kr")] string source,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ScraperSource>(source, true, out var scraperSource))
        {
            throw new InvalidOperationException($"不支持的爬虫源: {source}");
        }

        var scraper = scraperFactory.GetScraper(scraperSource);
        var result = await scraper.GetNewsAsync();
        return result;
    }

    [McpServerTool(Name = "lottery.types"), Description("返回当前支持的彩票类型列表（包含中文别名）")]
    public static object GetLotteryTypes()
    {
        var types = Enum.GetValues<LotteryType>()
            .Select(t => new
            {
                code = t.ToString(),
                value = (int)t,
                aliases = LotteryAliases
                    .Where(kv => kv.Value.Equals(t))
                    .Select(kv => kv.Key)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToArray()
            })
            .OrderBy(x => x.code)
            .ToArray();

        return new { types };
    }

    [McpServerTool(Name = "lottery.welfare.get"), Description("获取福利彩票开奖信息")]
    public static async Task<object> GetWelfareLottery(
        ILotteryScraper lotteryScraper,
        [Description("LotteryType 枚举名称或中文别名（例如 SSQ/双色球）")] string type,
        [Description("页码，默认 1")] int pageNo = 1,
        [Description("每页数量，默认 30")] int pageSize = 30,
        CancellationToken cancellationToken = default)
    {
        var lotteryType = ParseLotteryType(type);

        if (pageNo <= 0) pageNo = 1;
        if (pageSize <= 0) pageSize = 30;

        var result = await lotteryScraper.GetWelfareLotteryAsync(lotteryType, pageNo, pageSize);
        return result;
    }

    [McpServerTool(Name = "lottery.sport.get"), Description("获取体育彩票开奖信息")]
    public static async Task<object> GetSportLottery(
        ILotteryScraper lotteryScraper,
        ILoggerFactory loggerFactory,
        [Description("LotteryType 枚举名称或中文别名（例如 DLT/大乐透）")] string type,
        [Description("页码，默认 1")] int pageNo = 1,
        [Description("每页数量，默认 30")] int pageSize = 30,
        CancellationToken cancellationToken = default)
    {
        var logger = loggerFactory.CreateLogger("AneiangPaTools");
        var lotteryType = ParseLotteryType(type);

        if (pageNo <= 0) pageNo = 1;
        if (pageSize <= 0) pageSize = 30;

        logger.LogInformation("Calling GetSportLotteryAsync type={Type} pageNo={PageNo} pageSize={PageSize}", lotteryType, pageNo, pageSize);

        try
        {
            var result = await lotteryScraper.GetSportLotteryAsync(lotteryType, pageNo, pageSize);
            logger.LogInformation("GetSportLotteryAsync completed. IsSuccessd={IsSuccessd}", result.IsSuccessd);
            if (!result.IsSuccessd)
            {
                logger.LogError("GetSportLotteryAsync failed. ErrorMessage={ErrorMessage}", result.ErrorMessage);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetSportLotteryAsync threw exception");
            throw;
        }
    }

    [McpServerTool(Name = "lottery.sport.latest"), Description("获取最新一期体彩开奖信息（默认大乐透）")]
    public static async Task<object> GetSportLatest(
        ILotteryScraper lotteryScraper,
        [Description("彩种（默认 大乐透），支持 DLT/大乐透/PL3/排列三 等")]
        string? type = null,
        CancellationToken cancellationToken = default)
    {
        var lotteryType = ParseLotteryType(string.IsNullOrWhiteSpace(type) ? "DLT" : type);
        var result = await lotteryScraper.GetSportLotteryAsync(lotteryType, 1, 30);

        // 这里先直接返回原始 result；如果你想只返回“最新一期”我可以再做进一步字段抽取。
        return result;
    }

    [McpServerTool(Name = "lottery.welfare.latest"), Description("获取最新一期福彩开奖信息（默认双色球）")]
    public static async Task<object> GetWelfareLatest(
        ILotteryScraper lotteryScraper,
        [Description("彩种（默认 双色球），支持 SSQ/双色球/FC3D/福彩3D 等")]
        string? type = null,
        CancellationToken cancellationToken = default)
    {
        var lotteryType = ParseLotteryType(string.IsNullOrWhiteSpace(type) ? "SSQ" : type);
        var result = await lotteryScraper.GetWelfareLotteryAsync(lotteryType, 1, 30);
        return result;
    }

    [McpServerTool(Name = "assistant.query"), Description("自然语言查询入口：例如“我想知道今天的大乐透开奖结果”】【内部自动调用对应 tools】")]
    public static async Task<object> Query(
        ILotteryScraper lotteryScraper,
        [Description("用户自然语言问题")]
        string text,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("text 不能为空");
        }

        var normalized = text.Trim();

        // 只做最小规则路由：足够覆盖“今天/最新 + 大乐透/双色球”等
        // 你后续要更智能，可以再扩展为更多关键词/正则。

        var isSport = normalized.Contains("体彩", StringComparison.OrdinalIgnoreCase) ||
                      normalized.Contains("大乐透", StringComparison.OrdinalIgnoreCase) ||
                      normalized.Contains("排列", StringComparison.OrdinalIgnoreCase) ||
                      normalized.Contains("7星", StringComparison.OrdinalIgnoreCase) ||
                      normalized.Contains("七星彩", StringComparison.OrdinalIgnoreCase);

        var isWelfare = normalized.Contains("福彩", StringComparison.OrdinalIgnoreCase) ||
                        normalized.Contains("双色球", StringComparison.OrdinalIgnoreCase) ||
                        normalized.Contains("七乐彩", StringComparison.OrdinalIgnoreCase) ||
                        normalized.Contains("快乐8", StringComparison.OrdinalIgnoreCase) ||
                        normalized.Contains("3D", StringComparison.OrdinalIgnoreCase);

        // 默认彩种推断
        var inferredType = InferLotteryTypeFromText(normalized) ?? (isWelfare ? "SSQ" : "DLT");

        if (isWelfare && !isSport)
        {
            return await GetWelfareLatest(lotteryScraper, inferredType, cancellationToken);
        }

        // 默认走体彩
        return await GetSportLatest(lotteryScraper, inferredType, cancellationToken);
    }

    private static LotteryType ParseLotteryType(string type)
    {
        if (LotteryAliases.TryGetValue(type.Trim(), out var aliased))
        {
            return aliased;
        }

        if (Enum.TryParse<LotteryType>(type, true, out var lotteryType))
        {
            return lotteryType;
        }

        throw new InvalidOperationException($"不支持的彩票类型: {type}。可先调用 lottery.types 查看支持列表。");
    }

    private static string? InferLotteryTypeFromText(string text)
    {
        // 优先匹配中文名
        foreach (var kv in LotteryAliases)
        {
            if (text.Contains(kv.Key, StringComparison.OrdinalIgnoreCase))
            {
                return kv.Value.ToString();
            }
        }

        return null;
    }
}
