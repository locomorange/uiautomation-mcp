using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcpServer.Models;

namespace UiAutomationMcpServer.Services
{
    public interface IUIAutomationHelper
    {
        Task<OperationResult<AutomationElementCollection>> FindAllAsync(
            AutomationElement searchRoot,
            TreeScope scope,
            Condition condition,
            int timeoutSeconds = 90);

        Task<OperationResult<AutomationElement?>> FindFirstAsync(
            AutomationElement searchRoot,
            TreeScope scope,
            Condition condition,
            int timeoutSeconds = 30);

        Task<OperationResult<T>> ExecuteWithTimeoutAsync<T>(
            Func<T> operation,
            string operationName,
            int timeoutSeconds = 30);

        OperationResult<T> SafeExecute<T>(
            Func<T> operation,
            string operationName,
            T? defaultValue = default);

        OperationResult<AutomationElement?> FindElementSafely(
            AutomationElement? searchRoot,
            string? elementId,
            string? windowTitle = null,
            int? processId = null);
    }

    public class UIAutomationHelper : IUIAutomationHelper
    {
        private readonly ILogger<UIAutomationHelper> _logger;

        public UIAutomationHelper(ILogger<UIAutomationHelper> logger)
        {
            _logger = logger;
        }

        public async Task<OperationResult<AutomationElementCollection>> FindAllAsync(
            AutomationElement searchRoot,
            TreeScope scope,
            Condition condition,
            int timeoutSeconds = 90)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var result = await Task.Run(() => searchRoot.FindAll(scope, condition), cts.Token);
                return new OperationResult<AutomationElementCollection> { Success = true, Data = result };
            }
            catch (OperationCanceledException)
            {
                return new OperationResult<AutomationElementCollection>
                {
                    Success = false,
                    Error = $"Search timeout after {timeoutSeconds} seconds. Please narrow your search criteria by specifying windowTitle, controlType, or more specific search parameters to improve performance."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FindAll operation failed");
                return new OperationResult<AutomationElementCollection>
                {
                    Success = false,
                    Error = $"FindAll operation failed: {ex.Message}"
                };
            }
        }

        public async Task<OperationResult<AutomationElement?>> FindFirstAsync(
            AutomationElement searchRoot,
            TreeScope scope,
            Condition condition,
            int timeoutSeconds = 30)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var result = await Task.Run(() => searchRoot.FindFirst(scope, condition), cts.Token);
                return new OperationResult<AutomationElement?> { Success = true, Data = result };
            }
            catch (OperationCanceledException)
            {
                return new OperationResult<AutomationElement?>
                {
                    Success = false,
                    Error = $"Search timeout after {timeoutSeconds} seconds. Please narrow your search criteria by specifying windowTitle, controlType, or more specific search parameters to improve performance."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FindFirst operation failed");
                return new OperationResult<AutomationElement?>
                {
                    Success = false,
                    Error = $"FindFirst operation failed: {ex.Message}"
                };
            }
        }

        public async Task<OperationResult<T>> ExecuteWithTimeoutAsync<T>(
            Func<T> operation,
            string operationName,
            int timeoutSeconds = 30)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var result = await Task.Run(operation, cts.Token);
                return new OperationResult<T> { Success = true, Data = result };
            }
            catch (OperationCanceledException)
            {
                return new OperationResult<T>
                {
                    Success = false,
                    Error = $"{operationName} timeout after {timeoutSeconds} seconds. Please try with more specific parameters or check if the target element is available."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{OperationName} failed", operationName);
                return new OperationResult<T>
                {
                    Success = false,
                    Error = $"{operationName} failed: {ex.Message}"
                };
            }
        }

        public OperationResult<T> SafeExecute<T>(
            Func<T> operation,
            string operationName,
            T? defaultValue = default)
        {
            try
            {
                var result = operation();
                return new OperationResult<T> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{OperationName} failed, using default value", operationName);
                return new OperationResult<T> { Success = true, Data = defaultValue };
            }
        }

        public OperationResult<AutomationElement?> FindElementSafely(
            AutomationElement? searchRoot,
            string? elementId,
            string? windowTitle = null,
            int? processId = null)
        {
            try
            {
                if (searchRoot == null)
                {
                    return new OperationResult<AutomationElement?>
                    {
                        Success = false,
                        Error = "Search root element is null"
                    };
                }

                if (string.IsNullOrEmpty(elementId))
                {
                    return new OperationResult<AutomationElement?>
                    {
                        Success = false,
                        Error = "Element ID is required"
                    };
                }

                var conditions = new List<Condition>
                {
                    new PropertyCondition(AutomationElement.AutomationIdProperty, elementId),
                    new PropertyCondition(AutomationElement.NameProperty, elementId)
                };

                var condition = new OrCondition(conditions.ToArray());
                var element = searchRoot.FindFirst(TreeScope.Descendants, condition);

                if (element == null)
                {
                    return new OperationResult<AutomationElement?>
                    {
                        Success = false,
                        Error = $"Element with ID '{elementId}' not found. Please check the element ID and ensure the target application is running."
                    };
                }

                return new OperationResult<AutomationElement?> { Success = true, Data = element };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FindElementSafely failed for elementId: {ElementId}", elementId);
                return new OperationResult<AutomationElement?>
                {
                    Success = false,
                    Error = $"Failed to find element: {ex.Message}"
                };
            }
        }
    }

    public class OperationResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }
    }
}