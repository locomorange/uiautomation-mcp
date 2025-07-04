using Microsoft.Extensions.Logging;
using System.Windows.Automation;
using UiAutomationMcpServer.Models;
using UiAutomationMcpServer.Services.Windows;
using UiAutomationMcpServer.Services;

namespace UiAutomationMcpServer.Services.Patterns
{
    public interface ILayoutPatternService
    {
        Task<OperationResult> ExpandCollapseElementAsync(string elementId, bool? expand = null, string? windowTitle = null, int? processId = null);
        Task<OperationResult> ScrollElementAsync(string elementId, string? direction = null, double? horizontal = null, double? vertical = null, string? windowTitle = null, int? processId = null);
        Task<OperationResult> ScrollElementIntoViewAsync(string elementId, string? windowTitle = null, int? processId = null);
        Task<OperationResult> TransformElementAsync(string elementId, string action, double? x = null, double? y = null, double? width = null, double? height = null, double? degrees = null, string? windowTitle = null, int? processId = null);
        Task<OperationResult> DockElementAsync(string elementId, string position, string? windowTitle = null, int? processId = null);
    }

    public class LayoutPatternService : ILayoutPatternService
    {
        private readonly ILogger<LayoutPatternService> _logger;
        private readonly IWindowService _windowService;
        private readonly IUIAutomationHelper _uiAutomationHelper;

        public LayoutPatternService(ILogger<LayoutPatternService> logger, IWindowService windowService, IUIAutomationHelper uiAutomationHelper)
        {
            _logger = logger;
            _windowService = windowService;
            _uiAutomationHelper = uiAutomationHelper;
        }

        public async Task<OperationResult> ExpandCollapseElementAsync(string elementId, bool? expand = null, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var elementResult = await FindElementAsync(elementId, windowTitle, processId);
                if (!elementResult.Success || elementResult.Data == null)
                {
                    return new OperationResult { Success = false, Error = elementResult.Error ?? $"Element '{elementId}' not found" };
                }
                var element = elementResult.Data;

                if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var pattern) && pattern is ExpandCollapsePattern expandCollapsePattern)
                {
                    var currentState = expandCollapsePattern.Current.ExpandCollapseState;
                    
                    if (expand == null)
                    {
                        // Toggle behavior
                        if (currentState == ExpandCollapseState.Expanded)
                            expandCollapsePattern.Collapse();
                        else
                            expandCollapsePattern.Expand();
                    }
                    else if (expand.Value)
                    {
                        expandCollapsePattern.Expand();
                    }
                    else
                    {
                        expandCollapsePattern.Collapse();
                    }
                    
                    return new OperationResult { Success = true, Data = "Element expand/collapse executed successfully" };
                }
                
