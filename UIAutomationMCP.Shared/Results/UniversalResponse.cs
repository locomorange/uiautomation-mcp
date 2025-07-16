using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Universal response wrapper for MCP operations
    /// </summary>
    public class UniversalResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public object? Data { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        /// <summary>
        /// Create a success response
        /// </summary>
        public static UniversalResponse CreateSuccess(object? data = null)
        {
            return new UniversalResponse
            {
                Success = true,
                Data = data
            };
        }

        /// <summary>
        /// Create an error response
        /// </summary>
        public static UniversalResponse CreateError(string error)
        {
            return new UniversalResponse
            {
                Success = false,
                Error = error
            };
        }
    }
}
