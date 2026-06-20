# Recipe 编写指南

Aneiang.Pa 4.0 用 **Recipe** 描述"怎么抓 + 怎么解析"，支持四种来源。

## 核心结构

```yaml
name: Source 名（必填，唯一）
category: 分类（可选）
display_name: 显示名（可选）

fetch:
  method: GET | POST
  url: 地址（支持 {var} 占位）
  headers:
    User-Agent: ...
    Cookie: ...
  body: POST/PUT 用
  content_type: application/json
  timeout: 00:00:30

parse:
  type: html | json | regex
  # 以下取决于 type
  container: CSS 选择器或 xpath:...
  items_path: $.data.list   # JSON 用
  pattern: regex            # Regex 用
  fields:
    title:
      selector: "..."        # CSS / XPath / JsonPath
      attr: href             # HTML 属性
      trim: true             # 是否 Trim
      collapse: true         # 是否合并空白
      base: "https://..."    # 相对 URL 补全
      regex: "..."           # 从字段值正则抽取
```

## 四种来源

### 1. YAML / JSON 文件

```csharp
Pa.Configure(c => c.UseRecipesFolder("./recipes"));
```

支持递归子目录。同名 Recipe 后注册的覆盖前注册的。

### 2. Builder DSL（C# 内联）

```csharp
Pa.Define("Github.Releases", b => b
    .Category("Demo")
    .Get("https://api.github.com/repos/dotnet/runtime/releases")
    .WithUserAgent("Aneiang.Pa/4.0")
    .Header("Accept", "application/vnd.github+json")
    .ParseJson(p => p
        .Items("$")
        .Field("tag", "tag_name")
        .Field("name", "name")));
```

### 3. 特性标注（强类型）

```csharp
[Recipe("MySite.Posts", Category = "News")]
[Get("https://example.com/posts")]
[Container("article.post")]
public class Post
{
    [Selector("h2 a")]
    public string? Title { get; set; }

    [Selector("h2 a", Attr = "href", Base = "https://example.com")]
    public string? Url { get; set; }
}

// 启动时
Pa.DiscoverFromLoadedAssemblies();
```

### 4. 内置 YAML（嵌入资源）

`Aneiang.Pa.dll` 内置 12 个平台 YAML，启动自动加载。可通过 `Pa.Configure(c => c.Options.LoadBuiltInRecipes = false)` 关闭。

## 选择器速查

### HTML
- CSS：`article.item h2 a`
- XPath：`xpath://article[@class='item']/h2/a`
- 当前节点：`.`

### JSON
- 根：`$`
- 嵌套：`$.data.list`
- 数组下标：`$.items[0]`

### 正则
- 命名捕获：`(?<title>.+?)`
- 数字索引：在 `groups` 字段映射

## 分页

URL 模板支持占位，从代码注入：

```yaml
fetch:
  url: https://api.example.com/list?page={pageNo}&size={pageSize}
```

```csharp
var data = await Pa.Source("MySite").WithPaging(1, 30).GetAsync();
// 或自定义变量
var data = await Pa.Source("MySite").With("pageNo", 2).GetAsync();
```

## 字段后处理

| 选项 | 说明 |
|------|------|
| `trim` | 头尾空白（默认 true） |
| `collapse` | 连续空白合并为单空格（默认 true） |
| `base` | 相对 URL 补全 |
| `regex` | 从字段文本抽取（取第一个捕获组） |
| `attr` | HTML 元素属性 |

## 错误处理

```csharp
var result = await Pa.Source("X").GetAsync();
if (!result.IsSuccess)
    Console.Error.WriteLine(result.ErrorMessage);
```

中间件层会自动重试/熔断/超时，不需要业务方写 try/catch。
