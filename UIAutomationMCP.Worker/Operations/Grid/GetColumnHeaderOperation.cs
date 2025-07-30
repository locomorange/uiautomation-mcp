using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.Common.Abstractions;
using UIAutomationMCP.Common.Helpers;
using UIAutomationMCP.Common.Services;

namespace UIAutomationMCP.Worker.Operations.Grid
{
    public class GetColumnHeaderOperation : BaseUIAutomationOperation<GetColumnHeaderRequest, ElementSearchResult>
    {
        public GetColumnHeaderOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetColumnHeaderOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Core.Validation.ValidationResult ValidateRequest(GetColumnHeaderRequest request)
        {
            var baseResult = base.ValidateRequest(request);
            if (!baseResult.IsValid)
                return baseResult;

            if (request.Column < 0)
                return Core.Validation.ValidationResult.Failure("Column index must be non-negative");

            return Core.Validation.ValidationResult.Success;
        }

        protected override Task<ElementSearchResult> ExecuteOperationAsync(GetColumnHeaderRequest request)
        {
            var requiredPattern = AutomationPatternHelper.GetAutomationPattern(request.RequiredPattern) ?? GridPattern.Pattern;
                
            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
                RequiredPattern = requiredPattern?.ProgrammaticName,
            , WindowHandle = request.WindowHandle };
            var element = _elementFinderService.FindElement(searchCriteria);
            
            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("Operation", null, "Element not found");
            }

            if (!element.TryGetCurrentPattern(GridPattern.Pattern, out var pattern) || pattern is not GridPattern gridPattern)
            {
                throw new UIAutomationInvalidOperationException("Operation", null, "GridPattern not supported");
            }

            // Check if column is within bounds
            if (request.Column >= gridPattern.Current.ColumnCount)
            {
                throw new UIAutomationInvalidOperationException("Operation", null, "Column index out of range");
            }

            // Try to get the first item in the specified column (assuming header is at row 0)
            var headerElement = gridPattern.GetItem(0, request.Column);
            if (headerElement == null)
            {
                throw new UIAutomationElementNotFoundException("Operation", null, "No header element found at specified column");
            }

            var headerInfo = ElementInfoBuilder.CreateElementInfo(headerElement, includeDetails: true, _logger);

            var result = new ElementSearchResult
            {
                SearchCriteria = "Grid column header search"
            };
            result.Elements.Add(headerInfo);

            return Task.FromResult(result);
        }

    }
}