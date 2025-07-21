using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using System.Diagnostics;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class TextService : ITextService
    {
        private readonly ILogger<TextService> _logger;
        private readonly SubprocessExecutor _executor;

        public TextService(ILogger<TextService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<ServerEnhancedResponse<TextInfoResult>> GetTextAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"GetText validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<TextInfoResult>
                {
                    Success = false,
                    ErrorMessage = validationError,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["errorCategory"] = "Validation",
                            ["automationId"] = automationId ?? "<null>",
                            ["name"] = name ?? "<null>",
                            ["validationFailed"] = true
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetText",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["controlType"] = controlType ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Getting text from element with AutomationId: {automationId}, Name: {name}");

                var request = new GetTextRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var response = await _executor.ExecuteAsync<GetTextRequest, TextInfoResult>("GetText", request, timeoutSeconds);

                stopwatch.Stop();
                _logger.LogInformationWithOperation(operationId, $"GetText completed in {stopwatch.Elapsed.TotalMilliseconds:F1}ms");

                return new ServerEnhancedResponse<TextInfoResult>
                {
                    Success = response.Success,
                    Data = response,
                    ErrorMessage = response.ErrorMessage,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["operationType"] = "GetText",
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["success"] = response.Success
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetText",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["controlType"] = controlType ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var errorMessage = $"Failed to get text from element: {ex.Message}";
                _logger.LogErrorWithOperation(operationId, ex, errorMessage);

                return new ServerEnhancedResponse<TextInfoResult>
                {
                    Success = false,
                    ErrorMessage = errorMessage,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["errorCategory"] = "Exception",
                            ["exceptionType"] = ex.GetType().Name,
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? ""
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetText",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["controlType"] = controlType ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
            }
        }

        public async Task<ServerEnhancedResponse<TextInfoResult>> GetSelectedTextAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"GetSelectedText validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<TextInfoResult>
                {
                    Success = false,
                    ErrorMessage = validationError,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["errorCategory"] = "Validation",
                            ["automationId"] = automationId ?? "<null>",
                            ["name"] = name ?? "<null>",
                            ["validationFailed"] = true
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetSelectedText",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["controlType"] = controlType ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Getting selected text from element with AutomationId: {automationId}, Name: {name}");

                var request = new GetTextRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var response = await _executor.ExecuteAsync<GetTextRequest, TextInfoResult>("GetSelectedText", request, timeoutSeconds);

                stopwatch.Stop();
                _logger.LogInformationWithOperation(operationId, $"GetSelectedText completed in {stopwatch.Elapsed.TotalMilliseconds:F1}ms");

                return new ServerEnhancedResponse<TextInfoResult>
                {
                    Success = response.Success,
                    Data = response,
                    ErrorMessage = response.ErrorMessage,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["operationType"] = "GetSelectedText",
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["success"] = response.Success
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetSelectedText",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["controlType"] = controlType ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var errorMessage = $"Failed to get selected text from element: {ex.Message}";
                _logger.LogErrorWithOperation(operationId, ex, errorMessage);

                return new ServerEnhancedResponse<TextInfoResult>
                {
                    Success = false,
                    ErrorMessage = errorMessage,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["errorCategory"] = "Exception",
                            ["exceptionType"] = ex.GetType().Name,
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? ""
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetSelectedText",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["controlType"] = controlType ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
            }
        }

        public async Task<ServerEnhancedResponse<ActionResult>> SelectTextAsync(string? automationId = null, string? name = null, int startIndex = 0, int length = 1, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"SelectText validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<ActionResult>
                {
                    Success = false,
                    ErrorMessage = validationError,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["errorCategory"] = "Validation",
                            ["automationId"] = automationId ?? "<null>",
                            ["name"] = name ?? "<null>",
                            ["validationFailed"] = true
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "SelectText",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["startIndex"] = startIndex,
                            ["length"] = length,
                            ["controlType"] = controlType ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                return validationResponse;
            }

            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting SelectText: AutomationId={automationId}, Name={name}, StartIndex={startIndex}, Length={length}");

                var request = new SelectTextRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    StartIndex = startIndex,
                    Length = length,
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<SelectTextRequest, ActionResult>("SelectText", request, timeoutSeconds);

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<ActionResult>
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
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["startIndex"] = startIndex,
                            ["length"] = length,
                            ["operationType"] = "selectText"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "SelectText",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["startIndex"] = startIndex,
                            ["length"] = length,
                            ["controlType"] = controlType ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };

                _logger.LogInformationWithOperation(operationId, $"Successfully created enhanced response");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return serverResponse;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "Error in SelectText operation");
                
                var errorResponse = new ServerEnhancedResponse<ActionResult>
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
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["startIndex"] = startIndex,
                            ["length"] = length,
                            ["operationType"] = "selectText"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "SelectText",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["startIndex"] = startIndex,
                            ["length"] = length,
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

        public async Task<ServerEnhancedResponse<ActionResult>> SetTextAsync(string? automationId = null, string? name = null, string text = "", string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"SetText validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<ActionResult>
                {
                    Success = false,
                    ErrorMessage = validationError,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["errorCategory"] = "Validation",
                            ["automationId"] = automationId ?? "<null>",
                            ["name"] = name ?? "<null>",
                            ["validationFailed"] = true
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "SetText",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["text"] = text ?? "",
                            ["controlType"] = controlType ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                return validationResponse;
            }

            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting SetText: AutomationId={automationId}, Name={name}, TextLength={text?.Length ?? 0}");

                var request = new SetTextRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    Text = text ?? "",
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<SetTextRequest, ActionResult>("SetText", request, timeoutSeconds);

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<ActionResult>
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
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["textLength"] = text?.Length ?? 0,
                            ["operationType"] = "setText"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "SetText",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["text"] = text ?? "",
                            ["controlType"] = controlType ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };

                _logger.LogInformationWithOperation(operationId, $"Successfully created enhanced response");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return serverResponse;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "Error in SetText operation");
                
                var errorResponse = new ServerEnhancedResponse<ActionResult>
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
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["textLength"] = text?.Length ?? 0,
                            ["operationType"] = "setText"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "SetText",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["text"] = text ?? "",
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

        public async Task<ServerEnhancedResponse<ActionResult>> AppendTextAsync(string? automationId = null, string? name = null, string text = "", string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"AppendText validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<ActionResult>
                {
                    Success = false,
                    ErrorMessage = validationError,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["errorCategory"] = "Validation",
                            ["automationId"] = automationId ?? "<null>",
                            ["name"] = name ?? "<null>",
                            ["validationFailed"] = true
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "AppendText",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["text"] = text ?? "",
                            ["controlType"] = controlType ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                return validationResponse;
            }

            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting AppendText: AutomationId={automationId}, Name={name}, TextLength={text?.Length ?? 0}");

                var request = new SetTextRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    Text = text ?? "",
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<SetTextRequest, ActionResult>("AppendText", request, timeoutSeconds);

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<ActionResult>
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
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["textLength"] = text?.Length ?? 0,
                            ["operationType"] = "appendText"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "AppendText",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["text"] = text ?? "",
                            ["controlType"] = controlType ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };

                _logger.LogInformationWithOperation(operationId, $"Successfully created enhanced response");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return serverResponse;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "Error in AppendText operation");
                
                var errorResponse = new ServerEnhancedResponse<ActionResult>
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
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["textLength"] = text?.Length ?? 0,
                            ["operationType"] = "appendText"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "AppendText",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["text"] = text ?? "",
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