using System.Text.Json.Serialization;

namespace UIAutomationMCP.Models
{
    /// <summary>
    /// Base response model for MCP operations
    /// </summary>
    public abstract class McpResponseBase
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        
        [JsonPropertyName("error")]
        public string? Error { get; set; }
        
        [JsonPropertyName("executedAt")]
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Response model for application launch operations
    /// </summary>
    public class ProcessLaunchResponse : McpResponseBase
    {
        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }
        
        [JsonPropertyName("processName")]
        public string ProcessName { get; set; } = string.Empty;
        
        [JsonPropertyName("hasExited")]
        public bool HasExited { get; set; }
        
        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }
        
        [JsonPropertyName("windowHandle")]
        public long? WindowHandle { get; set; }
        
        /// <summary>
        /// Create a success response
        /// </summary>
        public static ProcessLaunchResponse CreateSuccess(int processId, string processName, bool hasExited, string? windowTitle = null, long? windowHandle = null)
        {
            return new ProcessLaunchResponse
            {
                Success = true,
                ProcessId = processId,
                ProcessName = processName,
                HasExited = hasExited,
                WindowTitle = windowTitle,
                WindowHandle = windowHandle
            };
        }
        
        /// <summary>
        /// Create an error response
        /// </summary>
        public static ProcessLaunchResponse CreateError(string error)
        {
            return new ProcessLaunchResponse
            {
                Success = false,
                Error = error
            };
        }
    }

    /// <summary>
    /// Generic response model for UI operations
    /// </summary>
    public class UIOperationResponse : McpResponseBase
    {
        [JsonPropertyName("data")]
        public object? Data { get; set; }
        
        /// <summary>
        /// Create a success response
        /// </summary>
        public static UIOperationResponse CreateSuccess(object? data = null)
        {
            return new UIOperationResponse
            {
                Success = true,
                Data = data
            };
        }
        
        /// <summary>
        /// Create an error response
        /// </summary>
        public static UIOperationResponse CreateError(string error)
        {
            return new UIOperationResponse
            {
                Success = false,
                Error = error
            };
        }
    }

    /// <summary>
    /// Response model for element search operations
    /// </summary>
    public class ElementSearchResponse : McpResponseBase
    {
        [JsonPropertyName("elements")]
        public List<ElementInfo> Elements { get; set; } = new();
        
        [JsonPropertyName("count")]
        public int Count { get; set; }
        
        /// <summary>
        /// Create a success response
        /// </summary>
        public static ElementSearchResponse CreateSuccess(List<ElementInfo> elements)
        {
            return new ElementSearchResponse
            {
                Success = true,
                Elements = elements,
                Count = elements.Count
            };
        }
        
        /// <summary>
        /// Create an error response
        /// </summary>
        public static ElementSearchResponse CreateError(string error)
        {
            return new ElementSearchResponse
            {
                Success = false,
                Error = error
            };
        }
    }
}