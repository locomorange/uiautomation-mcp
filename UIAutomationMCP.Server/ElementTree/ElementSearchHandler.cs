using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.Helpers;
using UiAutomationMcpServer.Core;

namespace UiAutomationMcpServer.ElementTree
{
    /// <summary>
    /// Handles element search operations in the UI Automation tree
    /// </summary>
    public class ElementSearchHandler : BaseAutomationHandler
    {
        private readonly ElementInfoExtractor _elementInfoExtractor;

        public ElementSearchHandler(
            ILogger<ElementSearchHandler> logger,
            AutomationHelper automationHelper,
            ElementInfoExtractor elementInfoExtractor)
            : base(logger, automationHelper)
        {
            _elementInfoExtractor = elementInfoExtractor;
        }

        /// <summary>
        /// Finds the first element matching the criteria
        /// </summary>
        public async Task<WorkerResult> ExecuteFindFirstAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync<ElementInfo?>(operation, () =>
            {
                var searchRoot = _automationHelper.GetSearchRoot(operation);
                if (searchRoot == null)
                {
                    throw new InvalidOperationException("Failed to get search root element");
                }

                var condition = _automationHelper.BuildCondition(operation);
                if (condition == null)
                {
                    throw new InvalidOperationException("Failed to build search condition");
                }

                if (!Enum.TryParse<TreeScope>(
                    operation.Parameters.GetValueOrDefault("Scope")?.ToString(), 
                    out var scope))
                {
                    scope = TreeScope.Descendants;
                }

                _logger.LogInformation("[ElementSearchHandler] FindFirst with scope: {Scope}", scope);

                var element = searchRoot.FindFirst(scope, condition);

                if (element != null)
                {
                    var elementInfoDict = _elementInfoExtractor.ExtractElementInfo(element);
                    return ConvertToElementInfo(elementInfoDict);
                }
                else
                {
                    return null;
                }
            }, "FindFirst");
        }

