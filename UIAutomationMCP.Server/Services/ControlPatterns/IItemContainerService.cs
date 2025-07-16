namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IItemContainerService
    {
        Task<object> FindItemByPropertyAsync(string containerId, string? propertyName = null, string? value = null, string? startAfterId = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}