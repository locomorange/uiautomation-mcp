namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IVirtualizedItemService
    {
        Task<object> RealizeItemAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}