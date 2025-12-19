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
        services.AddNewsScraper(context.Configuration);
        services.AddDynamicScraper(context.Configuration);
    })
    .Build();


Console.WriteLine("输出测试项：");
Console.WriteLine("1、使用新闻爬取器");
Console.WriteLine("2、使用动态爬取器");

var tag = Console.ReadLine();
switch (tag)
{
    case "1":
        await ScraperNews();
        break;
    case "2":
        await DynamicScraper();
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
        var newsScraper = newsScraperFactory.GetScraper(ScraperSource.IFeng);
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
        var testDataSets = await scraperFactory.DatasetScraper<TestDataSet>("https://www.de62.com/listinfo-16-0.html");
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