using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Table
{
    public class GetColumnHeaderItemsOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetColumnHeaderItemsOperation> _logger;

        public GetColumnHeaderItemsOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetColumnHeaderItemsOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<GetColumnHeaderItemsRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name,
                    controlType: typedRequest.ControlType,
                    processId: typedRequest.ProcessId);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element not found",
                        Data = new ElementSearchResult()
                    });
                }

                if (!element.TryGetCurrentPattern(TableItemPattern.Pattern, out var pattern) || pattern is not TableItemPattern tableItemPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "TableItemPattern not supported",
                        Data = new ElementSearchResult()
                    });
                }

                var columnHeaderItems = tableItemPattern.Current.GetColumnHeaderItems();
                if (columnHeaderItems == null || columnHeaderItems.Length == 0)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "No column header items found",
                        Data = new ElementSearchResult()
                    });
                }

                var result = new ElementSearchResult
                {
                    SearchCriteria = "Table column header items search (TableItemPattern)"
                };

                foreach (var header in columnHeaderItems)
                {
                    var headerInfo = new ElementInfo
                    {
                        AutomationId = header.Current.AutomationId,
                        Name = header.Current.Name,
                        ControlType = header.Current.ControlType.LocalizedControlType,
                        IsEnabled = header.Current.IsEnabled,
                        BoundingRectangle = new BoundingRectangle
                        {
                            X = header.Current.BoundingRectangle.X,
                            Y = header.Current.BoundingRectangle.Y,
                            Width = header.Current.BoundingRectangle.Width,
                            Height = header.Current.BoundingRectangle.Height
                        }
                    };
                    result.Elements.Add(headerInfo);
                }

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetColumnHeaderItems operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get column header items: {ex.Message}",
                    Data = new ElementSearchResult()
                });
            }
        }
    }
}