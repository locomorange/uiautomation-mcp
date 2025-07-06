using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;

namespace UiAutomationMcpServer.Services
{
    /// <summary>
    /// UIAutomation操作のビジネスロジックを提供
    /// ワーカーへの委譲とエラーハンドリングを担当
    /// </summary>
    public interface IUIAutomationService
    {
        /// <summary>
        /// 直接のワーカーアクセス（テスト用）
        /// </summary>
        IUIAutomationWorker Worker { get; }
        
        
        // Element Discovery
        Task<OperationResult<List<ElementInfo>>> FindElementsAsync(
            string? windowTitle = null,
            string? searchText = null,
            string? controlType = null,
            int? processId = null,
            int timeoutSeconds = 60);

        Task<OperationResult<ElementInfo>> FindFirstElementAsync(
            string? windowTitle = null,
            string? searchText = null,
            string? controlType = null,
            int? processId = null,
            int timeoutSeconds = 15);

        Task<OperationResult<Dictionary<string, object>>> GetElementTreeAsync(
            string? windowTitle = null,
            int? processId = null,
            int maxDepth = 3,
            int timeoutSeconds = 60);

        Task<OperationResult<Dictionary<string, object>>> GetElementPropertiesAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 60);

        Task<OperationResult<List<string>>> GetElementPatternsAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 60);

        // Basic Interactions
        Task<OperationResult<string>> InvokeElementAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<OperationResult<string>> SetElementValueAsync(
            string elementId,
            string value,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<OperationResult<string>> GetElementValueAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<OperationResult<string>> ToggleElementAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<OperationResult<string>> SelectElementAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        // Layout and Navigation
        Task<OperationResult<string>> ExpandCollapseElementAsync(
            string elementId,
            bool? expand = null,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<OperationResult<string>> ScrollElementAsync(
            string elementId,
            string? direction = null,
            double? horizontal = null,
            double? vertical = null,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<OperationResult<string>> ScrollElementIntoViewAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        // Value and Range
        Task<OperationResult<string>> SetRangeValueAsync(
            string elementId,
            double value,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<OperationResult<Dictionary<string, object>>> GetRangeValueAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        // Window Management
        Task<OperationResult<List<WindowInfo>>> GetWindowsAsync(int timeoutSeconds = 60);

        Task<OperationResult<string>> SetWindowStateAsync(
            string elementId,
            string state,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<OperationResult<string>> TransformElementAsync(
            string elementId,
            string action,
            double? x = null,
            double? y = null,
            double? width = null,
            double? height = null,
            double? degrees = null,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<OperationResult<string>> DockElementAsync(
            string elementId,
            string position,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        // Text Operations
        Task<OperationResult<string>> GetTextAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<OperationResult<string>> SelectTextAsync(
            string elementId,
            int startIndex,
            int length,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<OperationResult<Dictionary<string, object>>> FindTextAsync(
            string elementId,
            string searchText,
            bool backward = false,
            bool ignoreCase = false,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<OperationResult<List<Dictionary<string, object>>>> GetTextSelectionAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);
    }

    public class UIAutomationService : IUIAutomationService
    {
        private readonly IUIAutomationWorker _worker;
        private readonly ILogger<UIAutomationService> _logger;

        public UIAutomationService(
            IUIAutomationWorker worker,
            ILogger<UIAutomationService> logger)
        {
            _worker = worker;
            _logger = logger;
        }

        /// <summary>
        /// 直接のワーカーアクセス（テスト用）
        /// </summary>
        public IUIAutomationWorker Worker => _worker;

        // Element Discovery
        public Task<OperationResult<List<ElementInfo>>> FindElementsAsync(
            string? windowTitle = null,
            string? searchText = null,
            string? controlType = null,
            int? processId = null,
            int timeoutSeconds = 60)
        {
            var parameters = new
            {
                WindowTitle = windowTitle,
                SearchText = searchText,
                ControlType = controlType,
                ProcessId = processId
            };

            return _worker.ExecuteOperationAsync<List<ElementInfo>>(
                "findall", parameters, timeoutSeconds);
        }

        public Task<OperationResult<ElementInfo>> FindFirstElementAsync(
            string? windowTitle = null,
            string? searchText = null,
            string? controlType = null,
            int? processId = null,
            int timeoutSeconds = 15)
        {
            var parameters = new
            {
                WindowTitle = windowTitle,
                SearchText = searchText,
                ControlType = controlType,
                ProcessId = processId
            };

            return _worker.ExecuteOperationAsync<ElementInfo>(
                "findfirst", parameters, timeoutSeconds);
        }

        public Task<OperationResult<Dictionary<string, object>>> GetElementTreeAsync(
            string? windowTitle = null,
            int? processId = null,
            int maxDepth = 3,
            int timeoutSeconds = 60)
        {
            var parameters = new
            {
                WindowTitle = windowTitle,
                ProcessId = processId,
                MaxDepth = maxDepth
            };

            return _worker.ExecuteOperationAsync<Dictionary<string, object>>(
                "getelementtree", parameters, timeoutSeconds);
        }

        public Task<OperationResult<Dictionary<string, object>>> GetElementPropertiesAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 60)
        {
            var parameters = new
            {
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            return _worker.ExecuteOperationAsync<Dictionary<string, object>>(
                "getelementproperties", parameters, timeoutSeconds);
        }

        public Task<OperationResult<List<string>>> GetElementPatternsAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 60)
        {
            var parameters = new
            {
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            return _worker.ExecuteOperationAsync<List<string>>(
                "getelementpatterns", parameters, timeoutSeconds);
        }

        // Basic Interactions
        public async Task<OperationResult<string>> InvokeElementAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30)
        {
            var parameters = new
            {
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            var result = await _worker.ExecuteOperationAsync<object>(
                "invokeelement", parameters, timeoutSeconds);

            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? "Element invoked successfully" : null,
                Error = result.Error
            };
        }

        public async Task<OperationResult<string>> SetElementValueAsync(
            string elementId,
            string value,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30)
        {
            var parameters = new
            {
                ElementId = elementId,
                Value = value,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            var result = await _worker.ExecuteOperationAsync<object>(
                "setelementvalue", parameters, timeoutSeconds);

            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? $"Element value set to: {value}" : null,
                Error = result.Error
            };
        }

        public async Task<OperationResult<string>> GetElementValueAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30)
        {
            var parameters = new
            {
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            var result = await _worker.ExecuteOperationAsync<Dictionary<string, object>>(
                "getelementvalue", parameters, timeoutSeconds);

            if (!result.Success)
            {
                return new OperationResult<string>
                {
                    Success = false,
                    Error = result.Error
                };
            }

            var value = result.Data?.GetValueOrDefault("value")?.ToString();
            return new OperationResult<string>
            {
                Success = true,
                Data = value
            };
        }

        public async Task<OperationResult<string>> ToggleElementAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30)
        {
            var parameters = new
            {
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            var result = await _worker.ExecuteOperationAsync<object>(
                "toggleelement", parameters, timeoutSeconds);

            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? "Element toggled successfully" : null,
                Error = result.Error
            };
        }

        public async Task<OperationResult<string>> SelectElementAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30)
        {
            var parameters = new
            {
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            var result = await _worker.ExecuteOperationAsync<object>(
                "selectelement", parameters, timeoutSeconds);

            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? "Element selected successfully" : null,
                Error = result.Error
            };
        }

        // Layout and Navigation
        public async Task<OperationResult<string>> ExpandCollapseElementAsync(
            string elementId,
            bool? expand = null,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30)
        {
            var parameters = new
            {
                ElementId = elementId,
                Expand = expand,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            var result = await _worker.ExecuteOperationAsync<object>(
                "expandcollapseelement", parameters, timeoutSeconds);

            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? $"Element {(expand == true ? "expanded" : "collapsed")} successfully" : null,
                Error = result.Error
            };
        }

        public async Task<OperationResult<string>> ScrollElementAsync(
            string elementId,
            string? direction = null,
            double? horizontal = null,
            double? vertical = null,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30)
        {
            var parameters = new
            {
                ElementId = elementId,
                Direction = direction,
                Horizontal = horizontal,
                Vertical = vertical,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            var result = await _worker.ExecuteOperationAsync<object>(
                "scrollelement", parameters, timeoutSeconds);

            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? "Element scrolled successfully" : null,
                Error = result.Error
            };
        }

        public async Task<OperationResult<string>> ScrollElementIntoViewAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30)
        {
            var parameters = new
            {
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            var result = await _worker.ExecuteOperationAsync<object>(
                "scrollelementintoview", parameters, timeoutSeconds);

            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? "Element scrolled into view successfully" : null,
                Error = result.Error
            };
        }

        // Value and Range
        public async Task<OperationResult<string>> SetRangeValueAsync(
            string elementId,
            double value,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30)
        {
            var parameters = new
            {
                ElementId = elementId,
                Value = value,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            var result = await _worker.ExecuteOperationAsync<object>(
                "setrangevalue", parameters, timeoutSeconds);

            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? $"Range value set to {value}" : null,
                Error = result.Error
            };
        }

        public Task<OperationResult<Dictionary<string, object>>> GetRangeValueAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30)
        {
            var parameters = new
            {
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            return _worker.ExecuteOperationAsync<Dictionary<string, object>>(
                "getrangevalue", parameters, timeoutSeconds);
        }

        // Window Management
        public Task<OperationResult<List<WindowInfo>>> GetWindowsAsync(int timeoutSeconds = 60)
        {
            return _worker.ExecuteOperationAsync<List<WindowInfo>>(
                "getwindows", new { }, timeoutSeconds);
        }

        public async Task<OperationResult<string>> SetWindowStateAsync(
            string elementId,
            string state,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30)
        {
            var parameters = new
            {
                ElementId = elementId,
                State = state,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            var result = await _worker.ExecuteOperationAsync<object>(
                "setwindowstate", parameters, timeoutSeconds);

            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? $"Window state set to {state}" : null,
                Error = result.Error
            };
        }

        public async Task<OperationResult<string>> TransformElementAsync(
            string elementId,
            string action,
            double? x = null,
            double? y = null,
            double? width = null,
            double? height = null,
            double? degrees = null,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30)
        {
            var parameters = new
            {
                ElementId = elementId,
                Action = action,
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Degrees = degrees,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            var result = await _worker.ExecuteOperationAsync<object>(
                "transformelement", parameters, timeoutSeconds);

            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? $"Element transformed ({action}) successfully" : null,
                Error = result.Error
            };
        }

        public async Task<OperationResult<string>> DockElementAsync(
            string elementId,
            string position,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30)
        {
            var parameters = new
            {
                ElementId = elementId,
                Position = position,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            var result = await _worker.ExecuteOperationAsync<object>(
                "dockelement", parameters, timeoutSeconds);

            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? $"Element docked to {position}" : null,
                Error = result.Error
            };
        }

        // Text Operations
        public async Task<OperationResult<string>> GetTextAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30)
        {
            var parameters = new
            {
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            var result = await _worker.ExecuteOperationAsync<Dictionary<string, object>>(
                "gettext", parameters, timeoutSeconds);

            if (!result.Success)
            {
                return new OperationResult<string>
                {
                    Success = false,
                    Error = result.Error
                };
            }

            var text = result.Data?.GetValueOrDefault("text")?.ToString();
            return new OperationResult<string>
            {
                Success = true,
                Data = text
            };
        }

        public async Task<OperationResult<string>> SelectTextAsync(
            string elementId,
            int startIndex,
            int length,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30)
        {
            var parameters = new
            {
                ElementId = elementId,
                StartIndex = startIndex,
                Length = length,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            var result = await _worker.ExecuteOperationAsync<object>(
                "selecttext", parameters, timeoutSeconds);

            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? "Text selected successfully" : null,
                Error = result.Error
            };
        }

        public Task<OperationResult<Dictionary<string, object>>> FindTextAsync(
            string elementId,
            string searchText,
            bool backward = false,
            bool ignoreCase = false,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30)
        {
            var parameters = new
            {
                ElementId = elementId,
                SearchText = searchText,
                Backward = backward,
                IgnoreCase = ignoreCase,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            return _worker.ExecuteOperationAsync<Dictionary<string, object>>(
                "findtext", parameters, timeoutSeconds);
        }

        public Task<OperationResult<List<Dictionary<string, object>>>> GetTextSelectionAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30)
        {
            var parameters = new
            {
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            return _worker.ExecuteOperationAsync<List<Dictionary<string, object>>>(
                "gettextselection", parameters, timeoutSeconds);
        }
    }
}