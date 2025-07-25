using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Abstractions;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared.Abstractions;

namespace UIAutomationMCP.Server.Infrastructure
{
    /// <summary>
    /// Manages both Worker and Monitor processes
    /// </summary>
    public class ProcessManager : IProcessManager, IDisposable
    {
        private readonly SubprocessExecutor _workerExecutor;
        private readonly SubprocessExecutor? _monitorExecutor;
        private readonly ILogger<ProcessManager> _logger;
        private bool _disposed = false;

        public ProcessManager(
            ILogger<ProcessManager> logger,
            string workerPath,
            string? monitorPath = null)
        {
            _logger = logger;
            _workerExecutor = new SubprocessExecutor(
                logger.CreateLogger<SubprocessExecutor>(), 
                workerPath);

            if (!string.IsNullOrEmpty(monitorPath))
            {
                _monitorExecutor = new SubprocessExecutor(
                    logger.CreateLogger<SubprocessExecutor>(), 
                    monitorPath);
            }

            _logger.LogInformation("ProcessManager initialized - Worker: {WorkerPath}, Monitor: {MonitorPath}", 
                workerPath, monitorPath ?? "Not configured");
        }

        public bool IsWorkerProcessAvailable => !_disposed;

        public bool IsMonitorProcessAvailable => _monitorExecutor != null && !_disposed;

        /// <summary>
        /// Execute operation in Worker process
        /// </summary>
        public async Task<ServiceOperationResult<TResult>> ExecuteWorkerOperationAsync<TRequest, TResult>(
            string operationName, 
            TRequest request, 
            int timeoutSeconds = 60)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ProcessManager));

            _logger.LogDebug("Executing Worker operation: {OperationName}", operationName);
            
            return await ((IOperationExecutor)_workerExecutor).ExecuteAsync<TRequest, TResult>(
                operationName, request, timeoutSeconds);
        }

        /// <summary>
        /// Execute operation in Monitor process
        /// </summary>
        public async Task<ServiceOperationResult<TResult>> ExecuteMonitorOperationAsync<TRequest, TResult>(
            string operationName, 
            TRequest request, 
            int timeoutSeconds = 60)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ProcessManager));

            if (_monitorExecutor == null)
            {
                _logger.LogWarning("Monitor process not configured, falling back to Worker process for operation: {OperationName}", operationName);
                return await ExecuteWorkerOperationAsync<TRequest, TResult>(operationName, request, timeoutSeconds);
            }

            _logger.LogDebug("Executing Monitor operation: {OperationName}", operationName);
            
            return await ((IOperationExecutor)_monitorExecutor).ExecuteAsync<TRequest, TResult>(
                operationName, request, timeoutSeconds);
        }

        /// <summary>
        /// Default execution - routes to Worker process for compatibility
        /// </summary>
        public async Task<ServiceOperationResult<TResult>> ExecuteAsync<TRequest, TResult>(
            string operationName, 
            TRequest request, 
            int timeoutSeconds = 60)
        {
            // For backward compatibility, route to Worker by default
            return await ExecuteWorkerOperationAsync<TRequest, TResult>(operationName, request, timeoutSeconds);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _logger.LogInformation("Disposing ProcessManager");

            try
            {
                _workerExecutor?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing Worker executor");
            }

            try
            {
                _monitorExecutor?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing Monitor executor");
            }

            _disposed = true;
            _logger.LogInformation("ProcessManager disposed");
        }
    }
}