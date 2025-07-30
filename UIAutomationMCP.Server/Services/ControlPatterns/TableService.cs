using System.Diagnostics.CodeAnalysis;
using UIAutomationMCP.Models.Abstractions;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Server.Abstractions;
using UIAutomationMCP.Core.Abstractions;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Validation;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class TableService : BaseUIAutomationService<TableServiceMetadata>, ITableService
    {
        public TableService(IProcessManager processManager, ILogger<TableService> logger)
            : base(processManager, logger)
        {
        }

        protected override string GetOperationType() => "table";


        public async Task<ServerEnhancedResponse<ElementSearchResult>> GetRowHeadersAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30)
        {
            var request = new GetRowHeadersRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                WindowHandle = windowHandle
            };

            return await ExecuteServiceOperationAsync<GetRowHeadersRequest, ElementSearchResult>(
                "GetRowHeaders",
                request,
                nameof(GetRowHeadersAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> GetColumnHeadersAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30)
        {
            var request = new GetColumnHeadersRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                WindowHandle = windowHandle
            };

            return await ExecuteServiceOperationAsync<GetColumnHeadersRequest, ElementSearchResult>(
                "GetColumnHeaders",
                request,
                nameof(GetColumnHeadersAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }


        public async Task<ServerEnhancedResponse<ElementSearchResult>> GetColumnHeaderItemsAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30)
        {
            var request = new GetColumnHeaderItemsRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                WindowHandle = windowHandle
            };

            return await ExecuteServiceOperationAsync<GetColumnHeaderItemsRequest, ElementSearchResult>(
                "GetColumnHeaderItems",
                request,
                nameof(GetColumnHeaderItemsAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> GetRowHeaderItemsAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30)
        {
            var request = new GetRowHeaderItemsRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                WindowHandle = windowHandle
            };

            return await ExecuteServiceOperationAsync<GetRowHeaderItemsRequest, ElementSearchResult>(
                "GetRowHeaderItems",
                request,
                nameof(GetRowHeaderItemsAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        public async Task<ServerEnhancedResponse<ActionResult>> GetRowOrColumnMajorAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30)
        {
            var request = new GetTableInfoRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                WindowHandle = windowHandle
            };

            return await ExecuteServiceOperationAsync<GetTableInfoRequest, ActionResult>(
                "GetRowOrColumnMajor",
                request,
                nameof(GetRowOrColumnMajorAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        public async Task<ServerEnhancedResponse<TableInfoResult>> GetTableInfoAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30)
        {
            var request = new GetTableInfoRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                WindowHandle = windowHandle
            };

            return await ExecuteServiceOperationAsync<GetTableInfoRequest, TableInfoResult>(
                "GetTableInfo",
                request,
                nameof(GetTableInfoAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        private static ValidationResult ValidateElementIdentificationRequest<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T request) where T : class
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

        protected override TableServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);

            if (data is ElementSearchResult searchResult)
            {
                metadata.OperationSuccessful = searchResult.Success;
                metadata.ElementsFound = searchResult.Elements?.Count ?? 0;

                if (context.MethodName.Contains("RowHeaders"))
                {
                    metadata.ActionPerformed = "rowHeadersRetrieved";
                }
                else if (context.MethodName.Contains("ColumnHeaders"))
                {
                    metadata.ActionPerformed = "columnHeadersRetrieved";
                }
                else if (context.MethodName.Contains("RowHeaderItems"))
                {
                    metadata.ActionPerformed = "rowHeaderItemsRetrieved";
                }
                else if (context.MethodName.Contains("ColumnHeaderItems"))
                {
                    metadata.ActionPerformed = "columnHeaderItemsRetrieved";
                }
            }
            else if (data is ActionResult actionResult)
            {
                metadata.OperationSuccessful = actionResult.Success;
                metadata.ActionPerformed = "tablePropertyRetrieved";
            }
            else if (data is TableInfoResult tableResult)
            {
                metadata.OperationSuccessful = tableResult.Success;
                metadata.ActionPerformed = "tableInfoRetrieved";
            }

            return metadata;
        }
    }
}