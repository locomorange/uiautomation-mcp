using Microsoft.Extensions.Logging;
using System.Windows.Automation;
using UiAutomationMcp.Models;
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
        private readonly IUIAutomationWorker _uiAutomationWorker;

        public WindowPatternService(ILogger<WindowPatternService> logger, IWindowService windowService, IUIAutomationWorker uiAutomationWorker)
        {
            _logger = logger;
            _windowService = windowService;
            _uiAutomationWorker = uiAutomationWorker;
        }

        public async Task<OperationResult> WindowActionAsync(string elementId, string action, string? windowTitle = null, int? processId = null)
        {
            try
            {
                _logger.LogInformation("WindowAction called - ElementId: {ElementId}, Action: {Action}, WindowTitle: {WindowTitle}, ProcessId: {ProcessId}", 
                    elementId, action, windowTitle, processId);

                var elementResult = await FindElementAsync(elementId, windowTitle, processId);
                if (!elementResult.Success || elementResult.Data == null)
                {
                    _logger.LogError("Element not found - ElementId: {ElementId}, Error: {Error}", elementId, elementResult.Error);
                    return new OperationResult { Success = false, Error = elementResult.Error ?? $"Element '{elementId}' not found" };
                }
                var element = elementResult.Data;

                _logger.LogInformation("Element found - Name: {Name}, ControlType: {ControlType}, IsEnabled: {IsEnabled}, ProcessId: {ProcessId}", 
                    element.Current.Name, element.Current.ControlType.ProgrammaticName, element.Current.IsEnabled, element.Current.ProcessId);

                if (element.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern) && pattern is WindowPattern windowPattern)
                {
                    _logger.LogInformation("WindowPattern found - Current state: {VisualState}, Interaction state: {InteractionState}", 
                        windowPattern.Current.WindowVisualState, windowPattern.Current.WindowInteractionState);

                    switch (action.ToLower())
                    {
                        case "close":
                            _logger.LogInformation("Attempting to close window");
                            windowPattern.Close();
                            _logger.LogInformation("Window close command executed successfully");
                            return new OperationResult { Success = true, Data = "Window closed successfully" };
                        case "minimize":
                            _logger.LogInformation("Attempting to minimize window");
                            windowPattern.SetWindowVisualState(WindowVisualState.Minimized);
                            _logger.LogInformation("Window minimize command executed successfully");
                            return new OperationResult { Success = true, Data = "Window minimized successfully" };
                        case "maximize":
                            _logger.LogInformation("Attempting to maximize window");
                            windowPattern.SetWindowVisualState(WindowVisualState.Maximized);
                            _logger.LogInformation("Window maximize command executed successfully");
                            return new OperationResult { Success = true, Data = "Window maximized successfully" };
                        case "normal":
                            _logger.LogInformation("Attempting to set window to normal state");
                            windowPattern.SetWindowVisualState(WindowVisualState.Normal);
                            _logger.LogInformation("Window normal state command executed successfully");
                            return new OperationResult { Success = true, Data = "Window set to normal state successfully" };
                        default:
                            _logger.LogWarning("Unknown window action requested: {Action}", action);
                            return new OperationResult { Success = false, Error = $"Unknown window action: {action}" };
                    }
                }
                
                _logger.LogError("Element does not support WindowPattern - ElementId: {ElementId}, ControlType: {ControlType}", 
                    elementId, element.Current.ControlType.ProgrammaticName);
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
                _logger.LogInformation("FindElementAsync called - ElementId: {ElementId}, WindowTitle: {WindowTitle}, ProcessId: {ProcessId}", 
                    elementId, windowTitle, processId);

                AutomationElement? searchRoot = null;
                if (!string.IsNullOrEmpty(windowTitle))
                {
                    searchRoot = _windowService.FindWindowByTitle(windowTitle, processId);
                    if (searchRoot == null)
                    {
                        _logger.LogWarning("Window '{WindowTitle}' not found", windowTitle);
                        return new OperationResult<AutomationElement?> { Success = false, Error = $"Window '{windowTitle}' not found" };
                    }
                    _logger.LogInformation("Search root found - Window: {WindowName}, ProcessId: {ProcessId}", 
                        searchRoot.Current.Name, searchRoot.Current.ProcessId);
                }
                else
                {
                    searchRoot = AutomationElement.RootElement;
                    _logger.LogInformation("Using root element as search root");
                }

                var condition = new OrCondition(
                    new PropertyCondition(AutomationElement.AutomationIdProperty, elementId),
                    new PropertyCondition(AutomationElement.NameProperty, elementId)
                );

                _logger.LogInformation("Searching for element with AutomationId or Name: {ElementId}", elementId);
                
                // Add process ID filtering if specified
                Condition finalCondition = condition;
                if (processId.HasValue)
                {
                    finalCondition = new AndCondition(
                        condition,
                        new PropertyCondition(AutomationElement.ProcessIdProperty, processId.Value)
                    );
                    _logger.LogInformation("Added ProcessId filter: {ProcessId}", processId.Value);
                }
                
                var result = await Task.Run(() => 
                {
                    var element = searchRoot.FindFirst(TreeScope.Descendants, finalCondition);
                    return new OperationResult<AutomationElement?> { Success = true, Data = element };
                });
                
                if (result.Success && result.Data != null)
                {
                    _logger.LogInformation("Element found - Name: {Name}, AutomationId: {AutomationId}, ControlType: {ControlType}", 
                        result.Data.Current.Name, result.Data.Current.AutomationId, result.Data.Current.ControlType.ProgrammaticName);
                }
                else
                {
                    _logger.LogWarning("Element not found - ElementId: {ElementId}, Error: {Error}", elementId, result.Error);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding element {ElementId}", elementId);
                return new OperationResult<AutomationElement?> { Success = false, Error = ex.Message };
            }
        }
    }
}