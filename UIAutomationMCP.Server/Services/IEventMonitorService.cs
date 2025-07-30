using UIAutomationMCP.Models.Abstractions;
using System.Threading.Tasks;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Server.Services
{
    public interface IEventMonitorService
    {
        Task<ServerEnhancedResponse<EventMonitoringStartResult>> StartEventMonitoringAsync(string eventType, string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 60);
        Task<ServerEnhancedResponse<EventMonitoringStopResult>> StopEventMonitoringAsync(string? sessionId = null, int timeoutSeconds = 60);
        Task<ServerEnhancedResponse<EventLogResult>> GetEventLogAsync(string? sessionId = null, int maxCount = 100, int timeoutSeconds = 60);
    }
}
