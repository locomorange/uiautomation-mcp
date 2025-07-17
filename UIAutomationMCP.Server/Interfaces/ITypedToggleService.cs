using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Interfaces
{
    /// <summary>
    /// Strongly typed toggle service interface
    /// </summary>
    public interface ITypedToggleService
    {
        Task<ActionResult> ToggleAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ElementValueResult> GetToggleStateAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ActionResult> SetToggleStateAsync(
            string elementId,
            string state,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);
    }
}