using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Interfaces
{
    /// <summary>
    /// Strongly typed element search service interface
    /// </summary>
    public interface ITypedElementSearchService
    {
        Task<ElementSearchResult> FindElementAsync(
            string? windowTitle = null,
            int? processId = null,
            string? name = null,
            string? automationId = null,
            string? className = null,
            string? controlType = null,
            int timeoutSeconds = 30);

        Task<ElementSearchResult> FindElementsAsync(
            string? windowTitle = null,
            int? processId = null,
            string? name = null,
            string? automationId = null,
            string? className = null,
            string? controlType = null,
            int timeoutSeconds = 30);

        Task<ElementSearchResult> FindElementsByControlTypeAsync(
            string controlType,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ElementSearchResult> FindElementsByNameAsync(
            string name,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ElementSearchResult> FindElementsByAutomationIdAsync(
            string automationId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ElementSearchResult> FindElementsByClassNameAsync(
            string className,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);
    }
}