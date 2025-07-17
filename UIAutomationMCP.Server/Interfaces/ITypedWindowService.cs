using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Interfaces
{
    /// <summary>
    /// Strongly typed window service interface
    /// </summary>
    public interface ITypedWindowService
    {
        Task<ActionResult> WindowActionAsync(
            string action,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<WindowInfoResult> GetWindowInfoAsync(
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<WindowInteractionStateResult> GetWindowInteractionStateAsync(
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<WindowCapabilitiesResult> GetWindowCapabilitiesAsync(
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ActionResult> CloseWindowAsync(
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ActionResult> MinimizeWindowAsync(
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ActionResult> MaximizeWindowAsync(
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ActionResult> RestoreWindowAsync(
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);
    }
}