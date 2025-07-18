using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using System.Text.Json;
using System.Diagnostics;

namespace UIAutomationMCP.Server.Services
{
    public class ElementSearchService : IElementSearchService
    {
        private readonly ILogger<ElementSearchService> _logger;
        private readonly SubprocessExecutor _executor;
        private readonly IOptions<UIAutomationOptions> _options;

        public ElementSearchService(
            ILogger<ElementSearchService> logger,
            SubprocessExecutor executor,
            IOptions<UIAutomationOptions> options)
        {
            _logger = logger;
            _executor = executor;
            _options = options;
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> FindElementAsync(string? windowTitle = null, int? processId = null, string? name = null, string? automationId = null, string? className = null, string? controlType = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting FindElement with WindowTitle={windowTitle}, ProcessId={processId}, Name={name}, AutomationId={automationId}, ClassName={className}, ControlType={controlType}");

                var parameters = new Dictionary<string, object>
                {
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 },
                    { "name", name ?? "" },
                    { "automationId", automationId ?? "" },
                    { "className", className ?? "" },
                    { "controlType", controlType ?? "" }
                };

                var result = await _executor.ExecuteAsync<ElementSearchResult>("FindElement", parameters, timeoutSeconds);

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = result.Success,
                    Data = result,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["elementsFound"] = result.Count,
                            ["searchCriteria"] = $"WindowTitle={windowTitle}, ProcessId={processId}, Name={name}, AutomationId={automationId}, ClassName={className}, ControlType={controlType}"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "FindElement",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["name"] = name ?? "",
                            ["automationId"] = automationId ?? "",
                            ["className"] = className ?? "",
                            ["controlType"] = controlType ?? "",
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };

                _logger.LogInformationWithOperation(operationId, "Successfully created enhanced response");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return serverResponse;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "Error in FindElement operation");
                
                var errorResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["exceptionType"] = ex.GetType().Name,
                            ["stackTrace"] = ex.StackTrace ?? "",
                            ["searchCriteria"] = $"WindowTitle={windowTitle}, ProcessId={processId}, Name={name}, AutomationId={automationId}, ClassName={className}, ControlType={controlType}"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "FindElement",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["name"] = name ?? "",
                            ["automationId"] = automationId ?? "",
                            ["className"] = className ?? "",
                            ["controlType"] = controlType ?? "",
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> FindAllElementsAsync(string? windowTitle = null, int? processId = null, string? name = null, string? automationId = null, string? className = null, string? controlType = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting FindAllElements with WindowTitle={windowTitle}, ProcessId={processId}, Name={name}, AutomationId={automationId}, ClassName={className}, ControlType={controlType}");

                var parameters = new Dictionary<string, object>
                {
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 },
                    { "name", name ?? "" },
                    { "automationId", automationId ?? "" },
                    { "className", className ?? "" },
                    { "controlType", controlType ?? "" }
                };

