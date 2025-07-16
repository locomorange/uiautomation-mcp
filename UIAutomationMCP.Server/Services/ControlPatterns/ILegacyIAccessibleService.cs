namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface ILegacyIAccessibleService
    {
        Task<object> GetLegacyPropertiesAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> DoDefaultActionAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> SelectLegacyItemAsync(string elementId, int flagsSelect, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> SetLegacyValueAsync(string elementId, string value, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetLegacyStateAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}