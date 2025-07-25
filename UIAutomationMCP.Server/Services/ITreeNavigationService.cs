using System.Threading.Tasks;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Server.Services
{
    public interface ITreeNavigationService
    {
        Task<ServerEnhancedResponse<TreeNavigationResult>> GetChildrenAsync(string elementId, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<TreeNavigationResult>> GetParentAsync(string elementId, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<TreeNavigationResult>> GetSiblingsAsync(string elementId, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<TreeNavigationResult>> GetDescendantsAsync(string elementId, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<TreeNavigationResult>> GetAncestorsAsync(string elementId, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementTreeResult>> GetElementTreeAsync(int? processId = null, int maxDepth = 3, int timeoutSeconds = 60);
    }
}
