using System.Text.Json;
using System.Text.Json.Serialization;

namespace UIAutomationMCP.Models.Logging
{
    /// <summary>
    /// JSON serialization context for AOT compatibility
    /// </summary>
    [JsonSerializable(typeof(McpLogMessage))]
    [JsonSerializable(typeof(Dictionary<string, object?>))]
    [JsonSerializable(typeof(object))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(double))]
    [JsonSerializable(typeof(DateTime))]
    public partial class McpLogSerializationContext : JsonSerializerContext
    {
        public static new readonly JsonSerializerOptions Options = new()
        {
            TypeInfoResolver = Default,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
}
