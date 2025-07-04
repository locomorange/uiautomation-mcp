using System.Diagnostics;
using System.Text.Json;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;

namespace UiAutomationWorker
{
    /// <summary>
    /// Standalone worker process for UI Automation operations
    /// Prevents main process from hanging due to COM/native API blocking
    /// </summary>
    class Program
    {
        private static ILogger? _logger;

        static async Task<int> Main(string[] args)
        {
            // Initialize logging
            using var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            _logger = loggerFactory.CreateLogger<Program>();

            try
            {
                _logger.LogInformation("[UiAutomationWorker] Worker process starting, PID: {ProcessId}", 
                    Environment.ProcessId);

                // Read operation data from stdin
                var inputJson = await Console.In.ReadToEndAsync();
                
                if (string.IsNullOrEmpty(inputJson))
                {
                    _logger.LogError("[UiAutomationWorker] No input data received");
                    await Console.Error.WriteLineAsync("No input data received");
                    return 1;
                }

                _logger.LogInformation("[UiAutomationWorker] Processing operation: {InputLength} chars", inputJson.Length);

                // Parse operation
                var operation = JsonSerializer.Deserialize<WorkerOperation>(inputJson);
                if (operation == null)
                {
                    _logger.LogError("[UiAutomationWorker] Failed to parse operation JSON");
                    await Console.Error.WriteLineAsync("Failed to parse operation JSON");
                    return 1;
                }

                // Execute operation with timeout
                var result = await ExecuteOperationAsync(operation);
                
                // Output result
                var resultJson = JsonSerializer.Serialize(result);
                await Console.Out.WriteLineAsync(resultJson);
                
                _logger.LogInformation("[UiAutomationWorker] Operation completed successfully");
                return 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[UiAutomationWorker] Unhandled exception");
                await Console.Error.WriteLineAsync($"Unhandled exception: {ex.Message}");
                return 1;
            }
        }

