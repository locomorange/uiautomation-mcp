using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IItemContainerService
    {
        Task<ServerEnhancedResponse<ElementSearchResult>> FindItemByPropertyAsync(string containerId, string? propertyName = null, string? value = null, string? startAfterId = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}