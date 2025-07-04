using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.Services;

namespace UiAutomationMcp.Tests.Mocks
{
    /// <summary>
    /// UIAutomationWorkerのテスト用モック実装
    /// 実際のプロセス実行を行わず、即座にレスポンスを返す
    /// </summary>
    public class MockUIAutomationWorker : IUIAutomationWorker
    {
        private readonly ILogger<MockUIAutomationWorker> _logger;
        private bool _disposed = false;

        public MockUIAutomationWorker(ILogger<MockUIAutomationWorker> logger)
        {
            _logger = logger;
        }

        public Task<OperationResult<string>> ExecuteInProcessAsync(string operationJson, int timeoutSeconds = 10)
        {
            _logger.LogInformation("[MockUIAutomationWorker] ExecuteInProcessAsync called with operation: {Operation}", operationJson);
            return Task.FromResult(new OperationResult<string> 
            { 
                Success = true, 
                Data = "{\"Success\":true,\"Data\":\"Mock response\"}" 
            });
        }

        public Task<OperationResult<List<ElementInfo>>> FindAllAsync(string? windowTitle = null, string? searchText = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            _logger.LogInformation("[MockUIAutomationWorker] FindAllAsync called");
            return Task.FromResult(new OperationResult<List<ElementInfo>> 
            { 
                Success = true, 
                Data = new List<ElementInfo>() 
            });
        }

        public Task<OperationResult<ElementInfo?>> FindFirstAsync(string? windowTitle = null, string? searchText = null, string? controlType = null, int? processId = null, int timeoutSeconds = 15)
        {
            _logger.LogInformation("[MockUIAutomationWorker] FindFirstAsync called");
            return Task.FromResult(new OperationResult<ElementInfo?> 
            { 
                Success = true, 
                Data = new ElementInfo { Name = "MockElement", AutomationId = "mock-id" } 
            });
        }

        public Task<OperationResult<string>> InvokeElementAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 20)
        {
            _logger.LogInformation("[MockUIAutomationWorker] InvokeElementAsync called for element: {ElementId}", elementId);
            return Task.FromResult(new OperationResult<string> { Success = true, Data = "Mock invoke success" });
        }

        public Task<OperationResult<string>> SetElementValueAsync(string elementId, string value, string? windowTitle = null, int? processId = null, int timeoutSeconds = 20)
        {
            _logger.LogInformation("[MockUIAutomationWorker] SetElementValueAsync called for element: {ElementId}, value: {Value}", elementId, value);
            return Task.FromResult(new OperationResult<string> { Success = true, Data = "Mock set value success" });
        }

