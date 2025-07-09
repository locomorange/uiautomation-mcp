using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services
{
    public interface IElementSearchService
    {
        Task<object> FindElementAsync(string? windowTitle = null, int? processId = null, string? name = null, string? automationId = null, string? className = null, string? controlType = null, int timeoutSeconds = 30);
        Task<object> FindAllElementsAsync(string? windowTitle = null, int? processId = null, string? name = null, string? automationId = null, string? className = null, string? controlType = null, int timeoutSeconds = 30);
        Task<object> FindElementByXPathAsync(string xpath, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> FindElementsByTagNameAsync(string tagName, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetWindowsAsync(int timeoutSeconds = 60);
        Task<object> FindElementsAsync(string? windowTitle = null, string? searchText = null, string? controlType = null, int? processId = null, int timeoutSeconds = 60);
    }
}
