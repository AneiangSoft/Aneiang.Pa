---
title: "5 行代码搭建爬虫 API 服务 — Aneiang.Pa + ASP.NET Core"
date: 2026-06-22
tags: [.NET, ASP.NET Core, 爬虫, Web API]
---

## 场景

你有一个爬虫需求：抓微博热搜、知乎热榜、百度热榜，然后通过 API 暴露给前端或移动端。

传统做法：
1. 写 3 个爬虫类
2. 写 3 个 Controller
3. 配 HttpClient、配缓存、配日志
4. 写 Swagger 文档
5. 部署

**少说要半天。**

用 Aneiang.Pa 4.0：

```csharp
using Aneiang.Pa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPa();

var app = builder.Build();
app.MapPaApi();
app.Run();
```

**5 行代码，18 个 API 端点自动生成。**

---

## 自动暴露的 API

| 端点 | 说明 |
|------|------|
| `GET /api/pa/sources` | 列出所有可用源 |
| `GET /api/pa/source/WeiBo` | 抓微博热搜 |
| `GET /api/pa/source/ZhiHu` | 抓知乎热榜 |
| `GET /api/pa/source/BaiDu` | 抓百度热榜 |
| `GET /api/pa/source/Lottery.SSQ` | 抓双色球开奖 |
| `GET /api/pa/health` | 健康检查 |

所有 18 个内置平台自动可用，新增平台只需添加 YAML 文件。

---

## 加缓存

```csharp
builder.Services.AddPa(opt =>
{
    opt.DefaultCacheDuration = TimeSpan.FromMinutes(5);
});
```

所有 API 自动带 5 分钟缓存。支持单次跳过：

```
GET /api/pa/source/WeiBo?noCache=1
```

---

## 加代理

```csharp
Pa.Configure(c => c
    .UseHttpHandler(() => new HttpClientHandler
    {
        Proxy = new WebProxy("http://127.0.0.1:7890"),
        UseProxy = true
    }));
```

---

## 完整示例

```csharp
using Aneiang.Pa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPa(opt =>
{
    opt.DefaultCacheDuration = TimeSpan.FromMinutes(5);
    opt.EnableMetrics = true;
});

var app = builder.Build();

// 映射爬虫 API
app.MapPaApi();

// 自定义首页
app.MapGet("/", () => "Aneiang.Pa 4.0 — 爬虫 API 服务");

app.Run();
```

`Program.cs` 一共 18 行。

---

## 前端调用

```javascript
// 列出所有源
const sources = await fetch('/api/pa/sources').then(r => r.json());

// 抓微博热搜
const weibo = await fetch('/api/pa/source/WeiBo').then(r => r.json());
weibo.data.forEach(item => console.log(item.title));

// 抓双色球
const ssq = await fetch('/api/pa/source/Lottery.SSQ').then(r => r.json());
```

---

## Docker 部署

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY . .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PaWeb.dll"]
```

```bash
docker build -t pa-api .
docker run -p 8080:8080 pa-api
```

---

## 性能

实测单机（2C4G）：
- 缓存命中：< 1ms
- 首次抓取（含网络）：500ms-3s（取决于目标站点）
- 并发 100 请求：CPU < 30%

---

## 适用场景

- **个人博客/网站的热榜聚合页**
- **移动端新闻 Feed**
- **数据看板/大屏展示**
- **内部数据监控**

---

## 快速开始

```bash
dotnet new web -n PaApi
cd PaApi
dotnet add package Aneiang.Pa.AspNetCore
```

把上面的 18 行代码贴进 `Program.cs`，然后：

```bash
dotnet run
```

打开浏览器访问 `http://localhost:5000/api/pa/sources`。

---

GitHub: [https://github.com/AneiangSoft/Aneiang.Pa](https://github.com/AneiangSoft/Aneiang.Pa)
