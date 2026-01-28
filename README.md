<p align="center">
    <img src="assets/logo.png" alt="Aneiang.Pa" width="600" style="vertical-align:middle;border-radius:8px;">
</p>

<p align="center">
  一个基于 .NET 的开箱即用爬虫库：热榜 / 特定领域 / Web API / 缓存 / 可选授权
</p>

<p align="center">
  中文 | <a href="README.en.md">English</a>
</p>

<p align="center">
  <a href="https://www.nuget.org/packages/Aneiang.Pa"><img src="https://img.shields.io/nuget/v/Aneiang.Pa.svg?style=flat-square&logo=nuget" /></a>
  <a href="https://www.nuget.org/packages/Aneiang.Pa"><img src="https://img.shields.io/nuget/dt/Aneiang.Pa.svg?style=flat-square&logo=nuget" /></a>
  <img src="https://img.shields.io/badge/target-netstandard2.1%20%7C%20net6.0-blue?style=flat-square" />
  <img src="https://img.shields.io/badge/status-active-success?style=flat-square" />
  <img src="https://img.shields.io/github/stars/AneiangSoft/Aneiang.Pa" />
</p>

---

## ✨ 亮点特性

- ✅ **多平台热榜**：微博 / 知乎 / B 站 / 百度 / 抖音 / 虎扑 / 头条 / 腾讯 / 掘金 / 澎湃 / 凤凰网 / 豆瓣 / CSDN / 博客园等
- ✅ **特定领域爬虫**：`Dynamic`（动态数据集爬取）/ `Lottery`（彩票数据）
- ✅ **ASP.NET Core Web API**：开箱即用的 RESTful API（支持 **数据缓存** 与 **可选授权**）
- ✅ **缓存支持**：None / Memory / Redis（默认 1 小时，可配置）
- ✅ **代理池**：轮询/随机 + 认证代理，降低封禁风险

> **⚠️ 建议抓取间隔 ≥ 5 分钟**，避免频繁抓取导致 IP 被封禁。
>
> **⚠️ 爬取的数据仅限用于个人学习、研究或公益目的。不得用于商业售卖、攻击他人或任何非法活动，否则需自行承担法律责任。**

- GitHub：<https://github.com/AneiangSoft/Aneiang.Pa>
- Gitee（同步）：<https://gitee.com/aneiangsoft/Aneiang.Pa>
- 热点新闻在线 Demo：<https://news.aneiang.com>
>- DEMO代码已经开源，支持DOCKER一键部署
>- GitHub：<https://github.com/AneiangSoft/Aneiang.Pa.News>
>- Gitee（同步）：<https://gitee.com/aneiangsoft/Aneiang.Pa.News>

---

## 📚 目录

