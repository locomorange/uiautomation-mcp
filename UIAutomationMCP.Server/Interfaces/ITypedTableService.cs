using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Interfaces
{
    /// <summary>
    /// Strongly typed table service interface
    /// </summary>
    public interface ITypedTableService
    {
        Task<TableInfoResult> GetTableInfoAsync(
            string tableElementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ElementSearchResult> GetTableItemAsync(
            string tableElementId,
            int row,
            int column,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ElementSearchResult> GetRowHeadersAsync(
            string tableElementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ElementSearchResult> GetColumnHeadersAsync(
            string tableElementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ElementSearchResult> GetTableRowAsync(
            string tableElementId,
            int row,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ElementSearchResult> GetTableColumnAsync(
            string tableElementId,
            int column,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);
    }
}