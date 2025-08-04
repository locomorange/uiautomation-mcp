namespace UIAutomationMCP.Models.Results
{
    /// <summary>
    /// Server execution information
    /// </summary>
    public class ServerExecutionInfo
    {
        public string ExecutionId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public string ServerVersion { get; set; } = string.Empty;
        public string ServerProcessingTime { get; set; } = string.Empty;
        public string OperationId { get; set; } = string.Empty;
        public DateTime ServerExecutedAt { get; set; }
        public List<string> ServerLogs { get; set; } = new List<string>();
        public object? Metadata { get; set; }
    }

    /// <summary>
    /// Request metadata information
    /// </summary>
    public class RequestMetadata
    {
        public string RequestId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public DateTime RequestTime { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public string RequestedMethod { get; set; } = string.Empty;
        public string OperationId { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; }
    }

    /// <summary>
    /// Enhanced server response wrapper
    /// </summary>
    public class ServerEnhancedResponse<T>
    {
        public T? Data { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public ServerExecutionInfo ExecutionInfo { get; set; } = new ServerExecutionInfo();
        public RequestMetadata Metadata { get; set; } = new RequestMetadata();
        public RequestMetadata RequestMetadata { get; set; } = new RequestMetadata();
    }

    /// <summary>
    /// Universal response for flexible operations
    /// </summary>
    public class UniversalResponse : BaseOperationResult
    {
        public object? Data { get; set; }
        public string ResponseType { get; set; } = string.Empty;
        public new string Metadata { get; set; } = string.Empty;
    }
}
