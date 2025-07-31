using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.Subprocess.Core.Abstractions;
using UIAutomationMCP.Subprocess.Core.Helpers;
using UIAutomationMCP.Subprocess.Core.Services;

namespace UIAutomationMCP.Subprocess.Worker.Operations.Grid
{
    public class GetRowHeaderOperation : BaseUIAutomationOperation<GetRowHeaderRequest, ElementSearchResult>
    {
        public GetRowHeaderOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetRowHeaderOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override UIAutomationMCP.Core.Validation.ValidationResult ValidateRequest(GetRowHeaderRequest request)
        {
            var baseResult = base.ValidateRequest(request);
            if (!baseResult.IsValid)
                return baseResult;

            if (request.Row < 0)
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("Row index must be non-negative");

            return UIAutomationMCP.Core.Validation.ValidationResult.Success;
        }

        protected override Task<ElementSearchResult> ExecuteOperationAsync(GetRowHeaderRequest request)
        {
            var requiredPattern = AutomationPatternHelper.GetAutomationPattern(request.RequiredPattern) ?? GridPattern.Pattern;
                
            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
                RequiredPattern = requiredPattern?.ProgrammaticName,
                WindowHandle = request.WindowHandle
            };
            var element = _elementFinderService.FindElement(searchCriteria);
            
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

