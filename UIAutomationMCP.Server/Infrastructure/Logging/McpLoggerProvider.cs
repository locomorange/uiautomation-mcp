using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using ModelContextProtocol;
using System.Text.Json;

namespace UIAutomationMCP.Server.Infrastructure.Logging
{
    /// <summary>
    /// Configuration for MCP logging output
    /// </summary>
    public class McpLoggerOptions
    {
        public bool EnableNotifications { get; set; } = true;
        public bool EnableFileOutput { get; set; } = false;
        public string FileOutputPath { get; set; } = "mcp-logs.json";
        public string FileOutputFormat { get; set; } = "json";
        public LogLevel FileMinimumLevel { get; set; } = LogLevel.Information;
        public LogLevel NotificationMinimumLevel { get; set; } = LogLevel.Information;
        public int MaxFileSizeMB { get; set; } = 10;
        public int BackupFileCount { get; set; } = 5;
    }

    /// <summary>
    /// Composite logger provider that handles both MCP notifications and file output
    /// Uses AOT-compatible JSON serialization with source generators
    /// </summary>
    public class McpLoggerProvider : ILoggerProvider
    {
        private readonly IMcpServer _mcpServer;
        private readonly McpLoggerOptions _options;
        private readonly object _fileLock = new object();

        public McpLoggerProvider(IMcpServer mcpServer, McpLoggerOptions options)
        {
            _mcpServer = mcpServer;
            _options = options ?? new McpLoggerOptions();

            // Create log directories if they don't exist
            if (_options.EnableFileOutput)
            {
                EnsureLogDirectoryExists(_options.FileOutputPath);
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new McpLogger(categoryName, _mcpServer, _options, _fileLock);
        }

        public void Dispose()
        {
            // Clean up if needed
        }

        private static void EnsureLogDirectoryExists(string filePath)
        {
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            catch
            {
                // Ignore directory creation errors
            }
        }
    }

    /// <summary>
    /// Logger implementation that sends logs to both MCP notifications and file output
    /// </summary>
    internal class McpLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly IMcpServer _mcpServer;
        private readonly McpLoggerOptions _options;
        private readonly object _fileLock;

        public McpLogger(string categoryName, IMcpServer mcpServer, McpLoggerOptions options, object fileLock)
        {
            _categoryName = categoryName;
            _mcpServer = mcpServer;
            _options = options;
            _fileLock = fileLock;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            var timestamp = DateTimeOffset.Now;

            // Send MCP notification if enabled
            if (_options.EnableNotifications && logLevel >= _options.NotificationMinimumLevel)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await SendMcpNotificationAsync(logLevel, message, timestamp, exception);
                    }
                    catch
                    {
                        // Ignore MCP notification errors to avoid cascading failures
                    }
                });
            }

            // Write to file if enabled
            if (_options.EnableFileOutput && logLevel >= _options.FileMinimumLevel)
            {
                _ = Task.Run(() =>
                {
                    try
                    {
                        WriteToFile(logLevel, message, timestamp, exception);
                    }
                    catch
                    {
                        // Ignore file writing errors to avoid cascading failures
                    }
                });
            }
        }

        private async Task SendMcpNotificationAsync(LogLevel logLevel, string message, DateTimeOffset timestamp, Exception? exception)
        {
            var level = MapLogLevelToMcpString(logLevel);

            var data = new Dictionary<string, object?>
            {
                ["message"] = message,
                ["logger"] = _categoryName,
                ["timestamp"] = timestamp.ToString("yyyy/MM/dd H:mm:ss zzz")
            };

            if (exception != null)
            {
                data["exception"] = new Dictionary<string, object?>
                {
                    ["type"] = exception.GetType().Name,
                    ["message"] = exception.Message,
                    ["stackTrace"] = exception.StackTrace
                };
            }

            var notificationParams = new Dictionary<string, object>
            {
                ["level"] = level,
                ["data"] = data
            };

            await _mcpServer.SendNotificationAsync("notifications/message", notificationParams);
        }

        private void WriteToFile(LogLevel logLevel, string message, DateTimeOffset timestamp, Exception? exception)
        {
            lock (_fileLock)
            {
                if (_options.FileOutputFormat?.ToLower() == "json")
                {
                    WriteJsonLog(logLevel, message, timestamp, exception);
                }
                else
                {
                    WriteTextLog(logLevel, message, timestamp, exception);
                }
            }
        }

        private void WriteJsonLog(LogLevel logLevel, string message, DateTimeOffset timestamp, Exception? exception)
        {
            try
            {
                var logEntry = new McpFileLogEntry
                {
                    Timestamp = timestamp.DateTime,
                    Level = MapLogLevelToMcpString(logLevel),
                    Logger = _categoryName,
                    Message = message,
                    Source = "server",
                    ProcessId = Environment.ProcessId,
                    ThreadId = Environment.CurrentManagedThreadId,
                    Data = exception != null ? new Dictionary<string, object?>
                    {
                        ["exception"] = new
                        {
                            type = exception.GetType().Name,
                            message = exception.Message,
                            stackTrace = exception.StackTrace
                        }
                    } : null
                };

                // Use AOT-compatible JSON serialization
                var json = JsonSerializer.Serialize(logEntry, McpNotificationJsonContext.Default.McpFileLogEntry);

                // Rotate file if necessary
                RotateFileIfNeeded(_options.FileOutputPath);

                // Append to file
                File.AppendAllText(_options.FileOutputPath, json + Environment.NewLine);
            }
            catch
            {
                // Ignore JSON writing errors
            }
        }

        private void WriteTextLog(LogLevel logLevel, string message, DateTimeOffset timestamp, Exception? exception)
        {
            try
            {
                var logLine = $"[{timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{logLevel}] [{_categoryName}] {message}";

                if (exception != null)
                {
                    logLine += Environment.NewLine + $"Exception: {exception}";
                }

                // Rotate file if necessary
                RotateFileIfNeeded(_options.FileOutputPath);

                // Append to file
                File.AppendAllText(_options.FileOutputPath, logLine + Environment.NewLine);
            }
            catch
            {
                // Ignore text writing errors
            }
        }

        private void RotateFileIfNeeded(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return;

                var fileInfo = new FileInfo(filePath);
                var maxSizeBytes = _options.MaxFileSizeMB * 1024 * 1024;

                if (fileInfo.Length > maxSizeBytes)
                {
                    // Rotate files
                    for (int i = _options.BackupFileCount; i > 1; i--)
                    {
                        var oldFile = $"{filePath}.{i - 1}";
                        var newFile = $"{filePath}.{i}";

                        if (File.Exists(newFile))
                            File.Delete(newFile);

                        if (File.Exists(oldFile))
                            File.Move(oldFile, newFile);
                    }

                    // Move current file to .1
                    var backupFile = $"{filePath}.1";
                    if (File.Exists(backupFile))
                        File.Delete(backupFile);

                    File.Move(filePath, backupFile);
                }
            }
            catch
            {
                // Ignore rotation errors
            }
        }

        private static string MapLogLevelToMcpString(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "debug",
                LogLevel.Debug => "debug",
                LogLevel.Information => "info",
                LogLevel.Warning => "warning",
                LogLevel.Error => "error",
                LogLevel.Critical => "critical",
                _ => "info"
            };
        }

        private class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new NullDisposable();
            public void Dispose() { }
        }
    }
}
