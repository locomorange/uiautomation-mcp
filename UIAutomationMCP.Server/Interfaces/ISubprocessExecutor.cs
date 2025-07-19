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
    }
}