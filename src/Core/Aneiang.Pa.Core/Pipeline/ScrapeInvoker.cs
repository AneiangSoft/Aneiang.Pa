using Aneiang.Pa.Core.Models;
using Aneiang.Pa.Core.Scraper;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aneiang.Pa.Core.Pipeline
{
    /// <summary>
    /// 默认爬取调用器，按 Order 升序串联中间件
    /// </summary>
    public class ScrapeInvoker : IScrapeInvoker
    {
        private readonly IReadOnlyList<IScrapeMiddleware> _middlewares;
        private readonly IScraperRegistry _registry;

        /// <summary>
        /// 初始化调用器
        /// </summary>
        public ScrapeInvoker(IEnumerable<IScrapeMiddleware> middlewares, IScraperRegistry registry)
        {
            _middlewares = middlewares.OrderBy(m => m.Order).ToArray();
            _registry = registry;
        }

        /// <inheritdoc />
        public Task<ScraperResult<T>> InvokeAsync<T>(IScraper<T> scraper, CancellationToken cancellationToken = default)
            where T : class
        {
            var context = new ScrapeContext(scraper.Descriptor, cancellationToken);
            ScrapeDelegate<T> terminal = ctx => scraper.ScrapeAsync(ctx.CancellationToken);

            ScrapeDelegate<T> pipeline = terminal;
            for (var i = _middlewares.Count - 1; i >= 0; i--)
            {
                var middleware = _middlewares[i];
                var next = pipeline;
                pipeline = ctx => middleware.InvokeAsync(ctx, next);
            }
            return pipeline(context);
        }

        /// <inheritdoc />
        public Task<ScraperResult<T>> InvokeAsync<T>(string category, string source, CancellationToken cancellationToken = default)
            where T : class
        {
            var scraper = _registry.GetScraper(category, source);
            if (scraper == null)
                return Task.FromResult(ScraperResult<T>.Failure($"未找到爬虫: {category}/{source}"));
            if (scraper is not IScraper<T> typed)
                return Task.FromResult(ScraperResult<T>.Failure($"爬虫 {category}/{source} 的结果类型不匹配 {typeof(T).Name}"));
            return InvokeAsync<T>(typed, cancellationToken);
        }
    }
}
