using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UiAutomationWorker.Services;

namespace UiAutomationWorker.Core
{
    /// <summary>
    /// Worker process main execution logic manager
    /// Handles single operation execution without timeout management (Server manages timeouts)
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
        /// Executes a single UI Automation operation
        /// Timeout handling is managed by the Server process
        /// </summary>
        /// <returns>Exit code: 0 for success, 1 for failure</returns>
        public async Task<int> RunAsync()
        {
            try
            {
                _logger.LogInformation("[WorkerApplicationHost] Worker starting, PID: {ProcessId}", 
                    Environment.ProcessId);

                // Get required services
                var inputProcessor = _serviceProvider.GetRequiredService<InputProcessor>();
                var outputProcessor = _serviceProvider.GetRequiredService<OutputProcessor>();
                var operationExecutor = _serviceProvider.GetRequiredService<OperationExecutor>();

                // Read and parse input from Server
                var operation = await inputProcessor.ReadAndParseInputAsync();
                if (operation == null)
                {
                    return 1;
                }

                // Execute operation (Server handles timeout and process termination)
                var result = await operationExecutor.ExecuteOperationAsync(operation);
                
                // Send result back to Server
                await outputProcessor.WriteResultAsync(result);
                
                _logger.LogInformation("[WorkerApplicationHost] Operation completed");
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
