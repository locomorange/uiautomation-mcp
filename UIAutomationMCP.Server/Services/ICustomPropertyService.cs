using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services
{
    public interface ICustomPropertyService
    {
        Task<object> GetCustomPropertiesAsync(string elementId, string[] propertyIds, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> SetCustomPropertyAsync(string elementId, string propertyId, object value, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
