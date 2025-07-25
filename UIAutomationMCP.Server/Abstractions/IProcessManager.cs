using UIAutomationMCP.Core.Abstractions;

namespace UIAutomationMCP.Server.Abstractions
{
    /// <summary>
    /// Manages multiple types of processes (Worker, Monitor, etc.)
    /// </summary>
    public interface IProcessManager : IOperationExecutor
    {
        /// <summary>
        /// Execute operation in Worker process (short-term operations)
        /// </summary>
        Task<ServiceOperationResult<TResult>> ExecuteWorkerOperationAsync<TRequest, TResult>(
            string operationName, 
            TRequest request, 
            int timeoutSeconds = 60);

        /// <summary>
        /// Execute operation in Monitor process (long-term monitoring operations)
        /// </summary>
        Task<ServiceOperationResult<TResult>> ExecuteMonitorOperationAsync<TRequest, TResult>(
            string operationName, 
            TRequest request, 
            int timeoutSeconds = 60);

        /// <summary>
        /// Check if Monitor process is available
        /// </summary>
        bool IsMonitorProcessAvailable { get; }

        /// <summary>
        /// Check if Worker process is available
        /// </summary>
        bool IsWorkerProcessAvailable { get; }
    }
}