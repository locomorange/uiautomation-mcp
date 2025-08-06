using System.Text.Json.Serialization;

namespace UIAutomationMCP.Server.Infrastructure.Logging
{
    /// <summary>
    /// MCP notification parameters that are AOT-compatible
    /// </summary>
    public class McpNotificationParams
    {
        [JsonPropertyName("level")]
        public string Level { get; set; } = "info";

        [JsonPropertyName("logger")]
        public string Logger { get; set; } = "";

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        [JsonPropertyName("data")]
        public Dictionary<string, object?>? Data { get; set; }
    }

    /// <summary>
    /// File log entry for MCP logging with enhanced structure
    /// </summary>
    public class McpFileLogEntry
    {
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("level")]
        public string Level { get; set; } = "info";

        [JsonPropertyName("logger")]
        public string Logger { get; set; } = "";

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        [JsonPropertyName("data")]
        public Dictionary<string, object?>? Data { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; } = "server";

        [JsonPropertyName("processId")]
        public int ProcessId { get; set; } = Environment.ProcessId;

        [JsonPropertyName("threadId")]
        public int ThreadId { get; set; } = Environment.CurrentManagedThreadId;
    }

    /// <summary>
    /// JSON source generation context for AOT compatibility
    /// </summary>
    [JsonSourceGenerationOptions(
        WriteIndented = true,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(McpNotificationParams))]
    [JsonSerializable(typeof(McpFileLogEntry))]
    [JsonSerializable(typeof(Dictionary<string, object?>))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(DateTime))]
    [JsonSerializable(typeof(object))]
    public partial class McpNotificationJsonContext : JsonSerializerContext
    {
    }
}
