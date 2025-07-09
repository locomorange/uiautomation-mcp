using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Automation;
using System.Windows.Forms;
using UIAutomationMCP.Shared;

namespace UIAutomationMCP.Worker.Operations
{
    public class ScreenshotOperations
    {
        public OperationResult TakeScreenshot(string? windowTitle = null, string? outputPath = null, int maxTokens = 0, int processId = 0)
        {
            // Let exceptions flow naturally - no try-catch
            Bitmap screenshot;
            
            if (!string.IsNullOrEmpty(windowTitle) || processId > 0)
            {
                // Take screenshot of specific window
                var window = FindWindow(windowTitle, processId);
                if (window == null)
                    return new OperationResult { Success = false, Error = "Window not found" };

                var rect = window.Current.BoundingRectangle;
                if (rect.Width <= 0 || rect.Height <= 0)
                    return new OperationResult { Success = false, Error = "Window has invalid dimensions" };

                screenshot = new Bitmap((int)rect.Width, (int)rect.Height, PixelFormat.Format32bppArgb);
                using (var graphics = Graphics.FromImage(screenshot))
                {
                    graphics.CopyFromScreen((int)rect.X, (int)rect.Y, 0, 0, new Size((int)rect.Width, (int)rect.Height), CopyPixelOperation.SourceCopy);
                }
            }
            else
            {
                // Take full screen screenshot
                var primaryScreen = Screen.PrimaryScreen;
                if (primaryScreen == null)
                    return new OperationResult { Success = false, Error = "Primary screen not available" };
                    
                var bounds = primaryScreen.Bounds;
                screenshot = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
                using (var graphics = Graphics.FromImage(screenshot))
                {
                    graphics.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
                }
            }

            using (screenshot)
            {
                // Optimize image based on maxTokens
                Bitmap optimizedImage = screenshot;
                if (maxTokens > 0)
                {
                    optimizedImage = OptimizeImageForTokens(screenshot, maxTokens);
                }

                using (optimizedImage)
                {
                    // Save to file if path specified
                    if (!string.IsNullOrEmpty(outputPath))
                    {
                        var directory = Path.GetDirectoryName(outputPath);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }
                        optimizedImage.Save(outputPath, ImageFormat.Png);
                    }

                    // Convert to base64
                    using (var memoryStream = new MemoryStream())
                    {
                        optimizedImage.Save(memoryStream, ImageFormat.Png);
                        var base64 = Convert.ToBase64String(memoryStream.ToArray());

                        return new OperationResult
                        {
                            Success = true,
                            Data = new
                            {
                                Base64Image = base64,
                                Width = optimizedImage.Width,
                                Height = optimizedImage.Height,
                                Format = "PNG",
                                FilePath = outputPath,
                                SizeBytes = memoryStream.Length,
                                ApproximateTokens = EstimateTokensFromImageSize(memoryStream.Length)
                            }
                        };
                    }
                }
            }
        }

        private Bitmap OptimizeImageForTokens(Bitmap originalImage, int maxTokens)
        {
            // Rough estimation: 1 token ≈ 170 bytes for base64 encoded images
            var targetBytes = maxTokens * 170;
            
            // Start with quality reduction if image is too large
            using (var memoryStream = new MemoryStream())
            {
                originalImage.Save(memoryStream, ImageFormat.Png);
                if (memoryStream.Length <= targetBytes)
                {
                    return new Bitmap(originalImage);
                }
            }

            // Reduce dimensions to fit token limit
            var scaleFactor = Math.Sqrt((double)targetBytes / EstimateImageBytes(originalImage));
            scaleFactor = Math.Min(scaleFactor, 1.0); // Don't upscale

            var newWidth = (int)(originalImage.Width * scaleFactor);
            var newHeight = (int)(originalImage.Height * scaleFactor);

            // Ensure minimum size
            newWidth = Math.Max(newWidth, 100);
            newHeight = Math.Max(newHeight, 100);

            var resizedImage = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(resizedImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
            }

            return resizedImage;
        }

        private long EstimateImageBytes(Bitmap image)
        {
            // Rough estimation for PNG compressed size
            return image.Width * image.Height * 3; // Assume 3 bytes per pixel on average after PNG compression
        }

        private int EstimateTokensFromImageSize(long sizeInBytes)
        {
            // Rough estimation: 1 token ≈ 170 bytes for base64 encoded images
            return (int)(sizeInBytes / 170);
        }

        private AutomationElement? FindWindow(string? windowTitle, int processId)
        {
            if (processId > 0)
            {
                var condition = new PropertyCondition(AutomationElement.ProcessIdProperty, processId);
                return AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
            }
            else if (!string.IsNullOrEmpty(windowTitle))
            {
                var condition = new PropertyCondition(AutomationElement.NameProperty, windowTitle);
                return AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
            }
            return null;
        }
    }
}
