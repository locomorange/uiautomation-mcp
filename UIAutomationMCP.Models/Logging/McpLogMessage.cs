using System.Text.Json;

namespace UIAutomationMCP.Models.Logging
{
    /// <summary>
    /// MCP structured log message for inter-process communication and client notification
    /// </summary>
    public class McpLogMessage
    {
        /// <summary>
        /// MCP syslog level
        /// </summary>
        public McpLogLevel Level { get; set; }

        /// <summary>
        /// Logger name/category (e.g., "UIAutomation.Worker", "UIAutomation.Server")
        /// </summary>
        public string Logger { get; set; } = string.Empty;

        /// <summary>
        /// Primary log message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Source process (server, worker, monitor)
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Operation ID for tracing across process boundaries
        /// </summary>
        public string? OperationId { get; set; }

        /// <summary>
        /// Additional structured data (JSON serializable)
        /// </summary>
        public Dictionary<string, object?> Data { get; set; } = new();

        /// <summary>
        /// Timestamp of the log event
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Convert to MCP notification parameters format
        /// </summary>
        public object ToMcpNotificationParams()
        {
            var data = new Dictionary<string, object?>(Data)
            {
                ["message"] = Message,
                ["source"] = Source,
                ["timestamp"] = Timestamp.ToString("O")
            };

            if (!string.IsNullOrEmpty(OperationId))
            {
                data["operationId"] = OperationId;
            }

            return new
            {
                level = Level.ToMcpString(),
                logger = Logger,
                data = data
            };
        }

        /// <summary>
        /// Serialize for inter-process communication
        /// </summary>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        /// <summary>
        /// Deserialize from inter-process communication
        /// </summary>
        public static McpLogMessage? FromJson(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<McpLogMessage>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Builder for creating MCP log messages
    /// </summary>
    public class McpLogMessageBuilder
    {
        private readonly McpLogMessage _message = new();

        public McpLogMessageBuilder WithLevel(McpLogLevel level)
        {
            _message.Level = level;
            return this;
        }

        public McpLogMessageBuilder WithLevel(Microsoft.Extensions.Logging.LogLevel level)
        {
            _message.Level = level.ToMcpLogLevel();
            return this;
        }

        public McpLogMessageBuilder WithLogger(string logger)
        {
            _message.Logger = logger;
            return this;
        }

        public McpLogMessageBuilder WithMessage(string message)
        {
            _message.Message = message;
            return this;
        }

        public McpLogMessageBuilder WithSource(string source)
        {
            _message.Source = source;
            return this;
        }

        public McpLogMessageBuilder WithOperationId(string? operationId)
        {
            _message.OperationId = operationId;
            return this;
        }

        public McpLogMessageBuilder WithData(string key, object? value)
        {
            _message.Data[key] = value;
            return this;
        }

        public McpLogMessageBuilder WithData(Dictionary<string, object?> data)
        {
            foreach (var kvp in data)
            {
                _message.Data[kvp.Key] = kvp.Value;
            }
            return this;
        }

        public McpLogMessage Build()
        {
            return _message;
        }
    }
}