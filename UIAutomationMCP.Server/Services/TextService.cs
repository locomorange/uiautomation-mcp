using System.Windows.Automation;
using System.Windows.Automation.Text;
using Microsoft.Extensions.Logging;
using UiAutomationMcpServer.Helpers;

namespace UiAutomationMcpServer.Services
{
    public interface ITextService
    {
        Task<object> GetTextAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> SelectTextAsync(string elementId, int startIndex, int length, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> FindTextAsync(string elementId, string searchText, bool backward = false, bool ignoreCase = true, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetTextSelectionAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }

    public class TextService : ITextService
    {
        private readonly ILogger<TextService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;

        public TextService(ILogger<TextService> logger, UIAutomationExecutor executor, AutomationHelper automationHelper)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
        }

        public async Task<object> GetTextAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting text from element: {ElementId}", elementId);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                var text = await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) && pattern is TextPattern textPattern)
                    {
                        return textPattern.DocumentRange.GetText(-1);
                    }
                    else
                    {
                        // Fallback to Name property for elements without TextPattern
                        return element.Current.Name ?? "";
                    }
                }, timeoutSeconds, $"GetText_{elementId}");

                _logger.LogInformation("Text retrieved successfully from element: {ElementId}", elementId);
                return new { Success = true, Data = text };
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

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) && pattern is TextPattern textPattern)
                    {
                        var documentRange = textPattern.DocumentRange;
                        var fullText = documentRange.GetText(-1);
                        
                        if (startIndex < 0 || startIndex >= fullText.Length)
                        {
                            throw new ArgumentOutOfRangeException(nameof(startIndex), "Start index is out of range");
                        }
                        
                        if (startIndex + length > fullText.Length)
                        {
                            length = fullText.Length - startIndex;
                        }

                        // Create a range that covers the desired text selection
                        var rangeStart = documentRange.Clone();
                        rangeStart.Move(TextUnit.Character, startIndex);
                        var rangeEnd = rangeStart.Clone();
                        rangeEnd.Move(TextUnit.Character, length);
                        
                        rangeStart.MoveEndpointByRange(TextPatternRangeEndpoint.End, rangeEnd, TextPatternRangeEndpoint.Start);
                        rangeStart.Select();
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support TextPattern");
                    }
                }, timeoutSeconds, $"SelectText_{elementId}");

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

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                var result = await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) && pattern is TextPattern textPattern)
                    {
                        var documentRange = textPattern.DocumentRange;
                        var foundRange = documentRange.FindText(searchText, backward, ignoreCase);
                        
                        if (foundRange != null)
                        {
                            var foundText = foundRange.GetText(-1);
                            return (object)new
                            {
                                Found = true,
                                Text = foundText,
                                BoundingRectangle = foundRange.GetBoundingRectangles()
                            };
                        }
                        else
                        {
                            return (object)new { Found = false };
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support TextPattern");
                    }
                }, timeoutSeconds, $"FindText_{elementId}");

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

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                var selections = await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) && pattern is TextPattern textPattern)
                    {
                        var selectionRanges = textPattern.GetSelection();
                        var selectionInfo = new List<object>();

                        foreach (var range in selectionRanges)
                        {
                            selectionInfo.Add(new
                            {
                                Text = range.GetText(-1),
                                BoundingRectangle = range.GetBoundingRectangles()
                            });
                        }

                        return (object)selectionInfo;
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support TextPattern");
                    }
                }, timeoutSeconds, $"GetTextSelection_{elementId}");

                _logger.LogInformation("Text selection retrieved successfully from element: {ElementId}", elementId);
                return new { Success = true, Data = selections };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get text selection from element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}