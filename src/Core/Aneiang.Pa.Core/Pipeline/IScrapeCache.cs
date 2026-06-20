using Aneiang.Pa.Core.Models;
using System;
using System.Threading.Tasks;

namespace Aneiang.Pa.Core.Pipeline
{
    /// <summary>
    /// 管道层缓存抽象（与 AspNetCore 包的 ICacheService 解耦）
    /// </summary>
    public interface IScrapeCache
    {
        /// <summary>
        /// 获取或创建（命中返回，未命中执行 factory 并缓存）
        /// </summary>
        Task<ScraperResult<T>> GetOrCreateAsync<T>(string key, Func<Task<ScraperResult<T>>> factory, TimeSpan? duration = null) where T : class;
    }
}
