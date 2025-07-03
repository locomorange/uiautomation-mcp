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

        public Task<OperationResult> GetElementInfoAsync(string? windowTitle = null, string? controlType = null, int? windowIndex = null)
        {
            try
            {
                _logger.LogInformation("GetElementInfo called with windowTitle: {WindowTitle}, controlType: {ControlType}, windowIndex: {WindowIndex}", 
                    windowTitle, controlType, windowIndex);

                AutomationElement searchRoot = AutomationElement.RootElement;

                if (!string.IsNullOrEmpty(windowTitle))
                {
                    var window = FindWindowByTitle(windowTitle, windowIndex);
                    if (window != null)
                    {
                        searchRoot = window;
                        _logger.LogInformation("Found window: {WindowName}", window.Current.Name);
                    }
                    else
                    {
                        var message = windowIndex.HasValue 
                            ? $"Window '{windowTitle}' (index {windowIndex}) not found, searching in all windows"
                            : $"Window '{windowTitle}' not found, searching in all windows";
                        _logger.LogWarning(message);
                        // Continue with root element instead of failing
                    }
                }

                var elements = new List<ElementInfo>();
                var conditions = new List<Condition>();

                if (!string.IsNullOrEmpty(controlType))
                {
                    var ctrlType = GetControlTypeFromString(controlType);
                    if (ctrlType != null)
                        conditions.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, ctrlType));
                }

                Condition condition = conditions.Count > 0 ?
                    (conditions.Count == 1 ? conditions[0] : new AndCondition(conditions.ToArray())) :
                    Condition.TrueCondition;

                AutomationElementCollection? foundElements = null;
                try
                {
                    _logger.LogInformation("Starting FindAll operation on searchRoot");
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
                    _logger.LogError(ex, "Error in GetElementInfo: FindAll failed");
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
                        elements.Add(new ElementInfo
                        {
                            Name = element.Current.Name ?? "",
                            AutomationId = element.Current.AutomationId ?? "",
                            ControlType = element.Current.ControlType?.ProgrammaticName ?? "",
                            ClassName = element.Current.ClassName ?? "",
                            BoundingRectangle = new BoundingRectangle
                            {
                                X = element.Current.BoundingRectangle.X,
                                Y = element.Current.BoundingRectangle.Y,
                                Width = element.Current.BoundingRectangle.Width,
                                Height = element.Current.BoundingRectangle.Height
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

        public Task<OperationResult> ClickElementAsync(string elementId, string? windowTitle = null, int? windowIndex = null)
        {
            try
            {
                AutomationElement? element = null;

                if (!string.IsNullOrEmpty(windowTitle))
                {
                    var window = FindWindowByTitle(windowTitle, windowIndex);
                    if (window == null)
                    {
                        var errorMsg = windowIndex.HasValue 
                            ? $"Window '{windowTitle}' (index {windowIndex}) not found"
                            : $"Window '{windowTitle}' not found";
                        return Task.FromResult(new OperationResult { Success = false, Error = errorMsg });
                    }

                    element = FindElementInWindow(window, elementId);
                }
                else
                {
                    element = AutomationElement.RootElement.FindFirst(TreeScope.Descendants,
                        new OrCondition(
                            new PropertyCondition(AutomationElement.AutomationIdProperty, elementId),
                            new PropertyCondition(AutomationElement.NameProperty, elementId)
                        ));
                }

                if (element == null)
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });

                if (element.TryGetCurrentPattern(InvokePattern.Pattern, out object? invokePattern))
                {
                    ((InvokePattern)invokePattern).Invoke();
                    return Task.FromResult(new OperationResult { Success = true, Data = new { method = "invoke" } });
                }

                var rect = element.Current.BoundingRectangle;
                var centerX = rect.X + rect.Width / 2;
                var centerY = rect.Y + rect.Height / 2;

                // UI Automation fallback - try to use automation patterns only
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support Invoke pattern and mouse click functionality has been removed" });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        public Task<OperationResult> SendKeysAsync(string text, string? elementId = null, string? windowTitle = null, int? windowIndex = null)
        {
            try
            {
                _logger.LogInformation("Sending keys: {Text} to element: {ElementId}", text, elementId);

                AutomationElement? element = null;

                if (!string.IsNullOrEmpty(elementId))
                {
                    AutomationElement searchRoot = AutomationElement.RootElement;
                    if (!string.IsNullOrEmpty(windowTitle))
                    {
                        searchRoot = FindWindowByTitle(windowTitle, windowIndex) ?? AutomationElement.RootElement;
                        if (searchRoot == AutomationElement.RootElement && !string.IsNullOrEmpty(windowTitle))
                        {
                            var errorMsg = windowIndex.HasValue 
                                ? $"Window '{windowTitle}' (index {windowIndex}) not found"
                                : $"Window '{windowTitle}' not found";
                            return Task.FromResult(new OperationResult { Success = false, Error = errorMsg });
                        }
                    }

                    element = FindElementInWindow(searchRoot, elementId);
                }

                if (element != null)
                {
                    element.SetFocus();
                    if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object? valuePattern))
                    {
                        ((ValuePattern)valuePattern).SetValue(text);
                        _logger.LogInformation("Text sent using ValuePattern");
                        return Task.FromResult(new OperationResult { Success = true, Data = new { method = "value_pattern" } });
                    }
                }

                System.Windows.Forms.SendKeys.SendWait(text);
                _logger.LogInformation("Text sent using SendKeys");
                return Task.FromResult(new OperationResult { Success = true, Data = new { method = "sendkeys" } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending keys");
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        public Task<OperationResult> ExecuteElementPatternAsync(string elementId, string patternName, Dictionary<string, object>? parameters = null, string? windowTitle = null, int? windowIndex = null)
        {
            try
            {
                _logger.LogInformation("Executing pattern {PatternName} on element {ElementId}", patternName, elementId);

                // Find the element first
                AutomationElement? element = null;

                if (!string.IsNullOrEmpty(windowTitle))
                {
                    var window = FindWindowByTitle(windowTitle, windowIndex);
                    if (window == null)
                    {
                        var errorMsg = windowIndex.HasValue 
                            ? $"Window '{windowTitle}' (index {windowIndex}) not found"
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

                // Execute the appropriate pattern
                switch (patternName.ToLower())
                {
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
                    
                    case "scroll":
                        return ExecuteScrollPattern(element, parameters);
                    
                    case "rangevalue":
                        return ExecuteRangeValuePattern(element, parameters);
                    
                    case "text":
                        return ExecuteTextPattern(element, parameters);
                    
                    case "window":
                        return ExecuteWindowPattern(element, parameters);
                    
                    case "grid":
                        return ExecuteGridPattern(element, parameters);
                    
                    case "griditem":
                        return ExecuteGridItemPattern(element, parameters);
                    
                    case "table":
                        return ExecuteTablePattern(element, parameters);
                    
                    case "tableitem":
                        return ExecuteTableItemPattern(element, parameters);
                    
                    case "selection":
                        return ExecuteSelectionPattern(element, parameters);
                    
                    case "transform":
                        return ExecuteTransformPattern(element, parameters);
                    
                    case "dock":
                        return ExecuteDockPattern(element, parameters);
                    
                    default:
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Pattern '{patternName}' not supported" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing pattern {PatternName} on element {ElementId}", patternName, elementId);
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        public Task<ScreenshotResult> TakeScreenshotAsync(string? windowTitle = null, string? outputPath = null, int maxTokens = 0, int? windowIndex = null)
        {
            try
            {
                _logger.LogInformation("Taking screenshot of window: {WindowTitle}, maxTokens: {MaxTokens}", 
                    windowTitle, maxTokens);

                // Get screen bounds
                Rectangle bounds;
                if (!string.IsNullOrEmpty(windowTitle))
                {
                    var window = FindWindowByTitle(windowTitle, windowIndex);
                    if (window == null)
                    {
                        var errorMsg = windowIndex.HasValue 
                            ? $"Window '{windowTitle}' (index {windowIndex}) not found"
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

        public Task<OperationResult> FindElementsAsync(string? searchText = null, string? controlType = null, string? windowTitle = null, int? windowIndex = null)
        {
            try
            {
                _logger.LogInformation("Finding elements with searchText: {SearchText}, controlType: {ControlType}, window: {WindowTitle}", 
                    searchText, controlType, windowTitle);

                AutomationElement searchRoot = AutomationElement.RootElement;

                if (!string.IsNullOrEmpty(windowTitle))
                {
                    var window = FindWindowByTitle(windowTitle, windowIndex);
                    if (window != null)
                    {
                        searchRoot = window;
                    }
                    else
                    {
                        var message = windowIndex.HasValue 
                            ? $"Window '{windowTitle}' (index {windowIndex}) not found, searching in all windows"
                            : $"Window '{windowTitle}' not found, searching in all windows";
                        _logger.LogWarning(message);
                        // Continue with root element instead of failing
                    }
                }

                var conditions = new List<Condition>();

                if (!string.IsNullOrEmpty(searchText))
                {
                    conditions.Add(new OrCondition(
                        new PropertyCondition(AutomationElement.NameProperty, searchText),
                        new PropertyCondition(AutomationElement.AutomationIdProperty, searchText)
                    ));
                }

                if (!string.IsNullOrEmpty(controlType))
                {
                    var ctrlType = GetControlTypeFromString(controlType);
                    if (ctrlType != null)
                        conditions.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, ctrlType));
                }

                Condition finalCondition = conditions.Count > 0 ?
                    (conditions.Count == 1 ? conditions[0] : new AndCondition(conditions.ToArray())) :
                    Condition.TrueCondition;

                var elements = new List<ElementInfo>();
                AutomationElementCollection? foundElements = null;
                try
                {
                    foundElements = searchRoot.FindAll(TreeScope.Descendants, finalCondition);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in FindElements: FindAll failed");
                    return Task.FromResult(new OperationResult { Success = false, Error = $"FindAll failed: {ex}" });
                }

                if (foundElements == null)
                {
                    return Task.FromResult(new OperationResult { Success = true, Data = elements });
                }

                foreach (AutomationElement element in foundElements)
                {
                    try
                    {
                        elements.Add(new ElementInfo
                        {
                            Name = element.Current.Name,
                            AutomationId = element.Current.AutomationId,
                            ControlType = element.Current.ControlType.ProgrammaticName,
                            ClassName = element.Current.ClassName,
                            BoundingRectangle = new BoundingRectangle
                            {
                                X = element.Current.BoundingRectangle.X,
                                Y = element.Current.BoundingRectangle.Y,
                                Width = element.Current.BoundingRectangle.Width,
                                Height = element.Current.BoundingRectangle.Height
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

        private static AutomationElement? FindWindowByTitle(string title, int? windowIndex = null)
        {
            if (string.IsNullOrWhiteSpace(title)) return null;
            
            try
            {
                Console.WriteLine($"[DEBUG] FindWindowByTitle called with title: '{title}', windowIndex: {windowIndex}");
                
                var windows = AutomationElement.RootElement.FindAll(TreeScope.Children,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));
                
                Console.WriteLine($"[DEBUG] Found {windows.Count} total windows");
                
                var matchingWindows = new List<AutomationElement>();
                
                foreach (AutomationElement window in windows)
                {
                    try
                    {
                        var name = window.Current.Name;
                        Console.WriteLine($"[DEBUG] Checking window: '{name}'");
                        
                        if (!string.IsNullOrEmpty(name))
                        {
                            // Try exact match first (case insensitive)
                            if (string.Equals(name.Trim(), title.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"[DEBUG] Exact match found: '{name}'");
                                matchingWindows.Add(window);
                            }
                            // Try contains match for partial matching
                            else if (name.Contains(title, StringComparison.OrdinalIgnoreCase) || 
                                     title.Contains(name, StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"[DEBUG] Partial match found: '{name}'");
                                matchingWindows.Add(window);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DEBUG] Error reading window properties: {ex.Message}");
                        continue;
                    }
                }
                
                Console.WriteLine($"[DEBUG] Found {matchingWindows.Count} matching windows");
                
                if (matchingWindows.Count == 0)
                {
                    Console.WriteLine($"[DEBUG] No matching windows found");
                    return null;
                }
                    
                // If windowIndex is specified, use it
                if (windowIndex.HasValue)
                {
                    Console.WriteLine($"[DEBUG] Using windowIndex: {windowIndex.Value}");
                    if (windowIndex.Value >= 0 && windowIndex.Value < matchingWindows.Count)
                    {
                        var selectedWindow = matchingWindows[windowIndex.Value];
                        Console.WriteLine($"[DEBUG] Selected window at index {windowIndex.Value}: '{selectedWindow.Current.Name}'");
                        return selectedWindow;
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] Index {windowIndex.Value} is out of range (0-{matchingWindows.Count - 1})");
                        return null; // Index out of range
                    }
                }
                
                // If no index specified, prefer visible and enabled windows
                foreach (var window in matchingWindows)
                {
                    try
                    {
                        if (window.Current.IsEnabled && !window.Current.IsOffscreen)
                        {
                            Console.WriteLine($"[DEBUG] Returning first visible/enabled window: '{window.Current.Name}'");
                            return window;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DEBUG] Error checking window state: {ex.Message}");
                        continue;
                    }
                }
                
                // Return first matching window if no visible/enabled found
                Console.WriteLine($"[DEBUG] Returning first matching window: '{matchingWindows[0].Current.Name}'");
                return matchingWindows[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Error in window search: {ex.Message}");
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
                    _ => null
                };

                return result;
            }
            catch (Exception)
            {
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
            if (element.TryGetCurrentPattern(TextPattern.Pattern, out object? textPattern))
            {
                var pattern = (TextPattern)textPattern;
                
                if (parameters != null && parameters.TryGetValue("text", out var textValue))
                {
                    var text = textValue.ToString() ?? "";
                    var range = pattern.DocumentRange;
                    range.Select();
                    return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "TextPattern", selectedText = text } });
                }
                else
                {
                    var documentText = pattern.DocumentRange.GetText(-1);
                    return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "TextPattern", documentText } });
                }
            }
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support TextPattern" });
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

        private Task<OperationResult> ExecuteGridPattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            if (element.TryGetCurrentPattern(GridPattern.Pattern, out object? gridPattern))
            {
                var pattern = (GridPattern)gridPattern;
                
                if (parameters != null && parameters.TryGetValue("row", out var rowValue) && parameters.TryGetValue("column", out var columnValue))
                {
                    int row = Convert.ToInt32(rowValue);
                    int column = Convert.ToInt32(columnValue);
                    var item = pattern.GetItem(row, column);
                    return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "GridPattern", row, column, itemName = item?.Current.Name } });
                }
                else
                {
                    return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "GridPattern", rowCount = pattern.Current.RowCount, columnCount = pattern.Current.ColumnCount } });
                }
            }
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support GridPattern" });
        }

        private Task<OperationResult> ExecuteGridItemPattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            if (element.TryGetCurrentPattern(GridItemPattern.Pattern, out object? gridItemPattern))
            {
                var pattern = (GridItemPattern)gridItemPattern;
                return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "GridItemPattern", row = pattern.Current.Row, column = pattern.Current.Column, rowSpan = pattern.Current.RowSpan, columnSpan = pattern.Current.ColumnSpan } });
            }
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support GridItemPattern" });
        }

        private Task<OperationResult> ExecuteTablePattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            if (element.TryGetCurrentPattern(TablePattern.Pattern, out object? tablePattern))
            {
                var pattern = (TablePattern)tablePattern;
                var headers = pattern.Current.GetRowHeaders();
                var columnHeaders = pattern.Current.GetColumnHeaders();
                return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "TablePattern", rowCount = pattern.Current.RowCount, columnCount = pattern.Current.ColumnCount, headerCount = headers.Length, columnHeaderCount = columnHeaders.Length } });
            }
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support TablePattern" });
        }

        private Task<OperationResult> ExecuteTableItemPattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            if (element.TryGetCurrentPattern(TableItemPattern.Pattern, out object? tableItemPattern))
            {
                var pattern = (TableItemPattern)tableItemPattern;
                var rowHeaders = pattern.Current.GetRowHeaderItems();
                var columnHeaders = pattern.Current.GetColumnHeaderItems();
                return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "TableItemPattern", rowHeaderCount = rowHeaders.Length, columnHeaderCount = columnHeaders.Length } });
            }
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support TableItemPattern" });
        }

        private Task<OperationResult> ExecuteSelectionPattern(AutomationElement element, Dictionary<string, object>? parameters)
        {
            if (element.TryGetCurrentPattern(SelectionPattern.Pattern, out object? selectionPattern))
            {
                var pattern = (SelectionPattern)selectionPattern;
                var selection = pattern.Current.GetSelection();
                return Task.FromResult(new OperationResult { Success = true, Data = new { pattern = "SelectionPattern", canSelectMultiple = pattern.Current.CanSelectMultiple, isSelectionRequired = pattern.Current.IsSelectionRequired, selectionCount = selection.Length } });
            }
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support SelectionPattern" });
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

        #endregion


        #endregion
    }
}