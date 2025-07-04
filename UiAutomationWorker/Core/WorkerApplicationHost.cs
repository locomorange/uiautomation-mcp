using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationWorker.Services;

namespace UiAutomationWorker.Core
{
    /// <summary>
    /// ワーカープロセスのメイン実行ロジックを管理するクラス
    /// </summary>
    public class WorkerApplicationHost
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WorkerApplicationHost> _logger;

        public WorkerApplicationHost(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<WorkerApplicationHost>>();
        }

        /// <summary>
        /// ワーカーアプリケーションを実行します
        /// </summary>
        /// <returns>終了コード</returns>
        public async Task<int> RunAsync()
        {
            try
            {
                _logger.LogInformation("[WorkerApplicationHost] Worker process starting, PID: {ProcessId}", 
                    Environment.ProcessId);

                // Get required services
                var inputProcessor = _serviceProvider.GetRequiredService<InputProcessor>();
                var outputProcessor = _serviceProvider.GetRequiredService<OutputProcessor>();
                var operationExecutor = _serviceProvider.GetRequiredService<OperationExecutor>();

                // Read and parse input
                var operation = await inputProcessor.ReadAndParseInputAsync();
                if (operation == null)
                {
                    return 1;
                }

                // Execute operation
                var result = await operationExecutor.ExecuteOperationAsync(operation);
                
                // Output result
                await outputProcessor.WriteResultAsync(result);
                
                _logger.LogInformation("[WorkerApplicationHost] Operation completed successfully");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WorkerApplicationHost] Unhandled exception");
                await Console.Error.WriteLineAsync($"Unhandled exception: {ex.Message}");
                return 1;
            }
        }
    }
}
