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
    public class GetRowHeadersOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetRowHeadersOperation> _logger;

        public GetRowHeadersOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetRowHeadersOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<GetRowHeadersRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name,
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

                if (!element.TryGetCurrentPattern(TablePattern.Pattern, out var pattern) || pattern is not TablePattern tablePattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "TablePattern not supported",
                        Data = new ElementSearchResult()
                    });
                }

                var rowHeaders = tablePattern.Current.GetRowHeaders();
                if (rowHeaders == null || rowHeaders.Length == 0)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "No row headers found",
                        Data = new ElementSearchResult()
                    });
                }

                var result = new ElementSearchResult
                {
                    SearchCriteria = "Table row headers search (TablePattern)"
                };

                foreach (var header in rowHeaders)
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
                _logger.LogError(ex, "GetRowHeaders operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get row headers: {ex.Message}",
                    Data = new ElementSearchResult()
                });
            }
        }
    }
}