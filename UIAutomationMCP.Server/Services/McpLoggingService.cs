using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using UIAutomationMCP.Models.Logging;
using UIAutomationMCP.Models.Serialization;

namespace UIAutomationMCP.Server.Services
{
    /// <summary>
    /// MCP-compliant logging service that sends structured logs to MCP client
    /// </summary>
    public class McpLoggingService : IMcpLogService
    {
        private IMcpEndpoint? _mcpEndpoint;
        private readonly ILogger<McpLoggingService> _fallbackLogger;
        private readonly Queue<McpLogMessage> _pendingLogs = new();
        private readonly object _lock = new();
        private volatile bool _mcpEndpointAvailable = false;

        // Debug file logging feature
        private readonly bool _fileLoggingEnabled;
        private readonly string _debugLogPath;

        public McpLoggingService(ILogger<McpLoggingService> fallbackLogger, IMcpEndpoint? mcpEndpoint = null)
        {
            _fallbackLogger = fallbackLogger;
            _mcpEndpoint = mcpEndpoint;
            _mcpEndpointAvailable = mcpEndpoint != null;

            // Check environment variable for debug file logging (disabled by default)
            _fileLoggingEnabled = Environment.GetEnvironmentVariable("MCP_DEBUG_FILE_LOGGING")?.ToLowerInvariant() == "true";
            _debugLogPath = Environment.GetEnvironmentVariable("MCP_DEBUG_LOG_PATH") ?? "mcp-debug.log";

            if (_fileLoggingEnabled)
            {
                _fallbackLogger.LogInformation("Debug file logging enabled: {LogPath}", _debugLogPath);
                // Log file initialization in the background to avoid blocking startup
                Task.Run(() => LogToFile("INFO", "McpLoggingService", $"Debug file logging initialized - Server PID: {Environment.ProcessId}", "server"));
            }
        }

        /// <summary>
        /// Set MCP endpoint after service initialization (for DI scenarios)
        /// </summary>
        public void SetMcpEndpoint(IMcpEndpoint mcpEndpoint)
        {
            lock (_lock)
            {
                if (_mcpEndpoint == null)
                {
                    _mcpEndpoint = mcpEndpoint;
                    _mcpEndpointAvailable = true;

                    // Send any pending logs
                    while (_pendingLogs.Count > 0)
                    {
                        var pendingLog = _pendingLogs.Dequeue();
                        _ = Task.Run(async () => await SendToMcpClientAsync(pendingLog));
                    }
                }
            }
        }

        public async Task LogAsync(McpLogMessage message)
        {
            if (_mcpEndpointAvailable && _mcpEndpoint != null)
            {
                await SendToMcpClientAsync(message);
            }
            else
            {
                // Queue for later sending or fallback to stderr
                lock (_lock)
                {
                    _pendingLogs.Enqueue(message);

                    // Limit queue size to prevent memory issues
                    while (_pendingLogs.Count > 1000)
                    {
                        _pendingLogs.Dequeue();
                    }
                }

                // Fallback to stderr logging for now
                await LogToStderrFallback(message);
            }
        }

        public async Task LogAsync(McpLogLevel level, string logger, string message, string source = "server", string? operationId = null, Dictionary<string, object?>? data = null)
        {
            var logMessage = new McpLogMessageBuilder()
                .WithLevel(level)
                .WithLogger(logger)
                .WithMessage(message)
                .WithSource(source)
                .WithOperationId(operationId)
                .WithData(data ?? new Dictionary<string, object?>())
                .Build();

            await LogAsync(logMessage);
        }

        public async Task LogAsync(LogLevel level, string logger, string message, string source = "server", string? operationId = null, Dictionary<string, object?>? data = null)
        {
            await LogAsync(level.ToMcpLogLevel(), logger, message, source, operationId, data);
        }

        public async Task LogDebugAsync(string logger, string message, string source = "server", string? operationId = null, Dictionary<string, object?>? data = null)
        {
            await LogAsync(McpLogLevel.Debug, logger, message, source, operationId, data);
        }

        public async Task LogInformationAsync(string logger, string message, string source = "server", string? operationId = null, Dictionary<string, object?>? data = null)
        {
            await LogAsync(McpLogLevel.Info, logger, message, source, operationId, data);
        }

