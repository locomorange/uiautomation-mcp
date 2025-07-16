using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Requests;
using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class ObjectModelService : IObjectModelService
    {
        private readonly ISubprocessExecutor _subprocessExecutor;
        private readonly ILogger<ObjectModelService> _logger;

        public ObjectModelService(ISubprocessExecutor subprocessExecutor, ILogger<ObjectModelService> logger)
        {
            _subprocessExecutor = subprocessExecutor;
            _logger = logger;
        }

        public async Task<object> GetUnderlyingObjectModelAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            _logger.LogInformation("Getting underlying object model for element: {ElementId}", elementId);

            var request = new WorkerRequest
            {
                Operation = "GetUnderlyingObjectModel",
                Parameters = new GetUnderlyingObjectModelRequest
                {
                    ElementId = elementId,
                    WindowTitle = windowTitle ?? string.Empty,
                    ProcessId = processId
                }
            };

            var result = await _subprocessExecutor.ExecuteWithTimeoutAsync(request, timeoutSeconds);

            if (!result.Success)
            {
                _logger.LogError("Failed to get underlying object model: {Error}", result.Error);
                throw new InvalidOperationException($"Failed to get underlying object model: {result.Error}");
            }

            return result.Data ?? new { Message = "No object model data available" };
        }
    }
}