using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Models.Abstractions
{
    /// <summary>
    /// Interface for executing UI automation operations
    /// </summary>
    public interface IOperationExecutor
    {
        /// <summary>
        /// Execute an operation asynchronously
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="request">Request data</param>
        /// <param name="timeoutSeconds">Timeout in seconds</param>
        /// <returns>Operation result</returns>
        Task<ServiceOperationResult<TResult>> ExecuteAsync<TRequest, TResult>(
            string operationName, 
            TRequest request, 
            int timeoutSeconds = 30)
            where TRequest : notnull
            where TResult : notnull;
    }
}