using Aneiang.Pa.Core.Extensions;
using Aneiang.Pa.Core.Pipeline;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aneiang.Pa.Core.Modules
{
    /// <summary>
    /// 模块自动发现扩展
    /// </summary>
    public static class ModuleServiceCollectionExtensions
    {
        /// <summary>
        /// 从指定程序集集合发现并注册所有 IScraperModule
        /// </summary>
        public static IServiceCollection AddPaScraperModules(
            this IServiceCollection services,
            IEnumerable<Assembly> assemblies,
            IConfiguration? configuration = null)
        {
            var modules = DiscoverModules(assemblies);
            var builder = new ScraperModuleBuilder(services, configuration);
            foreach (var module in modules)
            {
                module.Register(builder);
            }
            return services;
        }

        /// <summary>
        /// 从当前 AppDomain 已加载的程序集发现并注册所有 IScraperModule，
        /// 自动追加 ScraperRegistry 与 Pipeline。
        /// </summary>
        public static IServiceCollection AddPaScraperFromLoadedAssemblies(
            this IServiceCollection services,
            IConfiguration? configuration = null)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            services.AddPaScraperModules(assemblies, configuration);
            services.AddScraperRegistry();
            services.AddPaScrapePipeline(configuration);
            return services;
        }

        private static IEnumerable<IScraperModule> DiscoverModules(IEnumerable<Assembly> assemblies)
        {
            var modules = new List<(int Order, IScraperModule Module)>();
            foreach (var asm in assemblies)
            {
                PaScraperModuleAttribute[] attrs;
                try
                {
                    attrs = asm.GetCustomAttributes<PaScraperModuleAttribute>().ToArray();
                }
                catch
                {
                    continue;
                }
                foreach (var attr in attrs)
                {
                    if (!typeof(IScraperModule).IsAssignableFrom(attr.ModuleType)) continue;
                    try
                    {
                        var instance = (IScraperModule)Activator.CreateInstance(attr.ModuleType)!;
                        modules.Add((instance.Order, instance));
                    }
                    catch
                    {
                        // 单个模块实例化失败不影响其他模块
                    }
                }
            }
            return modules.OrderBy(t => t.Order).Select(t => t.Module);
        }
    }
}
