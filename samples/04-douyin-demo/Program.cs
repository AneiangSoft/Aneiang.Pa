using Aneiang.Pa;

/*
 * 抖音热搜的特殊之处：
 *   1. 直接 GET 接口会被反爬拦截（403/空 body）
 *   2. 必须先访问 https://login.douyin.com/ 拿到 Set-Cookie，再带 Cookie 调真实接口
 *   3. Cookie 有时效，需要定期刷新
 *
 * 解决方案：Aneiang.Pa 内置 DouYinCookieMiddleware，
 *   会自动处理上述流程，用户无需任何配置即可调用抖音热搜。
 */

Console.WriteLine("=== Aneiang.Pa 4.0 — 抖音热搜 Demo ===\n");
Console.WriteLine("内置 DouYinCookieMiddleware 会自动处理 Cookie，零配置即可使用。\n");

try
{
    var result = await Pa.Source("DouYin").GetAsync();

    if (result.IsSuccess && result.Data.Count > 0)
    {
        Console.WriteLine($"成功！共 {result.Data.Count} 条热搜:\n");
        var i = 0;
        foreach (var item in result.Data.Take(15))
        {
            i++;
            Console.WriteLine($"  {i,2}. {item.GetValueOrDefault("title")}");
            var hot = item.GetValueOrDefault("hotValue");
            if (!string.IsNullOrEmpty(hot)) Console.WriteLine($"      🔥 {hot}");
            var url = item.GetValueOrDefault("url");
            if (!string.IsNullOrEmpty(url)) Console.WriteLine($"      🔗 {url}");
            Console.WriteLine();
        }
    }
    else
    {
        Console.WriteLine($"未抓到数据：{result.ErrorMessage ?? "data 为空"}");
        Console.WriteLine();
        Console.WriteLine("常见原因：");
        Console.WriteLine("  • 当前 IP 被抖音风控（试试代理）");
        Console.WriteLine("  • Cookie 拿取失败");
        Console.WriteLine();
        Console.WriteLine("使用代理示例：");
        Console.WriteLine("  Pa.Configure(c => c.UseHttpHandler(() =>");
        Console.WriteLine("      new HttpClientHandler { Proxy = new WebProxy(\"http://127.0.0.1:7890\"), UseProxy = true }));");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"异常：{ex.Message}");
}
