using UIAutomationMCP.Models.Abstractions;
using System.Threading.Tasks;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Server.Services
{
    public interface ITreeNavigationService
    {
        Task<ServerEnhancedResponse<TreeNavigationResult>> GetChildrenAsync(string elementId, long? windowHandle = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<TreeNavigationResult>> GetParentAsync(string elementId, long? windowHandle = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<TreeNavigationResult>> GetSiblingsAsync(string elementId, long? windowHandle = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<TreeNavigationResult>> GetDescendantsAsync(string elementId, long? windowHandle = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<TreeNavigationResult>> GetAncestorsAsync(string elementId, long? windowHandle = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementTreeResult>> GetElementTreeAsync(long? windowHandle = null, int maxDepth = 3, int timeoutSeconds = 60);
    }
}

