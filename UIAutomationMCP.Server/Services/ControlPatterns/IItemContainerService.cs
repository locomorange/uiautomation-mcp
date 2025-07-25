using System.Threading.Tasks;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IItemContainerService
    {
        Task<ServerEnhancedResponse<ElementSearchResult>> FindItemByPropertyAsync(string? automationId = null, string? name = null, string? propertyName = null, string? value = null, string? startAfterId = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
    }
}