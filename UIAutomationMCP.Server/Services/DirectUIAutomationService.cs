using Microsoft.Extensions.Logging;
using System.Windows.Automation;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.Helpers;
using UiAutomationMcpServer.ElementTree;
using UiAutomationMcpServer.Services;

namespace UiAutomationMcpServer.Services
{
    /// <summary>
    /// UI Automation操作を直接実行するサービス（Workerプロセスを使用しない）
    /// </summary>
    public class DirectUIAutomationService : IUIAutomationService
    {
        private readonly ILogger<DirectUIAutomationService> _logger;
        private readonly AutomationHelper _automationHelper;
        private readonly ElementSearchHandler _elementSearchHandler;
        private readonly TreeNavigationHandler _treeNavigationHandler;
        private readonly ElementInfoExtractor _elementInfoExtractor;
        private readonly ScreenshotService _screenshotService;

        public DirectUIAutomationService(
            ILogger<DirectUIAutomationService> logger,
            AutomationHelper automationHelper,
            ElementSearchHandler elementSearchHandler,
            TreeNavigationHandler treeNavigationHandler,
            ElementInfoExtractor elementInfoExtractor,
            ScreenshotService screenshotService)
        {
            _logger = logger;
            _automationHelper = automationHelper;
            _elementSearchHandler = elementSearchHandler;
            _treeNavigationHandler = treeNavigationHandler;
            _elementInfoExtractor = elementInfoExtractor;
            _screenshotService = screenshotService;
        }

        // Legacy compatibility - Workerプロパティ（DirectUIAutomationServiceでは使用しない）
        public IUIAutomationWorker Worker => throw new NotSupportedException("DirectUIAutomationService does not use Worker process");

