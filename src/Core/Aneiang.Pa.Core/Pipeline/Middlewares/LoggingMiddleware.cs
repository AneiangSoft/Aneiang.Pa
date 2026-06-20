using Aneiang.Pa.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Aneiang.Pa.Core.Pipeline.Middlewares
{
    /// <summary>
    /// 日志中间件：记录爬取开始/结束/耗时/错误
    /// </summary>
    public class LoggingMiddleware : IScrapeMiddleware
    {
        private readonly ILogger<LoggingMiddleware> _logger;

        /// <inheritdoc />
        public int Order => 0;

        /// <summary>
        /// 初始化日志中间件
        /// </summary>
        public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<ScraperResult<T>> InvokeAsync<T>(ScrapeContext context, ScrapeDelegate<T> next) where T : class
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation("[Scrape] Start {Category}/{Source} CorrelationId={CorrelationId}",
                context.Descriptor.Category, context.Descriptor.Source, context.CorrelationId);
            try
            {
                var result = await next(context).ConfigureAwait(false);
                sw.Stop();
                if (result.IsSuccess)
                {
                    _logger.LogInformation("[Scrape] OK    {Category}/{Source} Items={Count} ElapsedMs={Elapsed} CorrelationId={CorrelationId}",
                        context.Descriptor.Category, context.Descriptor.Source, result.Data.Count, sw.ElapsedMilliseconds, context.CorrelationId);
                }
                else
                {
                    _logger.LogWarning("[Scrape] FAIL  {Category}/{Source} Reason={Reason} ElapsedMs={Elapsed} CorrelationId={CorrelationId}",
                        context.Descriptor.Category, context.Descriptor.Source, result.ErrorMessage, sw.ElapsedMilliseconds, context.CorrelationId);
                }
                return result;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "[Scrape] EX    {Category}/{Source} ElapsedMs={Elapsed} CorrelationId={CorrelationId}",
                    context.Descriptor.Category, context.Descriptor.Source, sw.ElapsedMilliseconds, context.CorrelationId);
                throw;
            }
        }
    }
}
