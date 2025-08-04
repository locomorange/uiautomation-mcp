using UIAutomationMCP.Core.Abstractions;
using UIAutomationMCP.Models.Abstractions;
using UIAutomationMCP.Models.Results;

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
            int timeoutSeconds = 60)
            where TRequest : notnull
            where TResult : notnull;

        /// <summary>
        /// Execute operation in Monitor process (long-term monitoring operations)
        /// </summary>
        Task<ServiceOperationResult<TResult>> ExecuteMonitorOperationAsync<TRequest, TResult>(
            string operationName,
            TRequest request,
            int timeoutSeconds = 60)
            where TRequest : notnull
            where TResult : notnull;

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
