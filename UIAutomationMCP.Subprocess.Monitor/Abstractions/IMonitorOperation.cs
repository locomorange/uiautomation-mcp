using UIAutomationMCP.Models;

namespace UIAutomationMCP.Subprocess.Monitor.Abstractions
{
    /// <summary>
    /// Generic operation interface for type-safe Monitor operations
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResult">Result type</typeparam>
    public interface IMonitorOperation<TRequest, TResult>
        where TRequest : class
        where TResult : class
    {
        /// <summary>
        /// Execute the operation with typed request and result
        /// </summary>
        /// <param name="request">Typed request object</param>
        /// <returns>Operation result with typed data</returns>
        Task<OperationResult<TResult>> ExecuteAsync(TRequest request);
    }

    /// <summary>
    /// Type-safe operation interface for Monitor operations
    /// </summary>
    public interface IMonitorOperation
    {
        /// <summary>
        /// Execute Monitor operation with typed parameters
        /// </summary>
        /// <param name="parameters">Operation parameters as object</param>
        /// <returns>Operation result</returns>
        Task<OperationResult> ExecuteAsync(object? parameters);
    }
}

