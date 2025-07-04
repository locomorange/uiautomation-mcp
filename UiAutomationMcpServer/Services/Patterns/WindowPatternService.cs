using Microsoft.Extensions.Logging;
using System.Windows.Automation;
using UiAutomationMcpServer.Models;
using UiAutomationMcpServer.Services.Windows;

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

        public WindowPatternService(ILogger<WindowPatternService> logger, IWindowService windowService)
        {
            _logger = logger;
            _windowService = windowService;
        }

        public Task<OperationResult> WindowActionAsync(string elementId, string action, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var element = FindElement(elementId, windowTitle, processId);
                if (element == null)
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });
                }

                if (element.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern) && pattern is WindowPattern windowPattern)
                {
                    switch (action.ToLower())
                    {
                        case "close":
                            windowPattern.Close();
                            return Task.FromResult(new OperationResult { Success = true, Data = "Window closed successfully" });
                        case "minimize":
                            windowPattern.SetWindowVisualState(WindowVisualState.Minimized);
                            return Task.FromResult(new OperationResult { Success = true, Data = "Window minimized successfully" });
                        case "maximize":
                            windowPattern.SetWindowVisualState(WindowVisualState.Maximized);
                            return Task.FromResult(new OperationResult { Success = true, Data = "Window maximized successfully" });
                        case "normal":
                            windowPattern.SetWindowVisualState(WindowVisualState.Normal);
                            return Task.FromResult(new OperationResult { Success = true, Data = "Window set to normal state successfully" });
                        default:
                            return Task.FromResult(new OperationResult { Success = false, Error = $"Unknown window action: {action}" });
                    }
                }
                
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support WindowPattern" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing window action {Action} on element {ElementId}", action, elementId);
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