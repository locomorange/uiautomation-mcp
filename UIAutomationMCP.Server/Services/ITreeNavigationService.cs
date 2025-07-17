using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services
{
    public interface ITreeNavigationService
    {
        Task<object> GetChildrenAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetParentAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetSiblingsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetDescendantsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetAncestorsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetElementTreeAsync(string? windowTitle = null, int? processId = null, int maxDepth = 3, int timeoutSeconds = 60);
        Task<object> GetElementTreeAsJsonAsync(string? windowTitle = null, int? processId = null, int maxDepth = 3, int timeoutSeconds = 60);
    }
}
