using Microsoft.Extensions.Logging;
using System.Windows.Automation;
using UiAutomationMcpServer.Models;
using UiAutomationMcpServer.Services.Windows;

namespace UiAutomationMcpServer.Services.Patterns
{
    public interface IRangePatternService
    {
        Task<OperationResult> SetRangeValueAsync(string elementId, double value, string? windowTitle = null, int? processId = null);
        Task<OperationResult> GetRangeValueAsync(string elementId, string? windowTitle = null, int? processId = null);
    }

    public class RangePatternService : IRangePatternService
    {
        private readonly ILogger<RangePatternService> _logger;
        private readonly IWindowService _windowService;

        public RangePatternService(ILogger<RangePatternService> logger, IWindowService windowService)
        {
            _logger = logger;
            _windowService = windowService;
        }

        public Task<OperationResult> SetRangeValueAsync(string elementId, double value, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var element = FindElement(elementId, windowTitle, processId);
                if (element == null)
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });
                }

                if (element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var pattern) && pattern is RangeValuePattern rangeValuePattern)
                {
                    rangeValuePattern.SetValue(value);
                    return Task.FromResult(new OperationResult { Success = true, Data = "Range value set successfully" });
                }
                
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support RangeValuePattern" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting range value on element {ElementId}", elementId);
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        public Task<OperationResult> GetRangeValueAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var element = FindElement(elementId, windowTitle, processId);
                if (element == null)
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });
                }

                if (element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var pattern) && pattern is RangeValuePattern rangeValuePattern)
                {
                    var rangeInfo = new
                    {
                        Value = double.IsInfinity(rangeValuePattern.Current.Value) ? 0 : rangeValuePattern.Current.Value,
                        Minimum = double.IsInfinity(rangeValuePattern.Current.Minimum) ? 0 : rangeValuePattern.Current.Minimum,
                        Maximum = double.IsInfinity(rangeValuePattern.Current.Maximum) ? 0 : rangeValuePattern.Current.Maximum,
                        SmallChange = double.IsInfinity(rangeValuePattern.Current.SmallChange) ? 0 : rangeValuePattern.Current.SmallChange,
                        LargeChange = double.IsInfinity(rangeValuePattern.Current.LargeChange) ? 0 : rangeValuePattern.Current.LargeChange,
                        IsReadOnly = rangeValuePattern.Current.IsReadOnly
                    };
                    
                    return Task.FromResult(new OperationResult { Success = true, Data = rangeInfo });
                }
                
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support RangeValuePattern" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting range value from element {ElementId}", elementId);
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