using Aneiang.Pa.CnBlog.Models;
using Aneiang.Pa.CnBlog.News;
using Aneiang.Pa.Dynamic.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aneiang.Pa.CnBlog.Extensions
{
    /// <summary>
    /// The service collection extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册爬虫
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddCnBlogScraper(this IServiceCollection services, IConfiguration? configuration = null)
        {
            if (configuration != null)
            {
                services.Configure<CnBlogScraperOptions>(configuration.GetSection("Scraper:CnBlog"));
            }
            services.AddDynamicScraper();
            services.TryAddSingleton<ICnBlogNewScraper, CnBlogNewScraper>();
        }
    }
}
