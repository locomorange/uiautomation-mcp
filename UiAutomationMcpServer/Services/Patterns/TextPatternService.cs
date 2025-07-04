using Microsoft.Extensions.Logging;
using System.Windows.Automation;
using System.Windows.Automation.Text;
using UiAutomationMcpServer.Models;
using UiAutomationMcpServer.Services.Windows;

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

        public TextPatternService(ILogger<TextPatternService> logger, IWindowService windowService)
        {
            _logger = logger;
            _windowService = windowService;
        }

        public Task<OperationResult> GetTextAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var element = FindElement(elementId, windowTitle, processId);
                if (element == null)
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });
                }

                if (element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) && pattern is TextPattern textPattern)
                {
                    var documentRange = textPattern.DocumentRange;
                    var text = documentRange.GetText(-1);
                    
                    return Task.FromResult(new OperationResult { Success = true, Data = text });
                }
                
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support TextPattern" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting text from element {ElementId}", elementId);
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        public Task<OperationResult> SelectTextAsync(string elementId, int startIndex, int length, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var element = FindElement(elementId, windowTitle, processId);
                if (element == null)
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });
                }

                if (element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) && pattern is TextPattern textPattern)
                {
                    var documentRange = textPattern.DocumentRange;
                    var selectionRange = documentRange.GetText(-1);
                    
                    if (startIndex < 0 || startIndex >= selectionRange.Length)
                    {
                        return Task.FromResult(new OperationResult { Success = false, Error = "Start index out of range" });
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
                        return Task.FromResult(new OperationResult { Success = true, Data = "Text selected successfully" });
                    }
                    
                    return Task.FromResult(new OperationResult { Success = false, Error = "Could not create text range for selection" });
                }
                
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support TextPattern" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting text in element {ElementId}", elementId);
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        public Task<OperationResult> FindTextAsync(string elementId, string searchText, bool backward = false, bool ignoreCase = false, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var element = FindElement(elementId, windowTitle, processId);
                if (element == null)
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });
                }

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
                        
                        return Task.FromResult(new OperationResult { Success = true, Data = result });
                    }
                    
                    return Task.FromResult(new OperationResult { Success = true, Data = new { Found = false } });
                }
                
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support TextPattern" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding text in element {ElementId}", elementId);
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        public Task<OperationResult> GetTextSelectionAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var element = FindElement(elementId, windowTitle, processId);
                if (element == null)
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });
                }

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
                    
                    return Task.FromResult(new OperationResult { Success = true, Data = selectionInfo });
                }
                
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support TextPattern" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting text selection from element {ElementId}", elementId);
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        private AutomationElement? FindElement(string elementId, string? windowTitle, int? processId)
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
                        return null;
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

                return searchRoot.FindFirst(TreeScope.Descendants, condition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding element {ElementId}", elementId);
                return null;
            }
        }
    }
}