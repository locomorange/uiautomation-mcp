using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.UIAutomation.Abstractions;
using UIAutomationMCP.UIAutomation.Helpers;
using UIAutomationMCP.UIAutomation.Services;

namespace UIAutomationMCP.Worker.Operations.Grid
{
    public class GetRowHeaderOperation : BaseUIAutomationOperation<GetRowHeaderRequest, ElementSearchResult>
    {
        public GetRowHeaderOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetRowHeaderOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Core.Validation.ValidationResult ValidateRequest(GetRowHeaderRequest request)
        {
            var baseResult = base.ValidateRequest(request);
            if (!baseResult.IsValid)
                return baseResult;

            if (request.Row < 0)
                return Core.Validation.ValidationResult.Failure("Row index must be non-negative");

            return Core.Validation.ValidationResult.Success;
        }

        protected override Task<ElementSearchResult> ExecuteOperationAsync(GetRowHeaderRequest request)
        {
            var requiredPattern = AutomationPatternHelper.GetAutomationPattern(request.RequiredPattern) ?? GridPattern.Pattern;
                
            var element = _elementFinderService.FindElement(
                automationId: request.AutomationId, 
                name: request.Name,
                controlType: request.ControlType,
                processId: request.ProcessId,
                requiredPattern: requiredPattern);
            
            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("Operation", null, "Element not found");
            }

            if (!element.TryGetCurrentPattern(GridPattern.Pattern, out var pattern) || pattern is not GridPattern gridPattern)
            {
                throw new UIAutomationInvalidOperationException("Operation", null, "GridPattern not supported");
            }

            // Check if row is within bounds
            if (request.Row >= gridPattern.Current.RowCount)
            {
                throw new UIAutomationInvalidOperationException("Operation", null, "Row index out of range");
            }

            // Try to get the first item in the specified row (assuming header is at column 0)
            var headerElement = gridPattern.GetItem(request.Row, 0);
            if (headerElement == null)
            {
                throw new UIAutomationElementNotFoundException("Operation", null, "No header element found at specified row");
            }

            var headerInfo = ElementInfoBuilder.CreateElementInfo(headerElement, includeDetails: true, _logger);

            var result = new ElementSearchResult
            {
                SearchCriteria = "Grid row header search"
            };
            result.Elements.Add(headerInfo);

            return Task.FromResult(result);
        }

    }
}