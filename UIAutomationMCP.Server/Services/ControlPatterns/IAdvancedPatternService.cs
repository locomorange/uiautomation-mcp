using UIAutomationMCP.Models.Abstractions;
using System.Threading.Tasks;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IAdvancedPatternService
    {
        Task<ServerEnhancedResponse<ElementSearchResult>> SetViewAsync(string? automationId = null, string? name = null, int viewId = 0, string? controlType = null, long? windowHandle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> RealizeItemAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> StartListeningAsync(string? automationId = null, string? name = null, string inputType = "", string? controlType = null, long? windowHandle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> CancelAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
