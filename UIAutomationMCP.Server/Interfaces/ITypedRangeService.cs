using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Interfaces
{
    /// <summary>
    /// Strongly typed range service interface
    /// </summary>
    public interface ITypedRangeService
    {
        Task<ElementValueResult> GetRangeValueAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ActionResult> SetRangeValueAsync(
            string elementId,
            double value,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ElementValueResult> GetRangeInfoAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);
    }
}