        /// <summary>
        /// Finds all elements matching the criteria
        /// </summary>
        public async Task<WorkerResult> ExecuteFindAllAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync<List<ElementInfo>>(operation, () =>
            {
                var searchRoot = _automationHelper.GetSearchRoot(operation);
                if (searchRoot == null)
                {
                    throw new InvalidOperationException("Failed to get search root element");
                }

                var condition = _automationHelper.BuildCondition(operation);
                if (condition == null)
                {
                    throw new InvalidOperationException("Failed to build search condition");
                }

                if (!Enum.TryParse<TreeScope>(
                    operation.Parameters.GetValueOrDefault("Scope")?.ToString(), 
                    out var scope))
                {
                    scope = TreeScope.Descendants;
                }

                _logger.LogInformation("[ElementSearchHandler] FindAll with scope: {Scope}", scope);

                var elements = searchRoot.FindAll(scope, condition);
                var elementInfos = new List<ElementInfo>();
                
                if (elements != null && elements.Count > 0)
                {
                    foreach (AutomationElement element in elements)
                    {
                        try
                        {
                            var elementInfo = ConvertToElementInfo(_elementInfoExtractor.ExtractElementInfo(element));
                            elementInfos.Add(elementInfo);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "[ElementSearchHandler] Failed to extract info from element");
                            continue;
                        }
                    }
                }

                _logger.LogInformation("[ElementSearchHandler] FindAll found {Count} elements", elementInfos.Count);

                return elementInfos;
            }, "FindAll");
        }

        /// <summary>
        /// Gets properties of an element
        /// </summary>
        public async Task<WorkerResult> ExecuteGetPropertiesAsync(WorkerOperation operation)
        {
            _logger.LogInformation("[ElementSearchHandler] Executing GetProperties operation");

            try
            {
                return await ExecuteFindFirstAsync(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ElementSearchHandler] GetProperties operation failed");
                return new WorkerResult
                {
                    Success = false,
                    Error = $"GetProperties operation failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Converts a dictionary to ElementInfo object
        /// </summary>
        private ElementInfo ConvertToElementInfo(Dictionary<string, object> dict)
        {
            var elementInfo = new ElementInfo();

            if (dict.TryGetValue("Name", out var name))
                elementInfo.Name = name?.ToString() ?? "";

            if (dict.TryGetValue("AutomationId", out var automationId))
                elementInfo.AutomationId = automationId?.ToString() ?? "";

            if (dict.TryGetValue("ClassName", out var className))
                elementInfo.ClassName = className?.ToString() ?? "";

            if (dict.TryGetValue("ControlType", out var controlType))
                elementInfo.ControlType = controlType?.ToString() ?? "";

            if (dict.TryGetValue("ProcessId", out var processId) && processId != null)
            {
                if (int.TryParse(processId.ToString(), out var pid))
                    elementInfo.ProcessId = pid;
            }

            if (dict.TryGetValue("IsEnabled", out var isEnabled) && isEnabled != null)
            {
                if (bool.TryParse(isEnabled.ToString(), out var enabled))
                    elementInfo.IsEnabled = enabled;
            }

            if (dict.TryGetValue("IsVisible", out var isVisible) && isVisible != null)
            {
                if (bool.TryParse(isVisible.ToString(), out var visible))
                    elementInfo.IsVisible = visible;
            }

            if (dict.TryGetValue("HelpText", out var helpText))
                elementInfo.HelpText = helpText?.ToString() ?? "";

            if (dict.TryGetValue("Value", out var value))
                elementInfo.Value = value?.ToString();

            if (dict.TryGetValue("BoundingRectangle", out var boundingRect) && boundingRect is Dictionary<string, object> rectDict)
            {
                elementInfo.BoundingRectangle = new BoundingRectangle();
                
                if (rectDict.TryGetValue("X", out var x) && double.TryParse(x.ToString(), out var xVal))
                    elementInfo.BoundingRectangle.X = xVal;
                
                if (rectDict.TryGetValue("Y", out var y) && double.TryParse(y.ToString(), out var yVal))
                    elementInfo.BoundingRectangle.Y = yVal;
                
                if (rectDict.TryGetValue("Width", out var width) && double.TryParse(width.ToString(), out var widthVal))
                    elementInfo.BoundingRectangle.Width = widthVal;
                
                if (rectDict.TryGetValue("Height", out var height) && double.TryParse(height.ToString(), out var heightVal))
                    elementInfo.BoundingRectangle.Height = heightVal;
            }

            if (dict.TryGetValue("AvailableActions", out var availableActions) && availableActions is Dictionary<string, object> actionsDict)
            {
                elementInfo.AvailableActions = actionsDict.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => kvp.Value?.ToString() ?? "");
            }

            return elementInfo;
        }

        /// <summary>
        /// Finds all elements matching the criteria (synchronous version)
        /// </summary>
        public WorkerResult FindElements(WorkerOperation operation)
        {
            try
            {
                var searchRoot = _automationHelper.GetSearchRoot(operation);
                if (searchRoot == null)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "Failed to get search root element"
                    };
                }

                var condition = _automationHelper.BuildCondition(operation);
                if (condition == null)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "Failed to build search condition"
                    };
                }

                if (!Enum.TryParse<TreeScope>(
                    operation.Parameters.GetValueOrDefault("Scope")?.ToString(), 
                    out var scope))
                {
                    scope = TreeScope.Descendants;
                }

                _logger.LogInformation("[ElementSearchHandler] FindElements with scope: {Scope}", scope);

                var elements = searchRoot.FindAll(scope, condition);
                var elementInfos = new List<ElementInfo>();
                
                if (elements != null && elements.Count > 0)
                {
                    foreach (AutomationElement element in elements)
                    {
                        try
                        {
                            var elementInfoDict = _elementInfoExtractor.ExtractElementInfo(element);
                            var elementInfo = ConvertToElementInfo(elementInfoDict);
                            if (elementInfo != null)
                            {
                                elementInfos.Add(elementInfo);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "[ElementSearchHandler] Failed to extract info for element");
                        }
                    }
                }

                return new WorkerResult
                {
                    Success = true,
                    Data = elementInfos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ElementSearchHandler] FindElements failed");
                return new WorkerResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Finds the first element matching the criteria (synchronous version)
        /// </summary>
        public WorkerResult FindFirstElement(WorkerOperation operation)
        {
            try
            {
                var searchRoot = _automationHelper.GetSearchRoot(operation);
                if (searchRoot == null)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "Failed to get search root element"
                    };
                }

                var condition = _automationHelper.BuildCondition(operation);
                if (condition == null)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "Failed to build search condition"
                    };
                }

                if (!Enum.TryParse<TreeScope>(
                    operation.Parameters.GetValueOrDefault("Scope")?.ToString(), 
                    out var scope))
                {
                    scope = TreeScope.Descendants;
                }

                _logger.LogInformation("[ElementSearchHandler] FindFirstElement with scope: {Scope}", scope);

                var element = searchRoot.FindFirst(scope, condition);

                if (element != null)
                {
                    var elementInfoDict = _elementInfoExtractor.ExtractElementInfo(element);
                    var elementInfo = ConvertToElementInfo(elementInfoDict);
                    
                    return new WorkerResult
                    {
                        Success = true,
                        Data = elementInfo
                    };
                }
                else
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "Element not found"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ElementSearchHandler] FindFirstElement failed");
                return new WorkerResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }
    }
}