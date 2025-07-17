using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Interfaces
{
    /// <summary>
    /// Strongly typed text service interface
    /// </summary>
    public interface ITypedTextService
    {
        Task<TextInfoResult> GetTextAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ActionResult> SetTextAsync(
            string elementId,
            string text,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<TextInfoResult> GetSelectedTextAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<ActionResult> SelectTextAsync(
            string elementId,
            int startIndex,
            int length,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);
    }
}