using Microsoft.Extensions.Logging;
using System.Windows.Automation;
using System.Windows.Automation.Text;
using UiAutomationMcpServer.Models;
using UiAutomationMcpServer.Services.Windows;
using UiAutomationMcpServer.Services;

namespace UiAutomationMcpServer.Services.Patterns
{
    public interface ITextPatternService
    {
        Task<OperationResult> GetTextAsync(string elementId, string? windowTitle = null, int? processId = null);
        Task<OperationResult> SelectTextAsync(string elementId, int startIndex, int length, string? windowTitle = null, int? processId = null);
        Task<OperationResult> FindTextAsync(string elementId, string searchText, bool backward = false, bool ignoreCase = false, string? windowTitle = null, int? processId = null);
        Task<OperationResult> GetTextSelectionAsync(string elementId, string? windowTitle = null, int? processId = null);
    }

    public class TextPatternService : ITextPatternService
    {
        private readonly ILogger<TextPatternService> _logger;
        private readonly IWindowService _windowService;
        private readonly IUIAutomationHelper _uiAutomationHelper;

        public TextPatternService(ILogger<TextPatternService> logger, IWindowService windowService, IUIAutomationHelper uiAutomationHelper)
        {
            _logger = logger;
            _windowService = windowService;
            _uiAutomationHelper = uiAutomationHelper;
        }

        public async Task<OperationResult> GetTextAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var elementResult = await FindElementAsync(elementId, windowTitle, processId);
                if (!elementResult.Success || elementResult.Data == null)
                {
                    return new OperationResult { Success = false, Error = elementResult.Error ?? $"Element '{elementId}' not found" };
                }
                var element = elementResult.Data;

                if (element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) && pattern is TextPattern textPattern)
                {
                    var documentRange = textPattern.DocumentRange;
                    var text = documentRange.GetText(-1);
                    
                    return new OperationResult { Success = true, Data = text };
                }
                
                return new OperationResult { Success = false, Error = "Element does not support TextPattern" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting text from element {ElementId}", elementId);
                return new OperationResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<OperationResult> SelectTextAsync(string elementId, int startIndex, int length, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var elementResult = await FindElementAsync(elementId, windowTitle, processId);
                if (!elementResult.Success || elementResult.Data == null)
                {
                    return new OperationResult { Success = false, Error = elementResult.Error ?? $"Element '{elementId}' not found" };
                }
                var element = elementResult.Data;

                if (element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) && pattern is TextPattern textPattern)
                {
                    var documentRange = textPattern.DocumentRange;
                    var selectionRange = documentRange.GetText(-1);
                    
                    if (startIndex < 0 || startIndex >= selectionRange.Length)
                    {
                        return new OperationResult { Success = false, Error = "Start index out of range" };
                    }
                    
                    if (startIndex + length > selectionRange.Length)
                    {
                        length = selectionRange.Length - startIndex;
                    }
                    
                    var textRange = documentRange.GetText(startIndex + length);
                    var rangeToSelect = documentRange.FindText(textRange.Substring(startIndex, length), false, false);
                    
                    if (rangeToSelect != null)
                    {
                        rangeToSelect.Select();
                        return new OperationResult { Success = true, Data = "Text selected successfully" };
                    }
                    
                    return new OperationResult { Success = false, Error = "Could not create text range for selection" };
                }
                
                return new OperationResult { Success = false, Error = "Element does not support TextPattern" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting text in element {ElementId}", elementId);
                return new OperationResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<OperationResult> FindTextAsync(string elementId, string searchText, bool backward = false, bool ignoreCase = false, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var elementResult = await FindElementAsync(elementId, windowTitle, processId);
                if (!elementResult.Success || elementResult.Data == null)
                {
                    return new OperationResult { Success = false, Error = elementResult.Error ?? $"Element '{elementId}' not found" };
                }
                var element = elementResult.Data;

                if (element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) && pattern is TextPattern textPattern)
                {
                    var documentRange = textPattern.DocumentRange;
                    var foundRange = documentRange.FindText(searchText, backward, ignoreCase);
                    
                    if (foundRange != null)
                    {
                        var boundingRectangles = foundRange.GetBoundingRectangles();
                        var result = new
                        {
                            Text = foundRange.GetText(-1),
                            BoundingRectangles = boundingRectangles,
                            Found = true
                        };
                        
                        return new OperationResult { Success = true, Data = result };
                    }
                    
                    return new OperationResult { Success = true, Data = new { Found = false } };
                }
                
                return new OperationResult { Success = false, Error = "Element does not support TextPattern" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding text in element {ElementId}", elementId);
                return new OperationResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<OperationResult> GetTextSelectionAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var elementResult = await FindElementAsync(elementId, windowTitle, processId);
                if (!elementResult.Success || elementResult.Data == null)
                {
                    return new OperationResult { Success = false, Error = elementResult.Error ?? $"Element '{elementId}' not found" };
                }
                var element = elementResult.Data;

                if (element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) && pattern is TextPattern textPattern)
                {
                    var selections = textPattern.GetSelection();
                    var selectionInfo = new List<object>();
                    
                    foreach (var selection in selections)
                    {
                        selectionInfo.Add(new
                        {
                            Text = selection.GetText(-1),
                            BoundingRectangles = selection.GetBoundingRectangles()
                        });
                    }
                    
                    return new OperationResult { Success = true, Data = selectionInfo };
                }
                
                return new OperationResult { Success = false, Error = "Element does not support TextPattern" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting text selection from element {ElementId}", elementId);
                return new OperationResult { Success = false, Error = ex.Message };
            }
        }

        private async Task<OperationResult<AutomationElement?>> FindElementAsync(string elementId, string? windowTitle, int? processId)
        {
            try
            {
                AutomationElement? searchRoot = null;
                if (!string.IsNullOrEmpty(windowTitle))
                {
                    searchRoot = _windowService.FindWindowByTitle(windowTitle, processId);
                    if (searchRoot == null)
                    {
                        _logger.LogWarning("Window '{WindowTitle}' not found", windowTitle);
                        return new OperationResult<AutomationElement?> { Success = false, Error = $"Window '{windowTitle}' not found" };
                    }
                }
                else
                {
                    searchRoot = AutomationElement.RootElement;
                }

                var condition = new OrCondition(
                    new PropertyCondition(AutomationElement.AutomationIdProperty, elementId),
                    new PropertyCondition(AutomationElement.NameProperty, elementId)
                );

                return await _uiAutomationHelper.FindFirstAsync(searchRoot, TreeScope.Descendants, condition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding element {ElementId}", elementId);
                return new OperationResult<AutomationElement?> { Success = false, Error = ex.Message };
            }
        }
    }
}