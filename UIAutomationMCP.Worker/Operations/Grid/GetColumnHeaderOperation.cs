using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.UIAutomation.Abstractions;
using UIAutomationMCP.UIAutomation.Services;

namespace UIAutomationMCP.Worker.Operations.Grid
{
    public class GetColumnHeaderOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetColumnHeaderOperation> _logger;

        public GetColumnHeaderOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetColumnHeaderOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<GetColumnHeaderRequest>(parametersJson)!;
                
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

                // Check if column is within bounds
                if (typedRequest.Column >= gridPattern.Current.ColumnCount)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Column index out of range",
                        Data = new ElementSearchResult()
                    });
                }

                // Try to get the first item in the specified column (assuming header is at row 0)
                var headerElement = gridPattern.GetItem(0, typedRequest.Column);
                if (headerElement == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "No header element found at specified column",
                        Data = new ElementSearchResult()
                    });
                }

                var headerInfo = ElementInfoBuilder.CreateElementInfo(headerElement, includeDetails: true, _logger);

                var result = new ElementSearchResult
                {
                    SearchCriteria = "Grid column header search"
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
                _logger.LogError(ex, "GetColumnHeader operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get column header: {ex.Message}",
                    Data = new ElementSearchResult()
                });
            }
        }

    }
}