using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services
{
    public interface IAccessibilityService
    {
        Task<object> GetAccessibilityInfoAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> VerifyAccessibilityAsync(string? elementId = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 60);
        Task<object> GetLabeledByAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetDescribedByAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
