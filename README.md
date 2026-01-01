<p align="center">
    <img src="assets/logo.png" alt="Aneiang.Pa" width="600" style="vertical-align:middle;border-radius:8px;">
</p>

中文 | [English](README.en.md)

[![NuGet](https://img.shields.io/nuget/v/Aneiang.Pa.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Aneiang.Pa)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Aneiang.Pa.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Aneiang.Pa)
[![Target](https://img.shields.io/badge/target-netstandard2.1-blue?style=flat-square)](#)
[![Status](https://img.shields.io/badge/status-active-success?style=flat-square)](#)
![GitHub Repo stars](https://img.shields.io/github/stars/AneiangSoft/Aneiang.Pa)


一个基于 .NET 开箱即用的爬虫库，使用复杂度极低。项目将爬虫分为两大类：`News`（热榜）和 `Sectors`（特定领域）。

- **热榜**：预设了对多个主流平台热榜的爬取支持，包括微博、知乎、B 站、百度、抖音、虎扑、头条、腾讯、掘金、澎湃、凤凰网、豆瓣、CSDN、博客园等。
- **特定领域**：提供更灵活的爬虫，目前包括 `Dynamic`（动态数据集爬取）和 `Lottery`（彩票数据爬取）。

项目完全开源，后续将持续增加更多平台和数据类型的支持。

**⚠️ 抓取间隔建议控制在五分钟以上，避免频繁抓取导致 IP 被封禁**

**⚠️ 爬取的数据仅限用于个人学习、研究或公益目的。不得用于商业售卖、攻击他人或任何非法活动，否则需自行承担法律责任。**

- Github地址：[https://github.com/AneiangSoft/Aneiang.Pa](https://github.com/AneiangSoft/Aneiang.Pa)
- Gitee地址（同步更新）：[https://gitee.com/aneiangsoft/Aneiang.Pa](https://gitee.com/aneiangsoft/Aneiang.Pa)
- 热点新闻在线Demo：[https://news.aneiang.com](https://news.aneiang.com)

## 架构调整
为了更好地组织和扩展功能，项目架构已进行调整：
- `src/News`: 存放所有新闻热榜相关的爬虫项目。
- `src/Sectors`: 存放特定领域的爬虫项目，如动态爬虫和彩票爬虫。
- `src/Core`: 存放核心接口、模型和公共服务。

## 安装（NuGet）
项目提供两种聚合包和按需引用的单个功能包，开发者可根据需求选择。

### 聚合包
1.  **全局聚合包** (`Aneiang.Pa`)：包含所有已实现的功能，包括新闻热榜、特定领域爬虫等。
    ```bash
    dotnet add package Aneiang.Pa
    ```
2.  **热榜聚合包** (`Aneiang.Pa.News`)：仅包含所有新闻热榜相关的爬虫。
    ```bash
    dotnet add package Aneiang.Pa.News
    ```

### 单个功能包
如果只需要特定平台或功能，可以按需引用单个包以减小依赖体积。
```bash
# 示例：仅引用百度热榜爬虫
dotnet add package Aneiang.Pa.BaiDu
```

### 已发布包
| Package | 说明 |
| --- | --- |
| **Aneiang.Pa** | **聚合包，包含全部平台实现** |
| Aneiang.Pa.Core | 核心接口与模型、代理池功能 |
| Aneiang.Pa.AspNetCore | ASP.NET Core Web API 扩展（提供 RESTful API 控制器） |
| **--- News (热榜) ---** | **---** |
| Aneiang.Pa.News | 热榜聚合包，包含以下所有新闻平台 |
| Aneiang.Pa.BaiDu | 百度热榜爬虫 |
| Aneiang.Pa.Bilibili | B 站热搜爬虫 |
| Aneiang.Pa.WeiBo | 微博热搜爬虫 |
| Aneiang.Pa.ZhiHu | 知乎热榜爬虫 |
| Aneiang.Pa.DouYin | 抖音热榜爬虫 |
| Aneiang.Pa.HuPu | 虎扑热帖/热榜爬虫 |
| Aneiang.Pa.TouTiao | 今日头条热榜爬虫 |
| Aneiang.Pa.Tencent | 腾讯热榜爬虫 |
| Aneiang.Pa.JueJin | 掘金热榜爬虫 |
| Aneiang.Pa.ThePaper | 澎湃热榜爬虫 |
| Aneiang.Pa.DouBan | 豆瓣热榜爬虫 |
| Aneiang.Pa.IFeng | 凤凰网热榜爬虫 |
| Aneiang.Pa.Csdn | CSDN热榜爬虫 |
| Aneiang.Pa.CnBlog | 博客园热榜爬虫 |
| **--- Sectors (特定领域) ---** | **---** |
| Aneiang.Pa.Dynamic | 动态爬虫，可爬取任意网站的数据集合 |
| Aneiang.Pa.Lottery | 彩票数据爬虫 |

## 快速开始（本地 Demo）
1) 还原 & 构建
```bash
dotnet restore
dotnet build test/Aneiang.Pa.Demo/Aneiang.Pa.Demo.csproj
```
2) 运行 Demo（默认抓取百度热榜，可修改 `ScraperSource`）
```bash
dotnet run --project test/Aneiang.Pa.Demo
```
运行后，将在控制台看到抓取到的百度热榜数据。

## 在你的项目中使用

### 1. 注册服务

最简单的方式是使用全局注册方法，一键添加所有爬虫功能。

```csharp
// 注册所有爬虫（推荐）
services.AddPaScraper();
```

如果你只需要特定功能，也可以按需注册：

```csharp
// 仅注册热榜爬虫
services.AddNewsScraper();

// 仅注册彩票爬虫
services.AddLotteryScraper();

// 仅注册动态爬虫
services.AddDynamicScraper();

// 仅注册百度热榜爬虫
services.AddBaiDuScraper();
```

### 2. 使用爬虫

注册服务后，你可以从依赖注入容器中获取相应的服务实例。

**获取热榜数据**

```csharp
// 通过工厂模式获取
var factory = scope.ServiceProvider.GetRequiredService<INewsScraperFactory>();
var scraper = factory.GetScraper(ScraperSource.BaiDu);
var result = await scraper.GetNewsAsync();

// 或直接注入单个爬虫
var baiduScraper = scope.ServiceProvider.GetRequiredService<IBaiDuNewScraper>();
var baiduResult = await baiduScraper.GetNewsAsync();
```

**获取彩票数据**

```csharp
var lotteryScraper = scope.ServiceProvider.GetRequiredService<ILotteryScraper>();
var ssqResult = await lotteryScraper.GetLotteryDataAsync(LotteryType.SSQ); // 福利彩票
var dltResult = await lotteryScraper.GetLotteryDataAsync(LotteryType.DLT); // 体育彩票
```

**使用动态爬虫**

```csharp
var dynamicScraper = scope.ServiceProvider.GetRequiredService<IDynamicScraper>();
// 爬取博客园热门文章（需要先定义 CnBlogOriginalResult 模型）
var cnblogsPosts = await dynamicScraper.DatasetScraper<CnBlogOriginalResult>("https://www.cnblogs.com/pick");
```

## 🌐 代理池功能（Proxy Pool）

**支持配置多个代理服务器，自动轮询或随机选择代理进行请求，有效避免 IP 被封禁。**

### 功能特性

- ✅ 支持多个代理服务器配置
- ✅ 支持两种选择策略：轮询（RoundRobin）和随机（Random）
- ✅ 支持带认证的代理（`http://user:password@host:port`）
- ✅ 可通过配置文件或代码配置
- ✅ 未启用时自动退化为普通 HttpClient

### 使用方式

#### 方式1：通过配置文件（推荐）

在 `appsettings.json` 中配置：
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

在代码中注册：
```csharp
using Aneiang.Pa.Core.Proxy;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // 注册带代理池支持的默认 HttpClient
        services.AddPaDefaultHttpClientWithProxy(
            proxyConfiguration: context.Configuration.GetSection("Scraper:ProxyPool"));
        
        // 注册爬虫服务（会自动使用配置的 HttpClient）
        services.AddNewsScraper(context.Configuration);
    })
    .Build();
```

#### 方式2：通过代码配置

```csharp
using Aneiang.Pa.Core.Proxy;

services.AddPaDefaultHttpClientWithProxy(
    proxyConfigure: options =>
    {
        options.Enabled = true;
        options.Strategy = ProxySelectionStrategy.RoundRobin; // 或 Random
        options.Proxies = new List<string>
        {
            "http://127.0.0.1:7890",
            "http://user:password@proxy.example.com:8080",
            "http://192.168.1.100:3128"
        };
    });

services.AddNewsScraper();
```

#### 仅注册代理池服务（不注册 HttpClient）

如果只需要代理池服务，可以使用：
```csharp
// 仅注册代理池服务
services.AddPaProxyPool(
    configuration: context.Configuration.GetSection("Scraper:ProxyPool"));

// 或通过代码配置
services.AddPaProxyPool(
    configure: options =>
    {
        options.Enabled = true;
        options.Strategy = ProxySelectionStrategy.Random;
        options.Proxies = new List<string> { "http://127.0.0.1:7890" };
    });

// 然后注入 IProxyPool 使用
var proxyPool = serviceProvider.GetRequiredService<IProxyPool>();
var proxyUri = proxyPool.GetNextProxy();
```

### 代理选择策略

- **RoundRobin（轮询）**：按顺序依次使用代理服务器，确保负载均衡
- **Random（随机）**：每次随机选择一个代理服务器

### 代理地址格式

支持以下格式的代理地址：
- `http://host:port` - HTTP 代理（无认证）
- `http://user:password@host:port` - HTTP 代理（带认证）
- `https://host:port` - HTTPS 代理

### 注意事项

1. **启用检查**：如果 `Enabled = true` 但未配置代理列表，会抛出异常
2. **HttpClient 名称**：默认 HttpClient 名称为 `Aneiang.Pa.DefaultHttpClient`，爬虫会自动使用该 HttpClient
3. **代理优先级**：如果在 `AddNewsScraper` 之前调用 `AddPaDefaultHttpClientWithProxy`，爬虫会使用配置的代理池
4. **未启用时**：当 `Enabled = false` 或代理列表为空时，会自动退化为普通 HttpClient，不影响正常使用

## 🚀 ASP.NET Core Web API 集成（Aneiang.Pa.AspNetCore）

**提供开箱即用的 Web API 控制器，支持 RESTful API 调用和可选的授权功能。**

### 安装 ASP.NET Core 扩展包
```bash
dotnet add package Aneiang.Pa.AspNetCore
```

### 快速开始

#### 1. 注册服务
```csharp
using Aneiang.Pa.Extensions;
using Aneiang.Pa.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 注册新闻爬虫服务
builder.Services.AddNewsScraper(builder.Configuration);

// 添加爬虫控制器支持
builder.Services.AddScraperController(options =>
{
    options.RoutePrefix = "api/scraper";      // 路由前缀，默认 "api/scraper"
    options.UseLowercaseInRoute = true;       // 路由使用小写
    options.EnableResponseCaching = true;     // 启用响应缓存
    options.CacheDurationSeconds = 300;       // 缓存时长（秒）
});

var app = builder.Build();

// 启用响应缓存中间件（必须在使用缓存的端点之前调用）
app.UseResponseCaching();

app.MapControllers();
app.Run();
```

#### 2. API 端点

控制器提供以下 RESTful API 端点：

| 端点 | 方法 | 说明 | 示例 |
| --- | --- | --- | --- |
| `/api/scraper/{source}` | GET | 获取指定平台的新闻 | `/api/scraper/BaiDu` |
| `/api/scraper/available-sources` | GET | 获取所有支持的爬虫源列表 | `/api/scraper/available-sources` |
| `/api/scraper/health` | GET | 检查所有爬虫的健康状态 | `/api/scraper/health?timeoutMs=5000` |
| `/api/scraper/{source}/health` | GET | 检查指定爬虫的健康状态 | `/api/scraper/BaiDu/health?timeoutMs=5000` |

**支持的爬虫源**：`BaiDu`、`Bilibili`、`WeiBo`、`ZhiHu`、`DouYin`、`HuPu`、`TouTiao`、`Tencent`、`JueJin`、`ThePaper`、`DouBan`、`IFeng`、`Csdn`、`CnBlog`（支持大小写不敏感）

#### 3. 授权配置（可选）

默认情况下，授权功能是**未启用**的（`Enabled = false`），所有 API 端点都可以公开访问。如果需要保护 API，可以配置授权。

**方式1：通过配置文件（推荐）**

在 `appsettings.json` 中配置：
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
      "UnauthorizedMessage": "未授权访问"
    }
  }
}
```

然后在代码中启用：
```csharp
builder.Services.ConfigureAuthorization(builder.Configuration);
```

**方式2：通过代码配置**

```csharp
builder.Services.ConfigureAuthorization(options =>
{
    // 启用授权
    options.Enabled = true;
    
    // 设置授权方式：ApiKey、Custom 或 Combined
    options.Scheme = AuthorizationScheme.ApiKey;
    
    // 配置 API Key 列表
    options.ApiKeys = new List<string>
    {
        "your-api-key-1",
        "your-api-key-2"
    };
    
    // 设置 API Key 请求头名称（默认：X-API-Key）
    options.ApiKeyHeaderName = "X-API-Key";
    
    // 设置 API Key 查询参数名称（可选）
    options.ApiKeyQueryParameterName = "apiKey";
    
    // 排除不需要授权的路由（支持通配符）
    options.ExcludedRoutes = new List<string>
    {
        "/api/scraper/health",
        "/api/scraper/*/health"  // 通配符匹配
    };
    
    // 自定义未授权错误消息
    options.UnauthorizedMessage = "未授权访问";
});
```

**授权方式说明**：

- **ApiKey**：通过请求头 `X-API-Key` 或查询参数 `apiKey` 传递 API Key 进行验证
- **Custom**：使用自定义授权验证函数
- **Combined**：API Key 或自定义验证函数，满足任一即可

**自定义授权示例**：
```csharp
builder.Services.ConfigureAuthorization(options =>
{
    options.Enabled = true;
    options.Scheme = AuthorizationScheme.Custom;
    
    // 自定义授权验证函数
    options.CustomAuthorizationFunc = (httpContext) =>
    {
        var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
        {            var token = authHeader.Substring("Bearer ".Length).Trim();
            // 验证 token（例如：验证 JWT、查询数据库等）
            if (token == "valid-token")
            {
                // 可以返回 ClaimsPrincipal
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

**使用 API Key 调用 API**：

通过请求头：
```bash
curl -H "X-API-Key: your-api-key-1" https://your-api.com/api/scraper/BaiDu
```

通过查询参数：
```bash
curl https://your-api.com/api/scraper/BaiDu?apiKey=your-api-key-1
```

#### 4. 健康检查功能

健康检查功能需要注册 `IScraperHealthCheckService` 服务。如果使用 `AddNewsScraper()` 方法，该服务会自动注册。

健康检查端点：
- `GET /api/scraper/health?timeoutMs=5000` - 检查所有爬虫的健康状态
- `GET /api/scraper/{source}/health?timeoutMs=5000` - 检查指定爬虫的健康状态

参数说明：
- `timeoutMs`：超时时间（毫秒），范围 1-60000，默认 5000

#### 5. 示例项目

查看 `test/Aneiang.Pa.ClientDemo` 目录下的完整示例代码。

## ✨ 高阶用法 - 动态爬取（Aneiang.Pa.Dynamic）

**除了基础的热门数据爬取外，还提供了更加灵活、轻量、独立的爬虫库 - Aneiang.Pa.Dynamic，可以做到爬取任意网站的数据集合。**

### 引入Nuget
```bash
dotnet add package Aneiang.Pa.Dynamic
```
使用时通过定义模型特性来实现，以爬取[博客园](https://www.cnblogs.com/pick/)热门数据为例：

```csharp
services.AddDynamicScraper();
```

```csharp
var scraperFactory = scope.ServiceProvider.GetRequiredService<IDynamicScraper>();
var testDataSets = await scraperFactory.DatasetScraper<CnBlogOriginalResult>("https://www.cnblogs.com/pick");

```
重点在于定义CnBlogOriginalResult模型

```csharp
[HtmlContainer("div", htmlClass: "post-list",htmlId: "post_list", index: 1)]
[HtmlItem("article",htmlClass: "post-item")]
public class CnBlogOriginalResult
{
    [HtmlValue("a",htmlClass: "post-item-title")]
    public string Title { get; set; }

    [HtmlValue(".",attribute: "data-post-id")]
    public string Id { get; set; }

    [HtmlValue("a", htmlClass: "post-item-title",attribute: "href")]
    public string Url { get; set; }

    [HtmlValue(htmlXPath:".//a[@class=\"post-item-author\"]/span")]
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
爬取的博客园HTML部分代码如下：
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
    <!-- 多个 <article> -->
</div>
```
### 特性说明

- `HtmlContainerAttribute`：数据集容器特性，包含数据集标签的父级标签，可以不是直接父级，支持通过`id`、`class`查找，当无法通过`id`、`class`判断唯一的时候，可以通过设置`index`获取指定的HTML节点。
- `HtmlItemAttribute`：数据项特性，每条数据对应的HTML标签属性，支持通过`id`、`class`查找，当无法通过`id`、`class`判断唯一的时候，可以通过设置`index`获取指定的HTML节点。
- `HtmlValueAttribute`：数据值特性，每条数据，每个字段对应的HTML标签属性，支持通过`id`、`class`查找，当无法通过`id`、`class`判断唯一的时候，可以通过设置`index`获取指定的HTML节点；`htmlAttribute`字段指定从哪个html特性中获取值。

**PS:以上三个特性都支持XPath检索HTML标签，`HTMLXPath`不为空时，其他属性都不生效**

#### HtmlTag参数解析

`HtmlTag` 和 `HTMLXPath` 底层基于`XPath`规则开发，更多信息可查阅`XPath`相关文档。

| 选择器 | 匹配结构 | 示例 |
| --- | --- | --- |
| `p/b` | p直接包含b | `<p><b></b></p>` |
| `p//b` | p的任何后代中的p | `<p><div><b></b></div></p>` |
| `p/div/b` | a > div > img | `<p><div><b></b></div></p>` |
| `.` | 仅`HtmlValue`设置，表示取当前`HtmlItem`的HtmlTag|||

### 爬取结果截图
![](./assets/ScreenShot_D.png)

## 规划与 Roadmap
- ✅ 微博、知乎、B 站、百度、抖音、虎扑、头条、腾讯、掘金、澎湃、凤凰网、豆瓣热榜
- ✅ 彩票数据爬取（福利彩票、体育彩票）
- 🚧 计划：GitHub、Steam等更多平台
- 🧪 考虑：除热门新闻之外的其他数据爬取需求

## 贡献
- 欢迎 PR / Issue，尤其是新增平台爬虫、改进解析与健壮性
- 提交前请保持代码风格一致，并附带简要说明和必要的测试
- 如果希望在 NuGet 包中发布你新增的平台，请在 Issue 先讨论方案

## 许可证
Aneiang.Pa 采用 [MIT 许可证](LICENSE)。
