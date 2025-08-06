using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using UIAutomationMCP.Models.Logging;

namespace UIAutomationMCP.Server.Services
{
    /// <summary>
    /// Service responsible for relaying subprocess logs to the main logging infrastructure
    /// </summary>
    public class LogRelayService : IMcpLogService
    {
        private readonly ILogger<LogRelayService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public LogRelayService(ILogger<LogRelayService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task LogAsync(McpLogMessage message)
        {
            // Simply use the standard logging infrastructure - file logging is handled at the framework level
            var logLevel = message.Level switch
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

            _logger.Log(logLevel, "[{Source}] [{Logger}] {Message}", message.Source, message.Logger, message.Message);
            await Task.CompletedTask;
        }

        public async Task LogAsync(McpLogLevel level, string logger, string message, string source = "server", string? operationId = null, Dictionary<string, object?>? data = null)
        {
            var logLevel = level switch
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

            _logger.Log(logLevel, "[{Source}] [{Logger}] {Message}", source, logger, message);
            await Task.CompletedTask;
        }

        public async Task LogAsync(LogLevel level, string logger, string message, string source = "server", string? operationId = null, Dictionary<string, object?>? data = null)
        {
            _logger.Log(level, "[{Source}] [{Logger}] {Message}", source, logger, message);
            await Task.CompletedTask;
        }

        public async Task LogDebugAsync(string logger, string message, string source = "server", string? operationId = null, Dictionary<string, object?>? data = null)
        {
            _logger.LogDebug("[{Source}] [{Logger}] {Message}", source, logger, message);
            await Task.CompletedTask;
        }

        public async Task LogInformationAsync(string logger, string message, string source = "server", string? operationId = null, Dictionary<string, object?>? data = null)
        {
            _logger.LogInformation("[{Source}] [{Logger}] {Message}", source, logger, message);
            await Task.CompletedTask;
        }

        public async Task LogWarningAsync(string logger, string message, string source = "server", string? operationId = null, Dictionary<string, object?>? data = null)
        {
            _logger.LogWarning("[{Source}] [{Logger}] {Message}", source, logger, message);
            await Task.CompletedTask;
        }

        public async Task LogErrorAsync(string logger, string message, Exception? exception = null, string source = "server", string? operationId = null, Dictionary<string, object?>? data = null)
        {
            if (exception != null)
            {
                _logger.LogError(exception, "[{Source}] [{Logger}] {Message}", source, logger, message);
            }
            else
            {
                _logger.LogError("[{Source}] [{Logger}] {Message}", source, logger, message);
            }
            await Task.CompletedTask;
        }

        public async Task LogCriticalAsync(string logger, string message, Exception? exception = null, string source = "server", string? operationId = null, Dictionary<string, object?>? data = null)
        {
            if (exception != null)
            {
                _logger.LogCritical(exception, "[{Source}] [{Logger}] {Message}", source, logger, message);
            }
            else
            {
                _logger.LogCritical("[{Source}] [{Logger}] {Message}", source, logger, message);
            }
            await Task.CompletedTask;
        }

        public async Task ProcessInterProcessLogAsync(string logJson)
        {
            try
            {
                // Parse the subprocess log message
                var logMessage = McpLogMessage.FromJson(logJson);
                if (logMessage != null)
                {
                    // Convert to proper ILogger call so it goes through CompositeMcpLoggerProvider
                    var logLevel = logMessage.Level switch
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

                    // Create a logger with the subprocess source info
                    var subprocessLogger = _serviceProvider.GetService<ILoggerFactory>()?.CreateLogger($"[{logMessage.Source.ToUpper()}] {logMessage.Logger}");

                    if (subprocessLogger != null)
                    {
                        // Log through the logger infrastructure so it goes through CompositeMcpLoggerProvider
                        subprocessLogger.Log(logLevel, "{Message}", logMessage.Message);
                    }
                    else
                    {
                        // Fallback
                        _logger.LogInformation("[SUBPROCESS] [{Source}] [{Logger}] {Message}",
                            logMessage.Source, logMessage.Logger, logMessage.Message);
                    }
                }
                else
                {
                    // Fallback for malformed JSON
                    _logger.LogWarning("[SUBPROCESS] Failed to parse log JSON: {LogJson}", logJson);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SUBPROCESS] Error processing inter-process log: {LogJson}", logJson);
            }
            await Task.CompletedTask;
        }

    }
}
