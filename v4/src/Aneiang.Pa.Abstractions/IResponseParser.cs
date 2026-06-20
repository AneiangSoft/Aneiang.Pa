using System.Net.Http;
using System.Threading.Tasks;

namespace Aneiang.Pa.Abstractions;

/// <summary>
/// 响应解析器：把 HttpResponseMessage 转换为字段字典列表
/// </summary>
public interface IResponseParser
{
    /// <summary>是否能解析此规格</summary>
    bool CanParse(ParseSpec spec);

    /// <summary>执行解析</summary>
    Task<ScrapeResult> ParseAsync(HttpResponseMessage response, ParseSpec spec, ScrapeContext context);
}
