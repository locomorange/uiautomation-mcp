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

        public async Task<object> WindowOperationAsync(string operation, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(operation))
            {
                var validationError = "Window operation is required and cannot be empty";
                _logger.LogWarning("WindowAction operation failed due to validation: {Error}", validationError);
                return new { Success = false, Error = validationError, ErrorCategory = "Validation" };
            }

            try
            {
                _logger.LogInformation("Performing window operation: {Operation} on window: {WindowTitle} (ProcessId: {ProcessId})", 
                    operation, windowTitle ?? "any", processId ?? 0);

                var parameters = new Dictionary<string, object>
                {
                    { "action", operation },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("WindowAction", parameters, timeoutSeconds);

                _logger.LogInformation("Window operation performed successfully: {Operation}", operation);
                return new { 
                    Success = true, 
                    Message = $"Window operation '{operation}' performed successfully",
                    Operation = operation,
                    WindowTitle = windowTitle
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

        public async Task<object> GetWindowStateAsync(string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting window state for window: {WindowTitle} (ProcessId: {ProcessId})", 
                    windowTitle ?? "any", processId ?? 0);

                var parameters = new Dictionary<string, object>
                {
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetWindowState", parameters, timeoutSeconds);

                _logger.LogInformation("Window state retrieved successfully for window: {WindowTitle}", windowTitle ?? "any");
                return new { 
                    Success = true, 
                    Data = result,
                    Message = $"Window state retrieved successfully",
                    WindowTitle = windowTitle,
                    Operation = "GetWindowState"
                };
            }
            catch (Exception ex)
            {
                return SubprocessErrorHandler.HandleError(ex, "GetWindowState", windowTitle ?? "unknown", timeoutSeconds, _logger);
            }
        }

        public async Task<object> SetWindowStateAsync(string windowState, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(windowState))
            {
                var validationError = "Window state is required and cannot be empty";
                _logger.LogWarning("SetWindowState operation failed due to validation: {Error}", validationError);
                return new { Success = false, Error = validationError, ErrorCategory = "Validation" };
            }

            try
            {
                _logger.LogInformation("Setting window state to: {WindowState} for window: {WindowTitle} (ProcessId: {ProcessId})", 
                    windowState, windowTitle ?? "any", processId ?? 0);

                var parameters = new Dictionary<string, object>
                {
                    { "windowState", windowState },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("SetWindowState", parameters, timeoutSeconds);

                _logger.LogInformation("Window state set successfully to: {WindowState}", windowState);
                return new { 
                    Success = true, 
                    Message = $"Window state set to '{windowState}' successfully",
                    WindowState = windowState,
                    WindowTitle = windowTitle,
                    Operation = "SetWindowState"
                };
            }
            catch (Exception ex)
            {
                return SubprocessErrorHandler.HandleError(ex, "SetWindowState", windowTitle ?? "unknown", timeoutSeconds, _logger);
            }
        }

        public async Task<object> MoveWindowAsync(int x, int y, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Moving window to position: ({X}, {Y}) for window: {WindowTitle} (ProcessId: {ProcessId})", 
                    x, y, windowTitle ?? "any", processId ?? 0);

                var parameters = new Dictionary<string, object>
                {
                    { "x", x },
                    { "y", y },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("MoveWindow", parameters, timeoutSeconds);

                _logger.LogInformation("Window moved successfully to position: ({X}, {Y})", x, y);
                return new { 
                    Success = true, 
                    Message = $"Window moved to position ({x}, {y}) successfully",
                    Position = new { X = x, Y = y },
                    WindowTitle = windowTitle,
                    Operation = "MoveWindow"
                };
            }
            catch (Exception ex)
            {
                return SubprocessErrorHandler.HandleError(ex, "MoveWindow", windowTitle ?? "unknown", timeoutSeconds, _logger);
            }
        }

        public async Task<object> ResizeWindowAsync(int width, int height, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Resizing window to size: ({Width}, {Height}) for window: {WindowTitle} (ProcessId: {ProcessId})", 
                    width, height, windowTitle ?? "any", processId ?? 0);

                var parameters = new Dictionary<string, object>
                {
                    { "width", width },
                    { "height", height },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("ResizeWindow", parameters, timeoutSeconds);

                _logger.LogInformation("Window resized successfully to size: ({Width}, {Height})", width, height);
                return new { 
                    Success = true, 
                    Message = $"Window resized to size ({width}, {height}) successfully",
                    Size = new { Width = width, Height = height },
                    WindowTitle = windowTitle,
                    Operation = "ResizeWindow"
                };
            }
            catch (Exception ex)
            {
                return SubprocessErrorHandler.HandleError(ex, "ResizeWindow", windowTitle ?? "unknown", timeoutSeconds, _logger);
            }
        }
    }
}
