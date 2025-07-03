using UiAutomationMcpServer.Models;

namespace UiAutomationMcpServer.Services
{
    public interface IUIAutomationService
    {
        Task<OperationResult> GetWindowInfoAsync();
        Task<OperationResult> GetElementInfoAsync(string? windowTitle = null, string? controlType = null, int? windowIndex = null);
        Task<OperationResult> ExecuteElementPatternAsync(string elementId, string patternName, Dictionary<string, object>? parameters = null, string? windowTitle = null, int? windowIndex = null);
        Task<ScreenshotResult> TakeScreenshotAsync(string? windowTitle = null, string? outputPath = null, int maxTokens = 0, int? windowIndex = null);
        Task<ProcessResult> LaunchApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null);
        Task<OperationResult> FindElementsAsync(string? searchText = null, string? controlType = null, string? windowTitle = null, int? windowIndex = null);
    }
}