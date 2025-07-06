using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.Core;
using UiAutomationMcpServer.Helpers;

namespace UiAutomationMcpServer.Patterns.Selection
{
    /// <summary>
    /// Microsoft UI Automation SelectionItemPattern handler
    /// Provides functionality for selectable items in containers (list items, menu items, etc.)
    /// </summary>
    public class SelectionItemPatternHandler : BaseAutomationHandler
    {
        public SelectionItemPatternHandler(
            ILogger<SelectionItemPatternHandler> logger,
            AutomationHelper automationHelper)
            : base(logger, automationHelper)
        {
        }

        /// <summary>
        /// Selects an item
        /// </summary>
        public async Task<WorkerResult> ExecuteSelectAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("Select");
                }

                if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var patternObj) && 
                    patternObj is SelectionItemPattern selectionItemPattern)
                {
                    _logger.LogInformation("[SelectionItemPatternHandler] Selecting element: {ElementName}", 
                        SafeGetElementName(element));
                    
                    selectionItemPattern.Select();
                    
                    return new WorkerResult
                    {
                        Success = true,
                        Data = "Element selected successfully"
                    };
                }
                else
                {
                    return CreatePatternNotSupportedResult("SelectionItemPattern");
                }
            }, "Select");
        }

        /// <summary>
        /// Adds an item to the selection
        /// </summary>
        public async Task<WorkerResult> ExecuteAddToSelectionAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("AddToSelection");
                }

                if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var patternObj) && 
                    patternObj is SelectionItemPattern selectionItemPattern)
                {
                    _logger.LogInformation("[SelectionItemPatternHandler] Adding to selection: {ElementName}", 
                        SafeGetElementName(element));
                    
                    selectionItemPattern.AddToSelection();
                    
                    return new WorkerResult
                    {
                        Success = true,
                        Data = "Element added to selection successfully"
                    };
                }
                else
                {
                    return CreatePatternNotSupportedResult("SelectionItemPattern");
                }
            }, "AddToSelection");
        }

        /// <summary>
        /// Removes an item from the selection
        /// </summary>
        public async Task<WorkerResult> ExecuteRemoveFromSelectionAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("RemoveFromSelection");
                }

                if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var patternObj) && 
                    patternObj is SelectionItemPattern selectionItemPattern)
                {
                    _logger.LogInformation("[SelectionItemPatternHandler] Removing from selection: {ElementName}", 
                        SafeGetElementName(element));
                    
                    selectionItemPattern.RemoveFromSelection();
                    
                    return new WorkerResult
                    {
                        Success = true,
                        Data = "Element removed from selection successfully"
                    };
                }
                else
                {
                    return CreatePatternNotSupportedResult("SelectionItemPattern");
                }
            }, "RemoveFromSelection");
        }

        /// <summary>
        /// Gets the selection state of an item
        /// </summary>
        public async Task<WorkerResult> ExecuteGetSelectionStateAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("GetSelectionState");
                }

                if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var patternObj) && 
                    patternObj is SelectionItemPattern selectionItemPattern)
                {
                    var isSelected = selectionItemPattern.Current.IsSelected;
                    var selectionContainer = selectionItemPattern.Current.SelectionContainer;
                    
                    return new WorkerResult
                    {
                        Success = true,
                        Data = new Dictionary<string, object>
                        {
                            ["IsSelected"] = isSelected,
                            ["SelectionContainer"] = selectionContainer?.Current.Name ?? "Unknown"
                        }
                    };
                }
                else
                {
                    return CreatePatternNotSupportedResult("SelectionItemPattern");
                }
            }, "GetSelectionState");
        }
    }
}