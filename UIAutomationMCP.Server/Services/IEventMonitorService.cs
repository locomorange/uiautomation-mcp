using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services
{
    public interface IEventMonitorService
    {
        Task<ServerEnhancedResponse<EventMonitoringResult>> MonitorEventsAsync(string eventType, int duration, string? elementId = null, string? windowTitle = null, int? processId = null);
        Task<ServerEnhancedResponse<EventMonitoringStartResult>> StartEventMonitoringAsync(string eventType, string? elementId = null, string? windowTitle = null, int? processId = null);
        Task<ServerEnhancedResponse<EventMonitoringStopResult>> StopEventMonitoringAsync();
        Task<ServerEnhancedResponse<EventLogResult>> GetEventLogAsync();
    }
}
