namespace UIAutomationMCP.Models.Logging
{
    /// <summary>
    /// Relays log messages from subprocess (Worker/Monitor) to Server via stdout/stderr communication
    /// </summary>
    public static class ProcessLogRelay
    {
        private const string LOG_MESSAGE_PREFIX = "[MCP_LOG]";

        /// <summary>
        /// Send log message to Server process via stderr
        /// </summary>
        public static async Task SendLogToServerAsync(McpLogMessage logMessage)
        {
            try
            {
                var logJson = logMessage.ToJson();
                var logLine = $"{LOG_MESSAGE_PREFIX}{logJson}";
                
                // Send to stderr to avoid interfering with JSON responses on stdout
                await Console.Error.WriteLineAsync(logLine);
                await Console.Error.FlushAsync();
            }
            catch
            {
                // If log relay fails, silently continue to avoid cascading failures
            }
        }

        /// <summary>
        /// Send log message with specified parameters
        /// </summary>
        public static async Task SendLogToServerAsync(
            McpLogLevel level, 
            string logger, 
            string message, 
            string source,
            string? operationId = null, 
            Dictionary<string, object?>? data = null)
        {
            var logMessage = new McpLogMessageBuilder()
                .WithLevel(level)
                .WithLogger(logger)
                .WithMessage(message)
                .WithSource(source)
                .WithOperationId(operationId)
                .WithData(data ?? new Dictionary<string, object?>())
                .Build();

            await SendLogToServerAsync(logMessage);
        }

        /// <summary>
        /// Send information log
        /// </summary>
        public static async Task LogInfoAsync(string logger, string message, string source, string? operationId = null, Dictionary<string, object?>? data = null)
        {
            await SendLogToServerAsync(McpLogLevel.Info, logger, message, source, operationId, data);
        }

        /// <summary>
        /// Send debug log
        /// </summary>
        public static async Task LogDebugAsync(string logger, string message, string source, string? operationId = null, Dictionary<string, object?>? data = null)
        {
            await SendLogToServerAsync(McpLogLevel.Debug, logger, message, source, operationId, data);
        }

        /// <summary>
        /// Send warning log
        /// </summary>
        public static async Task LogWarningAsync(string logger, string message, string source, string? operationId = null, Dictionary<string, object?>? data = null)
        {
            await SendLogToServerAsync(McpLogLevel.Warning, logger, message, source, operationId, data);
        }

        /// <summary>
        /// Send error log
        /// </summary>
        public static async Task LogErrorAsync(string logger, string message, string source, Exception? exception = null, string? operationId = null, Dictionary<string, object?>? data = null)
        {
            var logData = data ?? new Dictionary<string, object?>();
            if (exception != null)
            {
                logData["exception"] = new
                {
                    type = exception.GetType().Name,
                    message = exception.Message,
                    stackTrace = exception.StackTrace
                };
            }

            await SendLogToServerAsync(McpLogLevel.Error, logger, message, source, operationId, logData);
        }

        /// <summary>
        /// Check if a stderr line contains a log message for relay
        /// </summary>
        public static bool IsLogMessage(string line)
        {
            return line.StartsWith(LOG_MESSAGE_PREFIX);
        }

        /// <summary>
        /// Extract log message JSON from stderr line
        /// </summary>
        public static string? ExtractLogJson(string line)
        {
            if (IsLogMessage(line))
            {
                return line.Substring(LOG_MESSAGE_PREFIX.Length);
            }
            return null;
        }
    }
}