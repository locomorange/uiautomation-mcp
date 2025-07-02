using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO;

namespace UiAutomationMcpServer.Services
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _filePath;
        private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();

        public FileLoggerProvider(string filePath)
        {
            _filePath = filePath;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new FileLogger(_filePath, name));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }

    public class FileLogger : ILogger
    {
        private readonly string _filePath;
        private readonly string _categoryName;
        private readonly object _lock = new object();

        public FileLogger(string filePath, string categoryName)
        {
            _filePath = filePath;
            _categoryName = categoryName;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            lock (_lock)
            {
                try
                {
                    var message = formatter(state, exception);
                    var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{logLevel}] {_categoryName}: {message}";
                    
                    if (exception != null)
                        logEntry += Environment.NewLine + exception.ToString();
                    
                    logEntry += Environment.NewLine;
                    
                    File.AppendAllText(_filePath, logEntry);
                }
                catch
                {
                    // Ignore logging errors to prevent application failure
                }
            }
        }
    }
}