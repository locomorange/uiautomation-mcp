using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface ITextService
    {
        Task<object> GetTextAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> SetTextAsync(string elementId, string text, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> AppendTextAsync(string elementId, string text, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> SelectTextAsync(string elementId, int startIndex, int length, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetSelectedTextAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
