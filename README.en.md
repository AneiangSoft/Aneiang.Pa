<p align="center">
    <img src="assets/logo.png" alt="Aneiang.Pa" width="600" style="vertical-align:middle;border-radius:8px;">
</p>

[中文](README.md) | English

[![NuGet](https://img.shields.io/nuget/v/Aneiang.Pa.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Aneiang.Pa)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Aneiang.Pa.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Aneiang.Pa)
[![Target](https://img.shields.io/badge/target-netstandard2.1-blue?style=flat-square)](#)
[![Status](https://img.shields.io/badge/status-active-success?style=flat-square)](#)

A multi-platform trending news/hotlist web scraping library based on .NET. Currently supports crawlers for Weibo, Zhihu, Bilibili, Baidu, Douyin, Hupu, Toutiao, Tencent, Juejin, The Paper, Phoenix News, Douban, and other platforms, complete with demo examples. Open-source project, with plans to add more platforms in the future.

**⚠️ Recommended scraping interval: 5 minutes or more to avoid IP blocking due to frequent requests.**

**⚠️ The scraped data is strictly for personal learning, research, or public welfare purposes only. Commercial sale, attacks on others, or any illegal activities are prohibited. Users assume full legal responsibility for any misuse.**

## Installation (NuGet)
Recommended aggregate package (includes all platforms):
```bash
dotnet add package Aneiang.Pa
```
Install individual packages as needed (example):
```bash
dotnet add package Aneiang.Pa.BaiDu
```

### Available Packages
| Package | Description |
| --- | --- |
| Aneiang.Pa | Aggregate package containing all platform implementations |
| Aneiang.Pa.Core | Core interfaces and models, proxy pool functionality |
| Aneiang.Pa.Dynamic | Dynamic web scraper |
| Aneiang.Pa.AspNetCore | ASP.NET Core Web API extension (provides RESTful API controllers) |
| Aneiang.Pa.BaiDu | Baidu Hotlist scraper |
| Aneiang.Pa.Bilibili | Bilibili Hot Search scraper |
| Aneiang.Pa.WeiBo | Weibo Hot Search scraper |
| Aneiang.Pa.ZhiHu | Zhihu Hotlist scraper |
| Aneiang.Pa.DouYin | Douyin Hotlist scraper |
| Aneiang.Pa.HuPu | Hupu Hot Posts/Hotlist scraper |
| Aneiang.Pa.TouTiao | Toutiao Hotlist scraper |
| Aneiang.Pa.Tencent | Tencent Hotlist scraper |
| Aneiang.Pa.JueJin | Juejin Hotlist scraper |
| Aneiang.Pa.ThePaper | The Paper Hotlist scraper |
| Aneiang.Pa.DouBan | Douban Hotlist scraper |
| Aneiang.Pa.IFeng | Phoenix News Hotlist scraper |
| Aneiang.Pa.Csdn | CSDN Hotlist scraper |
| Aneiang.Pa.CnBlog | Cnblogs Hotlist scraper |

## Quick Start (Local Demo)
1) Restore & Build
```bash
dotnet restore
dotnet build test/Aneiang.Pa.Demo/Aneiang.Pa.Demo.csproj
```
2) Run Demo (default scrapes Baidu Hotlist, modifiable via `ScraperSource`)
```bash
dotnet run --project test/Aneiang.Pa.Demo
```

## Usage in Your Project (NuGet)
```csharp

// Choose one of the following approaches:
// Automatically register all platform scrapers
services.AddNewsScraper();

// Register a single platform scraper
services.AddBaiDuScraper();
```

```csharp
// Get scraper instance via factory pattern
var factory = scope.ServiceProvider.GetRequiredService<INewsScraperFactory>();
var scraper = factory.GetScraper(ScraperSource.BaiDu);
var result = await scraper.GetNewsAsync();

// Directly inject single platform scraper
var scraper = scope.ServiceProvider.GetRequiredService<IBaiDuNewScraper>();
var result = await scraper.GetNewsAsync();
```

## 🌐 Proxy Pool Feature

**Supports configuring multiple proxy servers with automatic round-robin or random selection, effectively avoiding IP blocking.**

### Features

- ✅ Support for multiple proxy server configurations
- ✅ Two selection strategies: RoundRobin and Random
- ✅ Support for authenticated proxies (`http://user:password@host:port`)
- ✅ Configurable via configuration file or code
- ✅ Automatically degrades to regular HttpClient when not enabled

### Usage

#### Method 1: Configuration File (Recommended)

Configure in `appsettings.json`:
```json
{
  "Scraper": {
    "ProxyPool": {
      "Enabled": true,
      "Strategy": "RoundRobin",
      "Proxies": [
        "http://127.0.0.1:7890",
        "http://user:password@proxy.example.com:8080",
        "http://192.168.1.100:3128"
      ]
    }
  }
}
```

Register in code:
```csharp
using Aneiang.Pa.Core.Proxy;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Register default HttpClient with proxy pool support
        services.AddPaDefaultHttpClientWithProxy(
            proxyConfiguration: context.Configuration.GetSection("Scraper:ProxyPool"));
        
        // Register scraper services (will automatically use the configured HttpClient)
        services.AddNewsScraper(context.Configuration);
    })
    .Build();
```

#### Method 2: Code Configuration

```csharp
using Aneiang.Pa.Core.Proxy;

services.AddPaDefaultHttpClientWithProxy(
    proxyConfigure: options =>
    {
        options.Enabled = true;
        options.Strategy = ProxySelectionStrategy.RoundRobin; // or Random
        options.Proxies = new List<string>
        {
            "http://127.0.0.1:7890",
            "http://user:password@proxy.example.com:8080",
            "http://192.168.1.100:3128"
        };
    });

services.AddNewsScraper();
```

#### Register Proxy Pool Service Only (Without HttpClient)

If you only need the proxy pool service, you can use:
```csharp
// Register proxy pool service only
services.AddPaProxyPool(
    configuration: context.Configuration.GetSection("Scraper:ProxyPool"));

// Or configure via code
services.AddPaProxyPool(
    configure: options =>
    {
        options.Enabled = true;
        options.Strategy = ProxySelectionStrategy.Random;
        options.Proxies = new List<string> { "http://127.0.0.1:7890" };
    });

// Then inject IProxyPool to use
var proxyPool = serviceProvider.GetRequiredService<IProxyPool>();
var proxyUri = proxyPool.GetNextProxy();
```

### Proxy Selection Strategies

- **RoundRobin**: Uses proxy servers sequentially in order, ensuring load balancing
- **Random**: Randomly selects a proxy server each time

### Proxy Address Format

Supports the following proxy address formats:
- `http://host:port` - HTTP proxy (no authentication)
- `http://user:password@host:port` - HTTP proxy (with authentication)
- `https://host:port` - HTTPS proxy

### Notes

1. **Enable Check**: If `Enabled = true` but no proxy list is configured, an exception will be thrown
2. **HttpClient Name**: The default HttpClient name is `Aneiang.Pa.DefaultHttpClient`, scrapers will automatically use this HttpClient
3. **Proxy Priority**: If `AddPaDefaultHttpClientWithProxy` is called before `AddNewsScraper`, scrapers will use the configured proxy pool
4. **When Not Enabled**: When `Enabled = false` or the proxy list is empty, it automatically degrades to a regular HttpClient without affecting normal usage

## 🚀 ASP.NET Core Web API Integration (Aneiang.Pa.AspNetCore)

**Provides out-of-the-box Web API controllers with RESTful API support and optional authorization features.**

### Install ASP.NET Core Extension Package
```bash
dotnet add package Aneiang.Pa.AspNetCore
```

### Quick Start

#### 1. Register Services
```csharp
using Aneiang.Pa.Extensions;
using Aneiang.Pa.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register news scraper services
builder.Services.AddNewsScraper(builder.Configuration);

// Add scraper controller support
builder.Services.AddScraperController(options =>
{
    options.RoutePrefix = "api/scraper"; // Route prefix, default "api/scraper"
    options.UseLowercaseInRoute = true; // Use lowercase in routes
    options.EnableResponseCaching = false; // Enable response caching
    options.CacheDurationSeconds = 300; // Cache duration (seconds)
});

var app = builder.Build();
app.MapControllers();
app.Run();
```

#### 2. API Endpoints

The controller provides the following RESTful API endpoints:

| Endpoint | Method | Description | Example |
| --- | --- | --- | --- |
| `/api/scraper/{source}` | GET | Get news from specified platform | `/api/scraper/BaiDu` |
| `/api/scraper/available-sources` | GET | Get list of all supported scraper sources | `/api/scraper/available-sources` |
| `/api/scraper/health` | GET | Check health status of all scrapers | `/api/scraper/health?timeoutMs=5000` |
| `/api/scraper/{source}/health` | GET | Check health status of specified scraper | `/api/scraper/BaiDu/health?timeoutMs=5000` |

**Supported Scraper Sources**: `BaiDu`, `Bilibili`, `WeiBo`, `ZhiHu`, `DouYin`, `HuPu`, `TouTiao`, `Tencent`, `JueJin`, `ThePaper`, `DouBan`, `IFeng`, `Csdn`, `CnBlog` (case-insensitive)

#### 3. Authorization Configuration (Optional)

By default, authorization is **disabled** (`Enabled = false`), and all API endpoints are publicly accessible. You can configure authorization to protect your APIs.

**Method 1: Configuration File (Recommended)**

Configure in `appsettings.json`:
```json
{
  "Scraper": {
    "Authorization": {
      "Enabled": true,
      "Scheme": "ApiKey",
      "ApiKeys": [
        "your-api-key-1",
        "your-api-key-2"
      ],
      "ApiKeyHeaderName": "X-API-Key",
      "ApiKeyQueryParameterName": "apiKey",
      "ExcludedRoutes": [
        "/api/scraper/health",
        "/api/scraper/available-sources"
      ],
      "UnauthorizedMessage": "Unauthorized access"
    }
  }
}
```

Then enable in code:
```csharp
builder.Services.ConfigureAuthorization(builder.Configuration);
```

**Method 2: Code Configuration**

```csharp
builder.Services.ConfigureAuthorization(options =>
{
    // Enable authorization
    options.Enabled = true;
    
    // Set authorization scheme: ApiKey, Custom, or Combined
    options.Scheme = AuthorizationScheme.ApiKey;
    
    // Configure API Key list
    options.ApiKeys = new List<string>
    {
        "your-api-key-1",
        "your-api-key-2"
    };
    
    // Set API Key header name (default: X-API-Key)
    options.ApiKeyHeaderName = "X-API-Key";
    
    // Set API Key query parameter name (optional)
    options.ApiKeyQueryParameterName = "apiKey";
    
    // Exclude routes that don't require authorization (supports wildcards)
    options.ExcludedRoutes = new List<string>
    {
        "/api/scraper/health",
        "/api/scraper/*/health"  // Wildcard matching
    };
    
    // Custom unauthorized error message
    options.UnauthorizedMessage = "Unauthorized access";
});
```

**Authorization Scheme Description**:

- **ApiKey**: Validates API Key passed via `X-API-Key` header or `apiKey` query parameter
- **Custom**: Uses custom authorization validation function
- **Combined**: API Key or custom validation function, either one satisfies

**Custom Authorization Example**:
```csharp
builder.Services.ConfigureAuthorization(options =>
{
    options.Enabled = true;
    options.Scheme = AuthorizationScheme.Custom;
    
    // Custom authorization validation function
    options.CustomAuthorizationFunc = (httpContext) =>
    {
        var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            // Validate token (e.g., validate JWT, query database, etc.)
            if (token == "valid-token")
            {
                // Can return ClaimsPrincipal
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, "user"),
                    new Claim(ClaimTypes.Role, "admin")
                };
                var identity = new ClaimsIdentity(claims, "custom");
                var principal = new ClaimsPrincipal(identity);
                return (true, principal);
            }
        }
        return (false, null);
    };
});
```

**Calling API with API Key**:

Via request header:
```bash
curl -H "X-API-Key: your-api-key-1" https://your-api.com/api/scraper/BaiDu
```

Via query parameter:
```bash
curl https://your-api.com/api/scraper/BaiDu?apiKey=your-api-key-1
```

#### 4. Health Check Feature

The health check feature requires registration of the `IScraperHealthCheckService` service. If you use the `AddNewsScraper()` method, this service will be automatically registered.

Health check endpoints:
- `GET /api/scraper/health?timeoutMs=5000` - Check health status of all scrapers
- `GET /api/scraper/{source}/health?timeoutMs=5000` - Check health status of specified scraper

Parameter description:
- `timeoutMs`: Timeout duration (milliseconds), range 1-60000, default 5000

#### 5. Sample Project

See the complete sample code in the `test/Aneiang.Pa.ClientDemo` directory.

## ✨ Advanced Usage - Dynamic Scraping (Aneiang.Pa.Dynamic)

**In addition to basic hot data scraping, we also provide a more flexible, lightweight, and independent scraping library - Aneiang.Pa.Dynamic, capable of scraping data collections from any website.**

### Add NuGet Package
```bash
dotnet add package Aneiang.Pa.Dynamic
```
Usage is achieved by defining model attributes. Example: Scraping [Cnblogs](https://www.cnblogs.com/pick/) hot data:

```csharp
services.AddDynamicScraper();
```

```csharp
var scraperFactory = scope.ServiceProvider.GetRequiredService<IDynamicScraper>();
var testDataSets = await scraperFactory.DatasetScraper<CnBlogOriginalResult>("https://www.cnblogs.com/pick");

```
The key is defining the `CnBlogOriginalResult` model:

```csharp
[HtmlContainer("div", htmlClass: "post-list", htmlId: "post_list", index: 1)]
[HtmlItem("article", htmlClass: "post-item")]
public class CnBlogOriginalResult
{
    [HtmlValue("a", htmlClass: "post-item-title")]
    public string Title { get; set; }

    [HtmlValue(".", attribute: "data-post-id")]
    public string Id { get; set; }

    [HtmlValue("a", htmlClass: "post-item-title", attribute: "href")]
    public string Url { get; set; }

    [HtmlValue(htmlXPath: ".//a[@class=\"post-item-author\"]/span")]
    public string AuthorName { get; set; }

    [HtmlValue("a", htmlClass: "post-item-author", attribute: "href")]
    public string AuthorUrl { get; set; }

    [HtmlValue("p", htmlClass: "post-item-summary")]
    public string Desc { get; set; }

    [HtmlValue(htmlXPath: ".//footer[@class=\"post-item-foot\"]/span[1]")]
    public string CreateTime { get; set; }

    [HtmlValue(htmlXPath: ".//footer[@class=\"post-item-foot\"]/a[2]")]
    public string CommentCount { get; set; }

    [HtmlValue(htmlXPath: ".//footer[@class=\"post-item-foot\"]/a[3]")]
    public string LikeCount { get; set; }

    [HtmlValue(htmlXPath: ".//footer[@class=\"post-item-foot\"]/a[4]")]
    public string ReadCount { get; set; }
}
```
Partial HTML code of the scraped Cnblogs page:
```html
<div id="post_list" class="post-list">
    <article class="post-item" data-post-id="19326078">
        <section class="post-item-body">

            <div class="post-item-text">
                <a class="post-item-title" href="https://www.cnblogs.com/ydswin/p/19326078"
                    target="_blank">Keepalived详解：原理、编译安装与高可用集群配置</a>
                <p class="post-item-summary">
                    <a href="https://www.cnblogs.com/ydswin" target="_blank">
                        <img src="https://pic.cnblogs.com/face/1307305/20240510180945.png" class="avatar" alt="博主头像" />
                    </a>
                    在高可用架构中，避免单点故障至关重要。Keepalived正是为了解决这一问题而生的轻量级工具。本文将深入浅出地介绍Keepalived的工作原理，并提供从编译安装到实战配置的完整指南。
                    1. Keepalived简介与工作原理 Keepalived是一个基于VRRP协议（虚拟路由冗余协议） 实现的 ...
                </p>
            </div>
            <footer class="post-item-foot">
                <a href="https://www.cnblogs.com/ydswin" class="post-item-author"
                    target="_blank"><span>dashery</span></a>

                <span class="post-meta-item">
                <span>2025-12-09 13:01</span>
                </span>
                <a class="post-meta-item btn"
                    href="https://www.cnblogs.com/ydswin/p/19326078#commentform" title="评论 1">
                    <svg width="16" height="16" xmlns="http://www.w3.org/2000/svg">
                        <use xlink:href="#icon_comment"></use>
                    </svg>
                    <span>1</span>
                </a>
                <a id="digg_control_19326078" title="推荐 7" class="post-meta-item btn "
                    href="javascript:void(0)"
                    onclick="DiggPost('ydswin', 19326078, 817406, 1);return false;">
                    <svg width="16" height="16" viewBox="0 0 16 16"
                        xmlns="http://www.w3.org/2000/svg">
                        <use xlink:href="#icon_digg"></use>
                    </svg>
                    <span id="digg_count_19326078">7</span>
                </a>
                <a class="post-meta-item btn" href="https://www.cnblogs.com/ydswin/p/19326078"
                    title="阅读 1892">
                    <svg width="16" height="16" viewBox="0 0 16 16"
                        xmlns="http://www.w3.org/2000/svg">
                        <use xlink:href="#icon_views"></use>
                    </svg>
                    <span>1892</span>
                </a>
                <span id="digg_tip_19326078" class="digg-tip" style="color: red"></span>
            </footer>

        </section>
        <figure>
        </figure>
    </article>
    <!-- Multiple <article> elements -->
</div>
```
### Attribute Description

- `HtmlContainerAttribute`: Dataset container attribute, representing the parent tag containing dataset items. Does not need to be the direct parent. Supports lookup via `id` or `class`. When uniqueness cannot be determined by `id`/`class`, set `index` to retrieve the specified HTML node.
- `HtmlItemAttribute`: Data item attribute, representing the HTML tag attributes for each data record. Supports lookup via `id` or `class`. When uniqueness cannot be determined by `id`/`class`, set `index` to retrieve the specified HTML node.
- `HtmlValueAttribute`: Data value attribute, representing the HTML tag attributes for each field in each data record. Supports lookup via `id` or `class`. When uniqueness cannot be determined by `id`/`class`, set `index` to retrieve the specified HTML node. The `htmlAttribute` field specifies which HTML attribute to extract the value from.

**PS: All three attributes support XPath for HTML tag retrieval. When `HTMLXPath` is not empty, other properties are ignored.**

#### HtmlTag Parameter Explanation

`HtmlTag` and `HTMLXPath` are developed based on `XPath` rules. For more information, refer to `XPath` documentation.

| Selector | Matching Structure | Example |
| --- | --- | --- |
| `p/b` | p directly contains b | `<p><b></b></p>` |
| `p//b` | b within any descendant of p | `<p><div><b></b></div></p>` |
| `p/div/b` | a > div > img | `<p><div><b></b></div></p>` |
| `.` | Only for `HtmlValue`, represents taking the HtmlTag of the current `HtmlItem` | |

### Scraping Result Screenshot
![](./assets/ScreenShot_D.png)

## Roadmap
- ✅ Weibo, Zhihu, Bilibili, Baidu, Douyin, Hupu, Toutiao, Tencent, Juejin, The Paper, Phoenix News, Douban hotlists
- 🚧 Planned: More platforms like GitHub, Steam
- 🧪 Considering: Data scraping needs beyond trending news

## Contributing
- PRs and Issues are welcome, especially for new platform crawlers, improved parsing, and robustness
- Please maintain consistent code style and include brief descriptions and necessary tests before submission
- If you wish to publish your new platform in the NuGet package, please discuss the approach in an Issue first

## License
Aneiang.Pa is licensed under the [MIT License](LICENSE).
