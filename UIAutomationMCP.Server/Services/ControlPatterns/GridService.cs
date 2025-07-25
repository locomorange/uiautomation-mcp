using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Core.Abstractions;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Validation;
using UIAutomationMCP.Models.Metadata;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class GridService : BaseUIAutomationService<GridServiceMetadata>, IGridService
    {
        public GridService(IOperationExecutor executor, ILogger<GridService> logger)
            : base(executor, logger)
        {
        }

        protected override string GetOperationType() => "grid";

        public async Task<ServerEnhancedResponse<ElementSearchResult>> GetGridItemAsync(
            string? automationId = null, 
            string? name = null, 
            int row = 0, 
            int column = 0, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new GetGridItemRequest
            {
                AutomationId = automationId,
                Name = name,
                Row = row,
                Column = column,
                ControlType = controlType,
                ProcessId = processId ?? 0
            };

            return await ExecuteServiceOperationAsync<GetGridItemRequest, ElementSearchResult>(
                "GetGridItem",
                request,
                nameof(GetGridItemAsync),
                timeoutSeconds,
                ValidateGridItemRequest
            );
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> GetRowHeaderAsync(
            string? automationId = null, 
            string? name = null, 
            int row = 0, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new GetRowHeaderRequest
            {
                AutomationId = automationId,
                Name = name,
                Row = row,
                ControlType = controlType,
                ProcessId = processId ?? 0
            };

            return await ExecuteServiceOperationAsync<GetRowHeaderRequest, ElementSearchResult>(
                "GetRowHeader",
                request,
                nameof(GetRowHeaderAsync),
                timeoutSeconds,
                ValidateRowHeaderRequest
            );
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> GetColumnHeaderAsync(
            string? automationId = null, 
            string? name = null, 
            int column = 0, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new GetColumnHeaderRequest
            {
                AutomationId = automationId,
                Name = name,
                Column = column,
                ControlType = controlType,
                ProcessId = processId ?? 0
            };

            return await ExecuteServiceOperationAsync<GetColumnHeaderRequest, ElementSearchResult>(
                "GetColumnHeader",
                request,
                nameof(GetColumnHeaderAsync),
                timeoutSeconds,
                ValidateColumnHeaderRequest
            );
        }

        public async Task<ServerEnhancedResponse<GridInfoResult>> GetGridInfoAsync(
            string? automationId = null, 
            string? name = null, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new GetGridInfoRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                ProcessId = processId ?? 0
            };

            return await ExecuteServiceOperationAsync<GetGridInfoRequest, GridInfoResult>(
                "GetGridInfo",
                request,
                nameof(GetGridInfoAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        private static ValidationResult ValidateGridItemRequest(GetGridItemRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                errors.Add("Either AutomationId or Name is required for element identification");
            }

            if (request.Row < 0 || request.Column < 0)
            {
                errors.Add("Row and column indices must be non-negative");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        private static ValidationResult ValidateRowHeaderRequest(GetRowHeaderRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                errors.Add("Either AutomationId or Name is required for element identification");
            }

            if (request.Row < 0)
            {
                errors.Add("Row index must be non-negative");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        private static ValidationResult ValidateColumnHeaderRequest(GetColumnHeaderRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                errors.Add("Either AutomationId or Name is required for element identification");
            }

            if (request.Column < 0)
            {
                errors.Add("Column index must be non-negative");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        private static ValidationResult ValidateElementIdentificationRequest<T>(T request) where T : class
        {
            // Use reflection to check common properties for element identification
            var automationIdProp = typeof(T).GetProperty("AutomationId");
            var nameProp = typeof(T).GetProperty("Name");

            var automationId = automationIdProp?.GetValue(request) as string;
            var name = nameProp?.GetValue(request) as string;

            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                return ValidationResult.Failure("Either AutomationId or Name is required for element identification");
            }

            return ValidationResult.Success;
        }

        protected override GridServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);

            if (data is ElementSearchResult searchResult)
            {
                metadata.ElementsFound = searchResult.Count;
                metadata.OperationSuccessful = searchResult.Count > 0;

                if (context.MethodName.Contains("GetGridItem"))
                {
                    metadata.ActionPerformed = "gridItemRetrieved";
                    // Extract row/column from request if needed (would require request context)
                }
                else if (context.MethodName.Contains("GetRowHeader"))
                {
                    metadata.ActionPerformed = "rowHeaderRetrieved";
                    // Extract row from request if needed
                }
                else if (context.MethodName.Contains("GetColumnHeader"))
                {
                    metadata.ActionPerformed = "columnHeaderRetrieved";
                    // Extract column from request if needed
                }
            }
            else if (data is GridInfoResult gridResult)
            {
                metadata.ActionPerformed = "gridInfoRetrieved";
                metadata.TotalRows = gridResult.RowCount;
                metadata.TotalColumns = gridResult.ColumnCount;
                metadata.SupportsRowHeaders = gridResult.CanSelectMultiple;
                metadata.SupportsColumnHeaders = gridResult.CanSelectMultiple;
                metadata.OperationSuccessful = gridResult.Success;
            }

            return metadata;
        }
    }
}