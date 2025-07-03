using UiAutomationMcpServer.Models;

namespace UiAutomationMcpServer.Services
{
    public interface IUIAutomationService
    {
        Task<OperationResult> GetWindowInfoAsync();
        Task<OperationResult> GetElementInfoAsync(string? windowTitle = null, string? controlType = null);
        Task<OperationResult> ClickElementAsync(string elementId, string? windowTitle = null);
        Task<OperationResult> ExecuteElementPatternAsync(string elementId, string patternName, Dictionary<string, object>? parameters = null, string? windowTitle = null);
        Task<OperationResult> SendKeysAsync(string text, string? elementId = null, string? windowTitle = null);
        Task<ScreenshotResult> TakeScreenshotAsync(string? windowTitle = null, string? outputPath = null, int maxTokens = 0);
        Task<ProcessResult> LaunchApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null);
        Task<OperationResult> FindElementsAsync(string? searchText = null, string? controlType = null, string? windowTitle = null);
    }
}