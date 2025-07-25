namespace UIAutomationMCP.Core.Abstractions
{
    /// <summary>
    /// Type-safe operation execution result with strongly-typed success and error states
    /// </summary>
    public readonly record struct ServiceOperationResult<T>
    {
        public bool Success { get; init; }
        public T? Data { get; init; }
        public string? Error { get; init; }
        public string? ErrorCategory { get; init; }
        public string? ExceptionType { get; init; }
        
        public static ServiceOperationResult<T> FromSuccess(T data) => new()
        {
            Success = true,
            Data = data
        };
        
        public static ServiceOperationResult<T> FromError(string error, string? category = null, string? exceptionType = null) => new()
        {
            Success = false,
            Error = error,
            ErrorCategory = category,
            ExceptionType = exceptionType
        };
        
        public static ServiceOperationResult<T> FromException(Exception exception, string? category = null) => new()
        {
            Success = false,
            Error = exception.Message,
            ErrorCategory = category ?? "Exception",
            ExceptionType = exception.GetType().Name
        };
    }
}