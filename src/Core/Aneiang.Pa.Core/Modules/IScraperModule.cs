using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Aneiang.Pa.Core.Modules
{
    /// <summary>
    /// 爬虫模块构建器（提供给 IScraperModule.Register 使用）
    /// </summary>
    public interface IScraperModuleBuilder
    {
        /// <summary>
        /// 服务集合
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// 当前根配置（可能为 null）
        /// </summary>
        IConfiguration? Configuration { get; }
    }

    /// <summary>
    /// 默认构建器实现
    /// </summary>
    public class ScraperModuleBuilder : IScraperModuleBuilder
    {
        /// <inheritdoc />
        public IServiceCollection Services { get; }

        /// <inheritdoc />
        public IConfiguration? Configuration { get; }

        /// <summary>
        /// 初始化构建器
        /// </summary>
        public ScraperModuleBuilder(IServiceCollection services, IConfiguration? configuration)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            Configuration = configuration;
        }
    }

    /// <summary>
    /// 爬虫模块接口（每个 NuGet 包可声明一个或多个模块）
    /// </summary>
    public interface IScraperModule
    {
        /// <summary>
        /// 模块名称（用于日志/诊断）
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 注册阶段（默认 0；越小越早执行）
        /// </summary>
        int Order => 0;

        /// <summary>
        /// 向 DI 注册组件
        /// </summary>
        void Register(IScraperModuleBuilder builder);
    }

    /// <summary>
    /// 程序集级模块声明：标记此程序集包含一个 IScraperModule
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class PaScraperModuleAttribute : Attribute
    {
        /// <summary>
        /// 模块类型（必须实现 IScraperModule 且具有公共无参构造）
        /// </summary>
        public Type ModuleType { get; }

        /// <summary>
        /// 初始化属性
        /// </summary>
        public PaScraperModuleAttribute(Type moduleType)
        {
            ModuleType = moduleType ?? throw new ArgumentNullException(nameof(moduleType));
        }
    }
}
