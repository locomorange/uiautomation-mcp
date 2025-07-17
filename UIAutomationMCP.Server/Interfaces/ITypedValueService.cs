using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Interfaces
{
    /// <summary>
    /// Strongly typed value service interface
    /// </summary>
    public interface ITypedValueService
    {
        Task<ElementValueResult> GetValueAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ActionResult> SetValueAsync(
            string elementId,
            string value,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);
    }
}