using Aneiang.Pa.Extensions;
using Aneiang.Pa.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 注册新闻爬虫服务
builder.Services.AddNewsScraper(builder.Configuration);

// 添加爬虫控制器支持（自动生成 ScraperController）
// 可以自定义路由前缀等配置
builder.Services.AddScraperController(options =>
{
    options.RoutePrefix = "api/scraper"; // 路由前缀，可以自定义，如 "api/news" 或 "api/scraper/v1"
    options.UseLowercaseInRoute = true; // 路由使用小写，如 /api/scraper/weibo
    options.EnableResponseCaching = false; // 是否启用响应缓存（建议生产环境启用）
    options.CacheDurationSeconds = 300; // 缓存时长（秒），默认5分钟
});

// ========== 授权配置示例 ==========
// 方式1：通过配置文件配置授权（推荐）
// 从 appsettings.json 中的 "Scraper:Authorization" 配置节读取
builder.Services.ConfigureAuthorization(builder.Configuration);

// 方式2：通过代码配置授权（取消注释以启用）
/*
builder.Services.ConfigureAuthorization(options =>
{
    // 启用授权
    options.Enabled = true;
    
    // 设置授权方式：ApiKey、Custom 或 Combined
    options.Scheme = Aneiang.Pa.AspNetCore.Options.AuthorizationScheme.ApiKey;
    
    // 配置 API Key 列表
    options.ApiKeys = new List<string>
    {
        "your-api-key-1",
        "your-api-key-2"
    };
    
    // 设置 API Key 请求头名称（默认：X-API-Key）
    options.ApiKeyHeaderName = "X-API-Key";
    
    // 设置 API Key 查询参数名称（可选，如果设置了可以从查询字符串读取）
    options.ApiKeyQueryParameterName = "apiKey";
    
    // 排除不需要授权的路由（支持通配符）
    options.ExcludedRoutes = new List<string>
    {
        "/api/scraper/health",           // 精确匹配
        "/api/scraper/ * /health"          // 通配符匹配
    };
    
    // 自定义未授权错误消息
    options.UnauthorizedMessage = "未授权访问";
});
*/

// 方式3：自定义授权验证函数（取消注释以启用）
/*
builder.Services.ConfigureAuthorization(options =>
{
    options.Enabled = true;
    options.Scheme = Aneiang.Pa.AspNetCore.Options.AuthorizationScheme.Custom;
    
    // 自定义授权验证函数
    options.CustomAuthorizationFunc = (httpContext) =>
    {
        // 从请求头获取 Authorization
        var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader))
        {
            return (false, null);
        }
        
        // 简单的 Bearer Token 验证示例
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            
            // 这里可以添加实际的 token 验证逻辑
            // 例如：验证 JWT token、查询数据库等
            if (token == "valid-token-123")
            {
                // 创建 ClaimsPrincipal
                var claims = new[]
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "user"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "admin")
                };
                var identity = new System.Security.Claims.ClaimsIdentity(claims, "custom");
                var principal = new System.Security.Claims.ClaimsPrincipal(identity);
                
                return (true, principal);
            }
        }
        
        return (false, null);
    };
});
*/

// 方式4：组合授权方式（API Key 或自定义验证，满足任一即可）
/*
builder.Services.ConfigureAuthorization(options =>
{
    options.Enabled = true;
    options.Scheme = Aneiang.Pa.AspNetCore.Options.AuthorizationScheme.Combined;
    
    // API Key 配置
    options.ApiKeys = new List<string> { "api-key-123" };
    
    // 自定义验证函数
    options.CustomAuthorizationFunc = (httpContext) =>
    {
        // 自定义验证逻辑
        var customHeader = httpContext.Request.Headers["X-Custom-Auth"].FirstOrDefault();
        if (customHeader == "valid")
        {
            return (true, null);
        }
        return (false, null);
    };
});
*/

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// 映射控制器路由
app.MapControllers();

// （可选）映射爬虫控制器路由（如果需要在端点路由中自定义）
// app.MapScraperController();

app.Run();
