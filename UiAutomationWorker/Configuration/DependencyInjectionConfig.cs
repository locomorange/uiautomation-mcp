using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UiAutomationWorker.Services;
using UiAutomationWorker.PatternExecutors;
using UiAutomationWorker.Helpers;

namespace UiAutomationWorker.Configuration
{
    /// <summary>
    /// 依存性注入の設定を管理するクラス
    /// </summary>
    public static class DependencyInjectionConfig
    {
        /// <summary>
        /// 依存性注入を設定します
        /// </summary>
        /// <param name="services">サービスコレクション</param>
        public static void ConfigureServices(IServiceCollection services)
        {
            // Initialize logging - disable console output to avoid interfering with JSON output
            services.AddLogging(builder =>
                builder.SetMinimumLevel(LogLevel.Error)); // Only log errors, and not to console

            // Register helper services
            services.AddSingleton<AutomationHelper>();
            services.AddSingleton<ElementInfoExtractor>();

            // Register pattern executors
            services.AddSingleton<CorePatternExecutor>();
            services.AddSingleton<LayoutPatternExecutor>();
            services.AddSingleton<TreePatternExecutor>();

            // Register main services
            services.AddSingleton<ElementSearchService>();
            services.AddSingleton<OperationExecutor>();

            // Register application services
            services.AddSingleton<InputProcessor>();
            services.AddSingleton<OutputProcessor>();
        }
    }
}
