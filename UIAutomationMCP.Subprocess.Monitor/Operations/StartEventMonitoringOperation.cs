using Microsoft.Extensions.Logging;
using UIAutomationMCP.Subprocess.Monitor.Infrastructure;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Subprocess.Core.Services;
using UIAutomationMCP.Subprocess.Monitor.Abstractions;

namespace UIAutomationMCP.Subprocess.Monitor.Operations
{
    /// <summary>
    /// Start event monitoring operation for Monitor process
    /// </summary>
    public class StartEventMonitoringOperation : BaseMonitorOperation<StartEventMonitoringRequest, EventMonitoringStartResult>
    {
        public StartEventMonitoringOperation(
            SessionManager sessionManager,
            ElementFinderService elementFinderService,
            ILogger<StartEventMonitoringOperation> logger)
            : base(sessionManager, elementFinderService, logger)
        {
        }

        protected override async Task<EventMonitoringStartResult> ExecuteOperationAsync(StartEventMonitoringRequest request)
        {
            _logger.LogInformation("Starting event monitoring - EventTypes: [{EventTypes}], AutomationId: {AutomationId}, Name: {Name}",
                string.Join(", ", request.EventTypes), request.AutomationId ?? "null", request.Name ?? "null");

            // Validate that the target element exists if element identification is provided
            if (!string.IsNullOrEmpty(request.AutomationId) || !string.IsNullOrEmpty(request.Name))
            {
                var searchCriteria = new ElementSearchCriteria
                {
                    AutomationId = request.AutomationId,
                    Name = request.Name,
                    ControlType = request.ControlType,
                    WindowTitle = request.WindowTitle,
                    WindowHandle = request.WindowHandle
                };
                var targetElement = _elementFinderService.FindElement(searchCriteria);

                if (targetElement == null)
                {
                    var error = $"Target element not found: AutomationId='{request.AutomationId}', Name='{request.Name}', ControlType='{request.ControlType}'";
                    _logger.LogWarning(error);

                    throw new InvalidOperationException(error);
                }

                _logger.LogDebug("Target element found for monitoring: {ElementName}", targetElement.Current.Name);
            }

            var sessionId = Guid.NewGuid().ToString("N")[..8];

            var session = _sessionManager.CreateSession(
                sessionId,
                request.EventTypes,
                request.AutomationId,
                request.Name,
                request.ControlType,
                request.WindowTitle,
                request.WindowHandle);

            await session.StartAsync();

            var result = new EventMonitoringStartResult
            {
                Success = true,
                EventType = string.Join(", ", request.EventTypes),
                ElementId = request.AutomationId ?? string.Empty,
                WindowTitle = request.WindowTitle ?? string.Empty,
                WindowHandle = request.WindowHandle,
                SessionId = sessionId,
                MonitoringStatus = "Started"
            };

            _logger.LogInformation("Event monitoring started successfully - SessionId: {SessionId}", sessionId);

            return result;
        }
    }
}

