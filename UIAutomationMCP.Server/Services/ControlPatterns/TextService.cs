using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class TextService : ITextService
    {
        private readonly ILogger<TextService> _logger;
        private readonly SubprocessExecutor _executor;

        public TextService(ILogger<TextService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> GetTextAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            // Input validation
            var validationResult = SubprocessErrorHandler.ValidateElementId(elementId, "GetText", _logger);
            if (validationResult != null) return validationResult;

            try
            {
                _logger.LogInformation("Getting text from element: {ElementId} in window: {WindowTitle} (ProcessId: {ProcessId})", 
                    elementId, windowTitle ?? "any", processId ?? 0);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetText", parameters, timeoutSeconds);

                _logger.LogInformation("Text retrieved successfully from element: {ElementId}", elementId);
                return new { 
                    Success = true, 
                    Data = result,
                    Message = $"Text retrieved from element '{elementId}'",
                    ElementId = elementId,
                    Operation = "GetText"
                };
            }
            catch (Exception ex)
            {
                return SubprocessErrorHandler.HandleError(ex, "GetText", elementId, timeoutSeconds, _logger);
            }
        }

        public async Task<object> SelectTextAsync(string elementId, int startIndex, int length, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Selecting text in element: {ElementId} from {StartIndex} length {Length}", elementId, startIndex, length);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "startIndex", startIndex },
                    { "length", length },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("SelectText", parameters, timeoutSeconds);

                _logger.LogInformation("Text selected successfully in element: {ElementId}", elementId);
                return new { Success = true, Message = "Text selected successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to select text in element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> FindTextAsync(string elementId, string searchText, bool backward = false, bool ignoreCase = true, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Finding text '{SearchText}' in element: {ElementId}", searchText, elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "searchText", searchText },
                    { "backward", backward },
                    { "ignoreCase", ignoreCase },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("FindText", parameters, timeoutSeconds);

                _logger.LogInformation("Text search completed in element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find text in element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetTextSelectionAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting text selection from element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var selections = await _executor.ExecuteAsync<List<object>>("GetTextSelection", parameters, timeoutSeconds);

                _logger.LogInformation("Text selection retrieved successfully from element: {ElementId}", elementId);
                return new { Success = true, Data = selections };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get text selection from element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> SetTextAsync(string elementId, string text, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            // Input validation
            var validationResult = SubprocessErrorHandler.ValidateElementId(elementId, "SetText", _logger);
            if (validationResult != null) return validationResult;

            if (text == null)
            {
                var validationError = "Text value is required and cannot be null";
                _logger.LogWarning("SetText operation failed due to validation: {Error}", validationError);
                return new { Success = false, Error = validationError, ErrorCategory = "Validation" };
            }

            try
            {
                _logger.LogInformation("Setting text in element: {ElementId} to: {TextLength} characters", elementId, text.Length);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "text", text },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("SetText", parameters, timeoutSeconds);

                _logger.LogInformation("Text set successfully in element: {ElementId}", elementId);
                return new { 
                    Success = true, 
                    Message = $"Text set successfully in element '{elementId}'",
                    ElementId = elementId,
                    Operation = "SetText",
                    TextLength = text.Length
                };
            }
            catch (Exception ex)
            {
                return SubprocessErrorHandler.HandleError(ex, "SetText", elementId, timeoutSeconds, _logger);
            }
        }

        public async Task<object> TraverseTextAsync(string elementId, string direction, int count = 1, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Traversing text in element: {ElementId}, direction: {Direction}, count: {Count}", elementId, direction, count);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "direction", direction },
                    { "count", count },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<List<object>>("TraverseText", parameters, timeoutSeconds);

                _logger.LogInformation("Text traversal completed in element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to traverse text in element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetTextAttributesAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting text attributes from element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<List<object>>("GetTextAttributes", parameters, timeoutSeconds);

                _logger.LogInformation("Text attributes retrieved successfully from element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get text attributes from element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> AppendTextAsync(string elementId, string text, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            // Input validation
            var validationResult = SubprocessErrorHandler.ValidateElementId(elementId, "AppendText", _logger);
            if (validationResult != null) return validationResult;

            if (text == null)
            {
                var validationError = "Text value is required and cannot be null";
                _logger.LogWarning("AppendText operation failed due to validation: {Error}", validationError);
                return new { Success = false, Error = validationError, ErrorCategory = "Validation" };
            }

            try
            {
                _logger.LogInformation("Appending text to element: {ElementId} with {TextLength} characters", elementId, text.Length);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "text", text },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("AppendText", parameters, timeoutSeconds);

                _logger.LogInformation("Text appended successfully to element: {ElementId}", elementId);
                return new { 
                    Success = true, 
                    Message = $"Text appended successfully to element '{elementId}'",
                    ElementId = elementId,
                    Operation = "AppendText",
                    TextLength = text.Length
                };
            }
            catch (Exception ex)
            {
                return SubprocessErrorHandler.HandleError(ex, "AppendText", elementId, timeoutSeconds, _logger);
            }
        }

        public async Task<object> GetSelectedTextAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            // Input validation
            var validationResult = SubprocessErrorHandler.ValidateElementId(elementId, "GetSelectedText", _logger);
            if (validationResult != null) return validationResult;

            try
            {
                _logger.LogInformation("Getting selected text from element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetSelectedText", parameters, timeoutSeconds);

                _logger.LogInformation("Selected text retrieved successfully from element: {ElementId}", elementId);
                return new { 
                    Success = true, 
                    Data = result,
                    Message = $"Selected text retrieved from element '{elementId}'",
                    ElementId = elementId,
                    Operation = "GetSelectedText"
                };
            }
            catch (Exception ex)
            {
                return SubprocessErrorHandler.HandleError(ex, "GetSelectedText", elementId, timeoutSeconds, _logger);
            }
        }
    }
}
