using Microsoft.Extensions.Logging;
using UIAutomationMCP.Monitor.Infrastructure;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Serialization;

namespace UIAutomationMCP.Monitor.Operations
{
    /// <summary>
    /// Start event monitoring operation for Monitor process
    /// </summary>
    public class StartEventMonitoringOperation
    {
        private readonly SessionManager _sessionManager;
        private readonly ILogger<StartEventMonitoringOperation> _logger;

        public StartEventMonitoringOperation(
            SessionManager sessionManager,
            ILogger<StartEventMonitoringOperation> logger)
        {
            _sessionManager = sessionManager;
            _logger = logger;
        }

        public async Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                _logger.LogInformation("StartEventMonitoringOperation.ExecuteAsync started");
                
                if (string.IsNullOrEmpty(parametersJson))
                {
                    return new OperationResult
                    {
                        Success = false,
                        Error = "ParametersJson is null or empty",
                        Data = new EventMonitoringStartResult()
                    };
                }

                var request = JsonSerializationHelper.Deserialize<StartEventMonitoringRequest>(parametersJson);
                if (request == null)
                {
                    return new OperationResult
                    {
                        Success = false,
                        Error = "Failed to deserialize StartEventMonitoringRequest",
                        Data = new EventMonitoringStartResult()
                    };
                }

                _logger.LogInformation("Starting event monitoring - EventTypes: [{EventTypes}], AutomationId: {AutomationId}, Name: {Name}",
                    string.Join(", ", request.EventTypes), request.AutomationId ?? "null", request.Name ?? "null");

                var sessionId = Guid.NewGuid().ToString("N")[..8];

                var session = _sessionManager.CreateSession(
                    sessionId,
                    request.EventTypes,
                    request.AutomationId,
                    request.Name,
                    request.ControlType,
                    request.WindowTitle,
                    request.ProcessId);

                await session.StartAsync();

                var result = new EventMonitoringStartResult
                {
                    Success = true,
                    EventType = string.Join(", ", request.EventTypes),
                    ElementId = request.AutomationId,
                    WindowTitle = request.WindowTitle,
                    ProcessId = request.ProcessId,
                    SessionId = sessionId,
                    MonitoringStatus = "Started"
                };

                _logger.LogInformation("Event monitoring started successfully - SessionId: {SessionId}", sessionId);

                return new OperationResult
                {
                    Success = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start event monitoring: {ErrorMessage}", ex.Message);
                
                return new OperationResult
                {
                    Success = false,
                    Error = $"Start event monitoring failed: {ex.Message}",
                    Data = new EventMonitoringStartResult
                    {
                        Success = false,
                        MonitoringStatus = "Failed"
                    }
                };
            }
        }
    }
}