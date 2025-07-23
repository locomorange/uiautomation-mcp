using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface ITextService
    {
        Task<ServerEnhancedResponse<ActionResult>> SetTextAsync(string? automationId = null, string? name = null, string text = "", string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<TextInfoResult>> GetSelectedTextAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> AppendTextAsync(string? automationId = null, string? name = null, string text = "", string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> SelectTextAsync(string? automationId = null, string? name = null, int startIndex = 0, int length = 1, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
    }
}
