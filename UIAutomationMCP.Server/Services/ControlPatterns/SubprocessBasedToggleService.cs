using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class SubprocessBasedToggleService : IToggleService
    {
        private readonly ILogger<SubprocessBasedToggleService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedToggleService(ILogger<SubprocessBasedToggleService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> ToggleElementAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            // Input validation
            var validationResult = SubprocessErrorHandler.ValidateElementId(elementId, "ToggleElement", _logger);
            if (validationResult != null) return validationResult;

            try
            {
                _logger.LogInformation("Toggling element: {ElementId} in window: {WindowTitle} (ProcessId: {ProcessId})", 
                    elementId, windowTitle ?? "any", processId ?? 0);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("ToggleElement", parameters, timeoutSeconds);

                _logger.LogInformation("Element toggled successfully: {ElementId}", elementId);
                return new { 
                    Success = true, 
                    Message = $"Element '{elementId}' toggled successfully", 
                    Data = result,
                    ElementId = elementId,
                    Operation = "ToggleElement"
                };
            }
            catch (Exception ex)
            {
                return SubprocessErrorHandler.HandleError(ex, "ToggleElement", elementId, timeoutSeconds, _logger);
            }
        }

        public async Task<object> GetToggleStateAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            // Input validation
            var validationResult = SubprocessErrorHandler.ValidateElementId(elementId, "GetToggleState", _logger);
            if (validationResult != null) return validationResult;

            try
            {
                _logger.LogInformation("Getting toggle state from element: {ElementId} in window: {WindowTitle} (ProcessId: {ProcessId})", 
                    elementId, windowTitle ?? "any", processId ?? 0);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetToggleState", parameters, timeoutSeconds);

                _logger.LogInformation("Toggle state retrieved successfully from element: {ElementId}", elementId);
                return new { 
                    Success = true, 
                    Data = result,
                    Message = $"Toggle state retrieved from element '{elementId}'",
                    ElementId = elementId,
                    Operation = "GetToggleState"
                };
            }
            catch (Exception ex)
            {
                return SubprocessErrorHandler.HandleError(ex, "GetToggleState", elementId, timeoutSeconds, _logger);
            }
        }

        public async Task<object> SetToggleStateAsync(string elementId, string toggleState, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            // Input validation
            var validationResult = SubprocessErrorHandler.ValidateElementId(elementId, "SetToggleState", _logger);
            if (validationResult != null) return validationResult;

            if (string.IsNullOrWhiteSpace(toggleState))
            {
                var validationError = "Toggle state is required and cannot be empty";
                _logger.LogWarning("SetToggleState operation failed due to validation: {Error}", validationError);
                return new { Success = false, Error = validationError, ErrorCategory = "Validation" };
            }

            try
            {
                _logger.LogInformation("Setting toggle state in element: {ElementId} to: {ToggleState} in window: {WindowTitle} (ProcessId: {ProcessId})", 
                    elementId, toggleState, windowTitle ?? "any", processId ?? 0);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "toggleState", toggleState },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("SetToggleState", parameters, timeoutSeconds);

                _logger.LogInformation("Toggle state set successfully in element: {ElementId}", elementId);
                return new { 
                    Success = true, 
                    Message = $"Toggle state set to '{toggleState}' in element '{elementId}'",
                    ElementId = elementId,
                    ToggleState = toggleState,
                    Operation = "SetToggleState"
                };
            }
            catch (Exception ex)
            {
                return SubprocessErrorHandler.HandleError(ex, "SetToggleState", elementId, timeoutSeconds, _logger);
            }
        }
    }
}