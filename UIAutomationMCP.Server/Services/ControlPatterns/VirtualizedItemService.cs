using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Core.Abstractions;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Validation;
using UIAutomationMCP.Models.Metadata;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class VirtualizedItemService : BaseUIAutomationService<VirtualizedItemServiceMetadata>, IVirtualizedItemService
    {
        public VirtualizedItemService(IOperationExecutor executor, ILogger<VirtualizedItemService> logger)
            : base(executor, logger)
        {
        }

        protected override string GetOperationType() => "virtualizedItem";

        public async Task<ServerEnhancedResponse<ElementSearchResult>> RealizeItemAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var request = new RealizeVirtualizedItemRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                ProcessId = processId ?? 0
            };

            return await ExecuteServiceOperationAsync<RealizeVirtualizedItemRequest, ElementSearchResult>(
                "RealizeVirtualizedItem",
                request,
                nameof(RealizeItemAsync),
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

        protected override VirtualizedItemServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);

            if (data is ElementSearchResult searchResult)
            {
                metadata.OperationSuccessful = searchResult.Success;
                metadata.ItemRealized = searchResult.Success;
            }

            return metadata;
        }
    }
}