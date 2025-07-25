namespace UIAutomationMCP.Common.Abstractions
{
    /// <summary>
    /// Generic operation interface for type-safe operation execution
    /// </summary>
    public interface IOperation<TRequest, TResult>
    {
        Task<OperationResult<TResult>> ExecuteAsync(TRequest request);
    }

    /// <summary>
    /// Type-safe operation result
    /// </summary>
    public class OperationResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }
        public ErrorInfo? ErrorDetails { get; set; }

        public static OperationResult<T> CreateSuccess(T data)
        {
            return new OperationResult<T>
            {
                Success = true,
                Data = data
            };
        }

        public static OperationResult<T> CreateError(string error, ErrorInfo? errorDetails = null)
        {
            return new OperationResult<T>
            {
                Success = false,
                Error = error,
                ErrorDetails = errorDetails
            };
        }
    }

    /// <summary>
    /// Structured error information
    /// </summary>
    public class ErrorInfo
    {
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorCategory { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public Dictionary<string, string>? AdditionalInfo { get; set; }
    }
}