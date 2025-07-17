using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Interfaces
{
    /// <summary>
    /// Strongly typed selection service interface
    /// </summary>
    public interface ITypedSelectionService
    {
        Task<SelectionInfoResult> GetSelectionAsync(
            string containerElementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ActionResult> SelectAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ActionResult> AddToSelectionAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ActionResult> RemoveFromSelectionAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<BooleanResult> IsSelectedAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<BooleanResult> CanSelectMultipleAsync(
            string containerElementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);
    }
}