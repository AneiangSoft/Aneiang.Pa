// Program.cs

using Aneiang.Pa.Data;
using Aneiang.Pa.Extensions;
using Aneiang.Pa.News;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(a=>a.AddJsonFile("appsettings.json"))
    .ConfigureServices((context, services) =>
    {
        services.AddNewsScraper(context.Configuration);
    })
    .Build();

// 使用服务
using (var scope = builder.Services.CreateScope())
{
    var newsScraperFactory = scope.ServiceProvider.GetRequiredService<INewsScraperFactory>();
    var newsScraper = newsScraperFactory.GetScraper(ScraperSource.BaiDu);
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