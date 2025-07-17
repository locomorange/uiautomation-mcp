using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Interfaces
{
    /// <summary>
    /// Strongly typed custom property service interface
    /// </summary>
    public interface ITypedCustomPropertyService
    {
        Task<PropertyResult> GetCustomPropertiesAsync(
            string elementId,
            string[] propertyIds,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ActionResult> SetCustomPropertyAsync(
            string elementId,
            string propertyId,
            object value,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<PropertyResult> GetCustomPropertyAsync(
            string elementId,
            string propertyId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);
    }
}