        public Task<OperationResult<string>> GetElementValueAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 20)
        {
            _logger.LogInformation("[MockUIAutomationWorker] GetElementValueAsync called for element: {ElementId}", elementId);
            return Task.FromResult(new OperationResult<string> { Success = true, Data = "Mock value" });
        }

        public Task<OperationResult<string>> ToggleElementAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 20)
        {
            _logger.LogInformation("[MockUIAutomationWorker] ToggleElementAsync called for element: {ElementId}", elementId);
            return Task.FromResult(new OperationResult<string> { Success = true, Data = "Mock toggle success" });
        }

        public Task<OperationResult<string>> SelectElementAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 20)
        {
            _logger.LogInformation("[MockUIAutomationWorker] SelectElementAsync called for element: {ElementId}", elementId);
            return Task.FromResult(new OperationResult<string> { Success = true, Data = "Mock select success" });
        }

        public Task<OperationResult<string>> ExpandCollapseElementAsync(string elementId, bool? expand = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 20)
        {
            _logger.LogInformation("[MockUIAutomationWorker] ExpandCollapseElementAsync called for element: {ElementId}", elementId);
            return Task.FromResult(new OperationResult<string> { Success = true, Data = "Mock expand/collapse success" });
        }

        public Task<OperationResult<string>> ScrollElementAsync(string elementId, string? direction = null, double? horizontal = null, double? vertical = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 20)
        {
            _logger.LogInformation("[MockUIAutomationWorker] ScrollElementAsync called for element: {ElementId}", elementId);
            return Task.FromResult(new OperationResult<string> { Success = true, Data = "Mock scroll success" });
        }

        public Task<OperationResult<string>> ScrollElementIntoViewAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 20)
        {
            _logger.LogInformation("[MockUIAutomationWorker] ScrollElementIntoViewAsync called for element: {ElementId}", elementId);
            return Task.FromResult(new OperationResult<string> { Success = true, Data = "Mock scroll into view success" });
        }

        public Task<OperationResult<string>> TransformElementAsync(string elementId, string action, double? x = null, double? y = null, double? width = null, double? height = null, double? degrees = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 20)
        {
            _logger.LogInformation("[MockUIAutomationWorker] TransformElementAsync called for element: {ElementId}, action: {Action}", elementId, action);
            return Task.FromResult(new OperationResult<string> { Success = true, Data = "Mock transform success" });
        }

        public Task<OperationResult<string>> DockElementAsync(string elementId, string position, string? windowTitle = null, int? processId = null, int timeoutSeconds = 20)
        {
            _logger.LogInformation("[MockUIAutomationWorker] DockElementAsync called for element: {ElementId}, position: {Position}", elementId, position);
            return Task.FromResult(new OperationResult<string> { Success = true, Data = "Mock dock success" });
        }

        // Additional methods...
        public Task<OperationResult<List<ElementInfo>>> FindAllElementsAsync(ElementSearchParameters searchParams, int timeoutSeconds = 30)
        {
            return FindAllAsync(searchParams.WindowTitle, searchParams.SearchText, searchParams.ControlType, searchParams.ProcessId, timeoutSeconds);
        }

        public Task<OperationResult<ElementInfo?>> FindFirstElementAsync(ElementSearchParameters searchParams, int timeoutSeconds = 15)
        {
            return FindFirstAsync(searchParams.WindowTitle, searchParams.SearchText, searchParams.ControlType, searchParams.ProcessId, timeoutSeconds);
        }

        public Task<OperationResult<Dictionary<string, object>>> ExecuteAdvancedOperationAsync(AdvancedOperationParameters operationParams)
        {
            _logger.LogInformation("[MockUIAutomationWorker] ExecuteAdvancedOperationAsync called for operation: {Operation}", operationParams.Operation);
            return Task.FromResult(new OperationResult<Dictionary<string, object>> 
            { 
                Success = true, 
                Data = new Dictionary<string, object> { ["result"] = "Mock advanced operation success" }
            });
        }

        // UIAutomationHelper から移植したメソッド
        public Task<OperationResult<ElementInfo?>> FindElementSafelyAsync(string? elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 15)
        {
            _logger.LogInformation("[MockUIAutomationWorker] FindElementSafelyAsync called for element: {ElementId}", elementId);
            return Task.FromResult(new OperationResult<ElementInfo?> 
            { 
                Success = true, 
                Data = new ElementInfo { Name = "MockSafeElement", AutomationId = elementId ?? "safe-id" } 
            });
        }

        public Task<OperationResult<T>> ExecuteWithTimeoutAsync<T>(Func<Task<OperationResult<T>>> operation, string operationName, int timeoutSeconds = 30)
        {
            _logger.LogInformation("[MockUIAutomationWorker] ExecuteWithTimeoutAsync called for operation: {OperationName}", operationName);
            // For mocking, just return a successful result
            var defaultResult = new OperationResult<T> { Success = true };
            return Task.FromResult(defaultResult);
        }

        public OperationResult<T> SafeExecute<T>(Func<T> operation, string operationName, T? defaultValue = default)
        {
            _logger.LogInformation("[MockUIAutomationWorker] SafeExecute called for operation: {OperationName}", operationName);
            return new OperationResult<T> { Success = true, Data = defaultValue };
        }

        public Task<OperationResult<Dictionary<string, object>>> GetRangeValueAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 20)
        {
            _logger.LogInformation("[MockUIAutomationWorker] GetRangeValueAsync called for element: {ElementId}", elementId);
            return Task.FromResult(new OperationResult<Dictionary<string, object>> 
            { 
                Success = true, 
                Data = new Dictionary<string, object> 
                { 
                    ["Value"] = 50.0, 
                    ["Minimum"] = 0.0, 
                    ["Maximum"] = 100.0 
                }
            });
        }

        public Task<OperationResult<string>> GetTextAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 20)
        {
            _logger.LogInformation("[MockUIAutomationWorker] GetTextAsync called for element: {ElementId}", elementId);
            return Task.FromResult(new OperationResult<string> { Success = true, Data = "Mock text content" });
        }

        public Task<OperationResult<Dictionary<string, object>>> GetElementTreeAsync(string? windowTitle = null, int? processId = null, int maxDepth = 3, int timeoutSeconds = 30)
        {
            _logger.LogInformation("[MockUIAutomationWorker] GetElementTreeAsync called");
            return Task.FromResult(new OperationResult<Dictionary<string, object>> 
            { 
                Success = true, 
                Data = new Dictionary<string, object> 
                { 
                    ["Name"] = "MockWindow", 
                    ["Children"] = new List<object>() 
                }
            });
        }

        public Task<OperationResult<List<Dictionary<string, object>>>> GetElementChildrenAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 20)
        {
            _logger.LogInformation("[MockUIAutomationWorker] GetElementChildrenAsync called for element: {ElementId}", elementId);
            return Task.FromResult(new OperationResult<List<Dictionary<string, object>>> 
            { 
                Success = true, 
                Data = new List<Dictionary<string, object>>
                {
                    new() { ["Name"] = "MockChild1", ["AutomationId"] = "child1" },
                    new() { ["Name"] = "MockChild2", ["AutomationId"] = "child2" }
                }
            });
        }

        public Task<OperationResult<string>> SetRangeValueAsync(string elementId, double value, string? windowTitle = null, int? processId = null, int timeoutSeconds = 20)
        {
            _logger.LogInformation("[MockUIAutomationWorker] SetRangeValueAsync called for element: {ElementId}, value: {Value}", elementId, value);
            return Task.FromResult(new OperationResult<string> { Success = true, Data = "Mock range value set" });
        }

        public Task<OperationResult<string>> SelectTextAsync(string elementId, int startIndex, int length, string? windowTitle = null, int? processId = null, int timeoutSeconds = 20)
        {
            _logger.LogInformation("[MockUIAutomationWorker] SelectTextAsync called for element: {ElementId}", elementId);
            return Task.FromResult(new OperationResult<string> { Success = true, Data = "Mock text selected" });
        }

        public Task<OperationResult<string>> SetWindowStateAsync(string elementId, string state, string? windowTitle = null, int? processId = null, int timeoutSeconds = 20)
        {
            _logger.LogInformation("[MockUIAutomationWorker] SetWindowStateAsync called for element: {ElementId}, state: {State}", elementId, state);
            return Task.FromResult(new OperationResult<string> { Success = true, Data = "Mock window state set" });
        }

        // Additional Text Pattern methods
        public Task<OperationResult<Dictionary<string, object>>> FindTextAsync(
            string elementId,
            string searchText,
            bool backward = false,
            bool ignoreCase = false,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            _logger.LogInformation("[MockUIAutomationWorker] FindTextAsync called for element: {ElementId}, searchText: {SearchText}", 
                elementId, searchText);
            return Task.FromResult(new OperationResult<Dictionary<string, object>>
            {
                Success = true,
                Data = new Dictionary<string, object>
                {
                    ["Found"] = true,
                    ["StartIndex"] = 0,
                    ["Length"] = searchText.Length,
                    ["Text"] = searchText
                }
            });
        }

        public Task<OperationResult<List<Dictionary<string, object>>>> GetTextSelectionAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            _logger.LogInformation("[MockUIAutomationWorker] GetTextSelectionAsync called for element: {ElementId}", elementId);
            return Task.FromResult(new OperationResult<List<Dictionary<string, object>>>
            {
                Success = true,
                Data = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        ["StartIndex"] = 0,
                        ["Length"] = 10,
                        ["Text"] = "MockText"
                    }
                }
            });
        }

        // Window service methods
        public Task<OperationResult<List<WindowInfo>>> GetWindowsAsync(int timeoutSeconds = 30)
        {
            _logger.LogInformation("[MockUIAutomationWorker] GetWindowsAsync called");
            return Task.FromResult(new OperationResult<List<WindowInfo>>
            {
                Success = true,
                Data = new List<WindowInfo>
                {
                    new WindowInfo
                    {
                        Title = "Mock Window",
                        Name = "MockWindow",
                        ProcessId = 1234,
                        ProcessName = "MockProcess",
                        AutomationId = "mock-window-id"
                    }
                }
            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _logger.LogInformation("[MockUIAutomationWorker] Disposing mock worker instance");
                }
                _disposed = true;
            }
        }
    }
}
