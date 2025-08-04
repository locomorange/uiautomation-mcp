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
    public class GetGridItemOperation : BaseUIAutomationOperation<GetGridItemRequest, GridItemResult>
    {
        public GetGridItemOperation(
            ElementFinderService elementFinderService,
            ILogger<GetGridItemOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override UIAutomationMCP.Core.Validation.ValidationResult ValidateRequest(GetGridItemRequest request)
        {
            var baseResult = base.ValidateRequest(request);
            if (!baseResult.IsValid)
                return baseResult;

            if (request.Row < 0)
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("Row index must be non-negative");

            if (request.Column < 0)
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("Column index must be non-negative");

            return UIAutomationMCP.Core.Validation.ValidationResult.Success;
        }

        protected override Task<GridItemResult> ExecuteOperationAsync(GetGridItemRequest request)
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

            var gridItem = gridPattern.GetItem(request.Row, request.Column);
            if (gridItem == null)
            {
                throw new UIAutomationElementNotFoundException("Operation", null, "Grid item not found");
            }

            // Get GridItem pattern info
            GridItemPattern? gridItemPattern = null;
            if (gridItem.TryGetCurrentPattern(GridItemPattern.Pattern, out var itemPattern))
            {
                gridItemPattern = itemPattern as GridItemPattern;
            }

            var result = new GridItemResult
            {
                Row = gridItemPattern?.Current.Row ?? request.Row,
                Column = gridItemPattern?.Current.Column ?? request.Column,
                RowSpan = gridItemPattern?.Current.RowSpan ?? 1,
                ColumnSpan = gridItemPattern?.Current.ColumnSpan ?? 1,
                ContainingGridId = gridItemPattern?.Current.ContainingGrid?.Current.AutomationId ?? string.Empty,
                Element = ElementInfoBuilder.CreateElementInfo(gridItem, includeDetails: true, _logger)
            };

            return Task.FromResult(result);
        }

    }
}

