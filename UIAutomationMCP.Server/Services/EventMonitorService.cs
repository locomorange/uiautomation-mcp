using UIAutomationMCP.Models.Abstractions;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Server.Abstractions;
using UIAutomationMCP.Core.Abstractions;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Validation;

namespace UIAutomationMCP.Server.Services
{
    public class EventMonitorService : BaseUIAutomationService<EventMonitorServiceMetadata>, IEventMonitorService
    {
        public EventMonitorService(IProcessManager processManager, ILogger<EventMonitorService> logger)
            : base(processManager, logger)
        {
        }

        protected override string GetOperationType() => "eventMonitor";


        public async Task<ServerEnhancedResponse<EventMonitoringStartResult>> StartEventMonitoringAsync(
            string eventType, 
            string? automationId = null, 
            string? name = null, 
            string? controlType = null, 
            long? windowHandle = null, 
            int timeoutSeconds = 60)
        {
            var request = new StartEventMonitoringRequest
            {
                EventTypes = new[] { eventType },
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                WindowTitle = null,
                WindowHandle = windowHandle
            };

            return await ExecuteMonitorServiceOperationAsync<StartEventMonitoringRequest, EventMonitoringStartResult>(
                "StartEventMonitoring",
                request,
                nameof(StartEventMonitoringAsync),
                timeoutSeconds,
                ValidateStartEventMonitoringRequest
            );
        }

        public async Task<ServerEnhancedResponse<EventMonitoringStopResult>> StopEventMonitoringAsync(
            string? sessionId = null, 
            int timeoutSeconds = 60)
        {
            var request = new StopEventMonitoringRequest
            {
                MonitorId = sessionId ?? ""
            };

            return await ExecuteMonitorServiceOperationAsync<StopEventMonitoringRequest, EventMonitoringStopResult>(
                "StopEventMonitoring",
                request,
                nameof(StopEventMonitoringAsync),
                timeoutSeconds,
                ValidateStopEventMonitoringRequest
            );
        }

        public async Task<ServerEnhancedResponse<EventLogResult>> GetEventLogAsync(
            string? sessionId = null, 
            int maxCount = 100, 
            int timeoutSeconds = 60)
        {
            var request = new GetEventLogRequest
            {
                MonitorId = sessionId ?? "",
                MaxCount = maxCount
            };

            return await ExecuteMonitorServiceOperationAsync<GetEventLogRequest, EventLogResult>(
                "GetEventLog",
                request,
                nameof(GetEventLogAsync),
                timeoutSeconds,
                ValidateGetEventLogRequest
            );
        }


        private static ValidationResult ValidateStartEventMonitoringRequest(StartEventMonitoringRequest request)
        {
            if (request.EventTypes == null || request.EventTypes.Length == 0 || 
                string.IsNullOrWhiteSpace(request.EventTypes[0]))
            {
                return ValidationResult.Failure("Event type is required and cannot be empty");
            }

            return ValidationResult.Success;
        }

        private static ValidationResult ValidateStopEventMonitoringRequest(StopEventMonitoringRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MonitorId))
            {
                return ValidationResult.Failure("Monitor ID (session ID) is required to stop monitoring");
            }

            return ValidationResult.Success;
        }

        private static ValidationResult ValidateGetEventLogRequest(GetEventLogRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.MonitorId))
            {
                errors.Add("Monitor ID (session ID) is required to retrieve event log");
            }

            if (request.MaxCount <= 0)
            {
                errors.Add("Max count must be greater than 0");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        protected override EventMonitorServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);

            if (data is EventMonitoringStartResult startResult)
            {
                metadata.ActionPerformed = "monitoringStarted";
                metadata.SessionId = startResult.SessionId;
                metadata.EventType = startResult.EventType;
                metadata.MonitoringActive = startResult.Success;
                metadata.OperationSuccessful = startResult.Success;
            }
            else if (data is EventMonitoringStopResult stopResult)
            {
                metadata.ActionPerformed = "monitoringStopped";
                metadata.SessionId = stopResult.SessionId;
                metadata.EventsCount = stopResult.FinalEventCount;
                metadata.MonitoringActive = false;
                metadata.OperationSuccessful = stopResult.Success;
            }
            else if (data is EventLogResult logResult)
            {
                metadata.ActionPerformed = "eventLogRetrieved";
                metadata.SessionId = logResult.MonitorId;
                metadata.EventsCount = logResult.Events?.Count ?? 0;
                metadata.MonitoringActive = logResult.SessionActive;
                metadata.OperationSuccessful = logResult.Success;
            }

            return metadata;
        }
    }
}
