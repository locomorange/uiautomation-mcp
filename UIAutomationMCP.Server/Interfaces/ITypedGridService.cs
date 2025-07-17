using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Interfaces
{
    /// <summary>
    /// Strongly typed grid service interface
    /// </summary>
    public interface ITypedGridService
    {
        Task<GridInfoResult> GetGridInfoAsync(
            string gridElementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ElementSearchResult> GetGridItemAsync(
            string gridElementId,
            int row,
            int column,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ElementSearchResult> GetGridItemsByRowAsync(
            string gridElementId,
            int row,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ElementSearchResult> GetGridItemsByColumnAsync(
            string gridElementId,
            int column,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);
    }
}