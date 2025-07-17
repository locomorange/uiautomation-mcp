using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services
{
    public class TreeNavigationService : ITreeNavigationService
    {
        private readonly ILogger<TreeNavigationService> _logger;
        private readonly SubprocessExecutor _executor;

        public TreeNavigationService(
            ILogger<TreeNavigationService> logger,
            SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> GetChildrenAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting children for element: {ElementId}, WindowTitle={WindowTitle}, ProcessId={ProcessId}",
                    elementId, windowTitle, processId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<TreeNavigationResult>("GetChildren", parameters, timeoutSeconds);

                _logger.LogInformation("Got children successfully for element: {ElementId}", elementId);
                
                // Convert TreeNavigationResult to Dictionary for MCP compatibility
                var response = new Dictionary<string, object>
                {
                    ["success"] = result.Success,
                    ["elementId"] = result.ElementId ?? "",
                    ["childrenInfo"] = $"Found {result.Children.Count} children elements",
                    ["childCount"] = result.ChildCount,
                    ["executedAt"] = result.ExecutedAt
                };
                
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                    response["error"] = result.ErrorMessage;
                    
                return new Dictionary<string, object> { ["success"] = true, ["data"] = response };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get children for element: {ElementId}", elementId);
                return new Dictionary<string, object> { ["success"] = false, ["error"] = ex.Message };
            }
        }

        public async Task<object> GetParentAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting parent for element: {ElementId}, WindowTitle={WindowTitle}, ProcessId={ProcessId}",
                    elementId, windowTitle, processId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<TreeNavigationResult>("GetParent", parameters, timeoutSeconds);

                _logger.LogInformation("Got parent successfully for element: {ElementId}", elementId);
                return new Dictionary<string, object> { ["success"] = true, ["data"] = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get parent for element: {ElementId}", elementId);
                return new Dictionary<string, object> { ["success"] = false, ["error"] = ex.Message };
            }
        }

        public async Task<object> GetSiblingsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting siblings for element: {ElementId}, WindowTitle={WindowTitle}, ProcessId={ProcessId}",
                    elementId, windowTitle, processId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<TreeNavigationResult>("GetSiblings", parameters, timeoutSeconds);

                _logger.LogInformation("Got siblings successfully for element: {ElementId}", elementId);
                return new Dictionary<string, object> { ["success"] = true, ["data"] = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get siblings for element: {ElementId}", elementId);
                return new Dictionary<string, object> { ["success"] = false, ["error"] = ex.Message };
            }
        }

        public async Task<object> GetDescendantsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting descendants for element: {ElementId}, WindowTitle={WindowTitle}, ProcessId={ProcessId}",
                    elementId, windowTitle, processId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<TreeNavigationResult>("GetDescendants", parameters, timeoutSeconds);

                _logger.LogInformation("Got descendants successfully for element: {ElementId}", elementId);
                return new Dictionary<string, object> { ["success"] = true, ["data"] = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get descendants for element: {ElementId}", elementId);
                return new Dictionary<string, object> { ["success"] = false, ["error"] = ex.Message };
            }
        }

        public async Task<object> GetAncestorsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting ancestors for element: {ElementId}, WindowTitle={WindowTitle}, ProcessId={ProcessId}",
                    elementId, windowTitle, processId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<TreeNavigationResult>("GetAncestors", parameters, timeoutSeconds);

                _logger.LogInformation("Got ancestors successfully for element: {ElementId}", elementId);
                return new Dictionary<string, object> { ["success"] = true, ["data"] = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get ancestors for element: {ElementId}", elementId);
                return new Dictionary<string, object> { ["success"] = false, ["error"] = ex.Message };
            }
        }

        public async Task<object> GetElementTreeAsync(string? windowTitle = null, int? processId = null, int maxDepth = 3, int timeoutSeconds = 60)
        {
            try
            {
                _logger.LogInformation("Getting element tree with WindowTitle={WindowTitle}, ProcessId={ProcessId}, MaxDepth={MaxDepth}",
                    windowTitle, processId, maxDepth);

                var parameters = new Dictionary<string, object>
                {
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 },
                    { "maxDepth", maxDepth }
                };

                var result = await _executor.ExecuteAsync<ElementTreeResult>("GetElementTree", parameters, timeoutSeconds);

                _logger.LogInformation("Element tree built successfully");
                
                // Convert ElementTreeResult to Dictionary for MCP compatibility
                var response = new Dictionary<string, object>
                {
                    ["success"] = result.Success,
                    ["rootNode"] = ConvertTreeNodeToDict(result.RootNode),
                    ["totalElements"] = result.TotalElements,
                    ["maxDepth"] = result.MaxDepth,
                    ["processId"] = result.ProcessId,
                    ["buildDuration"] = result.BuildDuration.ToString(),
                    ["executedAt"] = result.ExecutedAt
                };
                
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                    response["error"] = result.ErrorMessage;
                    
                return new Dictionary<string, object> { ["success"] = true, ["data"] = response };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element tree");
                return new Dictionary<string, object> { ["success"] = false, ["error"] = ex.Message };
            }
        }

        public async Task<object> GetElementTreeAsJsonAsync(string? windowTitle = null, int? processId = null, int maxDepth = 3, int timeoutSeconds = 60)
        {
            try
            {
                _logger.LogInformation("Getting element tree as JSON with WindowTitle={WindowTitle}, ProcessId={ProcessId}, MaxDepth={MaxDepth}",
                    windowTitle, processId, maxDepth);

                var parameters = new Dictionary<string, object>
                {
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 },
                    { "maxDepth", maxDepth }
                };

                var result = await _executor.ExecuteAsync<ElementTreeResult>("GetElementTree", parameters, timeoutSeconds);

                _logger.LogInformation("Element tree built successfully, serializing to JSON");
                
                // Serialize the entire ElementTreeResult to JSON using our own serializer
                var jsonString = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.SerializeObject(result);
                
                _logger.LogInformation("Serialized ElementTreeResult to JSON (length: {Length})", jsonString.Length);
                
                // Return the JSON string directly
                return jsonString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element tree as JSON");
                return $"{{\"success\": false, \"error\": \"{ex.Message}\"}}";
            }
        }

        private object? ConvertTreeNodeToDict(TreeNode? node)
        {
            if (node == null) return null;
            
            // Flatten to simple structure avoiding nested complex objects
            var simple = new Dictionary<string, object>
            {
                ["elementId"] = node.ElementId ?? "",
                ["name"] = node.Name ?? "",
                ["automationId"] = node.AutomationId ?? "",
                ["className"] = node.ClassName ?? "",
                ["controlType"] = node.ControlType ?? "",
                ["localizedControlType"] = node.LocalizedControlType ?? "",
                ["isEnabled"] = node.IsEnabled,
                ["isKeyboardFocusable"] = node.IsKeyboardFocusable,
                ["hasKeyboardFocus"] = node.HasKeyboardFocus,
                ["isPassword"] = node.IsPassword,
                ["isOffscreen"] = node.IsOffscreen,
                ["boundingX"] = node.BoundingRectangle.X,
                ["boundingY"] = node.BoundingRectangle.Y,
                ["boundingWidth"] = node.BoundingRectangle.Width,
                ["boundingHeight"] = node.BoundingRectangle.Height,
                ["processId"] = node.ProcessId,
                ["runtimeId"] = node.RuntimeId ?? "",
                ["frameworkId"] = node.FrameworkId ?? "",
                ["supportedPatterns"] = string.Join(",", node.SupportedPatterns),
                ["depth"] = node.Depth,
                ["isExpanded"] = node.IsExpanded,
                ["hasChildren"] = node.HasChildren,
                ["parentElementId"] = node.ParentElementId ?? "",
                ["childrenCount"] = node.Children.Count
            };
            
            // Skip children arrays entirely for MCP compatibility
            
            return simple;
        }

        private Dictionary<string, object> ConvertTreeElementToDict(TreeElement element)
        {
            return new Dictionary<string, object>
            {
                ["elementId"] = element.ElementId,
                ["parentElementId"] = element.ParentElementId ?? "",
                ["name"] = element.Name ?? "",
                ["automationId"] = element.AutomationId ?? "",
                ["className"] = element.ClassName ?? "",
                ["controlType"] = element.ControlType ?? "",
                ["localizedControlType"] = element.LocalizedControlType ?? "",
                ["isEnabled"] = element.IsEnabled,
                ["isKeyboardFocusable"] = element.IsKeyboardFocusable,
                ["hasKeyboardFocus"] = element.HasKeyboardFocus,
                ["isPassword"] = element.IsPassword,
                ["isOffscreen"] = element.IsOffscreen,
                ["boundingX"] = element.BoundingRectangle.X,
                ["boundingY"] = element.BoundingRectangle.Y,
                ["boundingWidth"] = element.BoundingRectangle.Width,
                ["boundingHeight"] = element.BoundingRectangle.Height,
                ["supportedPatterns"] = string.Join(",", element.SupportedPatterns),
                ["processId"] = element.ProcessId,
                ["runtimeId"] = element.RuntimeId ?? "",
                ["frameworkId"] = element.FrameworkId ?? "",
                ["nativeWindowHandle"] = element.NativeWindowHandle,
                ["isControlElement"] = element.IsControlElement,
                ["isContentElement"] = element.IsContentElement,
                ["hasChildren"] = element.HasChildren,
                ["childCount"] = element.ChildCount
            };
        }
    }
}