                return new OperationResult { Success = false, Error = "Element does not support ExpandCollapsePattern" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing expand/collapse on element {ElementId}", elementId);
                return new OperationResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<OperationResult> ScrollElementAsync(string elementId, string? direction = null, double? horizontal = null, double? vertical = null, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var elementResult = await FindElementAsync(elementId, windowTitle, processId);
                if (!elementResult.Success || elementResult.Data == null)
                {
                    return new OperationResult { Success = false, Error = elementResult.Error ?? $"Element '{elementId}' not found" };
                }
                var element = elementResult.Data;

                if (element.TryGetCurrentPattern(ScrollPattern.Pattern, out var pattern) && pattern is ScrollPattern scrollPattern)
                {
                    if (!string.IsNullOrEmpty(direction))
                    {
                        switch (direction.ToLower())
                        {
                            case "up":
                                scrollPattern.ScrollVertical(ScrollAmount.SmallDecrement);
                                break;
                            case "down":
                                scrollPattern.ScrollVertical(ScrollAmount.SmallIncrement);
                                break;
                            case "left":
                                scrollPattern.ScrollHorizontal(ScrollAmount.SmallDecrement);
                                break;
                            case "right":
                                scrollPattern.ScrollHorizontal(ScrollAmount.SmallIncrement);
                                break;
                        }
                    }
                    else
                    {
                        if (horizontal.HasValue)
                        {
                            scrollPattern.SetScrollPercent(horizontal.Value, ScrollPattern.NoScroll);
                        }
                        if (vertical.HasValue)
                        {
                            scrollPattern.SetScrollPercent(ScrollPattern.NoScroll, vertical.Value);
                        }
                    }
                    
                    return new OperationResult { Success = true, Data = "Element scrolled successfully" };
                }
                
                return new OperationResult { Success = false, Error = "Element does not support ScrollPattern" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scrolling element {ElementId}", elementId);
                return new OperationResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<OperationResult> ScrollElementIntoViewAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var elementResult = await FindElementAsync(elementId, windowTitle, processId);
                if (!elementResult.Success || elementResult.Data == null)
                {
                    return new OperationResult { Success = false, Error = elementResult.Error ?? $"Element '{elementId}' not found" };
                }
                var element = elementResult.Data;

                if (element.TryGetCurrentPattern(ScrollItemPattern.Pattern, out var pattern) && pattern is ScrollItemPattern scrollItemPattern)
                {
                    scrollItemPattern.ScrollIntoView();
                    return new OperationResult { Success = true, Data = "Element scrolled into view successfully" };
                }
                
                return new OperationResult { Success = false, Error = "Element does not support ScrollItemPattern" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scrolling element into view {ElementId}", elementId);
                return new OperationResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<OperationResult> TransformElementAsync(string elementId, string action, double? x = null, double? y = null, double? width = null, double? height = null, double? degrees = null, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var elementResult = await FindElementAsync(elementId, windowTitle, processId);
                if (!elementResult.Success || elementResult.Data == null)
                {
                    return new OperationResult { Success = false, Error = elementResult.Error ?? $"Element '{elementId}' not found" };
                }
                var element = elementResult.Data;

                if (element.TryGetCurrentPattern(TransformPattern.Pattern, out var pattern) && pattern is TransformPattern transformPattern)
                {
                    switch (action.ToLower())
                    {
                        case "move":
                            if (x.HasValue && y.HasValue)
                            {
                                transformPattern.Move(x.Value, y.Value);
                                return new OperationResult { Success = true, Data = "Element moved successfully" };
                            }
                            break;
                        case "resize":
                            if (width.HasValue && height.HasValue)
                            {
                                transformPattern.Resize(width.Value, height.Value);
                                return new OperationResult { Success = true, Data = "Element resized successfully" };
                            }
                            break;
                        case "rotate":
                            if (degrees.HasValue)
                            {
                                transformPattern.Rotate(degrees.Value);
                                return new OperationResult { Success = true, Data = "Element rotated successfully" };
                            }
                            break;
                    }
                    
                    return new OperationResult { Success = false, Error = $"Invalid parameters for transform action '{action}'" };
                }
                
                return new OperationResult { Success = false, Error = "Element does not support TransformPattern" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transforming element {ElementId}", elementId);
                return new OperationResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<OperationResult> DockElementAsync(string elementId, string position, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var elementResult = await FindElementAsync(elementId, windowTitle, processId);
                if (!elementResult.Success || elementResult.Data == null)
                {
                    return new OperationResult { Success = false, Error = elementResult.Error ?? $"Element '{elementId}' not found" };
                }
                var element = elementResult.Data;

                if (element.TryGetCurrentPattern(DockPattern.Pattern, out var pattern) && pattern is DockPattern dockPattern)
                {
                    var dockPosition = position.ToLower() switch
                    {
                        "top" => DockPosition.Top,
                        "bottom" => DockPosition.Bottom,
                        "left" => DockPosition.Left,
                        "right" => DockPosition.Right,
                        "fill" => DockPosition.Fill,
                        "none" => DockPosition.None,
                        _ => DockPosition.None
                    };
                    
                    dockPattern.SetDockPosition(dockPosition);
                    return new OperationResult { Success = true, Data = "Element docked successfully" };
                }
                
                return new OperationResult { Success = false, Error = "Element does not support DockPattern" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error docking element {ElementId}", elementId);
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