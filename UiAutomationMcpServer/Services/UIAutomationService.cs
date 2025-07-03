using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Automation;
using System.IO;
using UiAutomationMcpServer.Models;

namespace UiAutomationMcpServer.Services
{
    public class UIAutomationService : IUIAutomationService
    {
        private readonly ILogger<UIAutomationService> _logger;

        public UIAutomationService(ILogger<UIAutomationService> logger)
        {
            _logger = logger;
        }

        public Task<OperationResult> GetWindowInfoAsync()
        {
            try
            {
                var windows = new List<WindowInfo>();
                AutomationElementCollection? windowElements = null;
                
                try
                {
                    windowElements = AutomationElement.RootElement.FindAll(TreeScope.Children,
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));
                }
                catch (Exception ex)
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Failed to find windows: {ex.Message}" });
                }

                if (windowElements == null)
                {
                    return Task.FromResult(new OperationResult { Success = true, Data = windows });
                }

                foreach (AutomationElement window in windowElements)
                {
                    try
                    {
                        var name = window?.Current.Name ?? "";
                        if (!string.IsNullOrEmpty(name) && window != null)
                        {
                            windows.Add(new WindowInfo
                            {
                                Name = name,
                                AutomationId = window.Current.AutomationId ?? "",
                                ProcessId = window.Current.ProcessId,
                                ClassName = window.Current.ClassName ?? "",
                                BoundingRectangle = new BoundingRectangle
                                {
                                    X = window.Current.BoundingRectangle.X,
                                    Y = window.Current.BoundingRectangle.Y,
                                    Width = window.Current.BoundingRectangle.Width,
                                    Height = window.Current.BoundingRectangle.Height
                                },
                                IsEnabled = window.Current.IsEnabled,
                                IsVisible = !window.Current.IsOffscreen
                            });
                        }
                    }
                    catch (Exception)
                    {
                        // Skip this window and continue
                        continue;
                    }
                }

