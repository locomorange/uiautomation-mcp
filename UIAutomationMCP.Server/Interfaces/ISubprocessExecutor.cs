namespace UIAutomationMCP.Server.Interfaces
{
    /// <summary>
    /// Interface for subprocess execution operations
    /// Enables mocking in unit tests
    /// </summary>
    public interface ISubprocessExecutor : IDisposable
    {
        /// <summary>
        /// Type-safe unified execute method - eliminates type branching and legacy support
        /// </summary>
        /// <typeparam name="TRequest">The request type</typeparam>
        /// <typeparam name="TResult">The expected result type</typeparam>
        /// <param name="operation">The operation name</param>
        /// <param name="request">The typed request object</param>
        /// <param name="timeoutSeconds">Timeout in seconds</param>
        /// <returns>The operation result</returns>
        Task<TResult> ExecuteAsync<TRequest, TResult>(string operation, TRequest request, int timeoutSeconds = 60) 
            where TRequest : notnull 
            where TResult : notnull;

        /// <summary>
        /// TEMPORARY: Legacy compatibility method - will be removed after all services are updated
        /// </summary>
        /// <typeparam name="TResult">The expected result type</typeparam>
        /// <param name="operation">The operation name to execute</param>
        /// <param name="parameters">Legacy parameters object</param>
        /// <param name="timeoutSeconds">Timeout in seconds (default 60)</param>
        /// <returns>The operation result</returns>
        [Obsolete("Use ExecuteAsync<TRequest, TResult> with typed request objects")]
        Task<TResult> ExecuteAsync<TResult>(string operation, object? parameters = null, int timeoutSeconds = 60) where TResult : notnull;
    }
}