        // Element Discovery
        public async Task<OperationResult<List<ElementInfo>>> FindElementsAsync(
            string? windowTitle = null,
            string? searchText = null,
            string? controlType = null,
            int? processId = null,
            int timeoutSeconds = 60)
        {
            try
            {
                _logger.LogInformation("Finding elements with WindowTitle={WindowTitle}, SearchText={SearchText}, ControlType={ControlType}, ProcessId={ProcessId}",
                    windowTitle, searchText, controlType, processId);

                var operation = new WorkerOperation
                {
                    Operation = "findall",
                    Parameters = new Dictionary<string, object>
                    {
                        ["WindowTitle"] = windowTitle ?? "",
                        ["SearchText"] = searchText ?? "",
                        ["ControlType"] = controlType ?? "",
                        ["ProcessId"] = processId ?? 0
                    },
                    Timeout = timeoutSeconds
                };

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var result = await Task.Run(() => _elementSearchHandler.FindElements(operation), cts.Token);

                if (result.Success && result.Data is List<ElementInfo> elements)
                {
                    return new OperationResult<List<ElementInfo>>
                    {
                        Success = true,
                        Data = elements
                    };
                }

                return new OperationResult<List<ElementInfo>>
                {
                    Success = false,
                    Error = result.Error ?? "Failed to find elements"
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("FindElements operation timed out after {Timeout}s", timeoutSeconds);
                return new OperationResult<List<ElementInfo>>
                {
                    Success = false,
                    Error = $"Operation timed out after {timeoutSeconds} seconds"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding elements");
                return new OperationResult<List<ElementInfo>>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<OperationResult<ElementInfo>> FindFirstElementAsync(
            string? windowTitle = null,
            string? searchText = null,
            string? controlType = null,
            int? processId = null,
            int timeoutSeconds = 15)
        {
            try
            {
                _logger.LogInformation("Finding first element with WindowTitle={WindowTitle}, SearchText={SearchText}, ControlType={ControlType}, ProcessId={ProcessId}",
                    windowTitle, searchText, controlType, processId);

                var operation = new WorkerOperation
                {
                    Operation = "findfirst",
                    Parameters = new Dictionary<string, object>
                    {
                        ["WindowTitle"] = windowTitle ?? "",
                        ["SearchText"] = searchText ?? "",
                        ["ControlType"] = controlType ?? "",
                        ["ProcessId"] = processId ?? 0
                    },
                    Timeout = timeoutSeconds
                };

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var result = await Task.Run(() => _elementSearchHandler.FindFirstElement(operation), cts.Token);

                if (result.Success && result.Data is ElementInfo element)
                {
                    return new OperationResult<ElementInfo>
                    {
                        Success = true,
                        Data = element
                    };
                }

                return new OperationResult<ElementInfo>
                {
                    Success = false,
                    Error = result.Error ?? "Failed to find element"
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("FindFirstElement operation timed out after {Timeout}s", timeoutSeconds);
                return new OperationResult<ElementInfo>
                {
                    Success = false,
                    Error = $"Operation timed out after {timeoutSeconds} seconds"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding first element");
                return new OperationResult<ElementInfo>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<OperationResult<Dictionary<string, object>>> GetElementTreeAsync(
            string? windowTitle = null,
            int? processId = null,
            int maxDepth = 3,
            int timeoutSeconds = 60)
        {
            try
            {
                _logger.LogInformation("Getting element tree with WindowTitle={WindowTitle}, ProcessId={ProcessId}, MaxDepth={MaxDepth}",
                    windowTitle, processId, maxDepth);

                var operation = new WorkerOperation
                {
                    Operation = "getelementtree",
                    Parameters = new Dictionary<string, object>
                    {
                        ["WindowTitle"] = windowTitle ?? "",
                        ["ProcessId"] = processId ?? 0,
                        ["MaxDepth"] = maxDepth
                    },
                    Timeout = timeoutSeconds
                };

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var result = await Task.Run(() => _treeNavigationHandler.GetElementTree(operation), cts.Token);

                if (result.Success && result.Data is Dictionary<string, object> tree)
                {
                    return new OperationResult<Dictionary<string, object>>
                    {
                        Success = true,
                        Data = tree
                    };
                }

                return new OperationResult<Dictionary<string, object>>
                {
                    Success = false,
                    Error = result.Error ?? "Failed to get element tree"
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GetElementTree operation timed out after {Timeout}s", timeoutSeconds);
                return new OperationResult<Dictionary<string, object>>
                {
                    Success = false,
                    Error = $"Operation timed out after {timeoutSeconds} seconds"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element tree");
                return new OperationResult<Dictionary<string, object>>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<OperationResult<Dictionary<string, object>>> GetElementPropertiesAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 60)
        {
            try
            {
                _logger.LogInformation("Getting element properties for ElementId={ElementId}", elementId);

                var operation = new WorkerOperation
                {
                    Operation = "getelementproperties",
                    Parameters = new Dictionary<string, object>
                    {
                        ["ElementId"] = elementId,
                        ["WindowTitle"] = windowTitle ?? "",
                        ["ProcessId"] = processId ?? 0
                    },
                    Timeout = timeoutSeconds
                };

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var result = await Task.Run(() => GetElementProperties(operation), cts.Token);

                if (result.Success && result.Data is Dictionary<string, object> properties)
                {
                    return new OperationResult<Dictionary<string, object>>
                    {
                        Success = true,
                        Data = properties
                    };
                }

                return new OperationResult<Dictionary<string, object>>
                {
                    Success = false,
                    Error = result.Error ?? "Failed to get element properties"
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GetElementProperties operation timed out after {Timeout}s", timeoutSeconds);
                return new OperationResult<Dictionary<string, object>>
                {
                    Success = false,
                    Error = $"Operation timed out after {timeoutSeconds} seconds"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element properties");
                return new OperationResult<Dictionary<string, object>>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<OperationResult<List<string>>> GetElementPatternsAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 60)
        {
            try
            {
                _logger.LogInformation("Getting element patterns for ElementId={ElementId}", elementId);

                var operation = new WorkerOperation
                {
                    Operation = "getelementpatterns",
                    Parameters = new Dictionary<string, object>
                    {
                        ["ElementId"] = elementId,
                        ["WindowTitle"] = windowTitle ?? "",
                        ["ProcessId"] = processId ?? 0
                    },
                    Timeout = timeoutSeconds
                };

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var result = await Task.Run(() => GetElementPatterns(operation), cts.Token);

                if (result.Success && result.Data is List<string> patterns)
                {
                    return new OperationResult<List<string>>
                    {
                        Success = true,
                        Data = patterns
                    };
                }

                return new OperationResult<List<string>>
                {
                    Success = false,
                    Error = result.Error ?? "Failed to get element patterns"
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GetElementPatterns operation timed out after {Timeout}s", timeoutSeconds);
                return new OperationResult<List<string>>
                {
                    Success = false,
                    Error = $"Operation timed out after {timeoutSeconds} seconds"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element patterns");
                return new OperationResult<List<string>>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        // Helper methods for element operations
        private WorkerResult GetElementProperties(WorkerOperation operation)
        {
            try
            {
                var searchRoot = _automationHelper.GetSearchRoot(operation);
                if (searchRoot == null)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "Could not find search root"
                    };
                }

                var elementId = operation.Parameters["ElementId"]?.ToString();
                if (string.IsNullOrEmpty(elementId))
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "ElementId is required"
                    };
                }

                var element = FindElementById(searchRoot, elementId);
                if (element == null)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = $"Element with ID '{elementId}' not found"
                    };
                }

                var properties = _elementInfoExtractor.ExtractElementInfo(element);
                return new WorkerResult
                {
                    Success = true,
                    Data = properties
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element properties");
                return new WorkerResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private WorkerResult GetElementPatterns(WorkerOperation operation)
        {
            try
            {
                var searchRoot = _automationHelper.GetSearchRoot(operation);
                if (searchRoot == null)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "Could not find search root"
                    };
                }

                var elementId = operation.Parameters["ElementId"]?.ToString();
                if (string.IsNullOrEmpty(elementId))
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "ElementId is required"
                    };
                }

                var element = FindElementById(searchRoot, elementId);
                if (element == null)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = $"Element with ID '{elementId}' not found"
                    };
                }

                var patterns = GetAvailablePatterns(element);
                return new WorkerResult
                {
                    Success = true,
                    Data = patterns
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element patterns");
                return new WorkerResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private AutomationElement? FindElementById(AutomationElement searchRoot, string elementId)
        {
            try
            {
                // AutomationIdで検索
                var condition = new PropertyCondition(AutomationElement.AutomationIdProperty, elementId);
                var element = searchRoot.FindFirst(TreeScope.Descendants, condition);
                
                if (element != null)
                    return element;

                // Nameで検索
                condition = new PropertyCondition(AutomationElement.NameProperty, elementId);
                return searchRoot.FindFirst(TreeScope.Descendants, condition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding element by ID: {ElementId}", elementId);
                return null;
            }
        }

        private List<string> GetAvailablePatterns(AutomationElement element)
        {
            var patterns = new List<string>();
            var supportedPatterns = element.GetSupportedPatterns();

            foreach (var pattern in supportedPatterns)
            {
                patterns.Add(pattern.ProgrammaticName);
            }

            return patterns;
        }

        // Basic Interactions
        public async Task<OperationResult<string>> InvokeElementAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Invoking element with ElementId={ElementId}", elementId);

                var operation = new WorkerOperation
                {
                    Operation = "invokeelement",
                    Parameters = new Dictionary<string, object>
                    {
                        ["ElementId"] = elementId,
                        ["WindowTitle"] = windowTitle ?? "",
                        ["ProcessId"] = processId ?? 0
                    },
                    Timeout = timeoutSeconds
                };

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var result = await Task.Run(() => InvokeElement(operation), cts.Token);

                return new OperationResult<string>
                {
                    Success = result.Success,
                    Data = result.Success ? "Element invoked successfully" : null,
                    Error = result.Error
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("InvokeElement operation timed out after {Timeout}s", timeoutSeconds);
                return new OperationResult<string>
                {
                    Success = false,
                    Error = $"Operation timed out after {timeoutSeconds} seconds"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invoking element");
                return new OperationResult<string>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private WorkerResult InvokeElement(WorkerOperation operation)
        {
            try
            {
                var searchRoot = _automationHelper.GetSearchRoot(operation);
                if (searchRoot == null)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "Could not find search root"
                    };
                }

                var elementId = operation.Parameters["ElementId"]?.ToString();
                if (string.IsNullOrEmpty(elementId))
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "ElementId is required"
                    };
                }

                var element = FindElementById(searchRoot, elementId);
                if (element == null)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = $"Element with ID '{elementId}' not found"
                    };
                }

                if (element.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern) && pattern is InvokePattern invokePattern)
                {
                    invokePattern.Invoke();
                    return new WorkerResult
                    {
                        Success = true,
                        Data = "Element invoked successfully"
                    };
                }

                return new WorkerResult
                {
                    Success = false,
                    Error = "Element does not support InvokePattern"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invoking element");
                return new WorkerResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        // Placeholder implementations for other methods - to be implemented similarly
        public async Task<OperationResult<string>> ToggleElementAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Toggling element with ElementId={ElementId}", elementId);

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var result = await Task.Run(() => ToggleElement(elementId, windowTitle, processId), cts.Token);

                return new OperationResult<string>
                {
                    Success = result.Success,
                    Data = result.Success ? "Element toggled successfully" : null,
                    Error = result.Error
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("ToggleElement operation timed out after {Timeout}s", timeoutSeconds);
                return new OperationResult<string>
                {
                    Success = false,
                    Error = $"Operation timed out after {timeoutSeconds} seconds"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling element");
                return new OperationResult<string>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<OperationResult<string>> SelectElementAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Selecting element with ElementId={ElementId}", elementId);

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var result = await Task.Run(() => SelectElement(elementId, windowTitle, processId), cts.Token);

                return new OperationResult<string>
                {
                    Success = result.Success,
                    Data = result.Success ? "Element selected successfully" : null,
                    Error = result.Error
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("SelectElement operation timed out after {Timeout}s", timeoutSeconds);
                return new OperationResult<string>
                {
                    Success = false,
                    Error = $"Operation timed out after {timeoutSeconds} seconds"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting element");
                return new OperationResult<string>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private WorkerResult ToggleElement(string elementId, string? windowTitle, int? processId)
        {
            try
            {
                var operation = new WorkerOperation
                {
                    Operation = "toggleelement",
                    Parameters = new Dictionary<string, object>
                    {
                        ["ElementId"] = elementId,
                        ["WindowTitle"] = windowTitle ?? "",
                        ["ProcessId"] = processId ?? 0
                    }
                };

                var searchRoot = _automationHelper.GetSearchRoot(operation);
                if (searchRoot == null)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "Could not find search root"
                    };
                }

                var element = FindElementById(searchRoot, elementId);
                if (element == null)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = $"Element with ID '{elementId}' not found"
                    };
                }

                if (element.TryGetCurrentPattern(TogglePattern.Pattern, out var pattern) && pattern is TogglePattern togglePattern)
                {
                    togglePattern.Toggle();
                    return new WorkerResult
                    {
                        Success = true,
                        Data = "Element toggled successfully"
                    };
                }

                return new WorkerResult
                {
                    Success = false,
                    Error = "Element does not support TogglePattern"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling element");
                return new WorkerResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private WorkerResult SelectElement(string elementId, string? windowTitle, int? processId)
        {
            try
            {
                var operation = new WorkerOperation
                {
                    Operation = "selectelement",
                    Parameters = new Dictionary<string, object>
                    {
                        ["ElementId"] = elementId,
                        ["WindowTitle"] = windowTitle ?? "",
                        ["ProcessId"] = processId ?? 0
                    }
                };

                var searchRoot = _automationHelper.GetSearchRoot(operation);
                if (searchRoot == null)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "Could not find search root"
                    };
                }

                var element = FindElementById(searchRoot, elementId);
                if (element == null)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = $"Element with ID '{elementId}' not found"
                    };
                }

                if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) && pattern is SelectionItemPattern selectionPattern)
                {
                    selectionPattern.Select();
                    return new WorkerResult
                    {
                        Success = true,
                        Data = "Element selected successfully"
                    };
                }

                return new WorkerResult
                {
                    Success = false,
                    Error = "Element does not support SelectionItemPattern"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting element");
                return new WorkerResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<OperationResult<string>> SetElementValueAsync(string elementId, string value, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Setting element value for ElementId={ElementId}, Value={Value}", elementId, value);

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var result = await Task.Run(() => SetElementValue(elementId, value, windowTitle, processId), cts.Token);

                return new OperationResult<string>
                {
                    Success = result.Success,
                    Data = result.Success ? "Element value set successfully" : null,
                    Error = result.Error
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("SetElementValue operation timed out after {Timeout}s", timeoutSeconds);
                return new OperationResult<string>
                {
                    Success = false,
                    Error = $"Operation timed out after {timeoutSeconds} seconds"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting element value");
                return new OperationResult<string>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<OperationResult<string>> GetElementValueAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting element value for ElementId={ElementId}", elementId);

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var result = await Task.Run(() => GetElementValue(elementId, windowTitle, processId), cts.Token);

                if (result.Success && result.Data is string elementValue)
                {
                    return new OperationResult<string>
                    {
                        Success = true,
                        Data = elementValue
                    };
                }

                return new OperationResult<string>
                {
                    Success = false,
                    Error = result.Error ?? "Failed to get element value"
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GetElementValue operation timed out after {Timeout}s", timeoutSeconds);
                return new OperationResult<string>
                {
                    Success = false,
                    Error = $"Operation timed out after {timeoutSeconds} seconds"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element value");
                return new OperationResult<string>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private WorkerResult SetElementValue(string elementId, string value, string? windowTitle, int? processId)
        {
            try
            {
                var operation = new WorkerOperation
                {
                    Operation = "setelementvalue",
                    Parameters = new Dictionary<string, object>
                    {
                        ["ElementId"] = elementId,
                        ["Value"] = value,
                        ["WindowTitle"] = windowTitle ?? "",
                        ["ProcessId"] = processId ?? 0
                    }
                };

                var searchRoot = _automationHelper.GetSearchRoot(operation);
                if (searchRoot == null)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "Could not find search root"
                    };
                }

                var element = FindElementById(searchRoot, elementId);
                if (element == null)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = $"Element with ID '{elementId}' not found"
                    };
                }

                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern) && pattern is ValuePattern valuePattern)
                {
                    valuePattern.SetValue(value);
                    return new WorkerResult
                    {
                        Success = true,
                        Data = "Element value set successfully"
                    };
                }

                return new WorkerResult
                {
                    Success = false,
                    Error = "Element does not support ValuePattern"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting element value");
                return new WorkerResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private WorkerResult GetElementValue(string elementId, string? windowTitle, int? processId)
        {
            try
            {
                var operation = new WorkerOperation
                {
                    Operation = "getelementvalue",
                    Parameters = new Dictionary<string, object>
                    {
                        ["ElementId"] = elementId,
                        ["WindowTitle"] = windowTitle ?? "",
                        ["ProcessId"] = processId ?? 0
                    }
                };

                var searchRoot = _automationHelper.GetSearchRoot(operation);
                if (searchRoot == null)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "Could not find search root"
                    };
                }

                var element = FindElementById(searchRoot, elementId);
                if (element == null)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = $"Element with ID '{elementId}' not found"
                    };
                }

                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern) && pattern is ValuePattern valuePattern)
                {
                    var currentValue = valuePattern.Current.Value;
                    return new WorkerResult
                    {
                        Success = true,
                        Data = currentValue ?? ""
                    };
                }

                return new WorkerResult
                {
                    Success = false,
                    Error = "Element does not support ValuePattern"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element value");
                return new WorkerResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public Task<OperationResult<string>> ExpandCollapseElementAsync(string elementId, bool? expand = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            throw new NotImplementedException("ExpandCollapseElement will be implemented based on pattern handlers");
        }

        public Task<OperationResult<string>> ScrollElementAsync(string elementId, string? direction = null, double? horizontal = null, double? vertical = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            throw new NotImplementedException("ScrollElement will be implemented based on pattern handlers");
        }

        public Task<OperationResult<string>> ScrollElementIntoViewAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            throw new NotImplementedException("ScrollElementIntoView will be implemented based on pattern handlers");
        }

        public Task<OperationResult<string>> SetRangeValueAsync(string elementId, double value, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            throw new NotImplementedException("SetRangeValue will be implemented based on pattern handlers");
        }

        public Task<OperationResult<Dictionary<string, object>>> GetRangeValueAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            throw new NotImplementedException("GetRangeValue will be implemented based on pattern handlers");
        }

        public async Task<OperationResult<List<WindowInfo>>> GetWindowsAsync(int timeoutSeconds = 60)
        {
            try
            {
                _logger.LogInformation("DirectUIAutomationService.GetWindowsAsync called with timeout={Timeout}", timeoutSeconds);

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var result = await Task.Run(() => GetWindows(), cts.Token);

                if (result.Success && result.Data is List<WindowInfo> windows)
                {
                    return new OperationResult<List<WindowInfo>>
                    {
                        Success = true,
                        Data = windows
                    };
                }

                return new OperationResult<List<WindowInfo>>
                {
                    Success = false,
                    Error = result.Error ?? "Failed to get windows"
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GetWindows operation timed out after {Timeout}s", timeoutSeconds);
                return new OperationResult<List<WindowInfo>>
                {
                    Success = false,
                    Error = $"Operation timed out after {timeoutSeconds} seconds"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting windows");
                return new OperationResult<List<WindowInfo>>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private WorkerResult GetWindows()
        {
            try
            {
                var windows = new List<WindowInfo>();
                var desktopChildren = AutomationElement.RootElement.FindAll(
                    TreeScope.Children, 
                    Condition.TrueCondition);

                if (desktopChildren != null)
                {
                    foreach (AutomationElement window in desktopChildren)
                    {
                        try
                        {
                            var windowInfo = new WindowInfo
                            {
                                Title = window.Current.Name ?? "",
                                ProcessId = window.Current.ProcessId,
                                ClassName = window.Current.ClassName ?? "",
                                AutomationId = window.Current.AutomationId ?? "",
                                IsVisible = !window.Current.IsOffscreen,
                                IsEnabled = window.Current.IsEnabled,
                                BoundingRectangle = new BoundingRectangle
                                {
                                    X = window.Current.BoundingRectangle.X,
                                    Y = window.Current.BoundingRectangle.Y,
                                    Width = window.Current.BoundingRectangle.Width,
                                    Height = window.Current.BoundingRectangle.Height
                                }
                            };
                            windows.Add(windowInfo);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "Failed to extract info for window");
                        }
                    }
                }

                return new WorkerResult
                {
                    Success = true,
                    Data = windows
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting windows");
                return new WorkerResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public Task<OperationResult<string>> SetWindowStateAsync(string elementId, string state, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            throw new NotImplementedException("SetWindowState will be implemented based on window handlers");
        }

        public Task<OperationResult<string>> TransformElementAsync(string elementId, string action, double? x = null, double? y = null, double? width = null, double? height = null, double? degrees = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            throw new NotImplementedException("TransformElement will be implemented based on transform handlers");
        }

        public Task<OperationResult<string>> DockElementAsync(string elementId, string position, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            throw new NotImplementedException("DockElement will be implemented based on dock handlers");
        }

        public Task<OperationResult<string>> GetTextAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            throw new NotImplementedException("GetText will be implemented based on text handlers");
        }

        public Task<OperationResult<string>> SelectTextAsync(string elementId, int startIndex, int length, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            throw new NotImplementedException("SelectText will be implemented based on text handlers");
        }

        public Task<OperationResult<Dictionary<string, object>>> FindTextAsync(string elementId, string searchText, bool backward = false, bool ignoreCase = false, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            throw new NotImplementedException("FindText will be implemented based on text handlers");
        }

        public Task<OperationResult<List<Dictionary<string, object>>>> GetTextSelectionAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            throw new NotImplementedException("GetTextSelection will be implemented based on text handlers");
        }

        public async Task<OperationResult<string>> LaunchApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null, int timeoutSeconds = 60)
        {
            try
            {
                _logger.LogInformation("Launching application: {ApplicationPath}", applicationPath);

                // ApplicationLauncherサービスは依存関係に含まれていないので、直接実装
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var result = await Task.Run(() => LaunchApplication(applicationPath, arguments, workingDirectory), cts.Token);

                return new OperationResult<string>
                {
                    Success = result.Success,
                    Data = result.Success ? $"Application launched successfully. ProcessId: {result.Data}" : null,
                    Error = result.Error
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("LaunchApplication operation timed out after {Timeout}s", timeoutSeconds);
                return new OperationResult<string>
                {
                    Success = false,
                    Error = $"Operation timed out after {timeoutSeconds} seconds"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error launching application");
                return new OperationResult<string>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private WorkerResult LaunchApplication(string applicationPath, string? arguments, string? workingDirectory)
        {
            try
            {
                if (!File.Exists(applicationPath))
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = $"Application not found: {applicationPath}"
                    };
                }

                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = applicationPath,
                    Arguments = arguments ?? "",
                    WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(applicationPath) ?? "",
                    UseShellExecute = false,
                    CreateNoWindow = false
                };

                var process = System.Diagnostics.Process.Start(startInfo);
                if (process == null)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "Failed to start process"
                    };
                }

                return new WorkerResult
                {
                    Success = true,
                    Data = process.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error launching application");
                return new WorkerResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<OperationResult<string>> TakeScreenshotAsync(string? windowTitle = null, int? processId = null, string? outputPath = null, int maxTokens = 0, int timeoutSeconds = 60)
        {
            try
            {
                var result = await _screenshotService.TakeScreenshotAsync(windowTitle, outputPath, maxTokens, processId, timeoutSeconds);
                
                return new OperationResult<string>
                {
                    Success = result.Success,
                    Data = result.Success ? (result.OutputPath ?? result.Base64Image ?? "Screenshot taken successfully") : null,
                    Error = result.Error
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error taking screenshot");
                return new OperationResult<string>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }
    }
}
