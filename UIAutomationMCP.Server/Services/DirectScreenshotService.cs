using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services
{
    public class DirectScreenshotService : IScreenshotService
    {
        private readonly ILogger<DirectScreenshotService> _logger;
        private readonly SubprocessExecutor _executor;

        public DirectScreenshotService(ILogger<DirectScreenshotService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<ScreenshotResult> TakeScreenshotAsync(string? windowTitle = null, string? outputPath = null, int maxTokens = 0, int? processId = null, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
        {
            try
            {
                return await Task.Run(async () =>
                {
                    _logger.LogInformation("Taking screenshot of window: {WindowTitle}, maxTokens: {MaxTokens}", windowTitle, maxTokens);

                    Rectangle captureArea;
                    IntPtr hwnd = IntPtr.Zero;

                    // Determine capture area
                    if (!string.IsNullOrEmpty(windowTitle) || processId.HasValue)
                    {
                        // Use Worker to get window information via UIAutomation
                        var windowInfo = await GetWindowInfoFromWorker(windowTitle, processId);
                        if (windowInfo == null)
                        {
                            _logger.LogWarning("Window not found: {WindowTitle}, ProcessId: {ProcessId}", windowTitle, processId);
                            return new ScreenshotResult { Success = false, Error = "Window not found" };
                        }

                        // Extract bounding rectangle from window info
                        if (windowInfo.TryGetValue("BoundingRectangle", out var boundingRectObj) && 
                            boundingRectObj is Dictionary<string, object> boundingRect)
                        {
                            var x = Convert.ToInt32(boundingRect["X"]);
                            var y = Convert.ToInt32(boundingRect["Y"]);
                            var width = Convert.ToInt32(boundingRect["Width"]);
                            var height = Convert.ToInt32(boundingRect["Height"]);
                            
                            captureArea = new Rectangle(x, y, width, height);
                        }
                        else
                        {
                            _logger.LogError("Failed to get window rectangle from UIAutomation");
                            return new ScreenshotResult { Success = false, Error = "Failed to get window rectangle" };
                        }

                        // Try to activate window using Windows API
                        if (windowInfo.TryGetValue("ProcessId", out var processIdObj))
                        {
                            var pid = Convert.ToInt32(processIdObj);
                            hwnd = FindWindowByProcessId(pid);
                            if (hwnd != IntPtr.Zero)
                            {
                                if (!ActivateWindow(hwnd))
                                {
                                    _logger.LogWarning("Failed to activate window: {WindowTitle}", windowTitle);
                                }
                                // Wait a bit for window to come to foreground
                                Thread.Sleep(500);
                            }
                        }
                    }
                    else
                    {
                        // Capture entire screen
                        var primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
                        if (primaryScreen == null)
                        {
                            _logger.LogError("Primary screen not found");
                            return new ScreenshotResult { Success = false, Error = "Primary screen not found" };
                        }
                        captureArea = primaryScreen.Bounds;
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
                    graphics.CopyFromScreen(captureArea.Location, Point.Empty, captureArea.Size);

                    // Generate output path if not provided
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
                    if (maxTokens > 0)
                    {
                        base64Image = await OptimizeImageForTokens(outputPath, maxTokens);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to take screenshot for window: {WindowTitle}", windowTitle);
                return new ScreenshotResult { Success = false, Error = ex.Message };
            }
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
                var parameters = new Dictionary<string, object>
                {
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<Dictionary<string, object>>("GetWindowInfo", parameters, 10);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get window info from worker");
                return null;
            }
        }

        private IntPtr FindWindowByProcessId(int processId)
        {
            try
            {
                var process = System.Diagnostics.Process.GetProcessById(processId);
                return process.MainWindowHandle;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        private bool ActivateWindow(IntPtr hwnd)
        {
            try
            {
                // Try to restore window if minimized
                if (IsIconic(hwnd))
                {
                    ShowWindow(hwnd, SW_RESTORE);
                    Thread.Sleep(100);
                }

                // Bring window to foreground
                SetForegroundWindow(hwnd);
                Thread.Sleep(100);

                // Alternative method if SetForegroundWindow fails
                if (GetForegroundWindow() != hwnd)
                {
                    var currentThread = GetCurrentThreadId();
                    var targetThread = GetWindowThreadProcessId(hwnd, out _);
                    
                    if (targetThread != currentThread)
                    {
                        AttachThreadInput(currentThread, targetThread, true);
                        SetForegroundWindow(hwnd);
                        AttachThreadInput(currentThread, targetThread, false);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to activate window");
                return false;
            }
        }

        // Windows API declarations
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        private const int SW_RESTORE = 9;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}