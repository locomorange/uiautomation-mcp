using System;
using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services.ControlTypes
{
    public interface ICalendarService
    {
        Task<object> CalendarOperationAsync(string elementId, string operation, DateTime? date = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
