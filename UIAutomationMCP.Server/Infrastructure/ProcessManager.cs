using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Abstractions;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Core.Abstractions;
using UIAutomationMCP.Models.Abstractions;

namespace UIAutomationMCP.Server.Infrastructure
{
    /// <summary>
    /// Manages both Worker and Monitor processes
    /// </summary>
    public class ProcessManager : IProcessManager, IDisposable, IAsyncDisposable
    {
        private readonly SubprocessExecutor _workerExecutor;
        private readonly SubprocessExecutor? _monitorExecutor;
        private readonly ILogger<ProcessManager> _logger;
        private bool _disposed = false;

        public ProcessManager(
            ILogger<ProcessManager> logger,
            ILoggerFactory loggerFactory,
            CancellationTokenSource shutdownCts,
            string workerPath,
            string? monitorPath = null)
        {
            _logger = logger;
            _workerExecutor = new SubprocessExecutor(
                loggerFactory.CreateLogger<SubprocessExecutor>(), 
                workerPath,
                shutdownCts);

            if (!string.IsNullOrEmpty(monitorPath))
            {
                _monitorExecutor = new SubprocessExecutor(
                    loggerFactory.CreateLogger<SubprocessExecutor>(), 
                    monitorPath,
                    shutdownCts);
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
            where TRequest : notnull
            where TResult : notnull
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
            where TRequest : notnull
            where TResult : notnull
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
            where TRequest : notnull
            where TResult : notnull
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

        /// <summary>
        /// Async disposal that waits for Worker operations to complete
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _logger.LogInformation("Async disposing ProcessManager - waiting for operations to complete");

            // Wait for Worker operations to complete
            if (_workerExecutor != null)
            {
                try
                {
                    await _workerExecutor.DisposeAsync();
                    _logger.LogDebug("Worker executor async disposal completed");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error during Worker executor async disposal");
                }
            }

            // Wait for Monitor operations to complete
            if (_monitorExecutor != null)
            {
                try
                {
                    await _monitorExecutor.DisposeAsync();
                    _logger.LogDebug("Monitor executor async disposal completed");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error during Monitor executor async disposal");
                }
            }

            _disposed = true;
            _logger.LogInformation("ProcessManager async disposal completed");
        }
    }
}