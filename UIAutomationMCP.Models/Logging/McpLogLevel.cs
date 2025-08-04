namespace UIAutomationMCP.Models.Logging
{
    /// <summary>
    /// MCP specification syslog severity levels
    /// </summary>
    public enum McpLogLevel
    {
        Debug = 0,
        Info = 1,
        Notice = 2,
        Warning = 3,
        Error = 4,
        Critical = 5,
        Alert = 6,
        Emergency = 7
    }

    /// <summary>
    /// Extensions for converting between .NET LogLevel and MCP LogLevel
    /// </summary>
    public static class McpLogLevelExtensions
    {
        public static McpLogLevel ToMcpLogLevel(this Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return logLevel switch
            {
                Microsoft.Extensions.Logging.LogLevel.Trace => McpLogLevel.Debug,
                Microsoft.Extensions.Logging.LogLevel.Debug => McpLogLevel.Debug,
                Microsoft.Extensions.Logging.LogLevel.Information => McpLogLevel.Info,
                Microsoft.Extensions.Logging.LogLevel.Warning => McpLogLevel.Warning,
                Microsoft.Extensions.Logging.LogLevel.Error => McpLogLevel.Error,
                Microsoft.Extensions.Logging.LogLevel.Critical => McpLogLevel.Critical,
                _ => McpLogLevel.Info
            };
        }

        public static string ToMcpString(this McpLogLevel level)
        {
            return level switch
            {
                McpLogLevel.Debug => "debug",
                McpLogLevel.Info => "info",
                McpLogLevel.Notice => "notice",
                McpLogLevel.Warning => "warning",
                McpLogLevel.Error => "error",
                McpLogLevel.Critical => "critical",
                McpLogLevel.Alert => "alert",
                McpLogLevel.Emergency => "emergency",
                _ => "info"
            };
        }
    }
}
