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

        // [McpServerTool, Description("Click on a UI element by automation ID or name")]
        // public async Task<object> ClickElement(
        //     [Description("Automation ID or name of the element to click")] string elementId,
        //     [Description("Title of the window containing the element (optional)")] string? windowTitle = null)
        // {
        //     return await _uiAutomationService.ClickElementAsync(elementId, windowTitle);
        // }

        [McpServerTool, Description("Execute any UI Automation pattern on an element")]
        public async Task<object> ExecuteElementPattern(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Pattern name: invoke, value, toggle, selectionitem, expandcollapse, scroll, rangevalue, text, window, grid, griditem, table, tableitem, selection, transform, dock")] string patternName,
            [Description("Pattern parameters as JSON object (optional). Examples: {\"value\":\"text\"}, {\"expand\":true}, {\"direction\":\"up\"}, {\"action\":\"close\"}")] Dictionary<string, object>? parameters = null,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null)
        {
            return await _uiAutomationService.ExecuteElementPatternAsync(elementId, patternName, parameters, windowTitle);
        }

        [McpServerTool, Description("Send text input to a UI element")]
        public async Task<object> SendKeys(
            [Description("Text to send")] string text,
            [Description("Automation ID or name of the element to send text to (optional)")] string? elementId = null,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null)
        {
            return await _uiAutomationService.SendKeysAsync(text, elementId, windowTitle);
        }


        [McpServerTool, Description("Take a screenshot of the desktop or specific window")]
        public async Task<object> TakeScreenshot(
            [Description("Title of the window to screenshot (optional, defaults to full screen)")] string? windowTitle = null,
            [Description("Path to save the screenshot (optional)")] string? outputPath = null,
            [Description("Maximum tokens for Base64 response (0 = no limit, auto-optimizes resolution and compression)")] int maxTokens = 0)
        {
            return await _uiAutomationService.TakeScreenshotAsync(windowTitle, outputPath, maxTokens);
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