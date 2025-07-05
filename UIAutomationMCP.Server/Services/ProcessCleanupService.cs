using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UiAutomationMcpServer.Services
{
    /// <summary>
    /// Background service that ensures proper cleanup of all processes during MCP server shutdown
    /// Implements IHostedService for integration with .NET hosting lifecycle
    /// </summary>
    public class ProcessCleanupService : IHostedService, IDisposable
    {
        private readonly ILogger<ProcessCleanupService> _logger;
        private readonly IProcessTimeoutManager _processTimeoutManager;
        private bool _disposed = false;

        public ProcessCleanupService(
            ILogger<ProcessCleanupService> logger,
            IProcessTimeoutManager processTimeoutManager)
        {
            _logger = logger;
            _processTimeoutManager = processTimeoutManager;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[ProcessCleanupService] Process cleanup service started");
            
            // Register for application shutdown events
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            Console.CancelKeyPress += OnCancelKeyPress;
            
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[ProcessCleanupService] Stopping - Initiating graceful shutdown");

            try
            {
                // Terminate all active worker processes
                _processTimeoutManager.TerminateAllActiveProcesses();
                
                // Give processes time to cleanup
                await Task.Delay(500, cancellationToken);
                
                _logger.LogInformation("[ProcessCleanupService] Graceful shutdown completed");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[ProcessCleanupService] Shutdown was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProcessCleanupService] Error during graceful shutdown");
            }
        }

        private void OnProcessExit(object? sender, EventArgs e)
        {
            _logger.LogWarning("[ProcessCleanupService] Process exit detected - Emergency cleanup");
            _processTimeoutManager.TerminateAllActiveProcesses();
        }

        private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            _logger.LogWarning("[ProcessCleanupService] Cancel key press detected - Initiating shutdown");
            
            // Allow graceful shutdown instead of immediate termination
            e.Cancel = true;
            
            // Trigger shutdown through the hosting system
            _ = Task.Run(async () =>
            {
                await StopAsync(CancellationToken.None);
                Environment.Exit(0);
            });
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _logger.LogDebug("[ProcessCleanupService] Disposing");
                
                // Unregister event handlers
                AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
                Console.CancelKeyPress -= OnCancelKeyPress;
                
                // Final cleanup
                _processTimeoutManager.TerminateAllActiveProcesses();
                
                _disposed = true;
            }
        }
    }
}