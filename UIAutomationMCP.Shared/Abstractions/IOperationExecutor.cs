namespace UIAutomationMCP.Core.Abstractions
{
    /// <summary>
    /// Executes operations in isolation (subprocess, separate service, etc.)
    /// </summary>
    public interface IOperationExecutor
    {
        /// <summary>
        /// Execute an operation with typed request and response
        /// </summary>
        Task<ServiceOperationResult<TResult>> ExecuteAsync<TRequest, TResult>(
            string operationName, 
            TRequest request, 
            int timeoutSeconds = 60);
    }
}