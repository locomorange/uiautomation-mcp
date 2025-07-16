using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Requests;
using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class StylesService : IStylesService
    {
        private readonly ISubprocessExecutor _subprocessExecutor;
        private readonly ILogger<StylesService> _logger;

        public StylesService(ISubprocessExecutor subprocessExecutor, ILogger<StylesService> logger)
        {
            _subprocessExecutor = subprocessExecutor;
            _logger = logger;
        }

        public async Task<object> GetStyleIdAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            _logger.LogInformation("Getting style ID for element: {ElementId}", elementId);

            var request = new WorkerRequest
            {
                Operation = "GetStyleId",
                Parameters = new GetStyleIdRequest
                {
                    ElementId = elementId,
                    WindowTitle = windowTitle ?? string.Empty,
                    ProcessId = processId
                }
            };

            var result = await _subprocessExecutor.ExecuteWithTimeoutAsync(request, timeoutSeconds);

            if (!result.Success)
            {
                _logger.LogError("Failed to get style ID: {Error}", result.Error);
                throw new InvalidOperationException($"Failed to get style ID: {result.Error}");
            }

            return result.Data ?? new { };
        }

        public async Task<object> GetStyleNameAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            _logger.LogInformation("Getting style name for element: {ElementId}", elementId);

            var request = new WorkerRequest
            {
                Operation = "GetStyleName",
                Parameters = new GetStyleNameRequest
                {
                    ElementId = elementId,
                    WindowTitle = windowTitle ?? string.Empty,
                    ProcessId = processId
                }
            };

            var result = await _subprocessExecutor.ExecuteWithTimeoutAsync(request, timeoutSeconds);

            if (!result.Success)
            {
                _logger.LogError("Failed to get style name: {Error}", result.Error);
                throw new InvalidOperationException($"Failed to get style name: {result.Error}");
            }

            return result.Data ?? new { };
        }

        public async Task<object> GetFillColorAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            _logger.LogInformation("Getting fill color for element: {ElementId}", elementId);

            var request = new WorkerRequest
            {
                Operation = "GetFillColor",
                Parameters = new GetFillColorRequest
                {
                    ElementId = elementId,
                    WindowTitle = windowTitle ?? string.Empty,
                    ProcessId = processId
                }
            };

            var result = await _subprocessExecutor.ExecuteWithTimeoutAsync(request, timeoutSeconds);

            if (!result.Success)
            {
                _logger.LogError("Failed to get fill color: {Error}", result.Error);
                throw new InvalidOperationException($"Failed to get fill color: {result.Error}");
            }

            return result.Data ?? new { };
        }

        public async Task<object> GetFillPatternColorAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            _logger.LogInformation("Getting fill pattern color for element: {ElementId}", elementId);

            var request = new WorkerRequest
            {
                Operation = "GetFillPatternColor",
                Parameters = new GetFillPatternColorRequest
                {
                    ElementId = elementId,
                    WindowTitle = windowTitle ?? string.Empty,
                    ProcessId = processId
                }
            };

            var result = await _subprocessExecutor.ExecuteWithTimeoutAsync(request, timeoutSeconds);

            if (!result.Success)
            {
                _logger.LogError("Failed to get fill pattern color: {Error}", result.Error);
                throw new InvalidOperationException($"Failed to get fill pattern color: {result.Error}");
            }

            return result.Data ?? new { };
        }

        public async Task<object> GetShapeAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            _logger.LogInformation("Getting shape for element: {ElementId}", elementId);

            var request = new WorkerRequest
            {
                Operation = "GetShape",
                Parameters = new GetShapeRequest
                {
                    ElementId = elementId,
                    WindowTitle = windowTitle ?? string.Empty,
                    ProcessId = processId
                }
            };

            var result = await _subprocessExecutor.ExecuteWithTimeoutAsync(request, timeoutSeconds);

            if (!result.Success)
            {
                _logger.LogError("Failed to get shape: {Error}", result.Error);
                throw new InvalidOperationException($"Failed to get shape: {result.Error}");
            }

            return result.Data ?? new { };
        }

        public async Task<object> GetFillPatternStyleAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            _logger.LogInformation("Getting fill pattern style for element: {ElementId}", elementId);

            var request = new WorkerRequest
            {
                Operation = "GetFillPatternStyle",
                Parameters = new GetFillPatternStyleRequest
                {
                    ElementId = elementId,
                    WindowTitle = windowTitle ?? string.Empty,
                    ProcessId = processId
                }
            };

            var result = await _subprocessExecutor.ExecuteWithTimeoutAsync(request, timeoutSeconds);

            if (!result.Success)
            {
                _logger.LogError("Failed to get fill pattern style: {Error}", result.Error);
                throw new InvalidOperationException($"Failed to get fill pattern style: {result.Error}");
            }

            return result.Data ?? new { };
        }

        public async Task<object> GetExtendedPropertiesAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            _logger.LogInformation("Getting extended properties for element: {ElementId}", elementId);

            var request = new WorkerRequest
            {
                Operation = "GetExtendedProperties",
                Parameters = new GetExtendedPropertiesRequest
                {
                    ElementId = elementId,
                    WindowTitle = windowTitle ?? string.Empty,
                    ProcessId = processId
                }
            };

            var result = await _subprocessExecutor.ExecuteWithTimeoutAsync(request, timeoutSeconds);

            if (!result.Success)
            {
                _logger.LogError("Failed to get extended properties: {Error}", result.Error);
                throw new InvalidOperationException($"Failed to get extended properties: {result.Error}");
            }

            return result.Data ?? new { };
        }
    }
}