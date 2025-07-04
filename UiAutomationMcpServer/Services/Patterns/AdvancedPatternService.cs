using Microsoft.Extensions.Logging;
using System.Windows.Automation;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.Services.Windows;
using UiAutomationMcpServer.Services;

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
        private readonly IUIAutomationWorker _uiAutomationWorker;

        public AdvancedPatternService(ILogger<AdvancedPatternService> logger, IWindowService windowService, IUIAutomationWorker uiAutomationWorker)
        {
            _logger = logger;
            _windowService = windowService;
            _uiAutomationWorker = uiAutomationWorker;
        }

        public async Task<OperationResult> ChangeViewAsync(string elementId, int viewId, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var elementResult = await FindElementAsync(elementId, windowTitle, processId);
                if (!elementResult.Success || elementResult.Data == null)
                {
                    return new OperationResult { Success = false, Error = elementResult.Error ?? $"Element '{elementId}' not found" };
                }
                var element = elementResult.Data;

                if (element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out var pattern) && pattern is MultipleViewPattern multipleViewPattern)
                {
                    multipleViewPattern.SetCurrentView(viewId);
                    return new OperationResult { Success = true, Data = "View changed successfully" };
                }
                
                return new OperationResult { Success = false, Error = "Element does not support MultipleViewPattern" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing view on element {ElementId}", elementId);
                return new OperationResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<OperationResult> RealizeVirtualizedItemAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var elementResult = await FindElementAsync(elementId, windowTitle, processId);
                if (!elementResult.Success || elementResult.Data == null)
                {
                    return new OperationResult { Success = false, Error = elementResult.Error ?? $"Element '{elementId}' not found" };
                }
                var element = elementResult.Data;

                if (element.TryGetCurrentPattern(VirtualizedItemPattern.Pattern, out var pattern) && pattern is VirtualizedItemPattern virtualizedItemPattern)
                {
                    virtualizedItemPattern.Realize();
                    return new OperationResult { Success = true, Data = "Virtualized item realized successfully" };
                }
                
                return new OperationResult { Success = false, Error = "Element does not support VirtualizedItemPattern" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error realizing virtualized item {ElementId}", elementId);
                return new OperationResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<OperationResult> FindItemInContainerAsync(string elementId, string findText, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var elementResult = await FindElementAsync(elementId, windowTitle, processId);
                if (!elementResult.Success || elementResult.Data == null)
                {
                    return new OperationResult { Success = false, Error = elementResult.Error ?? $"Element '{elementId}' not found" };
                }
                var element = elementResult.Data;

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
                        
                        return new OperationResult { Success = true, Data = itemInfo };
                    }
                    
                    return new OperationResult { Success = false, Error = $"Item '{findText}' not found in container" };
                }
                
                return new OperationResult { Success = false, Error = "Element does not support ItemContainerPattern" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding item in container {ElementId}", elementId);
                return new OperationResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<OperationResult> CancelSynchronizedInputAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var elementResult = await FindElementAsync(elementId, windowTitle, processId);
                if (!elementResult.Success || elementResult.Data == null)
                {
                    return new OperationResult { Success = false, Error = elementResult.Error ?? $"Element '{elementId}' not found" };
                }
                var element = elementResult.Data;

                if (element.TryGetCurrentPattern(SynchronizedInputPattern.Pattern, out var pattern) && pattern is SynchronizedInputPattern synchronizedInputPattern)
                {
                    synchronizedInputPattern.Cancel();
                    return new OperationResult { Success = true, Data = "Synchronized input cancelled successfully" };
                }
                
                return new OperationResult { Success = false, Error = "Element does not support SynchronizedInputPattern" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling synchronized input on element {ElementId}", elementId);
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

                // 暫定的に直接AutomationAPIを使用（理想的にはWorkerを使用したい）
                var result = await Task.Run(() => searchRoot.FindFirst(TreeScope.Descendants, condition));
                return new OperationResult<AutomationElement?> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding element {ElementId}", elementId);
                return new OperationResult<AutomationElement?> { Success = false, Error = ex.Message };
            }
        }
    }
}