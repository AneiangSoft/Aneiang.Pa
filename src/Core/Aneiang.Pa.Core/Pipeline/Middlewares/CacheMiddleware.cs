using Aneiang.Pa.Core.Models;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Aneiang.Pa.Core.Pipeline.Middlewares
{
    /// <summary>
    /// 缓存中间件：命中时直接返回，未命中时执行下游并缓存结果
    /// </summary>
    public class CacheMiddleware : IScrapeMiddleware
    {
        private readonly IScrapeCache? _cache;
        private readonly IOptionsMonitor<ScrapeCacheOptions> _options;

        /// <inheritdoc />
        public int Order => 30;

        /// <summary>
        /// 初始化缓存中间件
        /// </summary>
        public CacheMiddleware(IOptionsMonitor<ScrapeCacheOptions> options, IScrapeCache? cache = null)
        {
            _options = options;
            _cache = cache;
        }

        /// <inheritdoc />
        public async Task<ScraperResult<T>> InvokeAsync<T>(ScrapeContext context, ScrapeDelegate<T> next) where T : class
        {
            if (_cache == null)
                return await next(context).ConfigureAwait(false);

            var duration = _options.CurrentValue.GetFor(context.Descriptor.Category, context.Descriptor.Source);
            if (duration <= TimeSpan.Zero)
                return await next(context).ConfigureAwait(false);

            var key = context.Descriptor.ToCacheKey();
            return await _cache.GetOrCreateAsync<T>(key, () => next(context), duration).ConfigureAwait(false);
        }
    }
}
