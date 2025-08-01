using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Models.Logging
{
    /// <summary>
    /// Interface for MCP-compliant logging service
    /// </summary>
    public interface IMcpLogService
    {
        /// <summary>
        /// Send structured log message to MCP client
        /// </summary>
        Task LogAsync(McpLogMessage message);

        /// <summary>
        /// Send log message with level and content
        /// </summary>
        Task LogAsync(McpLogLevel level, string logger, string message, string source = "server", string? operationId = null, Dictionary<string, object?>? data = null);

        /// <summary>
        /// Send log message compatible with .NET ILogger
        /// </summary>
        Task LogAsync(LogLevel level, string logger, string message, string source = "server", string? operationId = null, Dictionary<string, object?>? data = null);

        /// <summary>
        /// Log debug message
        /// </summary>
        Task LogDebugAsync(string logger, string message, string source = "server", string? operationId = null, Dictionary<string, object?>? data = null);

        /// <summary>
        /// Log information message
        /// </summary>
        Task LogInformationAsync(string logger, string message, string source = "server", string? operationId = null, Dictionary<string, object?>? data = null);

        /// <summary>
        /// Log warning message
        /// </summary>
        Task LogWarningAsync(string logger, string message, string source = "server", string? operationId = null, Dictionary<string, object?>? data = null);

        /// <summary>
        /// Log error message
        /// </summary>
        Task LogErrorAsync(string logger, string message, Exception? exception = null, string source = "server", string? operationId = null, Dictionary<string, object?>? data = null);

        /// <summary>
        /// Log critical message
        /// </summary>
        Task LogCriticalAsync(string logger, string message, Exception? exception = null, string source = "server", string? operationId = null, Dictionary<string, object?>? data = null);

        /// <summary>
        /// Process log message from worker/monitor process
        /// </summary>
        Task ProcessInterProcessLogAsync(string logJson);
    }
}