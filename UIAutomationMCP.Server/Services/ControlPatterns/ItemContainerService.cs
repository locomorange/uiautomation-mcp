using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Core.Abstractions;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Validation;
using UIAutomationMCP.Models.Metadata;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class ItemContainerService : BaseUIAutomationService<ItemContainerServiceMetadata>, IItemContainerService
    {
        public ItemContainerService(IOperationExecutor executor, ILogger<ItemContainerService> logger)
            : base(executor, logger)
        {
        }

        protected override string GetOperationType() => "itemContainer";

        public async Task<ServerEnhancedResponse<ElementSearchResult>> FindItemByPropertyAsync(string? automationId = null, string? name = null, string? propertyName = null, string? value = null, string? startAfterId = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var request = new FindItemByPropertyRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                PropertyName = propertyName ?? "",
                Value = value ?? "",
                StartAfterId = startAfterId ?? "",
                ProcessId = processId ?? 0
            };

            return await ExecuteServiceOperationAsync<FindItemByPropertyRequest, ElementSearchResult>(
                "FindItemByProperty",
                request,
                nameof(FindItemByPropertyAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
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

        protected override ItemContainerServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);

            if (data is ElementSearchResult searchResult)
            {
                metadata.OperationSuccessful = searchResult.Success;
                metadata.ItemsFound = searchResult.Elements?.Count ?? 0;
            }

            return metadata;
        }
    }
}