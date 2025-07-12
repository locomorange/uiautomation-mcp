using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services
{
    public interface IControlTypeService
    {
        Task<object> GetControlTypeInfoAsync(
            string elementId, 
            bool validatePatterns = true, 
            bool includeDefaultProperties = true, 
            string? windowTitle = null, 
            int? processId = null, 
            int timeoutSeconds = 30);

        Task<object> ValidateControlTypePatternsAsync(
            string elementId, 
            string? windowTitle = null, 
            int? processId = null, 
            int timeoutSeconds = 30);

        Task<object> FindElementsByControlTypeAsync(
            string controlType, 
            bool validatePatterns = true, 
            string scope = "descendants", 
            string? windowTitle = null, 
            int? processId = null, 
            int maxResults = 100, 
            int timeoutSeconds = 30);
    }
}