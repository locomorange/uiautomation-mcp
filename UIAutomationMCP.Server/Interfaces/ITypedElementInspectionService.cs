using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Interfaces
{
    /// <summary>
    /// Strongly typed element inspection service interface
    /// </summary>
    public interface ITypedElementInspectionService
    {
        Task<ElementInspectionResult> GetElementPropertiesAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ElementInspectionResult> GetElementSupportedPatternsAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ElementInspectionResult> GetElementBoundingRectangleAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ElementInspectionResult> GetElementAutomationIdAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);
    }
}