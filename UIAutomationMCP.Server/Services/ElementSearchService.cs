using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services
{
    public class ElementSearchService : IElementSearchService
    {
        private readonly ILogger<ElementSearchService> _logger;
        private readonly ISubprocessExecutor _executor;
        private readonly IOptions<UIAutomationOptions> _options;

        public ElementSearchService(ILogger<ElementSearchService> logger, ISubprocessExecutor executor, IOptions<UIAutomationOptions> options)
        {
            _logger = logger;
            _executor = executor;
            _options = options;
        }

        public async Task<ServerEnhancedResponse<DesktopWindowsResult>> GetWindowsAsync(int timeoutSeconds = 60)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, "Starting GetWindows operation");

                var request = new GetDesktopWindowsRequest { IncludeInvisible = false };
                var result = await _executor.ExecuteAsync<GetDesktopWindowsRequest, DesktopWindowsResult>("GetDesktopWindows", request, timeoutSeconds);
                
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
                            ["windowsFound"] = result.Windows?.Count ?? 0
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetWindows",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["timeoutSeconds"] = timeoutSeconds
                        }
                    }
                };

                _logger.LogInformationWithOperation(operationId, $"Completed GetWindows operation. Success={result.Success}, WindowsFound={result.Windows?.Count ?? 0}");
                return serverResponse;
                
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "Error in GetWindows operation");
                
                var errorResponse = new ServerEnhancedResponse<DesktopWindowsResult>
                {
                    Success = false,
                    Data = new DesktopWindowsResult { Success = false, ErrorMessage = ex.Message },
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["error"] = ex.Message,
                            ["exception"] = ex.GetType().Name
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetWindows",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["timeoutSeconds"] = timeoutSeconds
                        }
                    }
                };
                
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> FindElementsAsync(string? windowTitle = null, string? searchText = null, string? controlType = null, int? processId = null, int timeoutSeconds = 60)
        {
            // Legacy method - delegate to enhanced version with defaults
            return await FindElementsAsync(windowTitle, searchText, controlType, processId, "descendants", true, 100, true, timeoutSeconds);
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> FindElementsAsync(string? windowTitle = null, string? searchText = null, string? controlType = null, int? processId = null, string scope = "descendants", bool validatePatterns = true, int maxResults = 100, bool useCache = true, int timeoutSeconds = 60)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting FindElements with WindowTitle={windowTitle}, SearchText={searchText}, ControlType={controlType}, ProcessId={processId}");

                // Create a FindElementsRequest with the provided parameters
                var request = new FindElementsRequest
                {
                    WindowTitle = windowTitle ?? "",
                    SearchText = searchText ?? "",
                    ControlType = controlType ?? "",
                    ProcessId = processId ?? 0,
                    Scope = scope,
                    MaxResults = maxResults,
                    UseCache = useCache,
                    ValidatePatterns = validatePatterns,
                    UseRegex = false,
                    UseWildcard = false
                };

                var result = await _executor.ExecuteAsync<FindElementsRequest, ElementSearchResult>("FindElements", request, timeoutSeconds);
                
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
                        }
                    }
                };

                _logger.LogInformationWithOperation(operationId, $"Completed FindElements operation. Success={result.Success}, ElementsFound={result.Count}");
                return serverResponse;
                
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "Error in FindElements operation");
                
                var errorResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = false,
                    Data = new ElementSearchResult { Success = false, ErrorMessage = ex.Message },
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["error"] = ex.Message,
                            ["exception"] = ex.GetType().Name,
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
                        }
                    }
                };
                
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<SearchElementsResult>> SearchElementsAsync(SearchElementsRequest request, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, "Starting SearchElements operation");

                var result = await _executor.ExecuteAsync<SearchElementsRequest, SearchElementsResult>("SearchElements", request, timeoutSeconds);
                
                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<SearchElementsResult>
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
                            ["searchCriteria"] = BuildSearchCriteria(request),
                            ["resultsCount"] = result.Elements?.Length ?? 0,
                            ["wasTruncated"] = result.Metadata?.WasTruncated ?? false
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "SearchElements",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["searchText"] = request.SearchText ?? "",
                            ["automationId"] = request.AutomationId ?? "",
                            ["name"] = request.Name ?? "",
                            ["controlType"] = request.ControlType ?? "",
                            ["processId"] = request.ProcessId ?? 0,
                            ["maxResults"] = request.MaxResults,
                            ["visibleOnly"] = request.VisibleOnly
                        }
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, "SearchElements completed successfully");
                return serverResponse;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "SearchElements operation failed");
                
                var errorResponse = new ServerEnhancedResponse<SearchElementsResult>
                {
                    Success = false,
                    Data = new SearchElementsResult { Elements = [], Metadata = new SearchMetadata { SearchCriteria = BuildSearchCriteria(request) } },
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["error"] = ex.Message,
                            ["exception"] = ex.GetType().Name,
                            ["searchCriteria"] = BuildSearchCriteria(request)
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "SearchElements",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["searchText"] = request.SearchText ?? "",
                            ["automationId"] = request.AutomationId ?? "",
                            ["name"] = request.Name ?? "",
                            ["controlType"] = request.ControlType ?? "",
                            ["processId"] = request.ProcessId ?? 0
                        }
                    }
                };
                
                return errorResponse;
            }
        }


        private static string BuildSearchCriteria(SearchElementsRequest request)
        {
            var criteria = new List<string>();
            
            if (!string.IsNullOrEmpty(request.SearchText))
                criteria.Add($"SearchText='{request.SearchText}'");
            if (!string.IsNullOrEmpty(request.AutomationId))
                criteria.Add($"AutomationId='{request.AutomationId}'");
            if (!string.IsNullOrEmpty(request.Name))
                criteria.Add($"Name='{request.Name}'");
            if (!string.IsNullOrEmpty(request.ControlType))
                criteria.Add($"ControlType='{request.ControlType}'");
            if (request.ProcessId.HasValue)
                criteria.Add($"ProcessId={request.ProcessId}");
            if (request.RequiredPatterns?.Length > 0)
                criteria.Add($"RequiredPatterns=[{string.Join(", ", request.RequiredPatterns)}]");
            if (request.VisibleOnly)
                criteria.Add("VisibleOnly=true");
                
            return string.Join(", ", criteria);
        }

    }
}