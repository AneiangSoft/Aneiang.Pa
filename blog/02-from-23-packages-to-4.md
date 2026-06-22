---
title: ".NET 爬虫库从 23 个包到 4 个包，我做了什么？"
date: 2026-06-22
tags: [.NET, 架构, 重构, 开源]
---

## 背景

Aneiang.Pa 是一个 .NET 爬虫库，最初设计是"每个平台一个 NuGet 包"。

微博一个包、知乎一个包、B站一个包……一共 16 个新闻平台 + 彩票 + 动态爬虫 + 核心库 + ASP.NET Core 集成 + MCP Server = **23 个 NuGet 包**。

用户问的第一个问题永远是：**"我应该引用哪个包？"**

4.0 重构后，变成了 **4 个包**。核心包 `Aneiang.Pa` 单包内置 18 个平台。

这篇文章聊聊重构背后的思考。

---

## 问题 1：每个平台一个项目，维护成本爆炸

旧结构：

```
src/
├── News/
│   ├── Aneiang.Pa.WeiBo/     ← 一个 csproj
│   ├── Aneiang.Pa.ZhiHu/     ← 一个 csproj
│   ├── Aneiang.Pa.Bilibili/  ← 一个 csproj
│   └── ...（16 个）
├── Lotteries/
├── Core/
├── Aneiang.Pa.AspNetCore/
├── Aneiang.Pa.McpServer/
└── Aneiang.Pa/               ← 聚合包
```

每个平台一个项目，意味着：
- 每个平台都要写一个接口、一个实现、一个 Options 类、一个 DI 扩展
- 新增平台要改聚合包的 csproj 引用
- 第三方扩展必须新建项目、发 NuGet 包

**核心问题：平台之间的差异不在于"代码逻辑"，而在于"URL 和解析规则"。**

---

## 解决方案：YAML Recipe

把"怎么抓"和"怎么解析"从 C# 代码里抽出来，变成 YAML 文件。

```yaml
name: WeiBo
category: News
display_name: 微博热搜
fetch:
  url: https://s.weibo.com/top/summary?cate=realtimehot
parse:
  type: html
  container: "xpath://*[@id='pl_top_realtimehot']//table/tbody/tr[position()>1]"
  fields:
    title: { selector: "xpath:.//td[@class='td-02']/a", trim: true }
    url:   { selector: "xpath:.//td[@class='td-02']/a", attr: href, base: "https://s.weibo.com" }
```

**新增一个平台 = 写一份 YAML = 0 行 C# 代码。**

18 个平台全部以 YAML 嵌入到 `Aneiang.Pa.dll` 中，启动自动加载。

---

## 问题 2：横切关注点散落在每个爬虫里

旧代码里每个爬虫都长这样：

```csharp
public async Task<AneiangGenericListResult<NewsItem>> GetNewsAsync()
{
    try
    {
        _options.Check();
        var client = ScraperHttpClientHelper.CreateConfiguredClient(...);
        var response = await client.GetStringAsync(url);
        // ... 解析 ...
    }
    catch (Exception e)
    {
        return ScraperHttpClientHelper.CreateNewsErrorResult(e, Source);
    }
}
```

每个爬虫重复：try/catch、HttpClient 创建、UA 设置、错误转换。

日志？没有。重试？没有。缓存？没有。限流？没有。

---

## 解决方案：中间件管道

借鉴 ASP.NET Core 的中间件模型：

```
请求 → Logging → Metrics → Tracing → Cache → RateLimit → CircuitBreaker → Retry → Timeout → Fetch → Parse → 响应
```

所有横切关注点集中在管道层，业务爬虫只关心"如何解析"。

```csharp
// 用户视角：完全无感
var result = await Pa.Source("WeiBo").GetAsync();
// 背后自动经过 8 个中间件
```

---

## 问题 3：23 个包，用户不知道选哪个

旧版包列表：

```
Aneiang.Pa (聚合包)
Aneiang.Pa.Core
Aneiang.Pa.AspNetCore
Aneiang.Pa.Dynamic
Aneiang.Pa.News (热榜聚合包)
Aneiang.Pa.BaiDu
Aneiang.Pa.Bilibili
Aneiang.Pa.WeiBo
...（16 个平台包）
Aneiang.Pa.Lottery
Aneiang.Pa.McpServer
```

用户："我就想抓个微博热搜，该装哪个？"

---

## 解决方案：单包策略

| 包 | 说明 |
|----|------|
| `Aneiang.Pa` | **核心 + 18 内置平台，单包搞定 90% 用户** |
| `Aneiang.Pa.AspNetCore` | Web API 集成（需要才装） |
| `Aneiang.Pa.Client` | HTTP 客户端（需要才装） |
| `Aneiang.Pa.Abstractions` | 接口与模型（扩展用） |

```bash
dotnet add package Aneiang.Pa
```

完事。

---

## 重构成果

| 指标 | 旧版 | 新版 |
|------|------|------|
| NuGet 包数量 | 23 | 4 |
| 内置平台 | 16 | 18 |
| 新增平台代码量 | 1 个 csproj + 3 个 .cs 文件 | 1 个 YAML 文件 |
| 入门代码行数 | 5+ 行（注入工厂 → 取爬虫 → 调方法） | 1 行（`Pa.Source("X").GetAsync()`） |
| 横切关注点 | 每个爬虫自己处理 | 8 个中间件自动处理 |
| 单元测试 | 0 | 15 |
| 构建时间 | ~3 分钟 | ~10 秒 |

---

## 一些教训

1. **过早拆分是万恶之源。** 16 个平台包在"有 100 个平台"时才有意义，在"只有 16 个"时只是增加复杂度。
2. **声明式优于命令式。** YAML 比 C# 更适合表达"抓取规则"这种配置型需求。
3. **管道模式是 .NET 生态的银弹。** ASP.NET Core 的中间件、HttpClient 的 DelegatingHandler，都是管道模式。爬虫也不例外。
4. **API 设计要面向"最常用场景"。** 90% 的用户只需要 `Pa.Source("X").GetAsync()`，不要让他们先理解 DI、工厂模式、接口抽象。

---

## 开源地址

GitHub: [https://github.com/AneiangSoft/Aneiang.Pa](https://github.com/AneiangSoft/Aneiang.Pa)

欢迎 Star、Issue、PR。
