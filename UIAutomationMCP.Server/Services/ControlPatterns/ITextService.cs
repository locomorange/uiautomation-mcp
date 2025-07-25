using System.Threading.Tasks;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface ITextService
    {
        Task<ServerEnhancedResponse<ActionResult>> SetTextAsync(string? automationId = null, string? name = null, string text = "", string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> AppendTextAsync(string? automationId = null, string? name = null, string text = "", string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> SelectTextAsync(string? automationId = null, string? name = null, int startIndex = 0, int length = 1, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<TextAttributesResult>> GetTextAttributesAsync(string? automationId = null, string? name = null, int startIndex = 0, int length = -1, string? attributeName = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<TextSearchResult>> FindTextAsync(string? automationId = null, string? name = null, string searchText = "", bool backward = false, bool ignoreCase = true, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
    }
}