                return Task.FromResult(new OperationResult { Success = true, Data = windows });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"GetWindowInfo failed: {ex.Message}" });
            }
        }

        public Task<OperationResult> GetElementInfoAsync(string? windowTitle = null, string? controlType = null, int? processId = null)
        {
            try
            {
                _logger.LogInformation("GetElementInfo called with windowTitle: {WindowTitle}, controlType: {ControlType}, processId: {ProcessId}", 
                    windowTitle, controlType, processId);

                AutomationElement searchRoot = AutomationElement.RootElement;

                if (!string.IsNullOrEmpty(windowTitle))
                {
                    var window = FindWindowByTitle(windowTitle, processId);
                    if (window != null)
                    {
                        searchRoot = window;
                        _logger.LogInformation("Found window: {WindowName}", window.Current.Name);
                    }
                    else
                    {
                        var message = processId.HasValue 
                            ? $"Window '{windowTitle}' (processId {processId}) not found, searching in all windows"
                            : $"Window '{windowTitle}' not found, searching in all windows";
                        _logger.LogWarning(message);
                        // Continue with root element instead of failing
                    }
                }

                var elements = new List<ElementInfo>();
                var conditions = new List<Condition>();

                if (!string.IsNullOrEmpty(controlType))
                {
                    _logger.LogInformation("Processing controlType: {ControlType}", controlType);
                    try
                    {
                        var ctrlType = GetControlTypeFromString(controlType);
                        if (ctrlType != null)
                        {
                            _logger.LogInformation("Mapped controlType '{ControlType}' to {MappedType}", controlType, ctrlType.ProgrammaticName);
                            conditions.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, ctrlType));
                        }
                        else
                        {
                            _logger.LogWarning("Unknown controlType: {ControlType}", controlType);
                            return Task.FromResult(new OperationResult { Success = false, Error = $"Unknown control type: {controlType}" });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error mapping controlType: {ControlType}", controlType);
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Error processing control type '{controlType}': {ex.Message}" });
                    }
                }

                Condition condition = conditions.Count > 0 ?
                    (conditions.Count == 1 ? conditions[0] : new AndCondition(conditions.ToArray())) :
                    Condition.TrueCondition;

                AutomationElementCollection? foundElements = null;
                try
                {
                    _logger.LogInformation("Starting FindAll operation on searchRoot with condition type: {ConditionType}", condition.GetType().Name);
                    // Use Children scope first to avoid deep traversal issues
                    foundElements = searchRoot.FindAll(TreeScope.Children, condition);
                    _logger.LogInformation("FindAll (Children) completed, found {Count} elements", foundElements?.Count ?? 0);
                    
                    // If no elements found with Children scope and we have a specific window, try Descendants with limited scope
                    if ((foundElements == null || foundElements.Count == 0) && !string.IsNullOrEmpty(windowTitle))
                    {
                        _logger.LogInformation("No elements found with Children scope, trying Descendants");
                        foundElements = searchRoot.FindAll(TreeScope.Descendants, condition);
                        _logger.LogInformation("FindAll (Descendants) completed, found {Count} elements", foundElements?.Count ?? 0);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in GetElementInfo: FindAll failed with condition: {ConditionType}", condition?.GetType()?.Name ?? "Unknown");
                    return Task.FromResult(new OperationResult { Success = false, Error = $"FindAll failed: {ex.Message}" });
                }

                if (foundElements == null || foundElements.Count == 0)
                {
                    _logger.LogInformation("No elements found, returning empty list");
                    return Task.FromResult(new OperationResult { Success = true, Data = elements });
                }

                // Limit the number of elements to avoid memory issues
                int maxElements = 100;
                int processedCount = 0;

                foreach (AutomationElement element in foundElements)
                {
                    if (processedCount >= maxElements)
                    {
                        _logger.LogWarning("Reached maximum element limit of {MaxElements}, stopping processing", maxElements);
                        break;
                    }

                    try
                    {
                        var rect = element.Current.BoundingRectangle;
                        elements.Add(new ElementInfo
                        {
                            Name = element.Current.Name ?? "",
                            AutomationId = element.Current.AutomationId ?? "",
                            ControlType = element.Current.ControlType?.ProgrammaticName ?? "",
                            ClassName = element.Current.ClassName ?? "",
                            BoundingRectangle = new BoundingRectangle
                            {
                                X = double.IsInfinity(rect.X) || double.IsNaN(rect.X) ? 0 : rect.X,
                                Y = double.IsInfinity(rect.Y) || double.IsNaN(rect.Y) ? 0 : rect.Y,
                                Width = double.IsInfinity(rect.Width) || double.IsNaN(rect.Width) ? 0 : rect.Width,
                                Height = double.IsInfinity(rect.Height) || double.IsNaN(rect.Height) ? 0 : rect.Height
                            },
                            IsEnabled = element.Current.IsEnabled,
                            IsVisible = !element.Current.IsOffscreen,
                            HelpText = element.Current.HelpText ?? "",
                            Value = GetElementValue(element) ?? ""
                        });
                        processedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Failed to read element properties: {Error}", ex.Message);
                        // Skip this element and continue with the next one
                        continue;
                    }
                }

                _logger.LogInformation("Successfully processed {ProcessedCount} elements", processedCount);
                return Task.FromResult(new OperationResult { Success = true, Data = elements });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetElementInfo: {Message}", ex.Message);
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }


        public Task<OperationResult> ExecuteElementPatternAsync(string elementId, string patternName, Dictionary<string, object>? parameters = null, string? windowTitle = null, int? processId = null)
        {
            try
            {
                _logger.LogInformation("Executing pattern {PatternName} on element {ElementId}", patternName, elementId);

                // Find the element first
                AutomationElement? element = null;

                if (!string.IsNullOrEmpty(windowTitle))
                {
                    var window = FindWindowByTitle(windowTitle, processId);
                    if (window == null)
                    {
                        var errorMsg = processId.HasValue 
                            ? $"Window '{windowTitle}' (processId {processId}) not found"
                            : $"Window '{windowTitle}' not found";
                        return Task.FromResult(new OperationResult { Success = false, Error = errorMsg });
                    }

                    element = FindElementInWindow(window, elementId);
                }
                else
                {
                    var condition = new OrCondition(
                        new PropertyCondition(AutomationElement.AutomationIdProperty, elementId),
                        new PropertyCondition(AutomationElement.NameProperty, elementId)
                    );
                    element = AutomationElement.RootElement.FindFirst(TreeScope.Descendants, condition);
                }

                if (element == null)
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });

                // Pattern execution switch statement
                // Based on Microsoft Learn Control Pattern Mapping guide:
                // https://learn.microsoft.com/en-us/dotnet/framework/ui-automation/control-pattern-mapping-for-ui-automation-clients
                switch (patternName.ToLower())
                {
                    // Core Patterns (Required for most controls)
                    case "invoke":
                        return ExecuteInvokePattern(element);
                    
                    case "value":
                        return ExecuteValuePattern(element, parameters);
                    
                    case "toggle":
                        return ExecuteTogglePattern(element);
                    
                    case "selectionitem":
                        return ExecuteSelectionItemPattern(element);
                    
                    case "expandcollapse":
                        return ExecuteExpandCollapsePattern(element, parameters);
                    
                    // Navigation and Layout Patterns
                    case "scroll":
                        return ExecuteScrollPattern(element, parameters);
                    
                    case "scrollitem":
                        return ExecuteScrollItemPattern(element, parameters);
                    
                    // Value and Range Patterns
                    case "rangevalue":
                        return ExecuteRangeValuePattern(element, parameters);
                    
                    // Text Patterns
                    case "text":
                        return ExecuteTextPattern(element, parameters);
                    
                    // Window Management Patterns
                    case "window":
                        return ExecuteWindowPattern(element, parameters);
                    
                    case "transform":
                        return ExecuteTransformPattern(element, parameters);
                    
                    case "dock":
                        return ExecuteDockPattern(element, parameters);
                             // Grid and Table Patterns (not yet implemented but referenced in switch)
            case "grid":
                return ExecuteGridPattern(element, parameters);
            
            case "griditem":
                return ExecuteGridItemPattern(element, parameters);
            
            case "table":
                return ExecuteTablePattern(element, parameters);
            
            case "tableitem":
                return ExecuteTableItemPattern(element, parameters);
            
            // Selection Patterns 
            case "selection":
                return ExecuteSelectionPattern(element, parameters);
                    
                    // New patterns based on Microsoft guidance
                    case "multipleview":
                        return ExecuteMultipleViewPattern(element, parameters);
                    
                    case "virtualized":
                        return ExecuteVirtualizedItemPattern(element);
                    
                    case "itemcontainer":
                        return ExecuteItemContainerPattern(element, parameters);
                    
                    case "synchronizedinput":
                        return ExecuteSynchronizedInputPattern(element, parameters);
                    
                    default:
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Pattern '{patternName}' not supported. Supported patterns: invoke, value, toggle, selectionitem, expandcollapse, scroll, scrollitem, rangevalue, text, window, transform, dock, grid, griditem, table, tableitem, selection, multipleview, virtualized, itemcontainer, synchronizedinput" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing pattern {PatternName} on element {ElementId}", patternName, elementId);
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        public Task<ScreenshotResult> TakeScreenshotAsync(string? windowTitle = null, string? outputPath = null, int maxTokens = 0, int? processId = null)
        {
            try
            {
                _logger.LogInformation("Taking screenshot of window: {WindowTitle}, maxTokens: {MaxTokens}", 
                    windowTitle, maxTokens);

                // Get screen bounds
                Rectangle bounds;
                if (!string.IsNullOrEmpty(windowTitle))
                {
                    var window = FindWindowByTitle(windowTitle, processId);
                    if (window == null)
                    {
                        var errorMsg = processId.HasValue 
                            ? $"Window '{windowTitle}' (processId {processId}) not found"
                            : $"Window '{windowTitle}' not found";
                        return Task.FromResult(new ScreenshotResult { Success = false, Error = errorMsg });
                    }

                    var rect = window.Current.BoundingRectangle;
                    bounds = new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
                }
                else
                {
                    bounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
                }

                // Capture original screenshot
                using var originalBitmap = new System.Drawing.Bitmap(bounds.Width, bounds.Height);
                using (var graphics = System.Drawing.Graphics.FromImage(originalBitmap))
                {
                    graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);
                }

                // If no token limit, save as PNG
                if (maxTokens <= 0)
                {
                    outputPath ??= Path.Combine(Path.GetTempPath(), $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                    originalBitmap.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);
                    
                    var base64Image = Convert.ToBase64String(File.ReadAllBytes(outputPath));
                    return Task.FromResult(new ScreenshotResult
                    {
                        Success = true,
                        OutputPath = outputPath,
                        Base64Image = base64Image,
                        Width = bounds.Width,
                        Height = bounds.Height
                    });
                }

                // Auto-optimize for token limit
                return Task.FromResult(OptimizeScreenshotForTokenLimit(originalBitmap, outputPath, maxTokens, bounds.Width, bounds.Height));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error taking screenshot");
                return Task.FromResult(new ScreenshotResult { Success = false, Error = ex.Message });
            }
        }

        public async Task<ProcessResult> LaunchApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null)
        {
            try
            {
                _logger.LogInformation("Launching application: {ApplicationPath} with arguments: {Arguments}", 
                    applicationPath, arguments);

                var startInfo = new ProcessStartInfo
                {
                    FileName = applicationPath,
                    Arguments = arguments ?? "",
                    WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(applicationPath) ?? "",
                    UseShellExecute = true
                };

                var process = Process.Start(startInfo);
                if (process == null)
                    return new ProcessResult { Success = false, Error = "Failed to start process" };

                await Task.Delay(1000); // Wait for process to initialize

                _logger.LogInformation("Application launched with PID: {ProcessId}", process.Id);

                return new ProcessResult
                {
                    Success = true,
                    ProcessId = process.Id,
                    ProcessName = process.ProcessName,
                    HasExited = process.HasExited
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error launching application");
                return new ProcessResult { Success = false, Error = ex.Message };
            }
        }

        public Task<OperationResult> FindElementsAsync(string? searchText = null, string? controlType = null, string? windowTitle = null, int? processId = null)
        {
            try
            {
                _logger.LogInformation("Finding elements with searchText: {SearchText}, controlType: {ControlType}, window: {WindowTitle}, processId: {ProcessId}", 
                    searchText, controlType, windowTitle, processId);

                AutomationElement searchRoot = AutomationElement.RootElement;

                if (!string.IsNullOrEmpty(windowTitle))
                {
                    var window = FindWindowByTitle(windowTitle, processId);
                    if (window != null)
                    {
                        searchRoot = window;
                        _logger.LogInformation("Found window for search: {WindowName}", window.Current.Name);
                    }
                    else
                    {
                        var message = processId.HasValue 
                            ? $"Window '{windowTitle}' (processId {processId}) not found, searching in all windows"
                            : $"Window '{windowTitle}' not found, searching in all windows";
                        _logger.LogWarning(message);
                        // Continue with root element instead of failing
                    }
                }

                var conditions = new List<Condition>();

                if (!string.IsNullOrEmpty(searchText))
                {
                    _logger.LogInformation("Adding search text condition: {SearchText}", searchText);
                    conditions.Add(new OrCondition(
                        new PropertyCondition(AutomationElement.NameProperty, searchText),
                        new PropertyCondition(AutomationElement.AutomationIdProperty, searchText)
                    ));
                }

                if (!string.IsNullOrEmpty(controlType))
                {
                    _logger.LogInformation("Processing controlType for FindElements: {ControlType}", controlType);
                    try
                    {
                        var ctrlType = GetControlTypeFromString(controlType);
                        if (ctrlType != null)
                        {
                            _logger.LogInformation("Mapped controlType '{ControlType}' to {MappedType}", controlType, ctrlType.ProgrammaticName);
                            conditions.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, ctrlType));
                        }
                        else
                        {
                            _logger.LogWarning("Unknown controlType in FindElements: {ControlType}", controlType);
                            return Task.FromResult(new OperationResult { Success = false, Error = $"Unknown control type: {controlType}" });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error mapping controlType in FindElements: {ControlType}", controlType);
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Error processing control type '{controlType}': {ex.Message}" });
                    }
                }

                Condition finalCondition = conditions.Count > 0 ?
                    (conditions.Count == 1 ? conditions[0] : new AndCondition(conditions.ToArray())) :
                    Condition.TrueCondition;

                var elements = new List<ElementInfo>();
                AutomationElementCollection? foundElements = null;
                try
                {
                    _logger.LogInformation("Starting FindAll for FindElements with condition: {ConditionType}", finalCondition.GetType().Name);
                    foundElements = searchRoot.FindAll(TreeScope.Descendants, finalCondition);
                    _logger.LogInformation("FindAll completed, found {Count} elements", foundElements?.Count ?? 0);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in FindElements: FindAll failed with condition: {ConditionType}", finalCondition?.GetType()?.Name ?? "Unknown");
                    return Task.FromResult(new OperationResult { Success = false, Error = $"FindAll failed: {ex.Message}" });
                }

                if (foundElements == null)
                {
                    return Task.FromResult(new OperationResult { Success = true, Data = elements });
                }

                foreach (AutomationElement element in foundElements)
                {
                    try
                    {
                        var rect = element.Current.BoundingRectangle;
                        elements.Add(new ElementInfo
                        {
                            Name = element.Current.Name,
                            AutomationId = element.Current.AutomationId,
                            ControlType = element.Current.ControlType.ProgrammaticName,
                            ClassName = element.Current.ClassName,
                            BoundingRectangle = new BoundingRectangle
                            {
                                X = double.IsInfinity(rect.X) || double.IsNaN(rect.X) ? 0 : rect.X,
                                Y = double.IsInfinity(rect.Y) || double.IsNaN(rect.Y) ? 0 : rect.Y,
                                Width = double.IsInfinity(rect.Width) || double.IsNaN(rect.Width) ? 0 : rect.Width,
                                Height = double.IsInfinity(rect.Height) || double.IsNaN(rect.Height) ? 0 : rect.Height
                            },
                            IsEnabled = element.Current.IsEnabled,
                            IsVisible = !element.Current.IsOffscreen
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Failed to read element properties in FindElements: {Error}", ex.Message);
                        continue;
                    }
                }

                _logger.LogInformation("Found {ElementCount} matching elements", elements.Count);
                return Task.FromResult(new OperationResult { Success = true, Data = elements });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FindElements (outer)");
                return Task.FromResult(new OperationResult { Success = false, Error = ex.ToString() });
            }
        }

        #region Helper Methods

        private ScreenshotResult OptimizeScreenshotForTokenLimit(
            Bitmap originalBitmap, string? outputPath, int maxTokens, int originalWidth, int originalHeight)
        {
            // Rough estimation: 1 token ≈ 1.33 characters, Base64 adds ~33% overhead
            // So 1 byte ≈ 2 tokens (conservative estimate)
            var maxFileSize = maxTokens / 2;

            outputPath ??= Path.Combine(Path.GetTempPath(), $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");

            // Try different optimization levels
            var optimizationAttempts = new[]
            {
                new { ScaleFactor = 1.0, Quality = 80L },
                new { ScaleFactor = 0.8, Quality = 70L },
                new { ScaleFactor = 0.6, Quality = 60L },
                new { ScaleFactor = 0.5, Quality = 50L },
                new { ScaleFactor = 0.4, Quality = 40L },
                new { ScaleFactor = 0.3, Quality = 30L },
                new { ScaleFactor = 0.25, Quality = 20L }
            };

            foreach (var attempt in optimizationAttempts)
            {
                try
                {
                    var newWidth = (int)(originalWidth * attempt.ScaleFactor);
                    var newHeight = (int)(originalHeight * attempt.ScaleFactor);

                    using var resizedBitmap = new Bitmap(newWidth, newHeight);
                    using (var graphics = Graphics.FromImage(resizedBitmap))
                    {
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.DrawImage(originalBitmap, 0, 0, newWidth, newHeight);
                    }

                    var tempPath = Path.ChangeExtension(outputPath, $".temp_{attempt.ScaleFactor}_{attempt.Quality}.jpg");

                    // Save with JPEG compression
                    var jpegEncoder = GetEncoder(ImageFormat.Jpeg);
                    if (jpegEncoder != null)
                    {
                        var encoderParameters = new EncoderParameters(1);
                        encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, attempt.Quality);
                        resizedBitmap.Save(tempPath, jpegEncoder, encoderParameters);
                    }
                    else
                    {
                        resizedBitmap.Save(tempPath, ImageFormat.Jpeg);
                    }

                    var fileSize = new FileInfo(tempPath).Length;
                    _logger.LogInformation("Optimization attempt: scale={ScaleFactor}, quality={Quality}, size={FileSize}KB", 
                        attempt.ScaleFactor, attempt.Quality, fileSize / 1024);

                    if (fileSize <= maxFileSize)
                    {
                        // Success! Move temp file to final output
                        if (File.Exists(outputPath))
                            File.Delete(outputPath);
                        File.Move(tempPath, outputPath);

                        var base64Image = Convert.ToBase64String(File.ReadAllBytes(outputPath));
                        var actualTokens = base64Image.Length * 4 / 3; // Rough token estimation

                        _logger.LogInformation("Screenshot optimized: {OriginalSize} -> {NewSize}, tokens: ~{Tokens}", 
                            $"{originalWidth}x{originalHeight}", $"{newWidth}x{newHeight}", actualTokens);

                        return new ScreenshotResult
                        {
                            Success = true,
                            OutputPath = outputPath,
                            Base64Image = base64Image,
                            Width = newWidth,
                            Height = newHeight
                        };
                    }
                    else
                    {
                        // Clean up temp file
                        if (File.Exists(tempPath))
                            File.Delete(tempPath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Optimization attempt failed: {Error}", ex.Message);
                    continue;
                }
            }

            // If all attempts failed, return error
            return new ScreenshotResult 
            { 
                Success = false, 
                Error = $"Could not optimize screenshot to fit within {maxTokens} tokens. Try a higher limit or smaller window." 
            };
        }

        private static ImageCodecInfo? GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageEncoders();
            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private AutomationElement? FindWindowByTitle(string title, int? processId = null)
        {
            if (string.IsNullOrWhiteSpace(title)) return null;
            
            try
            {
                _logger.LogDebug("FindWindowByTitle called with title: '{Title}', processId: {ProcessId}", title, processId);
                
                var windows = AutomationElement.RootElement.FindAll(TreeScope.Children,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));
                
                _logger.LogDebug("Found {WindowCount} total windows", windows.Count);
                
                var matchingWindows = new List<AutomationElement>();
                
                foreach (AutomationElement window in windows)
                {
                    try
                    {
                        var name = window.Current.Name;
                        var windowProcessId = window.Current.ProcessId;
                        _logger.LogDebug("Checking window: '{Name}' (ProcessId: {ProcessId})", name, windowProcessId);
                        
                        // Filter by processId if specified
                        if (processId.HasValue && windowProcessId != processId.Value)
                        {
                            _logger.LogDebug("Skipping window with ProcessId {WindowProcessId} (looking for {TargetProcessId})", windowProcessId, processId.Value);
                            continue;
                        }
                        
                        if (!string.IsNullOrEmpty(name))
                        {
                            // Try exact match first (case insensitive)
                            if (string.Equals(name.Trim(), title.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                _logger.LogDebug("Exact match found: '{Name}' (ProcessId: {ProcessId})", name, windowProcessId);
                                matchingWindows.Add(window);
                            }
                            // Try contains match for partial matching
                            else if (name.Contains(title, StringComparison.OrdinalIgnoreCase) || 
                                     title.Contains(name, StringComparison.OrdinalIgnoreCase))
                            {
                                _logger.LogDebug("Partial match found: '{Name}' (ProcessId: {ProcessId})", name, windowProcessId);
                                matchingWindows.Add(window);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error reading window properties");
                        continue;
                    }
                }
                
                _logger.LogDebug("Found {MatchingCount} matching windows", matchingWindows.Count);
                
                if (matchingWindows.Count == 0)
                {
                    _logger.LogDebug("No matching windows found for title: '{Title}'", title);
                    return null;
                }
                
                // If processId is specified, return the first matching window (should be the only one)
                if (processId.HasValue)
                {
                    var selectedWindow = matchingWindows[0];
                    _logger.LogDebug("Selected window for processId {ProcessId}: '{Name}'", processId.Value, selectedWindow.Current.Name);
                    return selectedWindow;
                }
                
                // If no processId specified, prefer visible and enabled windows
                foreach (var window in matchingWindows)
                {
                    try
                    {
                        if (window.Current.IsEnabled && !window.Current.IsOffscreen)
                        {
                            _logger.LogDebug("Returning first visible/enabled window: '{Name}' (ProcessId: {ProcessId})", window.Current.Name, window.Current.ProcessId);
                            return window;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error checking window state");
                        continue;
                    }
                }
                
                // Return first matching window if no visible/enabled found
                var firstWindow = matchingWindows[0];
                _logger.LogDebug("Returning first matching window: '{Name}' (ProcessId: {ProcessId})", firstWindow.Current.Name, firstWindow.Current.ProcessId);
                return firstWindow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in window search for title: '{Title}'", title);
                return null;
            }
        }

        // 文字列の正規化（小文字化・トリム・NFKC正規化）
        private static string NormalizeString(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            string trimmed = input.Trim().ToLowerInvariant();
            try
            {
                return trimmed.Normalize(System.Text.NormalizationForm.FormKC);
            }
            catch
            {
                return trimmed;
            }
        }

        private static AutomationElement? FindElementInWindow(AutomationElement window, string elementId)
        {
            return window.FindFirst(TreeScope.Descendants,
                new OrCondition(
                    new PropertyCondition(AutomationElement.AutomationIdProperty, elementId),
                    new PropertyCondition(AutomationElement.NameProperty, elementId)
                ));
        }

        private static ControlType? GetControlTypeFromString(string? controlTypeString)
        {
            if (string.IsNullOrEmpty(controlTypeString))
                return null;

            try
            {
                var normalized = controlTypeString.Trim().ToLower();
                
                var result = normalized switch
                {
                    "button" => ControlType.Button,
                    "edit" => ControlType.Edit,
                    "text" => ControlType.Text,
                    "window" => ControlType.Window,
                    "menuitem" => ControlType.MenuItem,
                    "list" => ControlType.List,
                    "listitem" => ControlType.ListItem,
                    "tree" => ControlType.Tree,
                    "treeitem" => ControlType.TreeItem,
                    "checkbox" => ControlType.CheckBox,
                    "radiobutton" => ControlType.RadioButton,
                    "combobox" => ControlType.ComboBox,
                    "slider" => ControlType.Slider,
                    "progressbar" => ControlType.ProgressBar,
                    "calendar" => ControlType.Calendar,
                    "datagrid" => ControlType.DataGrid,
                    "document" => ControlType.Document,
                    "group" => ControlType.Group,
                    "image" => ControlType.Image,
                    "pane" => ControlType.Pane,
                    "scrollbar" => ControlType.ScrollBar,
                    "separator" => ControlType.Separator,
                    "statusbar" => ControlType.StatusBar,
                    "tab" => ControlType.Tab,
                    "tabitem" => ControlType.TabItem,
                    "table" => ControlType.Table,
                    "titlebar" => ControlType.TitleBar,
                    "toolbar" => ControlType.ToolBar,
                    "tooltip" => ControlType.ToolTip,
                    "hyperlink" => ControlType.Hyperlink,
                    "spinner" => ControlType.Spinner,
                    "splitbutton" => ControlType.SplitButton,
                    "custom" => ControlType.Custom,
                    "dataitem" => ControlType.DataItem,
                    "header" => ControlType.Header,
                    "headeritem" => ControlType.HeaderItem,
                    "menu" => ControlType.Menu,
                    "menubar" => ControlType.MenuBar,
                    "thumb" => ControlType.Thumb,
                    _ => null
                };

                return result;
            }
            catch (Exception ex)
            {
                // Log exception details if possible (though we can't access logger here)
                System.Diagnostics.Debug.WriteLine($"GetControlTypeFromString error: {ex.Message}");
                return null;
            }
        }

        private static string? GetElementValue(AutomationElement element)
        {
            try
            {
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object? valuePattern))
                {
                    return ((ValuePattern)valuePattern).Current.Value;
                }
                if (element.TryGetCurrentPattern(TextPattern.Pattern, out object? textPattern))
                {
                    return ((TextPattern)textPattern).DocumentRange.GetText(-1);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        #region Pattern Implementation Methods

        private Task<OperationResult> ExecuteInvokePattern(AutomationElement element)
        {
            if (element.TryGetCurrentPattern(InvokePattern.Pattern, out object? invokePattern))
            {
                ((InvokePattern)invokePattern).Invoke();
                return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "InvokePattern" } });
            }
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support InvokePattern" });
        }

        private Task<OperationResult> ExecuteValuePattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object? valuePattern))
            {
                var pattern = (ValuePattern)valuePattern;
                
                if (parameters != null && parameters.TryGetValue("value", out var value))
                {
                    pattern.SetValue(value.ToString() ?? "");
                    return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "ValuePattern", value = value.ToString() } });
                }
                else
                {
                    var currentValue = pattern.Current.Value;
                    return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "ValuePattern", currentValue = currentValue } });
                }
            }
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support ValuePattern" });
        }

        private Task<OperationResult> ExecuteTogglePattern(AutomationElement element)
        {
            if (element.TryGetCurrentPattern(TogglePattern.Pattern, out object? togglePattern))
            {
                var pattern = (TogglePattern)togglePattern;
                pattern.Toggle();
                return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "TogglePattern", state = pattern.Current.ToggleState.ToString() } });
            }
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support TogglePattern" });
        }

        private Task<OperationResult> ExecuteSelectionItemPattern(AutomationElement element)
        {
            if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out object? selectionItemPattern))
            {
                var pattern = (SelectionItemPattern)selectionItemPattern;
                pattern.Select();
                return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "SelectionItemPattern", isSelected = pattern.Current.IsSelected } });
            }
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support SelectionItemPattern" });
        }

        private Task<OperationResult> ExecuteExpandCollapsePattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out object? expandCollapsePattern))
            {
                var pattern = (ExpandCollapsePattern)expandCollapsePattern;
                
                if (parameters != null && parameters.TryGetValue("expand", out var expandValue))
                {
                    bool expand = Convert.ToBoolean(expandValue);
                    if (expand)
                        pattern.Expand();
                    else
                        pattern.Collapse();
                    
                    return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "ExpandCollapsePattern", state = pattern.Current.ExpandCollapseState.ToString() } });
                }
                else
                {
                    // Toggle current state
                    if (pattern.Current.ExpandCollapseState == ExpandCollapseState.Expanded)
                        pattern.Collapse();
                    else
                        pattern.Expand();
                    
                    return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "ExpandCollapsePattern", state = pattern.Current.ExpandCollapseState.ToString() } });
                }
            }
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support ExpandCollapsePattern" });
        }

        private Task<OperationResult> ExecuteScrollPattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            if (element.TryGetCurrentPattern(ScrollPattern.Pattern, out object? scrollPattern))
            {
                var pattern = (ScrollPattern)scrollPattern;
                
                if (parameters != null)
                {
                    if (parameters.TryGetValue("horizontal", out var horizontalValue) && parameters.TryGetValue("vertical", out var verticalValue))
                    {
                        double horizontal = Convert.ToDouble(horizontalValue);
                        double vertical = Convert.ToDouble(verticalValue);
                        pattern.SetScrollPercent(horizontal, vertical);
                        return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "ScrollPattern", horizontal, vertical } });
                    }
                    else if (parameters.TryGetValue("direction", out var directionValue))
                    {
                        string direction = directionValue.ToString()?.ToLower() ?? "";
                        switch (direction)
                        {
                            case "up":
                                pattern.ScrollVertical(ScrollAmount.SmallDecrement);
                                break;
                            case "down":
                                pattern.ScrollVertical(ScrollAmount.SmallIncrement);
                                break;
                            case "left":
                                pattern.ScrollHorizontal(ScrollAmount.SmallDecrement);
                                break;
                            case "right":
                                pattern.ScrollHorizontal(ScrollAmount.SmallIncrement);
                                break;
                        }
                        return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "ScrollPattern", direction } });
                    }
                }
                
                return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "ScrollPattern", currentHorizontal = pattern.Current.HorizontalScrollPercent, currentVertical = pattern.Current.VerticalScrollPercent } });
            }
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support ScrollPattern" });
        }

        private Task<OperationResult> ExecuteScrollItemPattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            if (element.TryGetCurrentPattern(ScrollItemPattern.Pattern, out object? scrollItemPattern))
            {
                var pattern = (ScrollItemPattern)scrollItemPattern;
                
                // ScrollItemPattern doesn't have parameters, just scroll into view
                pattern.ScrollIntoView();
                return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "ScrollItemPattern", action = "scrolled_into_view" } });
            }
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support ScrollItemPattern" });
        }

        private Task<OperationResult> ExecuteRangeValuePattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            if (element.TryGetCurrentPattern(RangeValuePattern.Pattern, out object? rangeValuePattern))
            {
                var pattern = (RangeValuePattern)rangeValuePattern;
                
                if (parameters != null && parameters.TryGetValue("value", out var value))
                {
                    double newValue = Convert.ToDouble(value);
                    pattern.SetValue(newValue);
                    return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "RangeValuePattern", value = newValue } });
                }
                else
                {
                    return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "RangeValuePattern", currentValue = pattern.Current.Value, minimum = pattern.Current.Minimum, maximum = pattern.Current.Maximum } });
                }
            }
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support RangeValuePattern" });
        }

        private Task<OperationResult> ExecuteTextPattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            try
            {
                if (!element.TryGetCurrentPattern(TextPattern.Pattern, out object? textPatternObj))
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support TextPattern" });
                }

                var pattern = (TextPattern)textPatternObj;
                
                if (parameters != null)
                {
                    // Text search functionality
                    if (parameters.ContainsKey("searchText"))
                    {
                        var searchText = parameters["searchText"].ToString() ?? "";
                        var backward = parameters.ContainsKey("backward") && bool.Parse(parameters["backward"].ToString() ?? "false");
                        var ignoreCase = parameters.ContainsKey("ignoreCase") && bool.Parse(parameters["ignoreCase"].ToString() ?? "false");
                        
                        var searchRange = pattern.DocumentRange;
                        var foundRange = searchRange.FindText(searchText, backward, ignoreCase);
                        
                        if (foundRange != null)
                        {
                            foundRange.Select();
                            return Task.FromResult(new OperationResult 
                            { 
                                Success = true, 
                                Data = new 
                                { 
                                    pattern = "TextPattern", 
                                    action = "search",
                                    foundText = foundRange.GetText(-1),
                                    startPoint = foundRange.GetBoundingRectangles().FirstOrDefault()
                                } 
                            });
                        }
                        else
                        {
                            return Task.FromResult(new OperationResult { Success = false, Error = $"Text '{searchText}' not found" });
                        }
                    }
                    
                    // Get text selection
                    if (parameters.ContainsKey("getSelection"))
                    {
                        var selection = pattern.GetSelection();
                        var selectionData = selection?.Select(s => new
                        {
                            Text = s.GetText(-1),
                            BoundingRectangle = s.GetBoundingRectangles().FirstOrDefault()
                        }).ToArray();
                        
                        return Task.FromResult(new OperationResult 
                        { 
                            Success = true, 
                            Data = new { pattern = "TextPattern", action = "getSelection", selections = selectionData }
                        });
                    }
                    
                    // Range operations
                    if (parameters.ContainsKey("selectRange"))
                    {
                        if (parameters.TryGetValue("startOffset", out var startObj) && 
                            parameters.TryGetValue("endOffset", out var endObj) &&
                            int.TryParse(startObj.ToString(), out var startOffset) &&
                            int.TryParse(endObj.ToString(), out var endOffset))
                        {
                            var documentRange = pattern.DocumentRange;
                            var rangeStart = documentRange.Clone();
                            var rangeEnd = documentRange.Clone();
                            
                            rangeStart.Move(System.Windows.Automation.Text.TextUnit.Character, startOffset);
                            rangeEnd.Move(System.Windows.Automation.Text.TextUnit.Character, endOffset);
                            
                            var selectedRange = rangeStart.Clone();
                            selectedRange.MoveEndpointByRange(System.Windows.Automation.Text.TextPatternRangeEndpoint.End, rangeEnd, System.Windows.Automation.Text.TextPatternRangeEndpoint.Start);
                            selectedRange.Select();
                            
                            return Task.FromResult(new OperationResult 
                            { 
                                Success = true, 
                                Data = new 
                                { 
                                    pattern = "TextPattern", 
                                    action = "selectRange",
                                    selectedText = selectedRange.GetText(-1)
                                }
                            });
                        }
                        return Task.FromResult(new OperationResult { Success = false, Error = "startOffset and endOffset parameters required for selectRange" });
                    }
                    
                    // Get text attributes
                    if (parameters.ContainsKey("getAttributes"))
                    {
                        var documentRange = pattern.DocumentRange;
                        var attributesData = new Dictionary<string, object>();
                        
                        try
                        {
                            attributesData["FontName"] = documentRange.GetAttributeValue(TextPattern.FontNameAttribute) ?? "";
                            attributesData["FontSize"] = documentRange.GetAttributeValue(TextPattern.FontSizeAttribute) ?? "";
                            attributesData["FontWeight"] = documentRange.GetAttributeValue(TextPattern.FontWeightAttribute) ?? "";
                            attributesData["IsItalic"] = documentRange.GetAttributeValue(TextPattern.IsItalicAttribute) ?? false;
                            attributesData["ForegroundColor"] = documentRange.GetAttributeValue(TextPattern.ForegroundColorAttribute) ?? "";
                            attributesData["BackgroundColor"] = documentRange.GetAttributeValue(TextPattern.BackgroundColorAttribute) ?? "";
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("Error getting text attributes: {Error}", ex.Message);
                        }
                        
                        return Task.FromResult(new OperationResult 
                        { 
                            Success = true, 
                            Data = new { pattern = "TextPattern", action = "getAttributes", attributes = attributesData }
                        });
                    }
                    
                    // Legacy text selection
                    if (parameters.TryGetValue("text", out var textValue))
                    {
                        var text = textValue.ToString() ?? "";
                        var range = pattern.DocumentRange;
                        range.Select();
                        return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "TextPattern", selectedText = text } });
                    }
                }
                
                // Default: return document text and basic info
                var documentText = pattern.DocumentRange.GetText(-1);
                var supportedTextSelection = pattern.SupportedTextSelection;
                
                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = new 
                    { 
                        pattern = "TextPattern", 
                        documentText = documentText,
                        textLength = documentText.Length,
                        supportedTextSelection = supportedTextSelection.ToString(),
                        supportsTextSelection = supportedTextSelection != System.Windows.Automation.SupportedTextSelection.None
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing TextPattern");
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        private Task<OperationResult> ExecuteWindowPattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            if (element.TryGetCurrentPattern(WindowPattern.Pattern, out object? windowPattern))
            {
                var pattern = (WindowPattern)windowPattern;
                
                if (parameters != null && parameters.TryGetValue("action", out var actionValue))
                {
                    string action = actionValue.ToString()?.ToLower() ?? "";
                    switch (action)
                    {
                        case "close":
                            pattern.Close();
                            break;
                        case "minimize":
                            pattern.SetWindowVisualState(WindowVisualState.Minimized);
                            break;
                        case "maximize":
                            pattern.SetWindowVisualState(WindowVisualState.Maximized);
                            break;
                        case "normal":
                            pattern.SetWindowVisualState(WindowVisualState.Normal);
                            break;
                    }
                    return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "WindowPattern", action } });
                }
                else
                {
                    return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "WindowPattern", canMaximize = pattern.Current.CanMaximize, canMinimize = pattern.Current.CanMinimize, isModal = pattern.Current.IsModal, windowVisualState = pattern.Current.WindowVisualState.ToString() } });
                }
            }
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support WindowPattern" });
        }

        private Task<OperationResult> ExecuteTransformPattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            if (element.TryGetCurrentPattern(TransformPattern.Pattern, out object? transformPattern))
            {
                var pattern = (TransformPattern)transformPattern;
                
                if (parameters != null)
                {
                    if (parameters.TryGetValue("action", out var actionValue))
                    {
                        string action = actionValue.ToString()?.ToLower() ?? "";
                        switch (action)
                        {
                            case "move":
                                if (parameters.TryGetValue("x", out var xValue) && parameters.TryGetValue("y", out var yValue))
                                {
                                    double x = Convert.ToDouble(xValue);
                                    double y = Convert.ToDouble(yValue);
                                    pattern.Move(x, y);
                                    return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "TransformPattern", action = "move", x, y } });
                                }
                                break;
                            case "resize":
                                if (parameters.TryGetValue("width", out var widthValue) && parameters.TryGetValue("height", out var heightValue))
                                {
                                    double width = Convert.ToDouble(widthValue);
                                    double height = Convert.ToDouble(heightValue);
                                    pattern.Resize(width, height);
                                    return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "TransformPattern", action = "resize", width, height } });
                                }
                                break;
                            case "rotate":
                                if (parameters.TryGetValue("degrees", out var degreesValue))
                                {
                                    double degrees = Convert.ToDouble(degreesValue);
                                    pattern.Rotate(degrees);
                                    return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "TransformPattern", action = "rotate", degrees } });
                                }
                                break;
                        }
                    }
                }
                
                return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "TransformPattern", canMove = pattern.Current.CanMove, canResize = pattern.Current.CanResize, canRotate = pattern.Current.CanRotate } });
            }
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support TransformPattern" });
        }

        private Task<OperationResult> ExecuteDockPattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            if (element.TryGetCurrentPattern(DockPattern.Pattern, out object? dockPattern))
            {
                var pattern = (DockPattern)dockPattern;
                
                if (parameters != null && parameters.TryGetValue("position", out var positionValue))
                {
                    string position = positionValue.ToString()?.ToLower() ?? "";
                    DockPosition dockPosition = position switch
                    {
                        "top" => DockPosition.Top,
                        "bottom" => DockPosition.Bottom,
                        "left" => DockPosition.Left,
                        "right" => DockPosition.Right,
                        "fill" => DockPosition.Fill,
                        "none" => DockPosition.None,
                        _ => DockPosition.None
                    };
                    
                    pattern.SetDockPosition(dockPosition);
                    return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "DockPattern", position = dockPosition.ToString() } });
                }
                else
                {
                    return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "DockPattern", currentPosition = pattern.Current.DockPosition.ToString() } });
                }
            }
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support DockPattern" });
        }

        private Task<OperationResult> ExecuteMultipleViewPattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            if (element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out object? multipleViewPattern))
            {
                var pattern = (MultipleViewPattern)multipleViewPattern;
                
                if (parameters != null && parameters.TryGetValue("viewId", out var viewIdValue))
                {
                    int viewId = Convert.ToInt32(viewIdValue);
                    pattern.SetCurrentView(viewId);
                    return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "MultipleViewPattern", currentView = viewId } });
                }
                
                // Return current view information
                var currentView = pattern.Current.CurrentView;
                var supportedViews = pattern.Current.GetSupportedViews();
                return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "MultipleViewPattern", currentView, supportedViews } });
            }
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support MultipleViewPattern" });
        }

        private Task<OperationResult> ExecuteVirtualizedItemPattern(AutomationElement element)
        {
            if (element.TryGetCurrentPattern(VirtualizedItemPattern.Pattern, out object? virtualizedItemPattern))
            {
                var pattern = (VirtualizedItemPattern)virtualizedItemPattern;
                pattern.Realize();
                return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "VirtualizedItemPattern", action = "realized" } });
            }
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support VirtualizedItemPattern" });
        }

        private Task<OperationResult> ExecuteItemContainerPattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            if (element.TryGetCurrentPattern(ItemContainerPattern.Pattern, out object? itemContainerPattern))
            {
                var pattern = (ItemContainerPattern)itemContainerPattern;
                
                if (parameters != null && parameters.TryGetValue("findText", out var findTextValue))
                {
                    string findText = findTextValue.ToString() ?? "";
                    var foundItem = pattern.FindItemByProperty(null, AutomationElement.NameProperty, findText);
                    
                    if (foundItem != null)
                    {
                        return Task.FromResult(new OperationResult { 
                            Success = true, 
                            Data = new { 
                                pattern = "ItemContainerPattern", 
                                foundItem = new {
                                    name = foundItem.Current.Name,
                                    automationId = foundItem.Current.AutomationId
                                }
                            } 
                        });
                    }
                    else
                    {
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Item with text '{findText}' not found" });
                    }
                }
                
                return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "ItemContainerPattern", action = "ready_for_search" } });
            }
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support ItemContainerPattern" });
        }

        private Task<OperationResult> ExecuteSynchronizedInputPattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            if (element.TryGetCurrentPattern(SynchronizedInputPattern.Pattern, out object? synchronizedInputPattern))
            {
                var pattern = (SynchronizedInputPattern)synchronizedInputPattern;
                
                if (parameters != null && parameters.TryGetValue("cancel", out var cancelValue) && Convert.ToBoolean(cancelValue))
                {
                    pattern.Cancel();
                    return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "SynchronizedInputPattern", action = "cancelled" } });
                }
                
                // SynchronizedInputPattern の具体的な実装は環境に依存するため、基本的な対応のみ
                return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "SynchronizedInputPattern", action = "ready", note = "Use cancel:true to cancel listening" } });
            }
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support SynchronizedInputPattern" });
        }

        #endregion

        #region New Pattern-Specific Methods

        // Core Interaction Patterns
        public async Task<OperationResult> InvokeElementAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            return await ExecuteElementPatternAsync(elementId, "invoke", null, windowTitle, processId);
        }

        public async Task<OperationResult> SetElementValueAsync(string elementId, string value, string? windowTitle = null, int? processId = null)
        {
            var parameters = new Dictionary<string, object> { { "value", value } };
            return await ExecuteElementPatternAsync(elementId, "value", parameters, windowTitle, processId);
        }

        public async Task<OperationResult> GetElementValueAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            return await ExecuteElementPatternAsync(elementId, "value", null, windowTitle, processId);
        }

        public async Task<OperationResult> ToggleElementAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            return await ExecuteElementPatternAsync(elementId, "toggle", null, windowTitle, processId);
        }

        public async Task<OperationResult> SelectElementAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            return await ExecuteElementPatternAsync(elementId, "selectionitem", null, windowTitle, processId);
        }

        // Layout and Navigation Patterns
        public async Task<OperationResult> ExpandCollapseElementAsync(string elementId, bool? expand = null, string? windowTitle = null, int? processId = null)
        {
            var parameters = expand.HasValue ? new Dictionary<string, object> { { "expand", expand.Value } } : null;
            return await ExecuteElementPatternAsync(elementId, "expandcollapse", parameters, windowTitle, processId);
        }

        public async Task<OperationResult> ScrollElementAsync(string elementId, string? direction = null, double? horizontal = null, double? vertical = null, string? windowTitle = null, int? processId = null)
        {
            var parameters = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(direction))
                parameters["direction"] = direction;
            if (horizontal.HasValue)
                parameters["horizontal"] = horizontal.Value;
            if (vertical.HasValue)
                parameters["vertical"] = vertical.Value;
            
            return await ExecuteElementPatternAsync(elementId, "scroll", parameters.Count > 0 ? parameters : null, windowTitle, processId);
        }

        public async Task<OperationResult> ScrollElementIntoViewAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            return await ExecuteElementPatternAsync(elementId, "scrollitem", null, windowTitle, processId);
        }

        // Value and Range Patterns
        public async Task<OperationResult> SetRangeValueAsync(string elementId, double value, string? windowTitle = null, int? processId = null)
        {
            var parameters = new Dictionary<string, object> { { "value", value } };
            return await ExecuteElementPatternAsync(elementId, "rangevalue", parameters, windowTitle, processId);
        }

        public async Task<OperationResult> GetRangeValueAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            return await ExecuteElementPatternAsync(elementId, "rangevalue", null, windowTitle, processId);
        }

        // Window Management Patterns
        public async Task<OperationResult> WindowActionAsync(string elementId, string action, string? windowTitle = null, int? processId = null)
        {
            var parameters = new Dictionary<string, object> { { "action", action } };
            return await ExecuteElementPatternAsync(elementId, "window", parameters, windowTitle, processId);
        }

        public async Task<OperationResult> TransformElementAsync(string elementId, string action, double? x = null, double? y = null, double? width = null, double? height = null, double? degrees = null, string? windowTitle = null, int? processId = null)
        {
            var parameters = new Dictionary<string, object> { { "action", action } };
            if (x.HasValue) parameters["x"] = x.Value;
            if (y.HasValue) parameters["y"] = y.Value;
            if (width.HasValue) parameters["width"] = width.Value;
            if (height.HasValue) parameters["height"] = height.Value;
            if (degrees.HasValue) parameters["degrees"] = degrees.Value;
            
            return await ExecuteElementPatternAsync(elementId, "transform", parameters, windowTitle, processId);
        }

        public async Task<OperationResult> DockElementAsync(string elementId, string position, string? windowTitle = null, int? processId = null)
        {
            var parameters = new Dictionary<string, object> { { "position", position } };
            return await ExecuteElementPatternAsync(elementId, "dock", parameters, windowTitle, processId);
        }

        // Advanced Patterns
        public async Task<OperationResult> ChangeViewAsync(string elementId, int viewId, string? windowTitle = null, int? processId = null)
        {
            var parameters = new Dictionary<string, object> { { "viewId", viewId } };
            return await ExecuteElementPatternAsync(elementId, "multipleview", parameters, windowTitle, processId);
        }

        public async Task<OperationResult> RealizeVirtualizedItemAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            return await ExecuteElementPatternAsync(elementId, "virtualized", null, windowTitle, processId);
        }

        public async Task<OperationResult> FindItemInContainerAsync(string elementId, string findText, string? windowTitle = null, int? processId = null)
        {
            var parameters = new Dictionary<string, object> { { "findText", findText } };
            return await ExecuteElementPatternAsync(elementId, "itemcontainer", parameters, windowTitle, processId);
        }

        public async Task<OperationResult> CancelSynchronizedInputAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            var parameters = new Dictionary<string, object> { { "cancel", true } };
            return await ExecuteElementPatternAsync(elementId, "synchronizedinput", parameters, windowTitle, processId);
        }

        // Text Pattern - Complex text operations
        public async Task<OperationResult> GetTextAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            return await ExecuteElementPatternAsync(elementId, "text", null, windowTitle, processId);
        }

        public async Task<OperationResult> SelectTextAsync(string elementId, int startIndex, int length, string? windowTitle = null, int? processId = null)
        {
            var parameters = new Dictionary<string, object> 
            { 
                { "startIndex", startIndex },
                { "length", length }
            };
            return await ExecuteElementPatternAsync(elementId, "text", parameters, windowTitle, processId);
        }

        public async Task<OperationResult> FindTextAsync(string elementId, string searchText, bool backward = false, bool ignoreCase = false, string? windowTitle = null, int? processId = null)
        {
            var parameters = new Dictionary<string, object> 
            { 
                { "searchText", searchText },
                { "backward", backward },
                { "ignoreCase", ignoreCase }
            };
            return await ExecuteElementPatternAsync(elementId, "text", parameters, windowTitle, processId);
        }

        public async Task<OperationResult> GetTextSelectionAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            var parameters = new Dictionary<string, object> { { "getSelection", true } };
            return await ExecuteElementPatternAsync(elementId, "text", parameters, windowTitle, processId);
        }

        // Tree Navigation - New functionality
        public Task<OperationResult> GetElementTreeAsync(string? windowTitle = null, string treeView = "control", int maxDepth = 3, int? processId = null)
        {
            try
            {
                _logger.LogInformation("Getting element tree for window: {WindowTitle}, view: {TreeView}, maxDepth: {MaxDepth}", 
                    windowTitle, treeView, maxDepth);

                AutomationElement searchRoot = AutomationElement.RootElement;

                if (!string.IsNullOrEmpty(windowTitle))
                {
                    var window = FindWindowByTitle(windowTitle, processId);
                    if (window != null)
                    {
                        searchRoot = window;
                    }
                    else
                    {
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Window '{windowTitle}' not found" });
                    }
                }

                var treeData = BuildElementTree(searchRoot, treeView, maxDepth, 0);
                return Task.FromResult(new OperationResult { Success = true, Data = treeData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element tree");
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        public Task<OperationResult> GetElementPropertiesAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            try
            {
                AutomationElement? element = null;

                if (!string.IsNullOrEmpty(windowTitle))
                {
                    var window = FindWindowByTitle(windowTitle, processId);
                    if (window == null)
                    {
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Window '{windowTitle}' not found" });
                    }
                    element = FindElementInWindow(window, elementId);
                }
                else
                {
                    var condition = new OrCondition(
                        new PropertyCondition(AutomationElement.AutomationIdProperty, elementId),
                        new PropertyCondition(AutomationElement.NameProperty, elementId)
                    );
                    element = AutomationElement.RootElement.FindFirst(TreeScope.Descendants, condition);
                }

                if (element == null)
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });

                var properties = GetDetailedElementProperties(element);
                return Task.FromResult(new OperationResult { Success = true, Data = properties });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element properties");
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        public Task<OperationResult> GetElementPatternsAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            try
            {
                AutomationElement? element = null;

                if (!string.IsNullOrEmpty(windowTitle))
                {
                    var window = FindWindowByTitle(windowTitle, processId);
                    if (window == null)
                    {
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Window '{windowTitle}' not found" });
                    }
                    element = FindElementInWindow(window, elementId);
                }
                else
                {
                    var condition = new OrCondition(
                        new PropertyCondition(AutomationElement.AutomationIdProperty, elementId),
                        new PropertyCondition(AutomationElement.NameProperty, elementId)
                    );
                    element = AutomationElement.RootElement.FindFirst(TreeScope.Descendants, condition);
                }

                if (element == null)
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });

                var patterns = GetElementPatterns(element);
                return Task.FromResult(new OperationResult { Success = true, Data = patterns });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element patterns");
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        #endregion

        #region Helper Methods for New Functionality

        private object? BuildElementTree(AutomationElement element, string treeView, int maxDepth, int currentDepth)
        {
            if (currentDepth >= maxDepth)
                return null;

            try
            {
                var elementData = new
                {
                    Name = element.Current.Name ?? "",
                    AutomationId = element.Current.AutomationId ?? "",
                    ControlType = element.Current.ControlType?.ProgrammaticName ?? "",
                    ClassName = element.Current.ClassName ?? "",
                    IsEnabled = element.Current.IsEnabled,
                    IsVisible = !element.Current.IsOffscreen,
                    Children = new List<object>()
                };

                TreeScope scope = treeView.ToLower() switch
                {
                    "raw" => TreeScope.Children,
                    "control" => TreeScope.Children,
                    "content" => TreeScope.Children,
                    _ => TreeScope.Children
                };

                var condition = treeView.ToLower() switch
                {
                    "raw" => Condition.TrueCondition,
                    "control" => new PropertyCondition(AutomationElement.IsControlElementProperty, true),
                    "content" => new PropertyCondition(AutomationElement.IsContentElementProperty, true),
                    _ => new PropertyCondition(AutomationElement.IsControlElementProperty, true)
                };

                try
                {
                    var children = element.FindAll(scope, condition);
                    if (children != null)
                    {
                        var childList = (List<object>)elementData.Children;
                        foreach (AutomationElement child in children)
                        {
                            var childData = BuildElementTree(child, treeView, maxDepth, currentDepth + 1);
                            if (childData != null)
                                childList.Add(childData);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Error getting children for element: {Error}", ex.Message);
                }

                return elementData;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error building tree for element: {Error}", ex.Message);
                return null;
            }
        }

        private object GetDetailedElementProperties(AutomationElement element)
        {
            try
            {
                var rect = element.Current.BoundingRectangle;
                return new
                {
                    // Basic Properties
                    Name = element.Current.Name ?? "",
                    AutomationId = element.Current.AutomationId ?? "",
                    ControlType = element.Current.ControlType?.ProgrammaticName ?? "",
                    LocalizedControlType = element.Current.LocalizedControlType ?? "",
                    ClassName = element.Current.ClassName ?? "",
                    
                    // State Properties
                    IsEnabled = element.Current.IsEnabled,
                    IsVisible = !element.Current.IsOffscreen,
                    IsKeyboardFocusable = element.Current.IsKeyboardFocusable,
                    HasKeyboardFocus = element.Current.HasKeyboardFocus,
                    IsControlElement = element.Current.IsControlElement,
                    IsContentElement = element.Current.IsContentElement,
                    
                    // Layout Properties
                    BoundingRectangle = new
                    {
                        X = double.IsInfinity(rect.X) || double.IsNaN(rect.X) ? 0 : rect.X,
                        Y = double.IsInfinity(rect.Y) || double.IsNaN(rect.Y) ? 0 : rect.Y,
                        Width = double.IsInfinity(rect.Width) || double.IsNaN(rect.Width) ? 0 : rect.Width,
                        Height = double.IsInfinity(rect.Height) || double.IsNaN(rect.Height) ? 0 : rect.Height
                    },
                    
                    // Additional Properties
                    HelpText = element.Current.HelpText ?? "",
                    AcceleratorKey = element.Current.AcceleratorKey ?? "",
                    AccessKey = element.Current.AccessKey ?? "",
                    ProcessId = element.Current.ProcessId,
                    
                    // Value (if available)
                    Value = GetElementValue(element) ?? ""
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error getting detailed properties: {Error}", ex.Message);
                return new { Error = ex.Message };
            }
        }

        private object GetElementPatterns(AutomationElement element)
        {
            var supportedPatterns = new List<string>();
            var patternDetails = new Dictionary<string, object>();

            // Check each pattern
            var patterns = new[]
            {
                new { Name = "Invoke", Pattern = InvokePattern.Pattern },
                new { Name = "Value", Pattern = ValuePattern.Pattern },
                new { Name = "Toggle", Pattern = TogglePattern.Pattern },
                new { Name = "SelectionItem", Pattern = SelectionItemPattern.Pattern },
                new { Name = "ExpandCollapse", Pattern = ExpandCollapsePattern.Pattern },
                new { Name = "Scroll", Pattern = ScrollPattern.Pattern },
                new { Name = "ScrollItem", Pattern = ScrollItemPattern.Pattern },
                new { Name = "RangeValue", Pattern = RangeValuePattern.Pattern },
                new { Name = "Text", Pattern = TextPattern.Pattern },
                new { Name = "Window", Pattern = WindowPattern.Pattern },
                new { Name = "Transform", Pattern = TransformPattern.Pattern },
                new { Name = "Dock", Pattern = DockPattern.Pattern },
                new { Name = "MultipleView", Pattern = MultipleViewPattern.Pattern },
                new { Name = "VirtualizedItem", Pattern = VirtualizedItemPattern.Pattern },
                new { Name = "ItemContainer", Pattern = ItemContainerPattern.Pattern },
                new { Name = "SynchronizedInput", Pattern = SynchronizedInputPattern.Pattern }
            };

            foreach (var patternInfo in patterns)
            {
                try
                {
                    if (element.TryGetCurrentPattern(patternInfo.Pattern, out var pattern))
                    {
                        supportedPatterns.Add(patternInfo.Name);
                        
                        // Add pattern-specific details
                        switch (patternInfo.Name)
                        {
                            case "Value":
                                var valuePattern = (ValuePattern)pattern;
                                patternDetails[patternInfo.Name] = new
                                {
                                    IsReadOnly = valuePattern.Current.IsReadOnly,
                                    Value = valuePattern.Current.Value
                                };
                                break;
                            case "RangeValue":
                                var rangePattern = (RangeValuePattern)pattern;
                                patternDetails[patternInfo.Name] = new
                                {
                                    Value = rangePattern.Current.Value,
                                    Minimum = rangePattern.Current.Minimum,
                                    Maximum = rangePattern.Current.Maximum,
                                    SmallChange = rangePattern.Current.SmallChange,
                                    LargeChange = rangePattern.Current.LargeChange,
                                    IsReadOnly = rangePattern.Current.IsReadOnly
                                };
                                break;
                            case "Toggle":
                                var togglePattern = (TogglePattern)pattern;
                                patternDetails[patternInfo.Name] = new
                                {
                                    ToggleState = togglePattern.Current.ToggleState.ToString()
                                };
                                break;
                            case "Window":
                                var windowPattern = (WindowPattern)pattern;
                                patternDetails[patternInfo.Name] = new
                                {
                                    CanMaximize = windowPattern.Current.CanMaximize,
                                    CanMinimize = windowPattern.Current.CanMinimize,
                                    IsModal = windowPattern.Current.IsModal,
                                    WindowVisualState = windowPattern.Current.WindowVisualState.ToString(),
                                    WindowInteractionState = windowPattern.Current.WindowInteractionState.ToString()
                                };
                                break;
                            default:
                                patternDetails[patternInfo.Name] = new { Available = true };
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Error checking pattern {Pattern}: {Error}", patternInfo.Name, ex.Message);
                }
            }

            return new
            {
                SupportedPatterns = supportedPatterns,
                PatternDetails = patternDetails
            };
        }

        #endregion

        #region Grid and Table Pattern Methods

        private Task<OperationResult> ExecuteGridPattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            try
            {
                if (!element.TryGetCurrentPattern(GridPattern.Pattern, out var gridPatternObj))
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support GridPattern" });
                }

                var gridPattern = (GridPattern)gridPatternObj;
                
                if (parameters != null && parameters.ContainsKey("getItem"))
                {
                    if (parameters.TryGetValue("row", out var rowObj) && 
                        parameters.TryGetValue("column", out var columnObj) &&
                        int.TryParse(rowObj.ToString(), out var row) &&
                        int.TryParse(columnObj.ToString(), out var column))
                    {
                        var item = gridPattern.GetItem(row, column);
                        if (item != null)
                        {
                            return Task.FromResult(new OperationResult 
                            { 
                                Success = true, 
                                Data = new
                                {
                                    Name = item.Current.Name ?? "",
                                    AutomationId = item.Current.AutomationId ?? "",
                                    ControlType = item.Current.ControlType?.ProgrammaticName ?? "",
                                    Value = item.Current.IsContentElement ? (item.Current.Name ?? "") : "",
                                    Row = row,
                                    Column = column
                                }
                            });
                        }
                        else
                        {
                            return Task.FromResult(new OperationResult { Success = false, Error = $"Grid item at row {row}, column {column} not found" });
                        }
                    }
                    return Task.FromResult(new OperationResult { Success = false, Error = "Row and column parameters required for getItem operation" });
                }

                // Default: return grid information
                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = new
                    {
                        RowCount = gridPattern.Current.RowCount,
                        ColumnCount = gridPattern.Current.ColumnCount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing GridPattern");
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        private Task<OperationResult> ExecuteGridItemPattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            try
            {
                if (!element.TryGetCurrentPattern(GridItemPattern.Pattern, out var gridItemPatternObj))
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support GridItemPattern" });
                }

                var gridItemPattern = (GridItemPattern)gridItemPatternObj;
                
                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = new
                    {
                        Row = gridItemPattern.Current.Row,
                        Column = gridItemPattern.Current.Column,
                        RowSpan = gridItemPattern.Current.RowSpan,
                        ColumnSpan = gridItemPattern.Current.ColumnSpan,
                        ContainingGrid = gridItemPattern.Current.ContainingGrid?.Current.Name ?? ""
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing GridItemPattern");
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        private Task<OperationResult> ExecuteTablePattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            try
            {
                if (!element.TryGetCurrentPattern(TablePattern.Pattern, out var tablePatternObj))
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support TablePattern" });
                }

                var tablePattern = (TablePattern)tablePatternObj;
                
                if (parameters != null)
                {
                    if (parameters.ContainsKey("getColumnHeaders"))
                    {
                        var columnHeaders = tablePattern.Current.GetColumnHeaders();
                        var headerData = columnHeaders?.Cast<AutomationElement>()
                            .Select(h => new { 
                                Name = h.Current.Name ?? "", 
                                AutomationId = h.Current.AutomationId ?? "" 
                            }).ToArray();
                        
                        return Task.FromResult(new OperationResult { Success = true, Data = headerData });
                    }
                    
                    if (parameters.ContainsKey("getRowHeaders"))
                    {
                        var rowHeaders = tablePattern.Current.GetRowHeaders();
                        var headerData = rowHeaders?.Cast<AutomationElement>()
                            .Select(h => new { 
                                Name = h.Current.Name ?? "", 
                                AutomationId = h.Current.AutomationId ?? "" 
                            }).ToArray();
                        
                        return Task.FromResult(new OperationResult { Success = true, Data = headerData });
                    }
                }

                // Default: return table information
                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = new
                    {
                        RowCount = tablePattern.Current.RowCount,
                        ColumnCount = tablePattern.Current.ColumnCount,
                        RowOrColumnMajor = tablePattern.Current.RowOrColumnMajor.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing TablePattern");
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        private Task<OperationResult> ExecuteTableItemPattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            try
            {
                if (!element.TryGetCurrentPattern(TableItemPattern.Pattern, out var tableItemPatternObj))
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support TableItemPattern" });
                }

                var tableItemPattern = (TableItemPattern)tableItemPatternObj;
                
                var data = new
                {
                    Row = tableItemPattern.Current.Row,
                    Column = tableItemPattern.Current.Column,
                    RowSpan = tableItemPattern.Current.RowSpan,
                    ColumnSpan = tableItemPattern.Current.ColumnSpan,
                    ContainingGrid = tableItemPattern.Current.ContainingGrid?.Current.Name ?? ""
                };

                if (parameters != null)
                {
                    if (parameters.ContainsKey("getColumnHeaderItems"))
                    {
                        var columnHeaders = tableItemPattern.Current.GetColumnHeaderItems();
                        var headerData = columnHeaders?.Cast<AutomationElement>()
                            .Select(h => new { 
                                Name = h.Current.Name ?? "", 
                                AutomationId = h.Current.AutomationId ?? "" 
                            }).ToArray();
                        
                        return Task.FromResult(new OperationResult 
                        { 
                            Success = true, 
                            Data = new { TableItem = data, ColumnHeaders = headerData }
                        });
                    }
                    
                    if (parameters.ContainsKey("getRowHeaderItems"))
                    {
                        var rowHeaders = tableItemPattern.Current.GetRowHeaderItems();
                        var headerData = rowHeaders?.Cast<AutomationElement>()
                            .Select(h => new { 
                                Name = h.Current.Name ?? "", 
                                AutomationId = h.Current.AutomationId ?? "" 
                            }).ToArray();
                        
                        return Task.FromResult(new OperationResult 
                        { 
                            Success = true, 
                            Data = new { TableItem = data, RowHeaders = headerData }
                        });
                    }
                }

                return Task.FromResult(new OperationResult { Success = true, Data = data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing TableItemPattern");
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        private Task<OperationResult> ExecuteSelectionPattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            try
            {
                if (!element.TryGetCurrentPattern(SelectionPattern.Pattern, out var selectionPatternObj))
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support SelectionPattern" });
                }

                var selectionPattern = (SelectionPattern)selectionPatternObj;
                
                if (parameters != null)
                {
                    if (parameters.ContainsKey("getSelection"))
                    {
                        var selection = selectionPattern.Current.GetSelection();
                        var selectionData = selection?.Cast<AutomationElement>()
                            .Select(s => new { 
                                Name = s.Current.Name ?? "", 
                                AutomationId = s.Current.AutomationId ?? "",
                                ControlType = s.Current.ControlType?.ProgrammaticName ?? ""
                            }).ToArray();
                        
                        return Task.FromResult(new OperationResult { Success = true, Data = selectionData });
                    }
                }

                // Default: return selection container information
                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = new
                    {
                        CanSelectMultiple = selectionPattern.Current.CanSelectMultiple,
                        IsSelectionRequired = selectionPattern.Current.IsSelectionRequired,
                        SelectionCount = selectionPattern.Current.GetSelection()?.Length ?? 0
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing SelectionPattern");
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        #endregion

        #endregion
    }
}