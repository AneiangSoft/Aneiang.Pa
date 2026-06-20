using Aneiang.Pa;

Console.WriteLine("=== Aneiang.Pa 4.0 — 内置 Recipe ===\n");

// 列出所有内置 Recipe
var sources = Pa.Sources().OrderBy(s => s.Category).ThenBy(s => s.Name).ToList();
Console.WriteLine($"已注册 {sources.Count} 个内置 Recipe：");
foreach (var r in sources)
    Console.WriteLine($"  [{r.Category,-8}] {r.Name,-15} {r.DisplayName}");
Console.WriteLine();

// 验证一个能在网络受限环境跑通的（默认 BaiDu 嵌入 JSON 提取，受限于真实网络可达性）
Console.WriteLine("尝试调用 BaiDu 热榜（如果网络可达）...");
try
{
    var result = await Pa.Source("BaiDu").GetAsync(CancellationToken.None);
    if (result.IsSuccess)
    {
        Console.WriteLine($"成功 {result.Data.Count} 条:");
        foreach (var item in result.Data.Take(5))
            Console.WriteLine($"  - {item.GetValueOrDefault("title")}");
    }
    else
    {
        Console.WriteLine($"  → {result.ErrorMessage}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"  → 异常: {ex.Message}");
}


