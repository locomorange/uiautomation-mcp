using UIAutomationMCP.Models.Abstractions;
using System.Threading.Tasks;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IRangeService
    {
        Task<ServerEnhancedResponse<ActionResult>> SetRangeValueAsync(string? automationId = null, string? name = null, double value = 0, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<RangeValueResult>> GetRangeValueAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30);
    }
}
