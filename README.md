<p align="center">
    <img src="assets/logo.png" alt="Aneiang.Pa" width="600" style="vertical-align:middle;border-radius:8px;">
</p>

[![NuGet](https://img.shields.io/nuget/v/Aneiang.Pa.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Aneiang.Pa)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Aneiang.Pa.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Aneiang.Pa)
[![Target](https://img.shields.io/badge/target-netstandard2.1-blue?style=flat-square)](#)
[![Status](https://img.shields.io/badge/status-active-success?style=flat-square)](#)

一个基于 .NET 的多平台热门新闻/热榜爬虫库，当前支持微博、知乎、B 站、百度、抖音、虎扑、头条、腾讯、掘金、澎湃、凤凰网、豆瓣等平台爬虫，并附带 Demo 示例。项目开源，后续将增加更多平台。

**⚠️ 抓取间隔建议控制在五分钟以上，避免频繁抓取导致 IP 被封禁**

**⚠️ 爬取的数据仅限用于个人学习、研究或公益目的。不得用于商业售卖、攻击他人或任何非法活动，否则需自行承担法律责任。**

## 安装（NuGet）
推荐聚合包（含全部平台）：
```bash
dotnet add package Aneiang.Pa
```
按需引用单个包（示例）：
```bash
dotnet add package Aneiang.Pa.BaiDu
```

### 已发布包
| Package | 说明 |
| --- | --- |
| Aneiang.Pa | 聚合包，包含全部平台实现 |
| Aneiang.Pa.Core | 核心接口与模型 |
| Aneiang.Pa.Dynamic | 动态爬虫 |
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
| `.` | 仅`HtmlValue`设置，表示取当前`HtmlItem`的HtmlTag||

### 爬取结果截图
![](./assets/ScreenShot_D.png)

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

