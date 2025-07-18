using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IVirtualizedItemService
    {
        Task<ServerEnhancedResponse<ElementSearchResult>> RealizeItemAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}