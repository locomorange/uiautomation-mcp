using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Interfaces
{
    /// <summary>
    /// Strongly typed invoke service interface
    /// </summary>
    public interface ITypedInvokeService
    {
        Task<ActionResult> InvokeAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 30);
    }
}