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

                var result = await _executor.ExecuteAsync<ElementSearchResult>("FindElements", request, timeoutSeconds);
                
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
    }
}