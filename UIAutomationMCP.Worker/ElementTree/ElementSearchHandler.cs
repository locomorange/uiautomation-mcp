using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationWorker.Helpers;
using UiAutomationWorker.Core;

namespace UiAutomationWorker.ElementTree
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
            return await ExecuteWithTimeoutAsync<object?>(operation, () =>
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
                    var elementInfo = _elementInfoExtractor.ExtractElementInfo(element);
                    return elementInfo;
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
            return await ExecuteWithTimeoutAsync<List<Dictionary<string, object>>>(operation, () =>
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
                var elementInfos = new List<Dictionary<string, object>>();
                
                if (elements != null && elements.Count > 0)
                {
                    foreach (AutomationElement element in elements)
                    {
                        try
                        {
                            var elementInfo = _elementInfoExtractor.ExtractElementInfo(element);
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
    }
}