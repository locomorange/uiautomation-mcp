using UIAutomationMCP.Models.Abstractions;
using System.Threading.Tasks;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class InteractionService : IInteractionService
    {
        private readonly IInvokeService _invokeService;
        private readonly IValueService _valueService;
        private readonly IToggleService _toggleService;
        private readonly IRangeService _rangeService;

        public InteractionService(
            IInvokeService invokeService,
            IValueService valueService,
            IToggleService toggleService,
            IRangeService rangeService)
        {
            _invokeService = invokeService;
            _valueService = valueService;
            _toggleService = toggleService;
            _rangeService = rangeService;
        }

        public Task<ServerEnhancedResponse<ActionResult>> InvokeElementAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30)
            => _invokeService.InvokeElementAsync(automationId, name, controlType, windowHandle, timeoutSeconds);

        public Task<ServerEnhancedResponse<ActionResult>> SetValueAsync(string? automationId = null, string? name = null, string value = "", string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30)
            => _valueService.SetValueAsync(value, automationId, name, controlType, windowHandle, timeoutSeconds);

        public Task<ServerEnhancedResponse<ActionResult>> ToggleElementAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30)
            => _toggleService.ToggleElementAsync(automationId, name, controlType, windowHandle, timeoutSeconds);

        public Task<ServerEnhancedResponse<ActionResult>> SetRangeValueAsync(string? automationId = null, string? name = null, double value = 0, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30)
            => _rangeService.SetRangeValueAsync(automationId, name, value, controlType, windowHandle, timeoutSeconds);

        public Task<ServerEnhancedResponse<RangeValueResult>> GetRangeValueAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30)
            => _rangeService.GetRangeValueAsync(automationId, name, controlType, windowHandle, timeoutSeconds);
    }
}
