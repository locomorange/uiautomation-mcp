using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Windows.Forms;
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

        public Task<OperationResult> GetElementInfoAsync(string? windowTitle = null, string? controlType = null)
        {
            try
            {
                AutomationElement searchRoot = AutomationElement.RootElement;

                if (!string.IsNullOrEmpty(windowTitle))
                {
                    searchRoot = FindWindowByTitle(windowTitle) ?? searchRoot;
                    if (searchRoot == AutomationElement.RootElement && !string.IsNullOrEmpty(windowTitle))
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Window '{windowTitle}' not found" });
                }

                var elements = new List<ElementInfo>();
                Condition condition = Condition.TrueCondition;

                if (!string.IsNullOrEmpty(controlType))
                {
                    var ctrlType = GetControlTypeFromString(controlType);
                    if (ctrlType != null)
                        condition = new PropertyCondition(AutomationElement.ControlTypeProperty, ctrlType);
                }

                AutomationElementCollection? foundElements = null;
                try
                {
                    foundElements = searchRoot.FindAll(TreeScope.Descendants, condition);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in GetElementInfo: FindAll failed");
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
                            IsVisible = !element.Current.IsOffscreen,
                            HelpText = element.Current.HelpText,
                            Value = GetElementValue(element)
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Failed to read element properties: {Error}", ex.Message);
                        // Skip this element and continue with the next one
                        continue;
                    }
                }

                return Task.FromResult(new OperationResult { Success = true, Data = elements });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetElementInfo");
                return Task.FromResult(new OperationResult { Success = false, Error = ex.ToString() });
            }
        }

        public Task<OperationResult> ClickElementAsync(string elementId, string? windowTitle = null)
        {
            try
            {
                AutomationElement? element = null;

                if (!string.IsNullOrEmpty(windowTitle))
                {
                    var window = FindWindowByTitle(windowTitle);
                    if (window == null)
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Window '{windowTitle}' not found" });

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

                PerformMouseClick((int)centerX, (int)centerY);

                return Task.FromResult(new OperationResult { Success = true, Data = new { method = "mouse_click", x = centerX, y = centerY } });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        public Task<OperationResult> SendKeysAsync(string text, string? elementId = null, string? windowTitle = null)
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
                        searchRoot = FindWindowByTitle(windowTitle) ?? AutomationElement.RootElement;
                        if (searchRoot == AutomationElement.RootElement && !string.IsNullOrEmpty(windowTitle))
                            return Task.FromResult(new OperationResult { Success = false, Error = $"Window '{windowTitle}' not found" });
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

        public Task<OperationResult> MouseClickAsync(int x, int y, string button = "left")
        {
            try
            {
                _logger.LogInformation("Mouse click at ({X}, {Y}) with {Button} button", x, y, button);

                PerformMouseClick(x, y, button);

                return Task.FromResult(new OperationResult { Success = true, Data = new { x, y, button } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing mouse click");
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        public Task<ScreenshotResult> TakeScreenshotAsync(string? windowTitle = null, string? outputPath = null)
        {
            try
            {
                _logger.LogInformation("Taking screenshot of window: {WindowTitle}", windowTitle);

                outputPath ??= Path.Combine(Path.GetTempPath(), $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");

                Rectangle bounds;

                if (!string.IsNullOrEmpty(windowTitle))
                {
                    var window = FindWindowByTitle(windowTitle);
                    if (window == null)
                        return Task.FromResult(new ScreenshotResult { Success = false, Error = $"Window '{windowTitle}' not found" });

                    var rect = window.Current.BoundingRectangle;
                    bounds = new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
                }
                else
                {
                    bounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
                }

                using (var bitmap = new Bitmap(bounds.Width, bounds.Height))
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);
                    bitmap.Save(outputPath, ImageFormat.Png);
                }

                var base64Image = Convert.ToBase64String(File.ReadAllBytes(outputPath));

                _logger.LogInformation("Screenshot saved to: {OutputPath}", outputPath);

                return Task.FromResult(new ScreenshotResult
                {
                    Success = true,
                    OutputPath = outputPath,
                    Base64Image = base64Image,
                    Width = bounds.Width,
                    Height = bounds.Height
                });
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

        public Task<OperationResult> FindElementsAsync(string? searchText = null, string? controlType = null, string? windowTitle = null)
        {
            try
            {
                _logger.LogInformation("Finding elements with searchText: {SearchText}, controlType: {ControlType}, window: {WindowTitle}", 
                    searchText, controlType, windowTitle);

                AutomationElement searchRoot = AutomationElement.RootElement;

                if (!string.IsNullOrEmpty(windowTitle))
                {
                    searchRoot = FindWindowByTitle(windowTitle) ?? AutomationElement.RootElement;
                    if (searchRoot == AutomationElement.RootElement && !string.IsNullOrEmpty(windowTitle))
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Window '{windowTitle}' not found" });
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

        private static AutomationElement? FindWindowByTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return null;
            var windows = AutomationElement.RootElement.FindAll(TreeScope.Children,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));
            foreach (AutomationElement window in windows)
            {
                try
                {
                    var name = window.Current.Name;
                    if (!string.IsNullOrEmpty(name))
                    {
                        // Try exact match first
                        if (string.Equals(name.Trim(), title.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            return window;
                        }
                        // Then try contains match
                        if (name.Trim().Contains(title.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            return window;
                        }
                        // Try normalized string comparison for Japanese characters
                        var normalizedName = NormalizeString(name);
                        var normalizedTitle = NormalizeString(title);
                        if (normalizedName.Contains(normalizedTitle))
                        {
                            return window;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[FindWindowByTitle] Error: {ex.Message}");
                    continue;
                }
            }
            return null;
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

            return controlTypeString.ToLower() switch
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

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x20;
        private const int MOUSEEVENTF_MIDDLEUP = 0x40;

        private static void PerformMouseClick(int x, int y, string button = "left")
        {
            Cursor.Position = new Point(x, y);

            switch (button.ToLower())
            {
                case "left":
                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)x, (uint)y, 0, 0);
                    break;
                case "right":
                    mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, (uint)x, (uint)y, 0, 0);
                    break;
                case "middle":
                    mouse_event(MOUSEEVENTF_MIDDLEDOWN | MOUSEEVENTF_MIDDLEUP, (uint)x, (uint)y, 0, 0);
                    break;
            }
        }

        #endregion
    }
}