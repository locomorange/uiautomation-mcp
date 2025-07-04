using Microsoft.Extensions.Logging;
using System.Windows.Automation;
using UiAutomationMcpServer.Models;
using UiAutomationMcpServer.Services.Windows;
using UiAutomationMcpServer.Services;

namespace UiAutomationMcpServer.Services.Patterns
{
    public interface IWindowPatternService
    {
        Task<OperationResult> WindowActionAsync(string elementId, string action, string? windowTitle = null, int? processId = null);
    }

    public class WindowPatternService : IWindowPatternService
    {
        private readonly ILogger<WindowPatternService> _logger;
        private readonly IWindowService _windowService;
        private readonly IUIAutomationHelper _uiAutomationHelper;

        public WindowPatternService(ILogger<WindowPatternService> logger, IWindowService windowService, IUIAutomationHelper uiAutomationHelper)
        {
            _logger = logger;
            _windowService = windowService;
            _uiAutomationHelper = uiAutomationHelper;
        }

        public async Task<OperationResult> WindowActionAsync(string elementId, string action, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var elementResult = await FindElementAsync(elementId, windowTitle, processId);
                if (!elementResult.Success || elementResult.Data == null)
                {
                    return new OperationResult { Success = false, Error = elementResult.Error ?? $"Element '{elementId}' not found" };
                }
                var element = elementResult.Data;

                if (element.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern) && pattern is WindowPattern windowPattern)
                {
                    switch (action.ToLower())
                    {
                        case "close":
                            windowPattern.Close();
                            return new OperationResult { Success = true, Data = "Window closed successfully" };
                        case "minimize":
                            windowPattern.SetWindowVisualState(WindowVisualState.Minimized);
                            return new OperationResult { Success = true, Data = "Window minimized successfully" };
                        case "maximize":
                            windowPattern.SetWindowVisualState(WindowVisualState.Maximized);
                            return new OperationResult { Success = true, Data = "Window maximized successfully" };
                        case "normal":
                            windowPattern.SetWindowVisualState(WindowVisualState.Normal);
                            return new OperationResult { Success = true, Data = "Window set to normal state successfully" };
                        default:
                            return new OperationResult { Success = false, Error = $"Unknown window action: {action}" };
                    }
                }
                
                return new OperationResult { Success = false, Error = "Element does not support WindowPattern" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing window action {Action} on element {ElementId}", action, elementId);
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