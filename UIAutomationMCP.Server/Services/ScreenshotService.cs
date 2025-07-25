using UIAutomationMCP.Models.Abstractions;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Server.Abstractions;
using UIAutomationMCP.Core.Abstractions;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Validation;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Models;

namespace UIAutomationMCP.Server.Services
{
    public class ScreenshotService : BaseUIAutomationService<ScreenshotServiceMetadata>, IScreenshotService
    {
        private readonly IElementSearchService _elementSearchService;

        // Win32 API declarations for screen dimensions
        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        private const int SM_CXSCREEN = 0;  // Primary screen width
        private const int SM_CYSCREEN = 1;  // Primary screen height

        public ScreenshotService(IProcessManager processManager, ILogger<ScreenshotService> logger, IElementSearchService elementSearchService)
            : base(processManager, logger)
        {
            _elementSearchService = elementSearchService;
        }

        protected override string GetOperationType() => "screenshot";

        public async Task<ServerEnhancedResponse<ScreenshotResult>> TakeScreenshotAsync(
            string? windowTitle = null, 
            string? outputPath = null, 
            int maxTokens = 0, 
            int? processId = null, 
            int timeoutSeconds = 60, 
            CancellationToken cancellationToken = default)
        {
            var request = new TakeScreenshotRequest
            {
                WindowTitle = windowTitle,
                OutputPath = outputPath,
                MaxTokens = maxTokens,
                ProcessId = processId
            };

            // Use reflection to call the internal execution method
            try
            {
                var screenshotResult = await ExecuteScreenshotOperation(request, cancellationToken);
                var operationId = Guid.NewGuid().ToString("N")[..8];

                var context = new ServiceContext(nameof(TakeScreenshotAsync), timeoutSeconds);

                var metadata = CreateSuccessMetadata(screenshotResult, context);
                metadata.TargetWindowTitle = windowTitle;
                metadata.TargetProcessId = processId;
                metadata.MaxTokensRequested = maxTokens > 0 ? maxTokens : null;

                return new ServerEnhancedResponse<ScreenshotResult>
                {
                    Success = screenshotResult.Success,
                    Data = screenshotResult,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = "00:00:00.000",
                        OperationId = operationId,
                        ServerLogs = new List<string>()
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = nameof(TakeScreenshotAsync),
                        TimeoutSeconds = timeoutSeconds
                    }
                };
            }
            catch (Exception ex)
            {
                return new ServerEnhancedResponse<ScreenshotResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = "00:00:00.000",
                        OperationId = Guid.NewGuid().ToString("N")[..8],
                        ServerLogs = new List<string>()
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = nameof(TakeScreenshotAsync),
                        TimeoutSeconds = timeoutSeconds
                    }
                };
            }
        }

        private static ValidationResult ValidateTakeScreenshotRequest(TakeScreenshotRequest request)
        {
            var errors = new List<string>();

            if (request.MaxTokens < 0)
            {
                errors.Add("MaxTokens must be non-negative");
            }

            if (!string.IsNullOrEmpty(request.OutputPath))
            {
                try
                {
                    var directory = Path.GetDirectoryName(request.OutputPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        var parentDirectory = Path.GetDirectoryName(directory);
                        if (!string.IsNullOrEmpty(parentDirectory) && !Directory.Exists(parentDirectory))
                        {
                            errors.Add("Invalid output path - parent directory does not exist");
                        }
                    }
                }
                catch (Exception)
                {
                    errors.Add("Invalid output path format");
                }
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        private async Task<ScreenshotResult> ExecuteScreenshotOperation(TakeScreenshotRequest request, CancellationToken cancellationToken)
        {
            return await Task.Run(async () =>
            {
                _logger.LogInformation("Taking screenshot of window: {WindowTitle}, maxTokens: {MaxTokens}", request.WindowTitle, request.MaxTokens);

                System.Drawing.Rectangle captureArea;

                // Determine capture area
                if (!string.IsNullOrEmpty(request.WindowTitle) || request.ProcessId.HasValue)
                {
                    _logger.LogInformation("Attempting to get window info for title: {WindowTitle}, processId: {ProcessId}", request.WindowTitle, request.ProcessId);
                    
                    // Use Worker to get window information via UIAutomation
                    var windowInfo = await GetWindowInfoFromWorker(request.WindowTitle, request.ProcessId);
                    if (windowInfo == null)
                    {
                        _logger.LogWarning("Window not found: {WindowTitle}, ProcessId: {ProcessId}", request.WindowTitle, request.ProcessId);
                        return new ScreenshotResult { Success = false, Error = "Window not found" };
                    }

                    _logger.LogInformation("Window info retrieved successfully: {Keys}", string.Join(", ", windowInfo.Keys));

                    // Extract bounding rectangle from window info
                    if (windowInfo.TryGetValue("BoundingRectangle", out var boundingRectObj))
                    {
                        _logger.LogInformation("BoundingRectangle found, type: {Type}", 
                            boundingRectObj?.GetType().Name ?? "null");
                        
                        if (boundingRectObj is Dictionary<string, object> boundingRect)
                        {
                            var x = Convert.ToInt32(boundingRect["X"]);
                            var y = Convert.ToInt32(boundingRect["Y"]);
                            var width = Convert.ToInt32(boundingRect["Width"]);
                            var height = Convert.ToInt32(boundingRect["Height"]);
                            
                            captureArea = new System.Drawing.Rectangle(x, y, width, height);
                            _logger.LogInformation("Capture area set to: {X}, {Y}, {Width}, {Height}", x, y, width, height);
                        }
                        else if (boundingRectObj is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                        {
                            // Handle JsonElement case
                            var x = jsonElement.GetProperty("X").GetInt32();
                            var y = jsonElement.GetProperty("Y").GetInt32();
                            var width = jsonElement.GetProperty("Width").GetInt32();
                            var height = jsonElement.GetProperty("Height").GetInt32();
                            
                            captureArea = new System.Drawing.Rectangle(x, y, width, height);
                            _logger.LogInformation("Capture area set from JsonElement: {X}, {Y}, {Width}, {Height}", x, y, width, height);
                        }
                        else
                        {
                            _logger.LogError("BoundingRectangle is not in expected format. Type: {Type}", boundingRectObj?.GetType().Name ?? "null");
                            return new ScreenshotResult { Success = false, Error = "Invalid BoundingRectangle format" };
                        }
                    }
                    else
                    {
                        _logger.LogError("Failed to get window rectangle from UIAutomation. Available keys: {Keys}", string.Join(", ", windowInfo.Keys));
                        return new ScreenshotResult { Success = false, Error = "Failed to get window rectangle" };
                    }

                    // Try to activate window using Worker's WindowAction for better reliability
                    await ActivateWindowViaWorker(request.WindowTitle, request.ProcessId);
                    
                    // Wait a bit for window to come to foreground
                    Thread.Sleep(500);
                }
                else
                {
                    // Capture entire screen using Win32 API
                    int screenWidth = GetSystemMetrics(SM_CXSCREEN);
                    int screenHeight = GetSystemMetrics(SM_CYSCREEN);
                    
                    if (screenWidth <= 0 || screenHeight <= 0)
                    {
                        _logger.LogError("Failed to get screen dimensions");
                        return new ScreenshotResult { Success = false, Error = "Failed to get screen dimensions" };
                    }
                    
                    captureArea = new System.Drawing.Rectangle(0, 0, screenWidth, screenHeight);
                    _logger.LogInformation("Using primary screen dimensions: {Width}x{Height}", screenWidth, screenHeight);
                }

                // Validate capture area
                if (captureArea.Width <= 0 || captureArea.Height <= 0)
                {
                    _logger.LogError("Invalid capture area dimensions: {Width}x{Height}", captureArea.Width, captureArea.Height);
                    return new ScreenshotResult { Success = false, Error = "Invalid capture area dimensions" };
                }

                // Capture screenshot
                using var bitmap = new Bitmap(captureArea.Width, captureArea.Height);
                using var graphics = Graphics.FromImage(bitmap);
                graphics.CopyFromScreen(captureArea.Location, System.Drawing.Point.Empty, captureArea.Size);

                // Generate output path if not provided
                string outputPath = request.OutputPath;
                if (string.IsNullOrEmpty(outputPath))
                {
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var fileName = $"screenshot_{timestamp}.png";
                    outputPath = Path.Combine(Path.GetTempPath(), fileName);
                }

                // Ensure output directory exists
                var outputDir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                // Save screenshot
                bitmap.Save(outputPath, ImageFormat.Png);

                // Prepare base64 data if needed
                string? base64Image = null;
                if (request.MaxTokens > 0)
                {
                    base64Image = await OptimizeImageForTokens(outputPath, request.MaxTokens);
                }

                var fileInfo = new FileInfo(outputPath);
                var result = new ScreenshotResult
                {
                    Success = true,
                    OutputPath = outputPath,
                    Base64Image = base64Image ?? string.Empty,
                    Width = captureArea.Width,
                    Height = captureArea.Height,
                    FileSize = fileInfo.Length,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                _logger.LogInformation("Screenshot taken successfully: {OutputPath}, Size: {Width}x{Height}", outputPath, captureArea.Width, captureArea.Height);
                return result;
            }, cancellationToken);
        }

        private async Task<string> OptimizeImageForTokens(string imagePath, int maxTokens)
        {
            // Estimate that each base64 character is roughly 1 token
            // Base64 encoding increases size by ~33%, so we need to target image size accordingly
            var targetImageSize = (int)(maxTokens * 0.75); // Conservative estimate
            
            var originalBytes = File.ReadAllBytes(imagePath);
            var originalBase64 = Convert.ToBase64String(originalBytes);
            
            // If original image is already within token limit, return as-is
            if (originalBase64.Length <= maxTokens)
            {
                return originalBase64;
            }
            
            // Try different compression qualities until we fit within token limit
            var qualities = new[] { 75, 50, 25, 10 };
            
            foreach (var quality in qualities)
            {
                var optimizedBytes = await CompressImageWithQuality(imagePath, quality);
                var optimizedBase64 = Convert.ToBase64String(optimizedBytes);
                
                if (optimizedBase64.Length <= maxTokens)
                {
                    _logger.LogInformation("Optimized image from {OriginalSize} to {OptimizedSize} tokens using quality {Quality}%", 
                        originalBase64.Length, optimizedBase64.Length, quality);
                    return optimizedBase64;
                }
            }
            
            // If still too large, try reducing dimensions
            var scaleFactor = Math.Sqrt((double)maxTokens / originalBase64.Length);
            var optimizedBytesWithResize = await ResizeAndCompressImage(imagePath, scaleFactor, 25);
            var finalBase64 = Convert.ToBase64String(optimizedBytesWithResize);
            
            _logger.LogInformation("Heavily optimized image from {OriginalSize} to {OptimizedSize} tokens using resize factor {ScaleFactor:F2}", 
                originalBase64.Length, finalBase64.Length, scaleFactor);
            
            return finalBase64;
        }

        private async Task<byte[]> CompressImageWithQuality(string imagePath, int quality)
        {
            return await Task.Run(() =>
            {
                using var originalImage = Image.FromFile(imagePath);
                using var memoryStream = new MemoryStream();
                
                // Create encoder parameters for JPEG compression
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                
                // Get JPEG encoder
                var jpegEncoder = ImageCodecInfo.GetImageEncoders().First(x => x.FormatID == ImageFormat.Jpeg.Guid);
                
                originalImage.Save(memoryStream, jpegEncoder, encoderParameters);
                return memoryStream.ToArray();
            });
        }

        private async Task<byte[]> ResizeAndCompressImage(string imagePath, double scaleFactor, int quality)
        {
            return await Task.Run(() =>
            {
                using var originalImage = Image.FromFile(imagePath);
                var newWidth = (int)(originalImage.Width * scaleFactor);
                var newHeight = (int)(originalImage.Height * scaleFactor);
                
                using var resizedImage = new Bitmap(newWidth, newHeight);
                using var graphics = Graphics.FromImage(resizedImage);
                
                // Use high quality scaling
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                
                graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
                
                using var memoryStream = new MemoryStream();
                
                // Create encoder parameters for JPEG compression
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                
                // Get JPEG encoder
                var jpegEncoder = ImageCodecInfo.GetImageEncoders().First(x => x.FormatID == ImageFormat.Jpeg.Guid);
                
                resizedImage.Save(memoryStream, jpegEncoder, encoderParameters);
                return memoryStream.ToArray();
            });
        }

        private async Task<Dictionary<string, object>?> GetWindowInfoFromWorker(string? windowTitle, int? processId)
        {
            try
            {
                _logger.LogInformation("Searching for window using SearchElements: WindowTitle={WindowTitle}, ProcessId={ProcessId}", windowTitle, processId);
                
                var searchRequest = new SearchElementsRequest
                {
                    ControlType = "Window",
                    Scope = "children",
                    WindowTitle = windowTitle,
                    ProcessId = processId,
                    MaxResults = 1,
                    VisibleOnly = true
                };

                var searchResult = await _elementSearchService.SearchElementsAsync(searchRequest, 10);
                
                if (searchResult.Success && searchResult.Data?.Elements != null && searchResult.Data.Elements.Length > 0)
                {
                    var windowElement = searchResult.Data.Elements[0];
                    _logger.LogInformation("Found window element: Name={Name}, AutomationId={AutomationId}", 
                        windowElement.Name, windowElement.AutomationId);
                    
                    return new Dictionary<string, object>
                    {
                        ["BoundingRectangle"] = new Dictionary<string, object>
                        {
                            ["X"] = windowElement.BoundingRectangle?.X ?? 0,
                            ["Y"] = windowElement.BoundingRectangle?.Y ?? 0,
                            ["Width"] = windowElement.BoundingRectangle?.Width ?? 0,
                            ["Height"] = windowElement.BoundingRectangle?.Height ?? 0
                        }
                    };
                }
                
                _logger.LogWarning("No window found matching the criteria");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search for window using SearchElements");
                return null;
            }
        }

        private async Task ActivateWindowViaWorker(string? windowTitle, int? processId)
        {
            try
            {
                _logger.LogInformation("Attempting to activate window via Worker: {WindowTitle}, ProcessId: {ProcessId}", windowTitle, processId);

                // Use Worker's WindowAction operation to activate window
                var request = new WindowActionRequest
                {
                    Action = "normal",
                    WindowTitle = windowTitle ?? "",
                    ProcessId = processId ?? 0
                };

                var result = await _processManager.ExecuteWorkerOperationAsync<WindowActionRequest, WindowActionResult>("WindowAction", request, 5);
                if (result != null && result.Success)
                {
                    _logger.LogInformation("Window activated successfully via Worker: {WindowTitle}", windowTitle);
                }
                else
                {
                    _logger.LogWarning("Failed to activate window via Worker. Result: {Result}", 
                        result != null ? JsonSerializationHelper.Serialize(result) : "null");
                }

                // Wait for window state changes
                Thread.Sleep(300);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to activate window via Worker: {WindowTitle}", windowTitle);
            }
        }

        protected override ScreenshotServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);

            if (data is ScreenshotResult screenshotResult)
            {
                metadata.OperationSuccessful = screenshotResult.Success;
                metadata.OutputPath = screenshotResult.OutputPath;
                metadata.ScreenshotWidth = screenshotResult.Width;
                metadata.ScreenshotHeight = screenshotResult.Height;
                metadata.FileSize = screenshotResult.FileSize;
                metadata.HasBase64Data = !string.IsNullOrEmpty(screenshotResult.Base64Image);
                metadata.ScreenshotTimestamp = screenshotResult.Timestamp;
            }

            return metadata;
        }

    }

    // Helper class for screenshot requests
    public class TakeScreenshotRequest
    {
        public string? WindowTitle { get; set; }
        public string? OutputPath { get; set; }
        public int MaxTokens { get; set; }
        public int? ProcessId { get; set; }
    }
}