// Program.cs

using Aneiang.Pa.Dynamic;
using Aneiang.Pa.Dynamic.Attributes;
using Aneiang.Pa.Dynamic.Extensions;
using Aneiang.Pa.Extensions;
using Aneiang.Pa.Models;
using Aneiang.Pa.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(a => a.AddJsonFile("appsettings.json"))
    .ConfigureServices((context, services) =>
    {
        services.AddNewsScraper(context.Configuration).AddDynamicScraper();
    })
    .Build();


Console.WriteLine("输出测试项：");
Console.WriteLine("1、使用新闻爬取器");
Console.WriteLine("2、使用动态爬取器");
Console.WriteLine("3、测试所有爬取器 - 信息统计");

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
    default:
        Console.WriteLine("无效的选项");
        break;
}
// 使用新闻爬取器
async Task ScraperNews()
{
    using (var scope = builder.Services.CreateScope())
    {
        var newsScraperFactory = scope.ServiceProvider.GetRequiredService<INewsScraperFactory>();
        var newsScraper = newsScraperFactory.GetScraper(ScraperSource.CnBlog);
        var newsResult = await newsScraper.GetNewsAsync();
        foreach (var news in newsResult.Data)
        {
            Console.WriteLine($"标题: {news.Title}");
            Console.WriteLine($"URL: {news.Url}");
            if (!string.IsNullOrWhiteSpace(news.ExtensionData))
            {
                Console.WriteLine($"扩展: {news.ExtensionData}");
            }
            Console.WriteLine();
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
            Console.WriteLine();
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