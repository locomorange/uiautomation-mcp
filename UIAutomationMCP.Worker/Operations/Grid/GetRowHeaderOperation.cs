using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Grid
{
    public class GetRowHeaderOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetRowHeaderOperation> _logger;

        public GetRowHeaderOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetRowHeaderOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<GetRowHeaderRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name,
                    controlType: typedRequest.ControlType,
                    processId: typedRequest.ProcessId,
                    requiredPattern: GridPattern.Pattern);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element not found",
                        Data = new ElementSearchResult()
                    });
                }

                if (!element.TryGetCurrentPattern(GridPattern.Pattern, out var pattern) || pattern is not GridPattern gridPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "GridPattern not supported",
                        Data = new ElementSearchResult()
                    });
                }

                // Check if row is within bounds
                if (typedRequest.Row >= gridPattern.Current.RowCount)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Row index out of range",
                        Data = new ElementSearchResult()
                    });
                }

                // Try to get the first item in the specified row (assuming header is at column 0)
                var headerElement = gridPattern.GetItem(typedRequest.Row, 0);
                if (headerElement == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "No header element found at specified row",
                        Data = new ElementSearchResult()
                    });
                }

                var headerInfo = ElementInfoBuilder.CreateElementInfo(headerElement, includeDetails: true, _logger);

                var result = new ElementSearchResult
                {
                    SearchCriteria = "Grid row header search"
                };
                result.Elements.Add(headerInfo);

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetRowHeader operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get row header: {ex.Message}",
                    Data = new ElementSearchResult()
                });
            }
        }

    }
}