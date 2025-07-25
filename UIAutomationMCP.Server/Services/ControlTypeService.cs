using UIAutomationMCP.Models.Abstractions;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Server.Abstractions;
using UIAutomationMCP.Core.Abstractions;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Validation;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Server.Services
{
    public class ControlTypeService : BaseUIAutomationService<ControlTypeServiceMetadata>, IControlTypeService
    {
        public ControlTypeService(IProcessManager processManager, ILogger<ControlTypeService> logger)
            : base(processManager, logger)
        {
        }

        protected override string GetOperationType() => "controlType";

        public async Task<ServerEnhancedResponse<ElementSearchResult>> ValidateControlTypePatternsAsync(
            string? automationId = null, 
            string? name = null,
            string? controlType = null,
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new ValidateControlTypePatternsRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<ValidateControlTypePatternsRequest, ElementSearchResult>(
                "ValidateControlTypePatterns",
                request,
                nameof(ValidateControlTypePatternsAsync),
                timeoutSeconds,
                ValidateControlTypePatternsRequest
            );
        }

        private static ValidationResult ValidateControlTypePatternsRequest(ValidateControlTypePatternsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                return ValidationResult.Failure("Either AutomationId or Name is required for element identification");
            }

            return ValidationResult.Success;
        }

        protected override ControlTypeServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);

            if (data is ElementSearchResult searchResult)
            {
                metadata.ElementsFound = searchResult.Count;
                metadata.ValidationSuccessful = searchResult.Count > 0;
                
                // Extract control type from the first result if available
                if (searchResult.Items?.Count > 0)
                {
                    var firstItem = searchResult.Items[0];
                    metadata.ControlType = firstItem.ControlType;
                    metadata.SupportedPatternsCount = firstItem.SupportedPatterns?.Length ?? 0;
                }
            }

            return metadata;
        }
    }
}