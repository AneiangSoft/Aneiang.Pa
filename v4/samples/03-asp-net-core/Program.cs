using Aneiang.Pa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPa(opt =>
{
    opt.EnableMetrics = true;
    opt.DefaultCacheDuration = TimeSpan.FromMinutes(5);
});

var app = builder.Build();
app.MapPaApi();
app.MapGet("/", () => "Aneiang.Pa 4.0 — see /api/pa/sources or /api/pa/source/Github.Trending");
app.Run();
