using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.Core.News.Models;
using Microsoft.Extensions.Logging;

namespace Aneiang.Pa.News.News
{
    /// <summary>
    /// 爬虫健康检查服务
    /// </summary>
    public class NewsScraperHealthCheckService : IScraperHealthCheckService
    {
        private readonly INewsScraperFactory _scraperFactory;
        private readonly ILogger<NewsScraperHealthCheckService>? _logger;

        /// <summary>
        /// 初始化爬虫健康检查服务
        /// </summary>
        /// <param name="scraperFactory">爬虫工厂</param>
        /// <param name="logger">日志记录器（可选）</param>
        public NewsScraperHealthCheckService(
            INewsScraperFactory scraperFactory,
            ILogger<NewsScraperHealthCheckService>? logger = null)
        {
            _scraperFactory = scraperFactory ?? throw new ArgumentNullException(nameof(scraperFactory));
            _logger = logger;
        }

        /// <summary>
        /// 检查所有爬虫的健康状态
        /// </summary>
        /// <param name="timeoutMs">超时时间（毫秒），默认5000</param>
        /// <returns>健康检查结果</returns>
        public async Task<HealthCheckResult> CheckAllAsync(int timeoutMs = 5000)
        {
            var result = new HealthCheckResult
            {
                CheckTime = DateTime.UtcNow
            };

            var scrapers = _scraperFactory.GetAllScrapers().ToList();
            result.TotalCount = scrapers.Count;

            _logger?.LogInformation("开始检查 {Count} 个爬虫的健康状态", result.TotalCount);

            // 并行检查所有爬虫
            var tasks = scrapers.Select(scraper => CheckAsync(scraper, timeoutMs));
            var statuses = await Task.WhenAll(tasks);

            result.Scrapers = statuses.ToList();
            result.HealthyCount = statuses.Count(s => s.IsHealthy);
            result.UnhealthyCount = statuses.Count(s => !s.IsHealthy);

            _logger?.LogInformation(
                "健康检查完成：总计 {Total}，健康 {Healthy}，不健康 {Unhealthy}",
                result.TotalCount,
                result.HealthyCount,
                result.UnhealthyCount);

            return result;
        }

        /// <summary>
        /// 检查指定爬虫的健康状态
        /// </summary>
        /// <param name="scraper">爬虫实例</param>
        /// <param name="timeoutMs">超时时间（毫秒），默认5000</param>
        /// <returns>爬虫健康状态</returns>
        public async Task<ScraperHealthStatus> CheckAsync(INewsScraper scraper, int timeoutMs = 5000)
        {
            var status = new ScraperHealthStatus
            {
                Source = scraper.Source,
                CheckTime = DateTime.UtcNow
            };

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // 尝试获取新闻来验证爬虫是否正常工作
                // 使用 Task.Run 和 CancellationTokenSource 来实现超时控制
                using var cts = new CancellationTokenSource(timeoutMs);
                var getNewsTask = scraper.GetNewsAsync();
                
                // 创建一个超时任务
                var timeoutTask = Task.Delay(timeoutMs, cts.Token);
                var completedTask = await Task.WhenAny(getNewsTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    cts.Cancel();
                    throw new OperationCanceledException($"操作超时（超过 {timeoutMs}ms）");
                }
                
                var newsResult = await getNewsTask;
                
                stopwatch.Stop();
                status.ResponseTimeMs = stopwatch.ElapsedMilliseconds;

                // 判断是否健康：成功获取数据或者至少没有异常
                status.IsHealthy = newsResult.IsSuccessd;
                
                if (!newsResult.IsSuccessd && !string.IsNullOrWhiteSpace(newsResult.ErrorMessage))
                {
                    status.ErrorMessage = newsResult.ErrorMessage;
                }

                _logger?.LogDebug(
                    "爬虫 {Source} 健康检查完成：{IsHealthy}，响应时间 {ResponseTime}ms",
                    scraper.Source,
                    status.IsHealthy,
                    status.ResponseTimeMs);
            }
            catch (OperationCanceledException)
            {
                stopwatch.Stop();
                status.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                status.IsHealthy = false;
                status.ErrorMessage = $"健康检查超时（超过 {timeoutMs}ms）";
                
                _logger?.LogWarning("爬虫 {Source} 健康检查超时", scraper.Source);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                status.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                status.IsHealthy = false;
                status.ErrorMessage = ex.Message;
                
                _logger?.LogError(ex, "爬虫 {Source} 健康检查失败", scraper.Source);
            }

            return status;
        }
    }
}
