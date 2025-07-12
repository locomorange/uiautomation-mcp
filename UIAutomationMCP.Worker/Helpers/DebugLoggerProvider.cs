using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace UIAutomationMCP.Worker.Helpers
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
            
            if (exception != null)
            {
                logMessage += $"\nException: {exception}";
            }
            
            // Write to debug output and stderr for debugging SubprocessExecutor communication
            Debug.WriteLine(logMessage);
            Console.Error.WriteLine(logMessage);
        }

        private class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new();
            public void Dispose() { }
        }
    }
}