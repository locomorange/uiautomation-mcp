using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Interfaces
{
    /// <summary>
    /// Strongly typed tree navigation service interface
    /// </summary>
    public interface ITypedTreeNavigationService
    {
        Task<TreeNavigationResult> GetChildrenAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<TreeNavigationResult> GetParentAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<TreeNavigationResult> GetFirstChildAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<TreeNavigationResult> GetLastChildAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<TreeNavigationResult> GetNextSiblingAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<TreeNavigationResult> GetPreviousSiblingAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);
    }
}