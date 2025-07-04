using Microsoft.Extensions.Logging;
using System;
using System.Windows.Automation;
using UiAutomationMcpServer.Models;
using UiAutomationMcpServer.Services.Windows;
using UiAutomationMcpServer.Services;

namespace UiAutomationMcpServer.Services.Patterns
{
    public interface ICorePatternService
    {
        Task<OperationResult> InvokeElementAsync(string elementId, string? windowTitle = null, int? processId = null);
        Task<OperationResult> SetElementValueAsync(string elementId, string value, string? windowTitle = null, int? processId = null);
        Task<OperationResult> GetElementValueAsync(string elementId, string? windowTitle = null, int? processId = null);
        Task<OperationResult> ToggleElementAsync(string elementId, string? windowTitle = null, int? processId = null);
        Task<OperationResult> SelectElementAsync(string elementId, string? windowTitle = null, int? processId = null);
    }

    public class CorePatternService : ICorePatternService
    {
        private readonly ILogger<CorePatternService> _logger;
        private readonly IWindowService _windowService;
        private readonly IUIAutomationHelper _uiAutomationHelper;

        public CorePatternService(ILogger<CorePatternService> logger, IWindowService windowService, IUIAutomationHelper uiAutomationHelper)
        {
            _logger = logger;
            _windowService = windowService;
            _uiAutomationHelper = uiAutomationHelper;
        }

        public Task<OperationResult> InvokeElementAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            return ExecutePatternAsync(elementId, "invoke", null, windowTitle, processId);
        }

        public Task<OperationResult> SetElementValueAsync(string elementId, string value, string? windowTitle = null, int? processId = null)
        {
            return ExecutePatternAsync(elementId, "value", new Dictionary<string, object> { ["value"] = value }, windowTitle, processId);
        }

        public Task<OperationResult> GetElementValueAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            return ExecutePatternAsync(elementId, "get_value", null, windowTitle, processId);
        }

        public Task<OperationResult> ToggleElementAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            return ExecutePatternAsync(elementId, "toggle", null, windowTitle, processId);
        }

        public Task<OperationResult> SelectElementAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            return ExecutePatternAsync(elementId, "select", null, windowTitle, processId);
        }

        private async Task<OperationResult> ExecutePatternAsync(string elementId, string patternName, Dictionary<string, object>? parameters = null, string? windowTitle = null, int? processId = null)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("[ExecutePatternAsync] START: Pattern={PatternName}, Element={ElementId}, Window={WindowTitle}", 
                patternName, elementId, windowTitle);
            
            try
            {
                // 全体的なタイムアウト制御を追加（20秒）
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                
                var patternTask = Task.Run(async () =>
                {
                    _logger.LogInformation("Executing pattern {PatternName} on element {ElementId}", patternName, elementId);

                    var elementResult = await FindElementAsync(elementId, windowTitle, processId);
                    if (!elementResult.Success || elementResult.Data == null)
                    {
                        return new OperationResult { Success = false, Error = elementResult.Error ?? $"Element '{elementId}' not found" };
                    }
                    var element = elementResult.Data;

                    return await (patternName.ToLower() switch
                    {
                        "invoke" => ExecuteInvokePattern(element),
                        "value" => ExecuteValuePattern(element, parameters?.GetValueOrDefault("value") as string),
                        "get_value" => GetValuePattern(element),
                        "toggle" => ExecuteTogglePattern(element),
                        "select" => ExecuteSelectPattern(element),
                        _ => Task.FromResult(new OperationResult { Success = false, Error = $"Pattern '{patternName}' not supported" })
                    });
                }, cts.Token);
                
                var result = await patternTask;
                var elapsed = DateTime.UtcNow - startTime;
                _logger.LogInformation("[ExecutePatternAsync] SUCCESS: Pattern={PatternName}, Elapsed={Elapsed}ms", 
                    patternName, elapsed.TotalMilliseconds);
                
                return result;
            }
            catch (OperationCanceledException)
            {
                var elapsed = DateTime.UtcNow - startTime;
                _logger.LogWarning("[ExecutePatternAsync] TIMEOUT: Pattern={PatternName}, After {Elapsed}ms", 
                    patternName, elapsed.TotalMilliseconds);
                return new OperationResult { Success = false, Error = $"Pattern execution timeout after 20 seconds" };
            }
            catch (Exception ex)
            {
                var elapsed = DateTime.UtcNow - startTime;
                _logger.LogError(ex, "[ExecutePatternAsync] ERROR: Pattern={PatternName}, After {Elapsed}ms", 
                    patternName, elapsed.TotalMilliseconds);
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

                _logger.LogInformation("[FindElementAsync] FindFirstAsync start: elementId={ElementId}", elementId);
                var condition = new OrCondition(
                    new PropertyCondition(AutomationElement.AutomationIdProperty, elementId),
                    new PropertyCondition(AutomationElement.NameProperty, elementId)
                );

                // より短いタイムアウト(10秒)でハングを早期検出
                var result = await _uiAutomationHelper.FindFirstAsync(searchRoot, TreeScope.Descendants, condition, timeoutSeconds: 10);
                _logger.LogInformation("[FindElementAsync] FindFirstAsync end: elementId={ElementId}, Success={Success}", elementId, result.Success);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding element {ElementId}", elementId);
                return new OperationResult<AutomationElement?> { Success = false, Error = ex.Message };
            }
        }

        private Task<OperationResult> ExecuteInvokePattern(AutomationElement element)
        {
            try
            {
                if (element.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern) && pattern is InvokePattern invokePattern)
                {
                    invokePattern.Invoke();
                    return Task.FromResult(new OperationResult { Success = true, Data = "Element invoked successfully" });
                }
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support InvokePattern" });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        private Task<OperationResult> ExecuteValuePattern(AutomationElement element, string? value)
        {
            try
            {
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern) && pattern is ValuePattern valuePattern)
                {
                    if (value != null)
                    {
                        valuePattern.SetValue(value);
                        return Task.FromResult(new OperationResult { Success = true, Data = "Value set successfully" });
                    }
                    return Task.FromResult(new OperationResult { Success = false, Error = "Value parameter is required" });
                }
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support ValuePattern" });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        private Task<OperationResult> GetValuePattern(AutomationElement element)
        {
            try
            {
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern) && pattern is ValuePattern valuePattern)
                {
                    var value = valuePattern.Current.Value;
                    return Task.FromResult(new OperationResult { Success = true, Data = value });
                }
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support ValuePattern" });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        private Task<OperationResult> ExecuteTogglePattern(AutomationElement element)
        {
            try
            {
                if (element.TryGetCurrentPattern(TogglePattern.Pattern, out var pattern) && pattern is TogglePattern togglePattern)
                {
                    togglePattern.Toggle();
                    return Task.FromResult(new OperationResult { Success = true, Data = "Element toggled successfully" });
                }
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support TogglePattern" });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        private Task<OperationResult> ExecuteSelectPattern(AutomationElement element)
        {
            try
            {
                if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) && pattern is SelectionItemPattern selectionPattern)
                {
                    selectionPattern.Select();
                    return Task.FromResult(new OperationResult { Success = true, Data = "Element selected successfully" });
                }
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support SelectionItemPattern" });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }
    }
}