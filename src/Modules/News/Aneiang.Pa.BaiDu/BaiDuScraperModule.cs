using Aneiang.Pa.BaiDu.Models;
using Aneiang.Pa.BaiDu.News;
using Aneiang.Pa.Core.Extensions;
using Aneiang.Pa.Core.Modules;
using Aneiang.Pa.Core.News;
using Microsoft.Extensions.DependencyInjection;

[assembly: PaScraperModule(typeof(Aneiang.Pa.BaiDu.BaiDuScraperModule))]

namespace Aneiang.Pa.BaiDu
{
    /// <summary>
    /// 百度热榜爬虫模块（自动发现）
    /// </summary>
    public class BaiDuScraperModule : IScraperModule
    {
        /// <inheritdoc />
        public string Name => "Aneiang.Pa.BaiDu";

        /// <inheritdoc />
        public void Register(IScraperModuleBuilder builder)
        {
            builder.Services.AddScraper<IBaiDuNewScraper, BaiDuNewScraper, BaiDuScraperOptions>(
                "Scraper:BaiDu", builder.Configuration, addHttpClient: false);
            builder.Services.AddSingleton<INewsScraper>(sp => sp.GetRequiredService<IBaiDuNewScraper>());
        }
    }
}
