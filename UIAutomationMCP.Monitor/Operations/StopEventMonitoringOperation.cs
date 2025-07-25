using Microsoft.Extensions.Logging;
using UIAutomationMCP.Monitor.Infrastructure;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Serialization;

namespace UIAutomationMCP.Monitor.Operations
{
    /// <summary>
    /// Stop event monitoring operation for Monitor process
    /// </summary>
    public class StopEventMonitoringOperation
    {
        private readonly SessionManager _sessionManager;
        private readonly ILogger<StopEventMonitoringOperation> _logger;

        public StopEventMonitoringOperation(
            SessionManager sessionManager,
            ILogger<StopEventMonitoringOperation> logger)
        {
            _sessionManager = sessionManager;
            _logger = logger;
        }

        public async Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                if (string.IsNullOrEmpty(parametersJson))
                {
                    return new OperationResult
                    {
                        Success = false,
                        Error = "ParametersJson is null or empty",
                        Data = new EventMonitoringStopResult()
                    };
                }

                var request = JsonSerializationHelper.Deserialize<StopEventMonitoringRequest>(parametersJson);
                if (request == null)
                {
                    return new OperationResult
                    {
                        Success = false,
                        Error = "Failed to deserialize StopEventMonitoringRequest",
                        Data = new EventMonitoringStopResult()
                    };
                }

                _logger.LogInformation("Stopping event monitoring - MonitorId: {MonitorId}", request.MonitorId);

                var session = _sessionManager.GetSession(request.MonitorId);
                if (session == null)
                {
                    _logger.LogWarning("Session not found: {MonitorId}", request.MonitorId);
                    
                    return new OperationResult
                    {
                        Success = false,
                        Error = $"Monitoring session '{request.MonitorId}' not found",
                        Data = new EventMonitoringStopResult
                        {
                            Success = false,
                            SessionId = request.MonitorId,
                            FinalEventCount = 0,
                            MonitoringStatus = "Session not found"
                        }
                    };
                }

                var finalEventCount = session.EventCount;
                session.Stop();
                
                var removed = _sessionManager.RemoveSession(request.MonitorId);
                if (!removed)
                {
                    _logger.LogWarning("Failed to remove session from manager: {MonitorId}", request.MonitorId);
                }

                var result = new EventMonitoringStopResult
                {
                    Success = true,
                    SessionId = request.MonitorId,
                    FinalEventCount = finalEventCount,
                    MonitoringStatus = "Stopped"
                };

                _logger.LogInformation("Event monitoring stopped successfully - SessionId: {SessionId}, FinalEventCount: {EventCount}",
                    request.MonitorId, finalEventCount);

                return new OperationResult
                {
                    Success = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StopEventMonitoring operation failed");
                return new OperationResult
                {
                    Success = false,
                    Error = $"StopEventMonitoring failed: {ex.Message}",
                    Data = new EventMonitoringStopResult()
                };
            }
        }
    }
}