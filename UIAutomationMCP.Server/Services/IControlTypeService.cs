using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services
{
    public interface IControlTypeService
    {
        Task<ServerEnhancedResponse<ElementSearchResult>> ValidateControlTypePatternsAsync(
            string? automationId = null, 
            string? name = null,
            string? controlType = null,
            int? processId = null, 
            int timeoutSeconds = 30);
    }
}