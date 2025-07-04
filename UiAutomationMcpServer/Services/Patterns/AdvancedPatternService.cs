using Microsoft.Extensions.Logging;
using System.Windows.Automation;
using UiAutomationMcpServer.Models;
using UiAutomationMcpServer.Services.Windows;

namespace UiAutomationMcpServer.Services.Patterns
{
    public interface IAdvancedPatternService
    {
        Task<OperationResult> ChangeViewAsync(string elementId, int viewId, string? windowTitle = null, int? processId = null);
        Task<OperationResult> RealizeVirtualizedItemAsync(string elementId, string? windowTitle = null, int? processId = null);
        Task<OperationResult> FindItemInContainerAsync(string elementId, string findText, string? windowTitle = null, int? processId = null);
        Task<OperationResult> CancelSynchronizedInputAsync(string elementId, string? windowTitle = null, int? processId = null);
    }

    public class AdvancedPatternService : IAdvancedPatternService
    {
        private readonly ILogger<AdvancedPatternService> _logger;
        private readonly IWindowService _windowService;

        public AdvancedPatternService(ILogger<AdvancedPatternService> logger, IWindowService windowService)
        {
            _logger = logger;
            _windowService = windowService;
        }

        public Task<OperationResult> ChangeViewAsync(string elementId, int viewId, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var element = FindElement(elementId, windowTitle, processId);
                if (element == null)
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });
                }

                if (element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out var pattern) && pattern is MultipleViewPattern multipleViewPattern)
                {
                    multipleViewPattern.SetCurrentView(viewId);
                    return Task.FromResult(new OperationResult { Success = true, Data = "View changed successfully" });
                }
                
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support MultipleViewPattern" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing view on element {ElementId}", elementId);
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        public Task<OperationResult> RealizeVirtualizedItemAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var element = FindElement(elementId, windowTitle, processId);
                if (element == null)
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });
                }

                if (element.TryGetCurrentPattern(VirtualizedItemPattern.Pattern, out var pattern) && pattern is VirtualizedItemPattern virtualizedItemPattern)
                {
                    virtualizedItemPattern.Realize();
                    return Task.FromResult(new OperationResult { Success = true, Data = "Virtualized item realized successfully" });
                }
                
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support VirtualizedItemPattern" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error realizing virtualized item {ElementId}", elementId);
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        public Task<OperationResult> FindItemInContainerAsync(string elementId, string findText, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var element = FindElement(elementId, windowTitle, processId);
                if (element == null)
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });
                }

                if (element.TryGetCurrentPattern(ItemContainerPattern.Pattern, out var pattern) && pattern is ItemContainerPattern itemContainerPattern)
                {
                    var nameProperty = AutomationElement.NameProperty;
                    var foundItem = itemContainerPattern.FindItemByProperty(null, nameProperty, findText);
                    
                    if (foundItem != null)
                    {
                        var itemInfo = new
                        {
                            Name = foundItem.Current.Name,
                            AutomationId = foundItem.Current.AutomationId,
                            ControlType = foundItem.Current.ControlType.ProgrammaticName,
                            IsEnabled = foundItem.Current.IsEnabled,
                            BoundingRectangle = foundItem.Current.BoundingRectangle
                        };
                        
                        return Task.FromResult(new OperationResult { Success = true, Data = itemInfo });
                    }
                    
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Item '{findText}' not found in container" });
                }
                
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support ItemContainerPattern" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding item in container {ElementId}", elementId);
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        public Task<OperationResult> CancelSynchronizedInputAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var element = FindElement(elementId, windowTitle, processId);
                if (element == null)
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });
                }

                if (element.TryGetCurrentPattern(SynchronizedInputPattern.Pattern, out var pattern) && pattern is SynchronizedInputPattern synchronizedInputPattern)
                {
                    synchronizedInputPattern.Cancel();
                    return Task.FromResult(new OperationResult { Success = true, Data = "Synchronized input cancelled successfully" });
                }
                
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support SynchronizedInputPattern" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling synchronized input on element {ElementId}", elementId);
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