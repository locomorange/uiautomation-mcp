using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services
{
    public interface IEventMonitorService
    {
        Task<object> MonitorEventsAsync(string eventType, int duration, string? elementId = null, string? windowTitle = null, int? processId = null);
        Task<object> StartEventMonitoringAsync(string eventType, string? elementId = null, string? windowTitle = null, int? processId = null);
        Task<object> StopEventMonitoringAsync();
        Task<object> GetEventLogAsync();
    }
}
