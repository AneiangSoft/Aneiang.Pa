using System.Threading.Tasks;

namespace Aneiang.Pa.Abstractions;

/// <summary>管道中下一节点的委托</summary>
public delegate Task<ScrapeResult> ScrapeDelegate(ScrapeContext context);

/// <summary>爬取中间件</summary>
public interface IScrapeMiddleware
{
    /// <summary>顺序，数值越小越靠外层</summary>
    int Order { get; }

    /// <summary>名称（便于日志/跳过）</summary>
    string Name { get; }

    /// <summary>执行管道节点</summary>
    Task<ScrapeResult> InvokeAsync(ScrapeContext context, ScrapeDelegate next);
}
