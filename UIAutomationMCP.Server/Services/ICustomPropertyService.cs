using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services
{
    public interface ICustomPropertyService
    {
        Task<ServerEnhancedResponse<ElementSearchResult>> GetCustomPropertiesAsync(string? automationId = null, string? name = null, string[]? propertyIds = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> SetCustomPropertyAsync(string? automationId = null, string? name = null, string propertyId = "", object? value = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
    }
}
