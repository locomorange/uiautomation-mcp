using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Interfaces
{
    /// <summary>
    /// Strongly typed accessibility service interface
    /// </summary>
    public interface ITypedAccessibilityService
    {
        Task<AccessibilityInfoResult> GetAccessibilityInfoAsync(
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<AccessibilityInfoResult> GetElementAccessibilityInfoAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<AccessibilityInfoResult> GetWindowAccessibilityInfoAsync(
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);
    }
}