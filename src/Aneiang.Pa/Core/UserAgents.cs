using System;

namespace Aneiang.Pa.Core;

/// <summary>常用浏览器 UA</summary>
public static class UserAgents
{
    private static readonly string[] Pool =
    {
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:133.0) Gecko/20100101 Firefox/133.0",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 14_5) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.5 Safari/605.1.15",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0",
        "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36"
    };

    private static readonly Random Rng = new();

    /// <summary>Chrome 桌面版 UA</summary>
    public static string Chrome => Pool[0];

    /// <summary>随机一个</summary>
    public static string Random()
    {
        lock (Rng) return Pool[Rng.Next(Pool.Length)];
    }
}