        public async Task LogWarningAsync(string logger, string message, string source = "server", string? operationId = null, Dictionary<string, object?>? data = null)
        {
            await LogAsync(McpLogLevel.Warning, logger, message, source, operationId, data);
        }

        public async Task LogErrorAsync(string logger, string message, Exception? exception = null, string source = "server", string? operationId = null, Dictionary<string, object?>? data = null)
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

            await LogAsync(McpLogLevel.Error, logger, message, source, operationId, logData);
        }

        public async Task LogCriticalAsync(string logger, string message, Exception? exception = null, string source = "server", string? operationId = null, Dictionary<string, object?>? data = null)
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

            await LogAsync(McpLogLevel.Critical, logger, message, source, operationId, logData);
        }

        public async Task ProcessInterProcessLogAsync(string logJson)
        {
            var logMessage = McpLogMessage.FromJson(logJson);
            if (logMessage != null)
            {
                await LogAsync(logMessage);
            }
            else
            {
                await LogWarningAsync("McpLoggingService", $"Failed to parse inter-process log message: {logJson}");
            }
        }

        private async Task SendToMcpClientAsync(McpLogMessage message)
        {
            try
            {
                if (_mcpEndpoint != null)
                {
                    await _mcpEndpoint.SendNotificationAsync("notifications/message", message.ToMcpNotificationParams());
                }
            }
            catch (Exception ex)
            {
                // Fallback to stderr if MCP notification fails
                _fallbackLogger.LogError(ex, "Failed to send MCP log notification: {Message}", message.Message);
                await LogToStderrFallback(message);
            }
        }

        private async Task LogToStderrFallback(McpLogMessage message)
        {
            // Map MCP levels to .NET LogLevel for stderr fallback
            var netLogLevel = message.Level switch
            {
                McpLogLevel.Debug => LogLevel.Debug,
                McpLogLevel.Info => LogLevel.Information,
                McpLogLevel.Notice => LogLevel.Information,
                McpLogLevel.Warning => LogLevel.Warning,
                McpLogLevel.Error => LogLevel.Error,
                McpLogLevel.Critical => LogLevel.Critical,
                McpLogLevel.Alert => LogLevel.Critical,
                McpLogLevel.Emergency => LogLevel.Critical,
                _ => LogLevel.Information
            };

            try
            {
                var dataStr = message.Data.Count > 0 ? JsonSerializationHelper.Serialize(message.Data) : "";
                _fallbackLogger.Log(netLogLevel, "[{Source}] [{Logger}] {Message} {Data}",
                    message.Source, message.Logger, message.Message, dataStr);
            }
            catch
            {
                // If JSON serialization fails, just log without data
                _fallbackLogger.Log(netLogLevel, "[{Source}] [{Logger}] {Message}",
                    message.Source, message.Logger, message.Message);
            }

            // Also log to debug file if enabled
            if (_fileLoggingEnabled)
            {
                LogToFile(message.Level.ToString().ToUpperInvariant(), message.Logger, message.Message, message.Source);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Log message to debug file (only when MCP_DEBUG_FILE_LOGGING=true)
        /// </summary>
        private void LogToFile(string level, string logger, string message, string source)
        {
            if (!_fileLoggingEnabled) return;

            try
            {
                var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var processId = Environment.ProcessId;
                var logLine = $"[{timestamp}] [{level}] PID:{processId} [{source}] [{logger}] {message}";

                // Thread-safe file writing using FileStream for atomicity
                lock (_lock)
                {
                    using var stream = new FileStream(_debugLogPath, FileMode.Append, FileAccess.Write, FileShare.Read);
                    using var writer = new StreamWriter(stream);
                    writer.WriteLine(logLine);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DirectoryNotFoundException)
            {
                // Log file logging errors to the fallback logger to aid troubleshooting
                _fallbackLogger.LogError(ex, "Failed to write log entry to debug file '{DebugLogPath}'.", _debugLogPath);
            }
            catch
            {
                // Silently ignore other exceptions to avoid disrupting main functionality
            }
        }
    }
}
