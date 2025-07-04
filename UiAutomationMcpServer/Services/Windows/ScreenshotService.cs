using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using UiAutomationMcpServer.Models;

namespace UiAutomationMcpServer.Services.Windows
{
    public interface IScreenshotService
    {
        Task<ScreenshotResult> TakeScreenshotAsync(string? windowTitle = null, string? outputPath = null, int maxTokens = 0, int? processId = null);
    }

    public class ScreenshotService : IScreenshotService
    {
        private readonly ILogger<ScreenshotService> _logger;
        private readonly IWindowService _windowService;

        public ScreenshotService(ILogger<ScreenshotService> logger, IWindowService windowService)
        {
            _logger = logger;
            _windowService = windowService;
        }

        public Task<ScreenshotResult> TakeScreenshotAsync(string? windowTitle = null, string? outputPath = null, int maxTokens = 0, int? processId = null)
        {
            try
            {
                _logger.LogInformation("Taking screenshot of window: {WindowTitle}, maxTokens: {MaxTokens}", 
                    windowTitle, maxTokens);

                Rectangle bounds;
                if (!string.IsNullOrEmpty(windowTitle))
                {
                    var window = _windowService.FindWindowByTitle(windowTitle, processId);
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

                using var originalBitmap = new Bitmap(bounds.Width, bounds.Height);
                using (var graphics = Graphics.FromImage(originalBitmap))
                {
                    graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);
                }

                if (maxTokens <= 0)
                {
                    outputPath ??= Path.Combine(Path.GetTempPath(), $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                    originalBitmap.Save(outputPath, ImageFormat.Png);
                    
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

                return Task.FromResult(OptimizeScreenshotForTokenLimit(originalBitmap, outputPath, maxTokens, bounds.Width, bounds.Height));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error taking screenshot");
                return Task.FromResult(new ScreenshotResult { Success = false, Error = ex.Message });
            }
        }

        private ScreenshotResult OptimizeScreenshotForTokenLimit(
            Bitmap originalBitmap, string? outputPath, int maxTokens, int originalWidth, int originalHeight)
        {
            var maxFileSize = maxTokens / 2;

            outputPath ??= Path.Combine(Path.GetTempPath(), $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");

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
                        if (File.Exists(outputPath))
                            File.Delete(outputPath);
                        File.Move(tempPath, outputPath);

                        var base64Image = Convert.ToBase64String(File.ReadAllBytes(outputPath));
                        var actualTokens = base64Image.Length * 4 / 3;

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
    }
}