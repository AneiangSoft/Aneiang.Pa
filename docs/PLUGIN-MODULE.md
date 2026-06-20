# Aneiang.Pa 第三方插件开发指南

## 概述

3.0 引入 `IScraperModule` 抽象 + `[assembly: PaScraperModule]` 程序集声明，第三方 NuGet 包可通过此机制让用户**只需引用，无需手动注册**就能自动接入。

## 开发一个第三方爬虫包

### 1. 创建项目

```bash
dotnet new classlib -n MyCompany.Pa.Github
cd MyCompany.Pa.Github
dotnet add package Aneiang.Pa.Core
dotnet add package HtmlAgilityPack
```

### 2. 实现爬虫

```csharp
using Aneiang.Pa.Core.Models;
using Aneiang.Pa.Core.News.Models;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.Core.Scraper;

namespace MyCompany.Pa.Github;

public interface IGithubTrendingScraper : INewsScraper { }

public class GithubTrendingScraper : IGithubTrendingScraper
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GithubScraperOptions _options;

    public GithubTrendingScraper(IHttpClientFactory httpClientFactory, IOptions<GithubScraperOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public ScraperDescriptor Descriptor { get; } = new()
    {
        Category = ScraperCategories.News,
        Source = "Github",
        DisplayName = "GitHub Trending",
        ResultType = typeof(NewsItem)
    };

    public string Source => "Github";

    public async Task<AneiangGenericListResult<NewsItem>> GetNewsAsync()
    {
        // 实现抓取逻辑
    }

    public async Task<ScraperResult<NewsItem>> ScrapeAsync(CancellationToken ct = default)
    {
        var legacy = await GetNewsAsync();
        return new ScraperResult<NewsItem>
        {
            IsSuccess = legacy.IsSuccessd,
            ErrorMessage = legacy.ErrorMessage,
            UpdatedTime = legacy.UpdatedTime,
            Data = legacy.Data
        };
    }
}
```

### 3. 声明 Module（核心）

```csharp
using Aneiang.Pa.Core.Extensions;
using Aneiang.Pa.Core.Modules;
using Aneiang.Pa.Core.News;
using Microsoft.Extensions.DependencyInjection;

[assembly: PaScraperModule(typeof(MyCompany.Pa.Github.GithubScraperModule))]

namespace MyCompany.Pa.Github;

public class GithubScraperModule : IScraperModule
{
    public string Name => "MyCompany.Pa.Github";

    public void Register(IScraperModuleBuilder builder)
    {
        builder.Services.AddScraper<IGithubTrendingScraper, GithubTrendingScraper, GithubScraperOptions>(
            "Scraper:Github", builder.Configuration, addHttpClient: false);
        builder.Services.AddSingleton<INewsScraper>(sp =>
            sp.GetRequiredService<IGithubTrendingScraper>());
    }
}
```

### 4. 用户使用

```csharp
// 用户 Program.cs
services.AddPaScraperFromLoadedAssemblies(configuration);
// 自动发现并注册 MyCompany.Pa.Github + 所有官方包
```

或显式注册：

```csharp
services.AddPaScraper(configuration);
services.AddPaScraperModules(new[] { typeof(GithubScraperModule).Assembly }, configuration);
```

## 命名约定

| 项目 | 命名 |
|------|------|
| NuGet 包名 | `<Vendor>.Pa.<Source>` 或 `Aneiang.Pa.<Source>` |
| 接口 | `I<Source>NewScraper` 或 `I<Source>Scraper` |
| 实现 | `<Source>NewScraper` 或 `<Source>Scraper` |
| Options | `<Source>ScraperOptions` |
| Module | `<Source>ScraperModule` |
| Descriptor.Category | 使用 `ScraperCategories.News` 等常量；新分类直接用字符串 |
| Descriptor.Source | 单词大写驼峰，与配置 Section 一致 |

## 自动发现机制

`AddPaScraperFromLoadedAssemblies` 会：

1. 遍历 `AppDomain.CurrentDomain.GetAssemblies()`
2. 查找所有 `[assembly: PaScraperModule(typeof(X))]`
3. 创建 `X` 实例（必须有公开无参构造）
4. 按 `Order` 升序调用 `Register(builder)`
5. 自动追加 `IScraperRegistry` 与 `Pipeline`

## 测试你的模块

```csharp
[Fact]
public void Module_should_register_scraper()
{
    var services = new ServiceCollection();
    services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
    services.AddHttpClient();

    var module = new GithubScraperModule();
    var builder = new ScraperModuleBuilder(services, null);
    module.Register(builder);

    var sp = services.BuildServiceProvider();
    sp.GetService<IGithubTrendingScraper>().Should().NotBeNull();
    sp.GetService<INewsScraper>().Should().NotBeNull();
}
```
