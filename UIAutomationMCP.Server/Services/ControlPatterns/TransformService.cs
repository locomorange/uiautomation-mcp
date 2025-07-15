using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class TransformService : ITransformService
    {
        private readonly ILogger<TransformService> _logger;
        private readonly SubprocessExecutor _executor;

        public TransformService(ILogger<TransformService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<OperationResult> GetTransformCapabilitiesAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var validationResult = SubprocessErrorHandler.ValidateElementId(elementId, "GetTransformCapabilities", _logger);
            if (validationResult != null) 
                return new OperationResult { Success = false, Error = validationResult.ToString() };

            try
            {
                _logger.LogInformation("Getting transform capabilities for element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetTransformCapabilities", parameters, timeoutSeconds);

                _logger.LogInformation("Transform capabilities retrieved successfully for element: {ElementId}", elementId);
                return new OperationResult 
                { 
                    Success = true, 
                    Data = result
                };
            }
            catch (Exception ex)
            {
                var errorResult = SubprocessErrorHandler.HandleError(ex, "GetTransformCapabilities", elementId, timeoutSeconds, _logger);
                return new OperationResult { Success = false, Error = errorResult.ToString() };
            }
        }

        public async Task<OperationResult> MoveElementAsync(string elementId, double x, double y, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var validationResult = SubprocessErrorHandler.ValidateElementId(elementId, "MoveElement", _logger);
            if (validationResult != null) 
                return new OperationResult { Success = false, Error = validationResult.ToString() };

            try
            {
                _logger.LogInformation("Moving element: {ElementId} to position: ({X}, {Y})", elementId, x, y);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "x", x },
                    { "y", y },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("MoveElement", parameters, timeoutSeconds);

                _logger.LogInformation("Element moved successfully: {ElementId} to ({X}, {Y})", elementId, x, y);
                return new OperationResult 
                { 
                    Success = true, 
                    Data = new { ElementId = elementId, X = x, Y = y, Operation = "Move" }
                };
            }
            catch (Exception ex)
            {
                var errorResult = SubprocessErrorHandler.HandleError(ex, "MoveElement", elementId, timeoutSeconds, _logger);
                return new OperationResult { Success = false, Error = errorResult.ToString() };
            }
        }

        public async Task<OperationResult> ResizeElementAsync(string elementId, double width, double height, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var validationResult = SubprocessErrorHandler.ValidateElementId(elementId, "ResizeElement", _logger);
            if (validationResult != null) 
                return new OperationResult { Success = false, Error = validationResult.ToString() };

            try
            {
                _logger.LogInformation("Resizing element: {ElementId} to size: ({Width}, {Height})", elementId, width, height);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "width", width },
                    { "height", height },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("ResizeElement", parameters, timeoutSeconds);

                _logger.LogInformation("Element resized successfully: {ElementId} to ({Width}, {Height})", elementId, width, height);
                return new OperationResult 
                { 
                    Success = true, 
                    Data = new { ElementId = elementId, Width = width, Height = height, Operation = "Resize" }
                };
            }
            catch (Exception ex)
            {
                var errorResult = SubprocessErrorHandler.HandleError(ex, "ResizeElement", elementId, timeoutSeconds, _logger);
                return new OperationResult { Success = false, Error = errorResult.ToString() };
            }
        }

        public async Task<OperationResult> RotateElementAsync(string elementId, double degrees, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var validationResult = SubprocessErrorHandler.ValidateElementId(elementId, "RotateElement", _logger);
            if (validationResult != null) 
                return new OperationResult { Success = false, Error = validationResult.ToString() };

            try
            {
                _logger.LogInformation("Rotating element: {ElementId} by {Degrees} degrees", elementId, degrees);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "degrees", degrees },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("RotateElement", parameters, timeoutSeconds);

                _logger.LogInformation("Element rotated successfully: {ElementId} by {Degrees} degrees", elementId, degrees);
                return new OperationResult 
                { 
                    Success = true, 
                    Data = new { ElementId = elementId, Degrees = degrees, Operation = "Rotate" }
                };
            }
            catch (Exception ex)
            {
                var errorResult = SubprocessErrorHandler.HandleError(ex, "RotateElement", elementId, timeoutSeconds, _logger);
                return new OperationResult { Success = false, Error = errorResult.ToString() };
            }
        }
    }
}