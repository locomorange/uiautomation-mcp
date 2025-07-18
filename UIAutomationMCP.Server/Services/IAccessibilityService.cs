using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services
{
    public interface IAccessibilityService
    {
        Task<ServerEnhancedResponse<ElementSearchResult>> GetAccessibilityInfoAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> VerifyAccessibilityAsync(string? elementId = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 60);
        Task<ServerEnhancedResponse<ElementSearchResult>> GetLabeledByAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> GetDescribedByAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
