using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using UiAutomationMcpServer.Services;

namespace UiAutomationMcpServer.Tools
{
    [McpServerToolType]
    public class UIAutomationTools
    {
        private readonly IUIAutomationService _uiAutomationService;
        private readonly ILogger<UIAutomationTools> _logger;

        public UIAutomationTools(IUIAutomationService uiAutomationService, ILogger<UIAutomationTools> logger)
        {
            _uiAutomationService = uiAutomationService;
            _logger = logger;
        }

        [McpServerTool, Description("Get information about all open windows")]
        public async Task<object> GetWindowInfo()
        {
            return await _uiAutomationService.GetWindowInfoAsync();
        }

        [McpServerTool, Description("Get information about UI elements in a specific window")]
        public async Task<object> GetElementInfo(
            [Description("Title of the window to search in (optional)")] string? windowTitle = null,
            [Description("Type of control to filter by (optional)")] string? controlType = null)
        {
            return await _uiAutomationService.GetElementInfoAsync(windowTitle, controlType);
        }

        [McpServerTool, Description("Click on a UI element by automation ID or name")]
        public async Task<object> ClickElement(
            [Description("Automation ID or name of the element to click")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null)
        {
            return await _uiAutomationService.ClickElementAsync(elementId, windowTitle);
        }

        [McpServerTool, Description("Send text input to a UI element")]
        public async Task<object> SendKeys(
            [Description("Text to send")] string text,
            [Description("Automation ID or name of the element to send text to (optional)")] string? elementId = null,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null)
        {
            return await _uiAutomationService.SendKeysAsync(text, elementId, windowTitle);
        }

        [McpServerTool, Description("Perform mouse click at specific coordinates")]
        public async Task<object> MouseClick(
            [Description("X coordinate")] int x,
            [Description("Y coordinate")] int y,
            [Description("Mouse button (left, right, middle)")] string button = "left")
        {
            return await _uiAutomationService.MouseClickAsync(x, y, button);
        }

        [McpServerTool, Description("Take a screenshot of the desktop or specific window")]
        public async Task<object> TakeScreenshot(
            [Description("Title of the window to screenshot (optional, defaults to full screen)")] string? windowTitle = null,
            [Description("Path to save the screenshot (optional)")] string? outputPath = null,
            [Description("Enable JPEG compression to reduce response size (default: false)")] bool enableCompression = false,
            [Description("JPEG compression quality 1-100 (default: 75, only used when enableCompression is true)")] int compressionQuality = 75)
        {
            return await _uiAutomationService.TakeScreenshotAsync(windowTitle, outputPath, enableCompression, compressionQuality);
        }

        [McpServerTool, Description("Launch an application by executable path or name")]
        public async Task<object> LaunchApplication(
            [Description("Path to the executable to launch")] string applicationPath,
            [Description("Command line arguments (optional)")] string? arguments = null,
            [Description("Working directory (optional)")] string? workingDirectory = null)
        {
            return await _uiAutomationService.LaunchApplicationAsync(applicationPath, arguments, workingDirectory);
        }

        [McpServerTool, Description("Find UI elements by various criteria")]
        public async Task<object> FindElements(
            [Description("Text to search for in element names or automation IDs (optional)")] string? searchText = null,
            [Description("Type of control to filter by (optional)")] string? controlType = null,
            [Description("Title of the window to search in (optional)")] string? windowTitle = null)
        {
            return await _uiAutomationService.FindElementsAsync(searchText, controlType, windowTitle);
        }
    }
}