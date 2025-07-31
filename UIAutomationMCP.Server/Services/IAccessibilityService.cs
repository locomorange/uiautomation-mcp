using UIAutomationMCP.Models.Abstractions;
using System.Threading.Tasks;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Server.Services
{
    public interface IAccessibilityService
    {
        Task<ServerEnhancedResponse<ElementSearchResult>> GetDescribedByAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30);
    }
}

