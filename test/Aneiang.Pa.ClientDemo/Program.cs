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
    options.RoutePrefix = "api/scraper/v1"; // 默认路由前缀，可以自定义，如 "api/news"
    options.UseLowercaseInRoute = true; // 路由使用小写，如 /api/scraper/weibo
    options.EnableAllEndpoints = true; // 启用所有端点
});

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
