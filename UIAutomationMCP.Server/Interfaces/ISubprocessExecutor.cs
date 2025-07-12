namespace UIAutomationMCP.Server.Interfaces
{
    /// <summary>
    /// Interface for subprocess execution operations
    /// Enables mocking in unit tests
    /// </summary>
    public interface ISubprocessExecutor : IDisposable
    {
        /// <summary>
        /// Executes a worker operation asynchronously
        /// </summary>
        /// <typeparam name="TResult">The expected result type</typeparam>
        /// <param name="operation">The operation name to execute</param>
        /// <param name="parameters">Optional parameters for the operation</param>
        /// <param name="timeoutSeconds">Timeout in seconds (default 60)</param>
        /// <returns>The operation result</returns>
        Task<TResult> ExecuteAsync<TResult>(string operation, object? parameters = null, int timeoutSeconds = 60);
    }
}