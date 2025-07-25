using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using UIAutomationMCP.Monitor.Sessions;
using UIAutomationMCP.UIAutomation.Services;

namespace UIAutomationMCP.Monitor.Infrastructure
{
    /// <summary>
    /// Manages event monitoring sessions in the Monitor process
    /// </summary>
    public class SessionManager
    {
        private readonly ConcurrentDictionary<string, EventMonitoringSession> _activeSessions = new();
        private readonly ILogger<SessionManager> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ElementFinderService _elementFinderService;

        public SessionManager(ILogger<SessionManager> logger, ILoggerFactory loggerFactory, ElementFinderService elementFinderService)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _elementFinderService = elementFinderService;
        }

        /// <summary>
        /// Create a new monitoring session
        /// </summary>
        public EventMonitoringSession CreateSession(
            string sessionId,
            string[] eventTypes,
            string? automationId = null,
            string? name = null,
            string? controlType = null,
            string? windowTitle = null,
            int? processId = null)
        {
            if (_activeSessions.ContainsKey(sessionId))
            {
                throw new InvalidOperationException($"Session with ID '{sessionId}' already exists");
            }

            var session = new EventMonitoringSession(
                sessionId,
                eventTypes,
                automationId,
                name,
                controlType,
                windowTitle,
                processId,
                _elementFinderService,
                _loggerFactory.CreateLogger<EventMonitoringSession>());

            if (!_activeSessions.TryAdd(sessionId, session))
            {
                session.Dispose();
                throw new InvalidOperationException($"Failed to add session '{sessionId}' to active sessions");
            }

            _logger.LogInformation("Created monitoring session: {SessionId} for events: [{EventTypes}]", 
                sessionId, string.Join(", ", eventTypes));

            return session;
        }

        /// <summary>
        /// Get an active session
        /// </summary>
        public EventMonitoringSession? GetSession(string sessionId)
        {
            _activeSessions.TryGetValue(sessionId, out var session);
            return session;
        }

        /// <summary>
        /// Remove and dispose a session
        /// </summary>
        public bool RemoveSession(string sessionId)
        {
            if (_activeSessions.TryRemove(sessionId, out var session))
            {
                try
                {
                    session.Dispose();
                    _logger.LogInformation("Removed and disposed monitoring session: {SessionId}", sessionId);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing session {SessionId}", sessionId);
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Get all active session IDs
        /// </summary>
        public IEnumerable<string> GetActiveSessionIds()
        {
            return _activeSessions.Keys;
        }

        /// <summary>
        /// Get total active session count
        /// </summary>
        public int ActiveSessionCount => _activeSessions.Count;

        /// <summary>
        /// Clean up expired sessions
        /// </summary>
        public void CleanupExpiredSessions(TimeSpan maxAge)
        {
            var cutoffTime = DateTime.UtcNow - maxAge;
            var expiredSessions = _activeSessions
                .Where(kvp => kvp.Value.StartTime < cutoffTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var sessionId in expiredSessions)
            {
                if (RemoveSession(sessionId))
                {
                    _logger.LogInformation("Cleaned up expired session: {SessionId}", sessionId);
                }
            }

            if (expiredSessions.Count > 0)
            {
                _logger.LogInformation("Cleaned up {Count} expired sessions", expiredSessions.Count);
            }
        }

        /// <summary>
        /// Dispose all sessions
        /// </summary>
        public void Dispose()
        {
            _logger.LogInformation("Disposing SessionManager with {Count} active sessions", _activeSessions.Count);

            foreach (var kvp in _activeSessions)
            {
                try
                {
                    kvp.Value.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing session {SessionId} during cleanup", kvp.Key);
                }
            }

            _activeSessions.Clear();
            _logger.LogInformation("SessionManager disposed");
        }
    }
}