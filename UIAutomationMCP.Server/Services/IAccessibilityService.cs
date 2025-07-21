using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services
{
    public interface IAccessibilityService
    {
        Task<ServerEnhancedResponse<ElementSearchResult>> VerifyAccessibilityAsync(string? elementId = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 60);
        Task<ServerEnhancedResponse<ElementSearchResult>> GetLabeledByAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> GetDescribedByAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> GetAccessibilityInfoAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
    }
}
