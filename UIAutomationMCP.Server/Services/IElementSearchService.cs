using System.Text.Json;
using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services
{
    public interface IElementSearchService
    {
        Task<JsonElement> FindElementAsync(string? windowTitle = null, int? processId = null, string? name = null, string? automationId = null, string? className = null, string? controlType = null, int timeoutSeconds = 30);
        Task<JsonElement> FindAllElementsAsync(string? windowTitle = null, int? processId = null, string? name = null, string? automationId = null, string? className = null, string? controlType = null, int timeoutSeconds = 30);
        Task<JsonElement> FindElementByXPathAsync(string xpath, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<JsonElement> FindElementsByTagNameAsync(string tagName, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<JsonElement> GetWindowsAsync(int timeoutSeconds = 60);
        Task<JsonElement> FindElementsAsync(string? windowTitle = null, string? searchText = null, string? controlType = null, int? processId = null, int timeoutSeconds = 60);
    }
}
