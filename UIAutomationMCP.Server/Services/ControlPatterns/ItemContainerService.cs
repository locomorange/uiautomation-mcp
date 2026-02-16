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
    public class ItemContainerService : BaseUIAutomationService<ItemContainerServiceMetadata>, IItemContainerService
    {
        public ItemContainerService(IProcessManager processManager, ILogger<ItemContainerService> logger)
            : base(processManager, logger)
        {
        }

        protected override string GetOperationType() => "itemContainer";

        public async Task<ServerEnhancedResponse<FindItemResult>> FindItemByPropertyAsync(
            string? automationId = null,
            string? name = null,
            string? propertyName = null,
            string? value = null,
            string? startAfterId = null,
            string? controlType = null,
            long? windowHandle = null,
            int timeoutSeconds = 30)
        {
            var request = new FindItemByPropertyRequest
            {
                AutomationId = automationId,
                Name = name,
                PropertyName = propertyName ?? "",
                Value = value ?? "",
                StartAfterId = startAfterId ?? "",
                ControlType = controlType,
                WindowHandle = windowHandle,
            };

            return await ExecuteServiceOperationAsync<FindItemByPropertyRequest, FindItemResult>(
                "FindItemByProperty",
                request,
                nameof(FindItemByPropertyAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        private static ValidationResult ValidateElementIdentificationRequest<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T request) where T : class
        {
            var automationIdProp = typeof(T).GetProperty("AutomationId");
            var nameProp = typeof(T).GetProperty("Name");

            var automationId = automationIdProp?.GetValue(request) as string;
            var name = nameProp?.GetValue(request) as string;

            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                return ValidationResult.Failure("Either AutomationId or Name is required to identify the container element");
            }

            return ValidationResult.Success;
        }

        protected override ItemContainerServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);

            if (data is FindItemResult findResult)
            {
                metadata.OperationSuccessful = findResult.Success;
                metadata.ItemFound = findResult.FoundElement != null;
            }

            return metadata;
        }
    }
}
