using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface ISynchronizedInputService
    {
        Task<ServerEnhancedResponse<ElementSearchResult>> StartListeningAsync(string elementId, string inputType, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> CancelAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}