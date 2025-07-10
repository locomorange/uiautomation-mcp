using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Screenshot
{
    public class TakeScreenshotOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public TakeScreenshotOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var outputPath = request.Parameters?.GetValueOrDefault("outputPath")?.ToString() ?? "";
            var includeBase64 = request.Parameters?.GetValueOrDefault("includeBase64")?.ToString() is string includeBase64Str && 
                bool.TryParse(includeBase64Str, out var parsedIncludeBase64) ? parsedIncludeBase64 : false;
            var captureMode = request.Parameters?.GetValueOrDefault("captureMode")?.ToString() ?? "element"; // "element", "window", "screen"

            try
            {
                Rectangle captureArea;
                AutomationElement? targetElement = null;

                // Determine capture area based on mode
                switch (captureMode.ToLower())
                {
                    case "element":
                        if (string.IsNullOrEmpty(elementId))
                            return Task.FromResult(new OperationResult { Success = false, Error = "Element ID is required for element capture mode" });

                        targetElement = _elementFinderService.FindElementById(elementId, windowTitle, processId);
                        if (targetElement == null)
                            return Task.FromResult(new OperationResult { Success = false, Error = "Element not found" });

                        var rect = targetElement.Current.BoundingRectangle;
                        captureArea = new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
                        break;

                    case "window":
                        AutomationElement? windowElement = null;
                        if (processId > 0)
                        {
                            var processCondition = new PropertyCondition(AutomationElement.ProcessIdProperty, processId);
                            windowElement = AutomationElement.RootElement.FindFirst(TreeScope.Children, processCondition);
                        }
                        else if (!string.IsNullOrEmpty(windowTitle))
                        {
                            var titleCondition = new PropertyCondition(AutomationElement.NameProperty, windowTitle);
                            windowElement = AutomationElement.RootElement.FindFirst(TreeScope.Children, titleCondition);
                        }

                        if (windowElement == null)
                            return Task.FromResult(new OperationResult { Success = false, Error = "Window not found" });

                        var windowRect = windowElement.Current.BoundingRectangle;
                        captureArea = new Rectangle((int)windowRect.X, (int)windowRect.Y, (int)windowRect.Width, (int)windowRect.Height);
                        break;

                    case "screen":
                        captureArea = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                        break;

                    default:
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Unknown capture mode: {captureMode}" });
                }

                // Validate capture area
                if (captureArea.Width <= 0 || captureArea.Height <= 0)
                    return Task.FromResult(new OperationResult { Success = false, Error = "Invalid capture area dimensions" });

                // Capture screenshot
                using var bitmap = new Bitmap(captureArea.Width, captureArea.Height);
                using var graphics = Graphics.FromImage(bitmap);
                graphics.CopyFromScreen(captureArea.Location, Point.Empty, captureArea.Size);

                // Generate output path if not provided
                if (string.IsNullOrEmpty(outputPath))
                {
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var fileName = $"screenshot_{captureMode}_{timestamp}.png";
                    outputPath = Path.Combine(Path.GetTempPath(), fileName);
                }

                // Save screenshot
                bitmap.Save(outputPath, ImageFormat.Png);

                // Prepare result
                var result = new Dictionary<string, object>
                {
                    ["OutputPath"] = outputPath,
                    ["Width"] = captureArea.Width,
                    ["Height"] = captureArea.Height,
                    ["CaptureMode"] = captureMode,
                    ["Timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["FileSize"] = new FileInfo(outputPath).Length
                };

                // Add base64 if requested
                if (includeBase64)
                {
                    var imageBytes = File.ReadAllBytes(outputPath);
                    result["Base64Image"] = Convert.ToBase64String(imageBytes);
                }

                // Add element info if capturing an element
                if (targetElement != null)
                {
                    result["ElementInfo"] = new Dictionary<string, object>
                    {
                        ["Name"] = targetElement.Current.Name,
                        ["AutomationId"] = targetElement.Current.AutomationId,
                        ["ControlType"] = targetElement.Current.ControlType.LocalizedControlType,
                        ["BoundingRectangle"] = new
                        {
                            X = captureArea.X,
                            Y = captureArea.Y,
                            Width = captureArea.Width,
                            Height = captureArea.Height
                        }
                    };
                }

                return Task.FromResult(new OperationResult { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error taking screenshot: {ex.Message}" });
            }
        }
    }
}