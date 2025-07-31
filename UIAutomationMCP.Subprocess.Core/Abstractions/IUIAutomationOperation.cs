using UIAutomationMCP.Models;

namespace UIAutomationMCP.Subprocess.Core.Abstractions
{
    /// <summary>
    /// Generic operation interface for type-safe UI automation operations
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResult">Result type</typeparam>
    public interface IUIAutomationOperation<TRequest, TResult>
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
    /// Legacy string-based operation interface for backward compatibility
    /// </summary>
    public interface IUIAutomationOperation
    {
        /// <summary>
        /// Execute UI automation operation
        /// </summary>
        /// <param name="parametersJson">Operation parameters as JSON string</param>
        /// <returns>Operation result</returns>
        Task<OperationResult> ExecuteAsync(string parametersJson);
    }
}

