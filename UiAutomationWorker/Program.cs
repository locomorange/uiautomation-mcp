using System.Diagnostics;
using System.Text.Json;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;

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
            // Initialize logging - disable console output to avoid interfering with JSON output
            using var loggerFactory = LoggerFactory.Create(builder =>
                builder.SetMinimumLevel(LogLevel.Error)); // Only log errors, and not to console
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

                // Parse operation with UTF-8 encoding support
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
                };
                var operation = JsonSerializer.Deserialize<WorkerOperation>(inputJson, options);
                if (operation == null)
                {
                    _logger.LogError("[UiAutomationWorker] Failed to parse operation JSON");
                    await Console.Error.WriteLineAsync("Failed to parse operation JSON");
                    return 1;
                }

                // Execute operation with timeout
                var result = await ExecuteOperationAsync(operation);
                
                // Output result
                var outputOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
                };
                var resultJson = JsonSerializer.Serialize(result, outputOptions);
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
                    "value" => await ExecuteSetValueAsync(operation), // Alias for setvalue
                    "get_value" => await ExecuteGetValueAsync(operation),
                    "toggle" => await ExecuteToggleAsync(operation),
                    "select" => await ExecuteSelectAsync(operation),
                    "expandcollapse" => await ExecuteExpandCollapseAsync(operation),
                    "scroll" => await ExecuteScrollAsync(operation),
                    "scrollintoview" => await ExecuteScrollIntoViewAsync(operation),
                    "transform" => await ExecuteTransformAsync(operation),
                    "dock" => await ExecuteDockAsync(operation),
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

        private static async Task<WorkerResult> ExecuteFindAllAsync(WorkerOperation operation)
        {
            _logger?.LogInformation("[UiAutomationWorker] Executing FindAll operation");

            try
            {
                // Set up timeout for the entire operation
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));
                
                var result = await Task.Run(() =>
                {
                    try
                    {
                        var searchRoot = GetSearchRoot(operation);
                        if (searchRoot == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Failed to get search root"
                            };
                        }

                        var condition = BuildCondition(operation);
                        if (condition == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Failed to build search condition"
                            };
                        }

                        // Use Children scope for Window control type to improve performance
                        var scope = IsWindowControlType(operation) ? TreeScope.Children : TreeScope.Descendants;
                        _logger?.LogInformation("[UiAutomationWorker] Calling FindAll with scope: {Scope}", scope);

                        // This is the critical call that may hang - now isolated in subprocess
                        var elements = searchRoot.FindAll(scope, condition);
                        
                        var elementInfos = new List<object>();
                        int processedCount = 0;
                        
                        foreach (AutomationElement element in elements)
                        {
                            try
                            {
                                processedCount++;
                                if (processedCount % 10 == 0)
                                {
                                    _logger?.LogInformation("[UiAutomationWorker] Processing element {Count}/{Total}", processedCount, elements.Count);
                                }
                                
                                var elementInfo = ExtractElementInfo(element);
                                elementInfos.Add(elementInfo);
                                
                                // Limit results to prevent memory issues
                                if (elementInfos.Count >= 100)
                                {
                                    _logger?.LogWarning("[UiAutomationWorker] Limiting results to 100 elements");
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogWarning(ex, "[UiAutomationWorker] Failed to process element {Count}", processedCount);
                                continue;
                            }
                        }
                        
                        _logger?.LogInformation("[UiAutomationWorker] FindAll completed, processed {Count} elements", elementInfos.Count);

                        return new WorkerResult
                        {
                            Success = true,
                            Data = elementInfos
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "[UiAutomationWorker] FindAll execution failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"FindAll failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("[UiAutomationWorker] FindAll operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"FindAll operation timed out after {operation.Timeout} seconds"
                };
            }
        }

        private static Task<WorkerResult> ExecuteGetPropertiesAsync(WorkerOperation operation)
        {
            return Task.FromResult(new WorkerResult
            {
                Success = false,
                Error = "GetProperties operation not yet implemented"
            });
        }

        private static async Task<WorkerResult> ExecuteInvokeAsync(WorkerOperation operation)
        {
            _logger?.LogInformation("[UiAutomationWorker] Executing Invoke operation");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));
                
                var result = await Task.Run(() =>
                {
                    try
                    {
                        var element = FindElementByIdOrName(operation);
                        if (element == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element not found for invoke operation"
                            };
                        }

                        if (element.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern) && pattern is InvokePattern invokePattern)
                        {
                            invokePattern.Invoke();
                            _logger?.LogInformation("[UiAutomationWorker] Element invoked successfully");
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
                        _logger?.LogError(ex, "[UiAutomationWorker] Invoke execution failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"Invoke failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("[UiAutomationWorker] Invoke operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"Invoke operation timed out after {operation.Timeout} seconds"
                };
            }
        }

        private static async Task<WorkerResult> ExecuteSetValueAsync(WorkerOperation operation)
        {
            _logger?.LogInformation("[UiAutomationWorker] Executing SetValue operation");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));
                
                var result = await Task.Run(() =>
                {
                    try
                    {
                        var element = FindElementByIdOrName(operation);
                        if (element == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element not found for setvalue operation"
                            };
                        }

                        if (!operation.Parameters.TryGetValue("value", out var valueObj) || valueObj?.ToString() is not string value)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Value parameter is required for setvalue operation"
                            };
                        }

                        if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern) && pattern is ValuePattern valuePattern)
                        {
                            valuePattern.SetValue(value);
                            _logger?.LogInformation("[UiAutomationWorker] Value set successfully to: {Value}", value);
                            return new WorkerResult
                            {
                                Success = true,
                                Data = "Value set successfully"
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
                        _logger?.LogError(ex, "[UiAutomationWorker] SetValue execution failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"SetValue failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("[UiAutomationWorker] SetValue operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"SetValue operation timed out after {operation.Timeout} seconds"
                };
            }
        }

        private static AutomationElement? GetSearchRoot(WorkerOperation operation)
        {
            try
            {
                // Check if we need to find a specific window
                if (operation.Parameters.TryGetValue("WindowTitle", out var windowTitle) && 
                    windowTitle?.ToString() is string windowTitleStr && !string.IsNullOrEmpty(windowTitleStr))
                {
                    _logger?.LogInformation("[UiAutomationWorker] Finding window by title: '{WindowTitle}'", windowTitleStr);
                    
                    var windowCondition = new PropertyCondition(AutomationElement.NameProperty, windowTitleStr);
                    var window = AutomationElement.RootElement.FindFirst(TreeScope.Children, windowCondition);
                    
                    if (window != null)
                    {
                        _logger?.LogInformation("[UiAutomationWorker] Found window: {WindowName} (ProcessId: {ProcessId})", 
                            window.Current.Name, window.Current.ProcessId);
                        return window;
                    }
                    else
                    {
                        _logger?.LogWarning("[UiAutomationWorker] Window '{WindowTitle}' not found", windowTitleStr);
                        return null;
                    }
                }
                
                // Default to root element
                _logger?.LogInformation("[UiAutomationWorker] Using root element for search");
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

                // Handle searchText that maps to both Name and AutomationId (OR condition)
                var nameConditions = new List<Condition>();
                
                if (operation.Parameters.TryGetValue("Name", out var name) && 
                    name?.ToString() is string nameStr && !string.IsNullOrEmpty(nameStr))
                {
                    nameConditions.Add(new PropertyCondition(AutomationElement.NameProperty, nameStr));
                }
                
                if (operation.Parameters.TryGetValue("AutomationId", out var automationId) && 
                    automationId?.ToString() is string automationIdStr && !string.IsNullOrEmpty(automationIdStr))
                {
                    nameConditions.Add(new PropertyCondition(AutomationElement.AutomationIdProperty, automationIdStr));
                }

                // If we have both name and automationId from searchText, OR them together
                if (nameConditions.Count > 1)
                {
                    conditions.Add(new OrCondition(nameConditions.ToArray()));
                }
                else if (nameConditions.Count == 1)
                {
                    conditions.Add(nameConditions[0]);
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

        private static bool IsWindowControlType(WorkerOperation operation)
        {
            if (operation.Parameters.TryGetValue("ControlType", out var controlType) && 
                controlType?.ToString() is string controlTypeStr)
            {
                return controlTypeStr.ToLowerInvariant() == "window";
            }
            return false;
        }

        private static Dictionary<string, object> ExtractElementInfo(AutomationElement element)
        {
            try
            {
                var boundingRect = element.Current.BoundingRectangle;
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
                        ["X"] = SafeDoubleValue(boundingRect.X),
                        ["Y"] = SafeDoubleValue(boundingRect.Y),
                        ["Width"] = SafeDoubleValue(boundingRect.Width),
                        ["Height"] = SafeDoubleValue(boundingRect.Height)
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
        
        private static double SafeDoubleValue(double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
            {
                return 0.0;
            }
            return value;
        }

        private static async Task<WorkerResult> ExecuteGetValueAsync(WorkerOperation operation)
        {
            _logger?.LogInformation("[UiAutomationWorker] Executing GetValue operation");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));
                
                var result = await Task.Run(() =>
                {
                    try
                    {
                        var element = FindElementByIdOrName(operation);
                        if (element == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element not found for getvalue operation"
                            };
                        }

                        if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern) && pattern is ValuePattern valuePattern)
                        {
                            var value = valuePattern.Current.Value;
                            _logger?.LogInformation("[UiAutomationWorker] Value retrieved: {Value}", value);
                            return new WorkerResult
                            {
                                Success = true,
                                Data = value
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
                        _logger?.LogError(ex, "[UiAutomationWorker] GetValue execution failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"GetValue failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("[UiAutomationWorker] GetValue operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"GetValue operation timed out after {operation.Timeout} seconds"
                };
            }
        }

        private static async Task<WorkerResult> ExecuteToggleAsync(WorkerOperation operation)
        {
            _logger?.LogInformation("[UiAutomationWorker] Executing Toggle operation");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));
                
                var result = await Task.Run(() =>
                {
                    try
                    {
                        var element = FindElementByIdOrName(operation);
                        if (element == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element not found for toggle operation"
                            };
                        }

                        if (element.TryGetCurrentPattern(TogglePattern.Pattern, out var pattern) && pattern is TogglePattern togglePattern)
                        {
                            togglePattern.Toggle();
                            _logger?.LogInformation("[UiAutomationWorker] Element toggled successfully");
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
                        _logger?.LogError(ex, "[UiAutomationWorker] Toggle execution failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"Toggle failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("[UiAutomationWorker] Toggle operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"Toggle operation timed out after {operation.Timeout} seconds"
                };
            }
        }

        private static async Task<WorkerResult> ExecuteSelectAsync(WorkerOperation operation)
        {
            _logger?.LogInformation("[UiAutomationWorker] Executing Select operation");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));
                
                var result = await Task.Run(() =>
                {
                    try
                    {
                        var element = FindElementByIdOrName(operation);
                        if (element == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element not found for select operation"
                            };
                        }

                        if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) && pattern is SelectionItemPattern selectionPattern)
                        {
                            selectionPattern.Select();
                            _logger?.LogInformation("[UiAutomationWorker] Element selected successfully");
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
                        _logger?.LogError(ex, "[UiAutomationWorker] Select execution failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"Select failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("[UiAutomationWorker] Select operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"Select operation timed out after {operation.Timeout} seconds"
                };
            }
        }

        // Layout Pattern operations
        private static async Task<WorkerResult> ExecuteExpandCollapseAsync(WorkerOperation operation)
        {
            _logger?.LogInformation("[UiAutomationWorker] Executing ExpandCollapse operation");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));
                
                var result = await Task.Run(() =>
                {
                    try
                    {
                        var element = FindElementByIdOrName(operation);
                        if (element == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element not found for expandcollapse operation"
                            };
                        }

                        if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var pattern) && pattern is ExpandCollapsePattern expandCollapsePattern)
                        {
                            var currentState = expandCollapsePattern.Current.ExpandCollapseState;
                            
                            if (operation.Parameters.TryGetValue("expand", out var expandValue) && expandValue is bool expand)
                            {
                                if (expand)
                                {
                                    expandCollapsePattern.Expand();
                                }
                                else
                                {
                                    expandCollapsePattern.Collapse();
                                }
                            }
                            else
                            {
                                // Toggle behavior
                                if (currentState == ExpandCollapseState.Expanded)
                                    expandCollapsePattern.Collapse();
                                else
                                    expandCollapsePattern.Expand();
                            }
                            
                            return new WorkerResult
                            {
                                Success = true,
                                Data = "Element expand/collapse executed successfully"
                            };
                        }
                        
                        return new WorkerResult
                        {
                            Success = false,
                            Error = "Element does not support ExpandCollapsePattern"
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "[UiAutomationWorker] ExpandCollapse operation failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"ExpandCollapse operation failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("[UiAutomationWorker] ExpandCollapse operation timed out");
                return new WorkerResult
                {
                    Success = false,
                    Error = "ExpandCollapse operation timed out"
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[UiAutomationWorker] ExpandCollapse operation failed");
                return new WorkerResult
                {
                    Success = false,
                    Error = $"ExpandCollapse operation failed: {ex.Message}"
                };
            }
        }

        private static async Task<WorkerResult> ExecuteScrollAsync(WorkerOperation operation)
        {
            _logger?.LogInformation("[UiAutomationWorker] Executing Scroll operation");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));
                
                var result = await Task.Run(() =>
                {
                    try
                    {
                        var element = FindElementByIdOrName(operation);
                        if (element == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element not found for scroll operation"
                            };
                        }

                        if (element.TryGetCurrentPattern(ScrollPattern.Pattern, out var pattern) && pattern is ScrollPattern scrollPattern)
                        {
                            if (operation.Parameters.TryGetValue("direction", out var directionValue) && directionValue?.ToString() is string direction)
                            {
                                switch (direction.ToLower())
                                {
                                    case "up":
                                        scrollPattern.ScrollVertical(ScrollAmount.SmallDecrement);
                                        break;
                                    case "down":
                                        scrollPattern.ScrollVertical(ScrollAmount.SmallIncrement);
                                        break;
                                    case "left":
                                        scrollPattern.ScrollHorizontal(ScrollAmount.SmallDecrement);
                                        break;
                                    case "right":
                                        scrollPattern.ScrollHorizontal(ScrollAmount.SmallIncrement);
                                        break;
                                }
                            }
                            else
                            {
                                if (operation.Parameters.TryGetValue("horizontal", out var horizontalValue) && 
                                    horizontalValue is JsonElement horizontalElement && horizontalElement.TryGetDouble(out var horizontal))
                                {
                                    scrollPattern.SetScrollPercent(horizontal, ScrollPattern.NoScroll);
                                }
                                if (operation.Parameters.TryGetValue("vertical", out var verticalValue) && 
                                    verticalValue is JsonElement verticalElement && verticalElement.TryGetDouble(out var vertical))
                                {
                                    scrollPattern.SetScrollPercent(ScrollPattern.NoScroll, vertical);
                                }
                            }
                            
                            return new WorkerResult
                            {
                                Success = true,
                                Data = "Element scrolled successfully"
                            };
                        }
                        
                        return new WorkerResult
                        {
                            Success = false,
                            Error = "Element does not support ScrollPattern"
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "[UiAutomationWorker] Scroll operation failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"Scroll operation failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("[UiAutomationWorker] Scroll operation timed out");
                return new WorkerResult
                {
                    Success = false,
                    Error = "Scroll operation timed out"
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[UiAutomationWorker] Scroll operation failed");
                return new WorkerResult
                {
                    Success = false,
                    Error = $"Scroll operation failed: {ex.Message}"
                };
            }
        }

        private static async Task<WorkerResult> ExecuteScrollIntoViewAsync(WorkerOperation operation)
        {
            _logger?.LogInformation("[UiAutomationWorker] Executing ScrollIntoView operation");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));
                
                var result = await Task.Run(() =>
                {
                    try
                    {
                        var element = FindElementByIdOrName(operation);
                        if (element == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element not found for scrollintoview operation"
                            };
                        }

                        if (element.TryGetCurrentPattern(ScrollItemPattern.Pattern, out var pattern) && pattern is ScrollItemPattern scrollItemPattern)
                        {
                            scrollItemPattern.ScrollIntoView();
                            return new WorkerResult
                            {
                                Success = true,
                                Data = "Element scrolled into view successfully"
                            };
                        }
                        
                        return new WorkerResult
                        {
                            Success = false,
                            Error = "Element does not support ScrollItemPattern"
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "[UiAutomationWorker] ScrollIntoView operation failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"ScrollIntoView operation failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("[UiAutomationWorker] ScrollIntoView operation timed out");
                return new WorkerResult
                {
                    Success = false,
                    Error = "ScrollIntoView operation timed out"
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[UiAutomationWorker] ScrollIntoView operation failed");
                return new WorkerResult
                {
                    Success = false,
                    Error = $"ScrollIntoView operation failed: {ex.Message}"
                };
            }
        }

        private static async Task<WorkerResult> ExecuteTransformAsync(WorkerOperation operation)
        {
            _logger?.LogInformation("[UiAutomationWorker] Executing Transform operation");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));
                
                var result = await Task.Run(() =>
                {
                    try
                    {
                        var element = FindElementByIdOrName(operation);
                        if (element == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element not found for transform operation"
                            };
                        }

                        if (element.TryGetCurrentPattern(TransformPattern.Pattern, out var pattern) && pattern is TransformPattern transformPattern)
                        {
                            if (operation.Parameters.TryGetValue("action", out var actionValue) && actionValue?.ToString() is string action)
                            {
                                switch (action.ToLower())
                                {
                                    case "move":
                                        if (operation.Parameters.TryGetValue("x", out var xValue) && xValue is JsonElement xElement && xElement.TryGetDouble(out var x) &&
                                            operation.Parameters.TryGetValue("y", out var yValue) && yValue is JsonElement yElement && yElement.TryGetDouble(out var y))
                                        {
                                            transformPattern.Move(x, y);
                                            return new WorkerResult { Success = true, Data = "Element moved successfully" };
                                        }
                                        break;
                                    case "resize":
                                        if (operation.Parameters.TryGetValue("width", out var widthValue) && widthValue is JsonElement widthElement && widthElement.TryGetDouble(out var width) &&
                                            operation.Parameters.TryGetValue("height", out var heightValue) && heightValue is JsonElement heightElement && heightElement.TryGetDouble(out var height))
                                        {
                                            transformPattern.Resize(width, height);
                                            return new WorkerResult { Success = true, Data = "Element resized successfully" };
                                        }
                                        break;
                                    case "rotate":
                                        if (operation.Parameters.TryGetValue("degrees", out var degreesValue) && degreesValue is JsonElement degreesElement && degreesElement.TryGetDouble(out var degrees))
                                        {
                                            transformPattern.Rotate(degrees);
                                            return new WorkerResult { Success = true, Data = "Element rotated successfully" };
                                        }
                                        break;
                                }
                                
                                return new WorkerResult
                                {
                                    Success = false,
                                    Error = $"Invalid parameters for transform action '{action}'"
                                };
                            }
                            
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Transform action not specified"
                            };
                        }
                        
                        return new WorkerResult
                        {
                            Success = false,
                            Error = "Element does not support TransformPattern"
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "[UiAutomationWorker] Transform operation failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"Transform operation failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("[UiAutomationWorker] Transform operation timed out");
                return new WorkerResult
                {
                    Success = false,
                    Error = "Transform operation timed out"
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[UiAutomationWorker] Transform operation failed");
                return new WorkerResult
                {
                    Success = false,
                    Error = $"Transform operation failed: {ex.Message}"
                };
            }
        }

        private static async Task<WorkerResult> ExecuteDockAsync(WorkerOperation operation)
        {
            _logger?.LogInformation("[UiAutomationWorker] Executing Dock operation");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));
                
                var result = await Task.Run(() =>
                {
                    try
                    {
                        var element = FindElementByIdOrName(operation);
                        if (element == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element not found for dock operation"
                            };
                        }

                        if (element.TryGetCurrentPattern(DockPattern.Pattern, out var pattern) && pattern is DockPattern dockPattern)
                        {
                            if (operation.Parameters.TryGetValue("position", out var positionValue) && positionValue?.ToString() is string position)
                            {
                                var dockPosition = position.ToLower() switch
                                {
                                    "top" => DockPosition.Top,
                                    "bottom" => DockPosition.Bottom,
                                    "left" => DockPosition.Left,
                                    "right" => DockPosition.Right,
                                    "fill" => DockPosition.Fill,
                                    "none" => DockPosition.None,
                                    _ => DockPosition.None
                                };
                                
                                dockPattern.SetDockPosition(dockPosition);
                                return new WorkerResult
                                {
                                    Success = true,
                                    Data = "Element docked successfully"
                                };
                            }
                            
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Dock position not specified"
                            };
                        }
                        
                        return new WorkerResult
                        {
                            Success = false,
                            Error = "Element does not support DockPattern"
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "[UiAutomationWorker] Dock operation failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"Dock operation failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("[UiAutomationWorker] Dock operation timed out");
                return new WorkerResult
                {
                    Success = false,
                    Error = "Dock operation timed out"
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[UiAutomationWorker] Dock operation failed");
                return new WorkerResult
                {
                    Success = false,
                    Error = $"Dock operation failed: {ex.Message}"
                };
            }
        }

        private static AutomationElement? FindElementByIdOrName(WorkerOperation operation)
        {
            try
            {
                var searchRoot = GetSearchRoot(operation);
                if (searchRoot == null)
                {
                    return null;
                }

                // Build condition for finding element by ID or Name
                var conditions = new List<Condition>();
                
                if (operation.Parameters.TryGetValue("ElementId", out var elementId) && 
                    elementId?.ToString() is string elementIdStr && !string.IsNullOrEmpty(elementIdStr))
                {
                    conditions.Add(new PropertyCondition(AutomationElement.AutomationIdProperty, elementIdStr));
                    conditions.Add(new PropertyCondition(AutomationElement.NameProperty, elementIdStr));
                }
                else if (operation.Parameters.TryGetValue("Name", out var name) && 
                         name?.ToString() is string nameStr && !string.IsNullOrEmpty(nameStr))
                {
                    conditions.Add(new PropertyCondition(AutomationElement.NameProperty, nameStr));
                }
                else if (operation.Parameters.TryGetValue("AutomationId", out var automationId) && 
                         automationId?.ToString() is string automationIdStr && !string.IsNullOrEmpty(automationIdStr))
                {
                    conditions.Add(new PropertyCondition(AutomationElement.AutomationIdProperty, automationIdStr));
                }

                if (conditions.Count == 0)
                {
                    _logger?.LogWarning("[UiAutomationWorker] No valid element identifier found in parameters");
                    return null;
                }

                var condition = conditions.Count == 1 ? conditions[0] : new OrCondition(conditions.ToArray());
                return searchRoot.FindFirst(TreeScope.Descendants, condition);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[UiAutomationWorker] Failed to find element");
                return null;
            }
        }
    }

}
