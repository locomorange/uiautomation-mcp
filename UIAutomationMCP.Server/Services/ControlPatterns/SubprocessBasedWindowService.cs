using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class SubprocessBasedWindowService : IWindowService
    {
        private readonly ILogger<SubprocessBasedWindowService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedWindowService(ILogger<SubprocessBasedWindowService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> WindowActionAsync(string action, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(action))
            {
                var validationError = "Window action is required and cannot be empty";
                _logger.LogWarning("WindowAction operation failed due to validation: {Error}", validationError);
                return new { Success = false, Error = validationError, ErrorCategory = "Validation" };
            }

            try
            {
                _logger.LogInformation("Performing window action: {Action} on window: {WindowTitle} (ProcessId: {ProcessId})", 
                    action, windowTitle ?? "any", processId ?? 0);

                var parameters = new Dictionary<string, object>
                {
                    { "action", action },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("WindowAction", parameters, timeoutSeconds);

                _logger.LogInformation("Window action performed successfully: {Action}", action);
                return new { 
                    Success = true, 
                    Message = $"Window action '{action}' performed successfully",
                    Action = action,
                    WindowTitle = windowTitle,
                    Operation = "WindowAction"
                };
            }
            catch (Exception ex)
            {
                return SubprocessErrorHandler.HandleError(ex, "WindowAction", windowTitle ?? "unknown", timeoutSeconds, _logger);
            }
        }

        public async Task<object> TransformElementAsync(string elementId, string action, double? x = null, double? y = null, double? width = null, double? height = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            // Input validation
            var validationResult = SubprocessErrorHandler.ValidateElementId(elementId, "TransformElement", _logger);
            if (validationResult != null) return validationResult;

            if (string.IsNullOrWhiteSpace(action))
            {
                var validationError = "Transform action is required and cannot be empty";
                _logger.LogWarning("TransformElement operation failed due to validation: {Error}", validationError);
                return new { Success = false, Error = validationError, ErrorCategory = "Validation" };
            }

            try
            {
                _logger.LogInformation("Transforming element: {ElementId} with action: {Action} (x:{X}, y:{Y}, w:{Width}, h:{Height})", 
                    elementId, action, x ?? 0, y ?? 0, width ?? 0, height ?? 0);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "action", action },
                    { "x", x ?? 0 },
                    { "y", y ?? 0 },
                    { "width", width ?? 0 },
                    { "height", height ?? 0 },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("TransformElement", parameters, timeoutSeconds);

                _logger.LogInformation("Element transformed successfully: {ElementId}", elementId);
                return new { 
                    Success = true, 
                    Message = $"Element '{elementId}' transformed with action '{action}' successfully",
                    ElementId = elementId,
                    Action = action,
                    Coordinates = new { X = x ?? 0, Y = y ?? 0, Width = width ?? 0, Height = height ?? 0 },
                    Operation = "TransformElement"
                };
            }
            catch (Exception ex)
            {
                return SubprocessErrorHandler.HandleError(ex, "TransformElement", elementId, timeoutSeconds, _logger);
            }
        }
    }
}