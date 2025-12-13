<p align="center">
    <img src="assets/logo.png" alt="Aneiang.Pa" width="600" style="vertical-align:middle;border-radius:8px;">
</p>

[![NuGet](https://img.shields.io/nuget/v/Aneiang.Pa.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Aneiang.Pa)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Aneiang.Pa.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Aneiang.Pa)
[![Target](https://img.shields.io/badge/target-netstandard2.1-blue?style=flat-square)](#)
[![Status](https://img.shields.io/badge/status-active-success?style=flat-square)](#)

一个基于 .NET 的多平台热门新闻/热榜爬虫库，当前支持微博、知乎、B 站、百度、抖音、虎扑、头条、腾讯、掘金、澎湃、凤凰网、豆瓣等平台爬虫，并附带 Demo 示例。项目开源，后续将增加更多平台。

**⚠️ 抓取间隔建议控制在五分钟以上，避免频繁抓取导致 IP 被封禁**

## 安装（NuGet）
推荐聚合包（含全部平台）：
```bash
dotnet add package Aneiang.Pa --version 1.0.7
```
按需引用单个包（示例）：
```bash
dotnet add package Aneiang.Pa.BaiDu --version 1.0.7
```

### 已发布包
| Package | 说明 |
| --- | --- |
| Aneiang.Pa | 聚合包，包含全部平台实现 |
| Aneiang.Pa.Core | 核心接口与模型 |
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

## 在你的项目中使用（NuGet）
ConfigureServices:
```csharp

// 以下两种方式任选其一：
// 自动注册各平台爬虫
services.AddNewsScraper();

// 注册单个平台爬虫
services.AddBaiDuScraper();
```

```csharp
// 通过工厂模式获取爬虫实例
var factory = scope.ServiceProvider.GetRequiredService<INewsScraperFactory>();
var scraper = factory.GetScraper(ScraperSource.BaiDu);
var result = await scraper.GetNewsAsync();

// 直接注入单个平台爬虫
var scraper = scope.ServiceProvider.GetRequiredService<IBaiDuNewScraper>();
var result = await scraper.GetNewsAsync();
```

## 规划与 Roadmap
- ✅ 微博、知乎、B 站、百度、抖音、虎扑、头条、腾讯、掘金、澎湃、凤凰网、豆瓣热榜
- 🚧 计划：GitHub、Steam等更多平台
- 🧪 考虑：除热门新闻之外的其他数据爬取需求

## 贡献
- 欢迎 PR / Issue，尤其是新增平台爬虫、改进解析与健壮性
- 提交前请保持代码风格一致，并附带简要说明和必要的测试
- 如果希望在 NuGet 包中发布你新增的平台，请在 Issue 先讨论方案

## 许可证
Aneiang.Pa 采用 [MIT 许可证](LICENSE)。

