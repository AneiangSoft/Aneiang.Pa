// Program.cs

using Aneiang.Pa.Core.Proxy;
using Aneiang.Pa.Dynamic;
using Aneiang.Pa.Dynamic.Attributes;
using Aneiang.Pa.Extensions;
using Aneiang.Pa.Lottery.Data;
using Aneiang.Pa.Lottery.Extensions;
using Aneiang.Pa.Lottery.Services;
using Aneiang.Pa.News.Extensions;
using Aneiang.Pa.News.Models;
using Aneiang.Pa.News.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(a => a.AddJsonFile("appsettings.json"))
    .ConfigureServices((context, services) =>
    {
        //// 1. 注册带代理池支持的默认 HttpClient（使用配置中的 Scraper:ProxyPool）
        //services.AddPaDefaultHttpClientWithProxy(
        //    proxyConfiguration: context.Configuration.GetSection("Scraper:ProxyPool"));

        services.AddPaScraper();

        //// 2. 注册新闻爬取器（包含百度等多平台）
        //services.AddNewsScraper(context.Configuration); //.AddDynamicScraper();

        //services.AddLotteryScraper();
    })
    .Build();


Console.WriteLine("输出测试项：");
Console.WriteLine("1、使用新闻爬取器");
Console.WriteLine("2、使用动态爬取器");
Console.WriteLine("3、测试所有爬取器 - 信息统计");
Console.WriteLine("4、测试彩票爬取器");

var tag = Console.ReadLine();
switch (tag)
{
    case "1":
        await ScraperNews();
        break;
    case "2":
        await DynamicScraper();
        break;
    case "3":
        await TestScraperAllNews();
        break;
    case "4":
        await TestLotteryScraper();
        break;
    default:
        Console.WriteLine("无效的选项");
        break;
}

Console.WriteLine();
// 使用新闻爬取器
async Task ScraperNews()
{
    using (var scope = builder.Services.CreateScope())
    {
        var newsScraperFactory = scope.ServiceProvider.GetRequiredService<INewsScraperFactory>();
        var newsScraper = newsScraperFactory.GetScraper(ScraperSource.Bilibili);
        var newsResult = await newsScraper.GetNewsAsync();
        foreach (var news in newsResult.Data)
        {
            Console.WriteLine($"标题: {news.Title}");
            Console.WriteLine($"URL: {news.Url}");
            if (!string.IsNullOrWhiteSpace(news.ExtensionData))
            {
                Console.WriteLine($"扩展: {news.ExtensionData}");
            }
        }
    }
}

// 使用动态爬取器
async Task DynamicScraper()
{
    using (var scope = builder.Services.CreateScope())
    {
        var scraperFactory = scope.ServiceProvider.GetRequiredService<IDynamicScraper>();
        var testDataSets = await scraperFactory.DatasetScraperAsync<TestDataSet>("https://www.de62.com/listinfo-16-0.html");
        foreach (var testDataSet in testDataSets)
        {
            Console.WriteLine($"Title: {testDataSet.Title}");
            Console.WriteLine($"Url: {testDataSet.Url}");
            Console.WriteLine($"Icon: {testDataSet.Icon}");
            Console.WriteLine($"Desc: {testDataSet.Desc}");
        }

    }
}

// 测试所有新闻爬取器
async Task TestScraperAllNews()
{
    using (var scope = builder.Services.CreateScope())
    {
        // 并行获取所有新闻源
        var newsScraperFactory = scope.ServiceProvider.GetRequiredService<INewsScraperFactory>();
        var availableSources = Enum.GetValues<ScraperSource>();
        var tasks = availableSources.Select(async source =>
        {
            var scraper = newsScraperFactory.GetScraper(source);
            var data = await scraper.GetNewsAsync();
            Console.WriteLine($"【{source}】: 是否成功：{data.IsSuccessd}；总条数{data.Data.Count}");
        });

        await Task.WhenAll(tasks);
    }
}

// 测试彩票爬取器
async Task TestLotteryScraper()
{
    using (var scope = builder.Services.CreateScope())
    {
        var lotteryScraper = scope.ServiceProvider.GetRequiredService<ILotteryScraper>();
        var ssq = await lotteryScraper.GetWelfareLotteryAsync(LotteryType.SSQ);
        if (ssq is { IsSuccessd: true, Data.State: 0 })
        {
            var last = ssq.Data.Result.FirstOrDefault();
            if (last == null)
            {
                Console.WriteLine($"获取福利彩票-双色球数据失败！");
            }
            else
            {
                Console.WriteLine($"{last.Name} - {last.Code} - {last.Date}开奖结果: {last.Red} - {last.Blue}");
            }

        }
        else
        {
            Console.WriteLine($"获取福利彩票-双色球数据失败！");
        }

        var dlt = await lotteryScraper.GetSportLotteryAsync(LotteryType.DLT);
        if (dlt is { IsSuccessd: true, Data.ErrorCode: "0" })
        {
            var last = dlt.Data.Value.LastPoolDraw;
            Console.WriteLine($"{last.LotteryGameName} - {last.LotteryDrawNum} - {last.LotteryDrawTime}开奖结果: {last.LotteryDrawResult}");
        }
        else
        {
            Console.WriteLine($"获取福利彩票-双色球数据失败！");
        }
    }
}


[HtmlContainer("div", htmlClass: "blogs-list", index: 1)]
[HtmlItem("li")]
public class TestDataSet
{
    [HtmlValue("h2/a")]
    public string Title { get; set; }

    [HtmlValue("h2/a", attribute: "href")]
    public string Url { get; set; }

    [HtmlValue("i/a/img", attribute: "src")]
    public string Icon { get; set; }

    [HtmlValue("p")]
    public string Desc { get; set; }
}
