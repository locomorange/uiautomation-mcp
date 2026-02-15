using UIAutomationMCP.Models.Abstractions;
using System.Threading.Tasks;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class AdvancedPatternService : IAdvancedPatternService
    {
        private readonly IMultipleViewService _multipleViewService;
        private readonly IVirtualizedItemService _virtualizedItemService;
        private readonly ISynchronizedInputService _synchronizedInputService;

        public AdvancedPatternService(
            IMultipleViewService multipleViewService,
            IVirtualizedItemService virtualizedItemService,
            ISynchronizedInputService synchronizedInputService)
        {
            _multipleViewService = multipleViewService;
            _virtualizedItemService = virtualizedItemService;
            _synchronizedInputService = synchronizedInputService;
        }

        public Task<ServerEnhancedResponse<ElementSearchResult>> SetViewAsync(string? automationId = null, string? name = null, int viewId = 0, string? controlType = null, long? windowHandle = null, int? processId = null, int timeoutSeconds = 30)
            => _multipleViewService.SetViewAsync(automationId, name, viewId, controlType, windowHandle, processId, timeoutSeconds);

        public Task<ServerEnhancedResponse<ElementSearchResult>> RealizeItemAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int? processId = null, int timeoutSeconds = 30)
            => _virtualizedItemService.RealizeItemAsync(automationId, name, controlType, windowHandle, processId, timeoutSeconds);

        public Task<ServerEnhancedResponse<ElementSearchResult>> StartListeningAsync(string? automationId = null, string? name = null, string inputType = "", string? controlType = null, long? windowHandle = null, int? processId = null, int timeoutSeconds = 30)
            => _synchronizedInputService.StartListeningAsync(automationId, name, inputType, controlType, windowHandle, processId, timeoutSeconds);

        public Task<ServerEnhancedResponse<ElementSearchResult>> CancelAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int? processId = null, int timeoutSeconds = 30)
            => _synchronizedInputService.CancelAsync(automationId, name, controlType, windowHandle, processId, timeoutSeconds);
    }
}