        private static async Task<WorkerResult> ExecuteOperationAsync(WorkerOperation operation)
        {
            try
            {
                _logger?.LogInformation("[UiAutomationWorker] Executing operation: {Operation}", operation.Operation);

                return operation.Operation.ToLowerInvariant() switch
                {
                    "findfirst" => await ExecuteFindFirstAsync(operation),
                    "findall" => await ExecuteFindAllAsync(operation),
                    "getproperties" => await ExecuteGetPropertiesAsync(operation),
                    "invoke" => await ExecuteInvokeAsync(operation),
                    "setvalue" => await ExecuteSetValueAsync(operation),
                    _ => new WorkerResult
                    {
                        Success = false,
                        Error = $"Unknown operation: {operation.Operation}"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[UiAutomationWorker] Operation execution failed");
                return new WorkerResult
                {
                    Success = false,
                    Error = $"Operation execution failed: {ex.Message}"
                };
            }
        }

        private static async Task<WorkerResult> ExecuteFindFirstAsync(WorkerOperation operation)
        {
            _logger?.LogInformation("[UiAutomationWorker] Executing FindFirst operation");

            try
            {
                // Set up timeout for the entire operation
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));
                
                var result = await Task.Run(() =>
                {
                    try
                    {
                        // Get search root
                        var searchRoot = GetSearchRoot(operation);
                        if (searchRoot == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Failed to get search root element"
                            };
                        }

                        // Build condition
                        var condition = BuildCondition(operation);
                        if (condition == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Failed to build search condition"
                            };
                        }

                        // Parse scope
                        if (!Enum.TryParse<TreeScope>(
                            operation.Parameters.GetValueOrDefault("Scope")?.ToString(), 
                            out var scope))
                        {
                            scope = TreeScope.Descendants;
                        }

                        _logger?.LogInformation("[UiAutomationWorker] Calling FindFirst with scope: {Scope}", scope);

                        // This is the critical call that may hang
                        var element = searchRoot.FindFirst(scope, condition);

                        if (element != null)
                        {
                            // Extract element information instead of returning the element itself
                            // (AutomationElement cannot be serialized across processes)
                            var elementInfo = ExtractElementInfo(element);
                            
                            return new WorkerResult
                            {
                                Success = true,
                                Data = elementInfo
                            };
                        }
                        else
                        {
                            return new WorkerResult
                            {
                                Success = true,
                                Data = null
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "[UiAutomationWorker] FindFirst execution failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"FindFirst failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("[UiAutomationWorker] FindFirst operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"FindFirst operation timed out after {operation.Timeout} seconds"
                };
            }
        }

        private static Task<WorkerResult> ExecuteFindAllAsync(WorkerOperation operation)
        {
            // Similar implementation to FindFirst but for FindAll
            return Task.FromResult(new WorkerResult
            {
                Success = false,
                Error = "FindAll operation not yet implemented"
            });
        }

        private static Task<WorkerResult> ExecuteGetPropertiesAsync(WorkerOperation operation)
        {
            return Task.FromResult(new WorkerResult
            {
                Success = false,
                Error = "GetProperties operation not yet implemented"
            });
        }

        private static Task<WorkerResult> ExecuteInvokeAsync(WorkerOperation operation)
        {
            return Task.FromResult(new WorkerResult
            {
                Success = false,
                Error = "Invoke operation not yet implemented"
            });
        }

        private static Task<WorkerResult> ExecuteSetValueAsync(WorkerOperation operation)
        {
            return Task.FromResult(new WorkerResult
            {
                Success = false,
                Error = "SetValue operation not yet implemented"
            });
        }

        private static AutomationElement? GetSearchRoot(WorkerOperation operation)
        {
            try
            {
                // For now, use AutomationElement.RootElement
                // In a full implementation, we'd parse search root information
                return AutomationElement.RootElement;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[UiAutomationWorker] Failed to get search root");
                return null;
            }
        }

        private static Condition? BuildCondition(WorkerOperation operation)
        {
            try
            {
                // Build condition based on operation parameters
                var conditions = new List<Condition>();

                if (operation.Parameters.TryGetValue("AutomationId", out var automationId) && 
                    automationId?.ToString() is string automationIdStr && !string.IsNullOrEmpty(automationIdStr))
                {
                    conditions.Add(new PropertyCondition(AutomationElement.AutomationIdProperty, automationIdStr));
                }

                if (operation.Parameters.TryGetValue("Name", out var name) && 
                    name?.ToString() is string nameStr && !string.IsNullOrEmpty(nameStr))
                {
                    conditions.Add(new PropertyCondition(AutomationElement.NameProperty, nameStr));
                }

                if (operation.Parameters.TryGetValue("ControlType", out var controlType) && 
                    controlType?.ToString() is string controlTypeStr && !string.IsNullOrEmpty(controlTypeStr))
                {
                    // Parse control type string to ControlType
                    var controlTypeValue = ParseControlType(controlTypeStr);
                    if (controlTypeValue != null)
                    {
                        conditions.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, controlTypeValue));
                    }
                }

                if (conditions.Count == 0)
                {
                    return Condition.TrueCondition;
                }
                else if (conditions.Count == 1)
                {
                    return conditions[0];
                }
                else
                {
                    return new AndCondition(conditions.ToArray());
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[UiAutomationWorker] Failed to build condition");
                return null;
            }
        }

        private static ControlType? ParseControlType(string controlTypeStr)
        {
            // Simple control type parsing - can be extended
            return controlTypeStr.ToLowerInvariant() switch
            {
                "button" => ControlType.Button,
                "edit" => ControlType.Edit,
                "text" => ControlType.Text,
                "window" => ControlType.Window,
                "pane" => ControlType.Pane,
                "document" => ControlType.Document,
                "list" => ControlType.List,
                "listitem" => ControlType.ListItem,
                _ => null
            };
        }

        private static Dictionary<string, object> ExtractElementInfo(AutomationElement element)
        {
            try
            {
                return new Dictionary<string, object>
                {
                    ["Name"] = element.Current.Name ?? "",
                    ["AutomationId"] = element.Current.AutomationId ?? "",
                    ["ClassName"] = element.Current.ClassName ?? "",
                    ["ControlType"] = element.Current.ControlType.ProgrammaticName ?? "",
                    ["ProcessId"] = element.Current.ProcessId,
                    ["IsEnabled"] = element.Current.IsEnabled,
                    ["IsVisible"] = !element.Current.IsOffscreen,
                    ["BoundingRectangle"] = new Dictionary<string, double>
                    {
                        ["X"] = element.Current.BoundingRectangle.X,
                        ["Y"] = element.Current.BoundingRectangle.Y,
                        ["Width"] = element.Current.BoundingRectangle.Width,
                        ["Height"] = element.Current.BoundingRectangle.Height
                    }
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[UiAutomationWorker] Failed to extract element info");
                return new Dictionary<string, object>
                {
                    ["Error"] = $"Failed to extract element info: {ex.Message}"
                };
            }
        }
    }

    public class WorkerOperation
    {
        public string Operation { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public int Timeout { get; set; } = 10;
    }

    public class WorkerResult
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
        public string? Error { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
