using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IRangeService
    {
        Task<ServerEnhancedResponse<ActionResult>> SetRangeValueAsync(string? automationId = null, string? name = null, double value = 0, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<RangeValueResult>> GetRangeValueAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
    }
}
