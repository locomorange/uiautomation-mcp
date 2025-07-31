using UIAutomationMCP.Models.Abstractions;
using System.Threading.Tasks;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IVirtualizedItemService
    {
        Task<ServerEnhancedResponse<ElementSearchResult>> RealizeItemAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30);
    }
}
