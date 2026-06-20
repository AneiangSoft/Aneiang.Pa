using Aneiang.Pa.Core.Extensions;
using Aneiang.Pa.Core.Modules;
using Aneiang.Pa.Core.News;
using Aneiang.Pa.WeiBo.News;
using Microsoft.Extensions.DependencyInjection;

[assembly: PaScraperModule(typeof(Aneiang.Pa.WeiBo.WeiBoScraperModule))]

namespace Aneiang.Pa.WeiBo
{
    /// <summary>
    /// 微博热搜爬虫模块（自动发现）
    /// </summary>
    public class WeiBoScraperModule : IScraperModule
    {
        /// <inheritdoc />
        public string Name => "Aneiang.Pa.WeiBo";

        /// <inheritdoc />
        public void Register(IScraperModuleBuilder builder)
        {
            // WeiBo 使用 WeiBoNewScraper 既是接口又是 options 类型（项目原设计）
            builder.Services.AddScraper<IWeiBoNewScraper, WeiBoNewScraper, WeiBoNewScraper>(
                "Scraper:WeiBo", builder.Configuration);
            builder.Services.AddSingleton<INewsScraper>(sp => sp.GetRequiredService<IWeiBoNewScraper>());
        }
    }
}