                var result = await _executor.ExecuteAsync<ElementSearchResult>("FindAllElements", parameters, timeoutSeconds);

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = result.Success,
                    Data = result,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["elementsFound"] = result.Count,
                            ["totalResults"] = result.Items?.Count ?? 0,
                            ["searchCriteria"] = $"WindowTitle={windowTitle}, ProcessId={processId}, Name={name}, AutomationId={automationId}, ClassName={className}, ControlType={controlType}"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "FindAllElements",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["name"] = name ?? "",
                            ["automationId"] = automationId ?? "",
                            ["className"] = className ?? "",
                            ["controlType"] = controlType ?? "",
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };

                _logger.LogInformationWithOperation(operationId, "Successfully created enhanced response");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return serverResponse;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "Error in FindAllElements operation");
                
                var errorResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["exceptionType"] = ex.GetType().Name,
                            ["stackTrace"] = ex.StackTrace ?? "",
                            ["searchCriteria"] = $"WindowTitle={windowTitle}, ProcessId={processId}, Name={name}, AutomationId={automationId}, ClassName={className}, ControlType={controlType}"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "FindAllElements",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["name"] = name ?? "",
                            ["automationId"] = automationId ?? "",
                            ["className"] = className ?? "",
                            ["controlType"] = controlType ?? "",
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> FindElementByXPathAsync(string xpath, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting FindElementByXPath with XPath={xpath}, WindowTitle={windowTitle}, ProcessId={processId}");

                var parameters = new Dictionary<string, object>
                {
                    { "xpath", xpath },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<ElementSearchResult>("FindElementByXPath", parameters, timeoutSeconds);

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = result.Success,
                    Data = result,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["elementsFound"] = result.Count,
                            ["xpath"] = xpath,
                            ["searchCriteria"] = $"XPath={xpath}, WindowTitle={windowTitle}, ProcessId={processId}"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "FindElementByXPath",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["xpath"] = xpath,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };

                _logger.LogInformationWithOperation(operationId, "Successfully created enhanced response");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return serverResponse;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "Error in FindElementByXPath operation");
                
                var errorResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["exceptionType"] = ex.GetType().Name,
                            ["stackTrace"] = ex.StackTrace ?? "",
                            ["xpath"] = xpath,
                            ["searchCriteria"] = $"XPath={xpath}, WindowTitle={windowTitle}, ProcessId={processId}"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "FindElementByXPath",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["xpath"] = xpath,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> FindElementsByTagNameAsync(string tagName, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting FindElementsByTagName with TagName={tagName}, WindowTitle={windowTitle}, ProcessId={processId}");

                var parameters = new Dictionary<string, object>
                {
                    { "tagName", tagName },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<ElementSearchResult>("FindElementsByTagName", parameters, timeoutSeconds);

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = result.Success,
                    Data = result,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["elementsFound"] = result.Count,
                            ["totalResults"] = result.Items?.Count ?? 0,
                            ["tagName"] = tagName,
                            ["searchCriteria"] = $"TagName={tagName}, WindowTitle={windowTitle}, ProcessId={processId}"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "FindElementsByTagName",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["tagName"] = tagName,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };

                _logger.LogInformationWithOperation(operationId, "Successfully created enhanced response");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return serverResponse;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "Error in FindElementsByTagName operation");
                
                var errorResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["exceptionType"] = ex.GetType().Name,
                            ["stackTrace"] = ex.StackTrace ?? "",
                            ["tagName"] = tagName,
                            ["searchCriteria"] = $"TagName={tagName}, WindowTitle={windowTitle}, ProcessId={processId}"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "FindElementsByTagName",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["tagName"] = tagName,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<DesktopWindowsResult>> GetWindowsAsync(int timeoutSeconds = 60)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, "Starting GetWindows operation");

                var parameters = new Dictionary<string, object>
                {
                    { "operation", "GetDesktopWindows" },
                    { "includeInvisible", false }
                };

                var result = await _executor.ExecuteAsync<DesktopWindowsResult>("GetDesktopWindows", parameters, timeoutSeconds);

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<DesktopWindowsResult>
                {
                    Success = result.Success,
                    Data = result,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["windowsFound"] = result.Windows?.Count ?? 0,
                            ["includeInvisible"] = false
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetWindows",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["timeoutSeconds"] = timeoutSeconds,
                            ["includeInvisible"] = false
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };

                _logger.LogInformationWithOperation(operationId, "Successfully created enhanced response");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return serverResponse;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "Error in GetWindows operation");
                
                var errorResponse = new ServerEnhancedResponse<DesktopWindowsResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["exceptionType"] = ex.GetType().Name,
                            ["stackTrace"] = ex.StackTrace ?? ""
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetWindows",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["timeoutSeconds"] = timeoutSeconds,
                            ["includeInvisible"] = false
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorResponse;
            }
        }

        // New typed method with default application
        public async Task<ServerEnhancedResponse<ElementSearchResult>> FindElementsAsync(FindElementsRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                // Apply defaults at server level
                if (string.IsNullOrEmpty(request.Scope))
                    request.Scope = _options.Value.ElementSearch.DefaultScope;
                if (request.UseCache == default)
                    request.UseCache = _options.Value.ElementSearch.UseCache;
                if (request.UseRegex == default)
                    request.UseRegex = _options.Value.ElementSearch.UseRegex;
                if (request.UseWildcard == default)
                    request.UseWildcard = _options.Value.ElementSearch.UseWildcard;
                if (request.MaxResults == default)
                    request.MaxResults = _options.Value.ElementSearch.MaxResults;
                if (request.ValidatePatterns == default)
                    request.ValidatePatterns = _options.Value.ElementSearch.ValidatePatterns;
                
                _logger.LogInformationWithOperation(operationId, $"Starting FindElements with WindowTitle={request.WindowTitle}, SearchText={request.SearchText}, ControlType={request.ControlType}, ProcessId={request.ProcessId}");

                // Pass typed request to subprocess
                var result = await _executor.ExecuteAsync<ElementSearchResult>("FindElements", request, request.TimeoutSeconds);

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = result.Success,
                    Data = result,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId)
                    }
                };

                return serverResponse;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithOperation(operationId, ex, "Error in FindElements");
                stopwatch.Stop();
                
                return new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = false,
                    Data = new ElementSearchResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message
                    },
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId)
                    }
                };
            }
        }
        
        // Legacy method for backward compatibility
        public async Task<ServerEnhancedResponse<ElementSearchResult>> FindElementsAsync(string? windowTitle = null, string? searchText = null, string? controlType = null, int? processId = null, int timeoutSeconds = 60)
        {
            var request = new FindElementsRequest
            {
                WindowTitle = windowTitle,
                SearchText = searchText,
                ControlType = controlType,
                ProcessId = processId,
                TimeoutSeconds = timeoutSeconds
            };
            
            return await FindElementsAsync(request);
        }

        // Original method preserved for compatibility
        public async Task<ServerEnhancedResponse<ElementSearchResult>> FindElementsAsync_Original(string? windowTitle = null, string? searchText = null, string? controlType = null, int? processId = null, int timeoutSeconds = 60)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting FindElements with WindowTitle={windowTitle}, SearchText={searchText}, ControlType={controlType}, ProcessId={processId}");

                var parameters = new Dictionary<string, object>
                {
                    { "windowTitle", windowTitle ?? "" },
                    { "searchText", searchText ?? "" },
                    { "controlType", controlType ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<ElementSearchResult>("FindElements", parameters, timeoutSeconds);

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = result.Success,
                    Data = result,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["elementsFound"] = result.Count,
                            ["totalResults"] = result.Items?.Count ?? 0,
                            ["searchCriteria"] = $"WindowTitle={windowTitle}, SearchText={searchText}, ControlType={controlType}, ProcessId={processId}"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "FindElements",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["windowTitle"] = windowTitle ?? "",
                            ["searchText"] = searchText ?? "",
                            ["controlType"] = controlType ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };

                _logger.LogInformationWithOperation(operationId, "Successfully created enhanced response");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return serverResponse;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "Error in FindElements operation");
                
                var errorResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["exceptionType"] = ex.GetType().Name,
                            ["stackTrace"] = ex.StackTrace ?? "",
                            ["searchCriteria"] = $"WindowTitle={windowTitle}, SearchText={searchText}, ControlType={controlType}, ProcessId={processId}"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "FindElements",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["windowTitle"] = windowTitle ?? "",
                            ["searchText"] = searchText ?? "",
                            ["controlType"] = controlType ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorResponse;
            }
        }

    }
}
