using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UIAutomationMCP.Models.Logging;

namespace UIAutomationMCP.Subprocess.Worker.Helpers
{
    public class DebugLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new DebugLogger(categoryName);
        }

        public void Dispose() { }
    }

    public class DebugLogger : ILogger
    {
        private readonly string _categoryName;

        public DebugLogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logMessage = $"[{timestamp}] [{logLevel}] [{_categoryName}] {message}";

            // Send to MCP relay asynchronously
            _ = Task.Run(async () =>
            {
                try
                {
                    var data = new Dictionary<string, object?>
                    {
                        ["timestamp"] = timestamp,
                        ["categoryName"] = _categoryName,
                        ["eventId"] = eventId.Id
                    };

                    await ProcessLogRelay.SendLogToServerAsync(
                        logLevel.ToMcpLogLevel(),
                        _categoryName,
                        message,
                        "worker",
                        null,
                        data
                    );

                    // If there's an exception, send a separate error log
                    if (exception != null)
                    {
                        await ProcessLogRelay.LogErrorAsync(_categoryName,
                            $"Exception in {_categoryName}", "worker", exception);
                    }
                }
                catch
                {
                    // Fallback to stderr if MCP relay fails
                    Debug.WriteLine(logMessage);
                    Console.Error.WriteLine($"[WORKER] {logMessage}");
                    if (exception != null)
                    {
                        Console.Error.WriteLine($"[WORKER] Exception: {exception}");
                    }
                }
            });
        }

        private class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new();
            public void Dispose() { }
        }
    }
}

