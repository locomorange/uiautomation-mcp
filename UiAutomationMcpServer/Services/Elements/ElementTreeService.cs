using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.Services;

namespace UiAutomationMcpServer.Services.Elements
{
    public interface IElementTreeService
    {
        Task<OperationResult> GetElementTreeAsync(string? windowTitle = null, string treeView = "control", int maxDepth = 3, int? processId = null);
        Task<OperationResult> GetElementChildrenAsync(string elementId, string? windowTitle = null, int? processId = null);
    }

    public class ElementTreeService : IElementTreeService
    {
        private readonly ILogger<ElementTreeService> _logger;
        private readonly IUIAutomationWorker _uiAutomationWorker;

        public ElementTreeService(ILogger<ElementTreeService> logger, IUIAutomationWorker uiAutomationWorker)
        {
            _logger = logger;
            _uiAutomationWorker = uiAutomationWorker;
        }

        public async Task<OperationResult> GetElementTreeAsync(string? windowTitle = null, string treeView = "control", int maxDepth = 3, int? processId = null)
        {
            _logger.LogInformation("Getting element tree for window '{WindowTitle}', treeView: {TreeView}, maxDepth: {MaxDepth}", 
                windowTitle, treeView, maxDepth);

            var result = await _uiAutomationWorker.GetElementTreeAsync(windowTitle, processId, maxDepth);
            return new OperationResult { Success = result.Success, Data = result.Data, Error = result.Error };
        }

        public async Task<OperationResult> GetElementChildrenAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            _logger.LogInformation("Getting children for element '{ElementId}' in window '{WindowTitle}'", elementId, windowTitle);

            var result = await _uiAutomationWorker.GetElementChildrenAsync(elementId, windowTitle, processId);
            return new OperationResult { Success = result.Success, Data = result.Data, Error = result.Error };
        }

        // All tree operations are now handled by the UIAutomationWorker subprocess
        // to prevent main process hanging
    }
}
