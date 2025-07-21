using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services
{
    public interface IEventMonitorService
    {
        Task<ServerEnhancedResponse<EventMonitoringResult>> MonitorEventsAsync(string eventType, int duration, string? automationId = null, string? name = null, string? controlType = null, int? processId = null);
        Task<ServerEnhancedResponse<EventMonitoringStartResult>> StartEventMonitoringAsync(string eventType, string? automationId = null, string? name = null, string? controlType = null, int? processId = null);
        Task<ServerEnhancedResponse<EventMonitoringStopResult>> StopEventMonitoringAsync();
        Task<ServerEnhancedResponse<EventLogResult>> GetEventLogAsync();
    }
}