- [架构调整](#架构调整)
- [安装（NuGet）](#安装nuget)
  - [聚合包](#聚合包)
  - [单个功能包](#单个功能包)
  - [已发布包](#已发布包)
- [快速开始（本地 Demo）](#快速开始本地-demo)
- [在你的项目中使用](#在你的项目中使用)
- [🌐 代理池功能（Proxy Pool）](#-代理池功能proxy-pool)
- [🚀 ASP.NET Core Web API 集成（Aneiang.Pa.AspNetCore）](#-aspnet-core-web-api-集成aneiangpaaspnetcore)
  - [快速开始（只调用两个扩展方法）](#快速开始只调用两个扩展方法)
  - [缓存配置（Scraper）](#缓存配置scraper)
  - [授权配置（Scraper:Authorization）](#授权配置scraperauthorization)
  - [API 端点说明](#api-端点说明)
- [✨ 高阶用法 - 动态爬取（Aneiang.Pa.Dynamic）](#-高阶用法---动态爬取aneiangpadynamic)
- [贡献](#贡献)
- [许可证](#许可证)

---

## 架构调整

为了更好地组织和扩展功能，项目架构已进行调整：

- `src/News`: 存放所有新闻热榜相关的爬虫项目。
- `src/Sectors`: 存放特定领域的爬虫项目，如动态爬虫和彩票爬虫。
- `src/Core`: 存放核心接口、模型和公共服务。

---

## 安装（NuGet）

项目提供两种聚合包和按需引用的单个功能包，开发者可根据需求选择。

### 聚合包

1. **全局聚合包** (`Aneiang.Pa`)：包含所有已实现的功能。

```bash
dotnet add package Aneiang.Pa
```

2. **热榜聚合包** (`Aneiang.Pa.News`)：仅包含所有新闻热榜相关的爬虫。

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

---

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

---

## 版本与更新

- 版本号以 `Directory.Build.props` 中的 `Version` 为准（当前为 `2.1.5`）。
- 版本变更记录见 [CHANGELOG.md](CHANGELOG.md)。

---

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

---

## 🌐 代理池功能（Proxy Pool）

**支持配置多个代理服务器，自动轮询或随机选择代理进行请求，有效降低封禁风险。**

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

---

## 🚀 ASP.NET Core Web API 集成（Aneiang.Pa.AspNetCore）

**提供开箱即用的 Web API 控制器，支持 RESTful API 调用和可选授权功能。**

### 安装

```bash
dotnet add package Aneiang.Pa.AspNetCore
```

### 快速开始（只调用两个扩展方法）

> 设计目标：外部项目尽量“少写代码”。
>
> - `AddPaScraperApi(...)`：注册 API + 缓存
> - `AddPaScraperAuthorization(...)`：按需启用授权（支持配置文件 + 可选代码覆盖）

```csharp
using Aneiang.Pa.AspNetCore.Extensions;
using Aneiang.Pa.Lottery.Extensions;
using Aneiang.Pa.News.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// 业务服务（热榜/彩票等）
builder.Services.AddNewsScraper(builder.Configuration);
builder.Services.AddLotteryScraper();

// 1) 注册 API + 缓存（读取 Scraper 配置节）
builder.Services.AddPaScraperApi(builder.Configuration);

// 2) 如需授权再启用（读取 Scraper:Authorization 配置节，支持可选代码覆盖）
builder.Services.AddPaScraperAuthorization(builder.Configuration);

var app = builder.Build();
app.MapControllers();
app.Run();
```

### 缓存配置（Scraper）

> 默认：`CacheProvider=None`（不缓存），`CacheDuration=01:00:00`（1小时）。

```json
{
  "Scraper": {
    "CacheProvider": "Memory",
    "CacheDuration": "01:00:00",

    "Redis": {
      "Configuration": "localhost:6379,password=your_password,defaultDatabase=2",
      "InstanceName": "Aneiang.Pa:"
    }
  }
}
```

**CacheProvider 可选值**：
- `None`：不启用缓存
- `Memory`：进程内内存缓存
- `Redis`：Redis 分布式缓存

**Redis 连接字符串常用参数**（StackExchange.Redis）：
- `password=xxx`：Redis 密码（requirepass / ACL）
- `user=xxx`：ACL 用户（Redis 6+）
- `defaultDatabase=2`：指定 DB（注意：Redis Cluster 不支持多 DB）

### 授权配置（Scraper:Authorization）

授权支持三种策略：
- `ApiKey`：请求头或查询参数 API Key
- `Custom`：自定义策略 `CustomAuthorizationFunc`
- `Combined`：ApiKey 或 Custom 任意一种通过即可

#### 方式 1：仅配置文件（推荐）

```json
{
  "Scraper": {
    "Authorization": {
      "Enabled": true,
      "Scheme": "ApiKey",
      "ApiKeys": ["demo-api-key-12345"],
      "ApiKeyHeaderName": "X-API-Key",
      "ApiKeyQueryParameterName": "apiKey",
      "ExcludedRoutes": [
        "/api/scraper/health",
        "/api/scraper/news/sources"
      ],
      "UnauthorizedMessage": "未授权访问"
    }
  }
}
```

代码只需：

```csharp
builder.Services.AddPaScraperAuthorization(builder.Configuration);
```

#### 方式 2：配置文件 + 可选代码覆盖（自定义策略示例）

```csharp
builder.Services.AddPaScraperAuthorization(builder.Configuration, configure: opt =>
{
    opt.Enabled = true;
    opt.Scheme = Aneiang.Pa.AspNetCore.Options.AuthorizationScheme.Custom;
    opt.CustomAuthorizationFunc = httpContext =>
    {
        var token = httpContext.Request.Headers["X-Demo-Token"].ToString();
        return token == "valid-token" ? (true, null) : (false, null);
    };
});
```

### API 端点说明

| 端点 | 方法 | 说明 | 示例 |
| --- | --- | --- | --- |
| `/api/scraper/news/{source}` | GET | 获取指定平台的热榜 | `/api/scraper/news/BaiDu` |
| `/api/scraper/news/sources` | GET | 获取所有支持的热榜源 | `/api/scraper/news/sources` |
| `/api/scraper/lottery/welfare/{type}` | GET | 获取福利彩票开奖信息 | `/api/scraper/lottery/welfare/SSQ` |
| `/api/scraper/lottery/sport/{type}` | GET | 获取体育彩票开奖信息 | `/api/scraper/lottery/sport/DLT` |
| `/api/scraper/lottery/types` | GET | 获取所有支持的彩票类型 | `/api/scraper/lottery/types` |
| `/api/scraper/health` | GET | 检查所有爬虫健康状态 | `/api/scraper/health?timeoutMs=5000` |
| `/api/scraper/{source}/health` | GET | 检查指定爬虫健康状态 | `/api/scraper/BaiDu/health?timeoutMs=5000` |

---

## ✨ 高阶用法 - 动态爬取（Aneiang.Pa.Dynamic）

> 为了避免 README 过长，这里将详细示例折叠。你可以直接展开查看完整示例。

<details>
<summary><b>展开：Dynamic 动态爬取完整示例（CnBlogs）</b></summary>

### 引入 NuGet

```bash
dotnet add package Aneiang.Pa.Dynamic
```

### 注册

```csharp
services.AddDynamicScraper();
```

### 使用

```csharp
var scraperFactory = scope.ServiceProvider.GetRequiredService<IDynamicScraper>();
var testDataSets = await scraperFactory.DatasetScraper<CnBlogOriginalResult>("https://www.cnblogs.com/pick");
```

### 定义模型（CnBlogOriginalResult）

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

### HTML 示例（节选）

```html
<div id="post_list" class="post-list">
    <article class="post-item" data-post-id="19326078">
        <section class="post-item-body">
            <div class="post-item-text">
                <a class="post-item-title" href="https://www.cnblogs.com/ydswin/p/19326078" target="_blank">...</a>
                <p class="post-item-summary">...</p>
            </div>
        </section>
    </article>
</div>
```

### 特性说明

- `HtmlContainerAttribute`：数据集容器特性（支持 id/class/xpath）
- `HtmlItemAttribute`：数据项特性（支持 id/class/xpath）
- `HtmlValueAttribute`：字段取值特性（支持 id/class/xpath，可指定 attribute）

**PS：以上三个特性都支持 XPath 检索 HTML 标签，`HTMLXPath` 不为空时，其他属性不生效。**

</details>

---

## 贡献

- 欢迎 PR / Issue，尤其是新增平台爬虫、改进解析与健壮性
- 提交前请保持代码风格一致，并附带简要说明和必要的测试
- 如果希望在 NuGet 包中发布你新增的平台，请在 Issue 先讨论方案

## 许可证

Aneiang.Pa 采用 [MIT 许可证](LICENSE)。
