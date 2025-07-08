using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class SubprocessBasedTextService : ITextService
    {
        private readonly ILogger<SubprocessBasedTextService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedTextService(ILogger<SubprocessBasedTextService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> GetTextAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting text from element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetText", parameters, timeoutSeconds);

                _logger.LogInformation("Text retrieved successfully from element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get text from element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
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
            try
            {
                _logger.LogInformation("Setting text in element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "text", text },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("SetText", parameters, timeoutSeconds);

                _logger.LogInformation("Text set successfully in element: {ElementId}", elementId);
                return new { Success = true, Message = "Text set successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set text in element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
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
    }
}