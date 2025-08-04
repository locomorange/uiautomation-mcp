using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace UIAutomationMCP.Server.Helpers
{
    /// <summary>
    /// Helper class for collecting logs related to operations.
    /// Used to track log entries related to a specific operation ID and include them in responses.
    /// </summary>
    public class OperationLogCollector
    {
        private readonly ConcurrentDictionary<string, List<string>> _operationLogs = new();
        private readonly object _lockObject = new object();

        /// <summary>
        /// Adds a log entry related to an operation.
        /// </summary>
        /// <param name="operationId">Unique identifier for the operation</param>
        /// <param name="logMessage">Log message</param>
        public void AddLog(string operationId, string logMessage)
        {
            if (string.IsNullOrEmpty(operationId) || string.IsNullOrEmpty(logMessage))
                return;

            lock (_lockObject)
            {
                if (!_operationLogs.ContainsKey(operationId))
                {
                    _operationLogs[operationId] = new List<string>();
                }

                var timestamp = DateTime.UtcNow.ToString("HH:mm:ss.fff");
                _operationLogs[operationId].Add($"[{timestamp}] {logMessage}");

                // Prevent memory leaks by limiting old log entries
                if (_operationLogs[operationId].Count > 100)
                {
                    _operationLogs[operationId] = _operationLogs[operationId].TakeLast(50).ToList();
                }
            }
        }

        /// <summary>
        /// Gets log entries related to an operation.
        /// </summary>
        /// <param name="operationId">Unique identifier for the operation</param>
        /// <returns>List of log entries</returns>
        public List<string> GetLogs(string operationId)
        {
            if (string.IsNullOrEmpty(operationId))
                return new List<string>();

            lock (_lockObject)
            {
                if (_operationLogs.TryGetValue(operationId, out var logs))
                {
                    return new List<string>(logs);
                }

                return new List<string>();
            }
        }

        /// <summary>
        /// Clears log entries for an operation and frees memory.
        /// </summary>
        /// <param name="operationId">Unique identifier for the operation</param>
        public void ClearLogs(string operationId)
        {
            if (string.IsNullOrEmpty(operationId))
                return;

            lock (_lockObject)
            {
                _operationLogs.TryRemove(operationId, out _);
            }
        }

        /// <summary>
        /// Cleans up old operation logs (simplified implementation for now).
        /// </summary>
        /// <param name="maxAgeMinutes">Maximum age in minutes to keep</param>
        public void CleanupOldLogs(int maxAgeMinutes = 30)
        {
            lock (_lockObject)
            {
                // For simplicity, clear all logs when count becomes too high
                // In real usage, timestamp-based cleanup would be needed
                if (_operationLogs.Count > 1000)
                {
                    _operationLogs.Clear();
                }
            }
        }
    }

    /// <summary>
    /// Helper methods for using OperationLogCollector.
    /// </summary>
    public static class LogCollectorExtensions
    {
        private static readonly OperationLogCollector _collector = new();

        /// <summary>
        /// Global log collector instance.
        /// </summary>
        public static OperationLogCollector Instance => _collector;

        /// <summary>
        /// Extends ILogger to record logs along with operation ID.
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="operationId">Operation ID</param>
        /// <param name="logLevel">Log level</param>
        /// <param name="message">Log message</param>
        public static void LogWithOperation(this ILogger logger, string operationId, LogLevel logLevel, string message)
        {
            logger.Log(logLevel, message);
            _collector.AddLog(operationId, $"{logLevel}: {message}");
        }

        /// <summary>
        /// Records information log along with operation ID.
        /// </summary>
        public static void LogInformationWithOperation(this ILogger logger, string operationId, string message)
        {
            logger.LogInformation(message);
            _collector.AddLog(operationId, $"INFO: {message}");
        }

        /// <summary>
        /// Records error log along with operation ID.
        /// </summary>
        public static void LogErrorWithOperation(this ILogger logger, string operationId, Exception ex, string message)
        {
            logger.LogError(ex, message);
            _collector.AddLog(operationId, $"ERROR: {message} - {ex.Message}");
        }

        /// <summary>
        /// Records warning log along with operation ID.
        /// </summary>
        public static void LogWarningWithOperation(this ILogger logger, string operationId, string message)
        {
            logger.LogWarning(message);
            _collector.AddLog(operationId, $"WARN: {message}");
        }
    }
}