using UIAutomationMCP.Models.Abstractions;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Server.Abstractions;
using UIAutomationMCP.Core.Abstractions;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Validation;
using System.Diagnostics.CodeAnalysis;

namespace UIAutomationMCP.Server.Services
{
    public class CustomPropertyService : BaseUIAutomationService<CustomPropertyServiceMetadata>, ICustomPropertyService
    {
        public CustomPropertyService(IProcessManager processManager, ILogger<CustomPropertyService> logger)
            : base(processManager, logger)
        {
        }

        protected override string GetOperationType() => "customProperty";

        public async Task<ServerEnhancedResponse<ElementSearchResult>> GetCustomPropertiesAsync(
            string? automationId = null, 
            string? name = null, 
            string[]? propertyIds = null, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new GetCustomPropertiesRequest
            {
                AutomationId = automationId,
                Name = name,
                PropertyIds = propertyIds ?? Array.Empty<string>(),
                ControlType = controlType,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<GetCustomPropertiesRequest, ElementSearchResult>(
                "GetCustomProperties",
                request,
                nameof(GetCustomPropertiesAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> SetCustomPropertyAsync(
            string? automationId = null, 
            string? name = null, 
            string propertyId = "", 
            object? value = null, 
            string? controlType = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new SetCustomPropertyRequest
            {
                AutomationId = automationId,
                Name = name,
                PropertyId = propertyId,
                Value = value?.ToString() ?? "",
                ControlType = controlType,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<SetCustomPropertyRequest, ElementSearchResult>(
                "SetCustomProperty",
                request,
                nameof(SetCustomPropertyAsync),
                timeoutSeconds,
                ValidateSetCustomPropertyRequest
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

        private static ValidationResult ValidateSetCustomPropertyRequest(SetCustomPropertyRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                errors.Add("Either AutomationId or Name is required for element identification");
            }

            if (string.IsNullOrWhiteSpace(request.PropertyId))
            {
                errors.Add("Property ID is required and cannot be empty");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        protected override CustomPropertyServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);

            if (data is ElementSearchResult searchResult)
            {
                metadata.ElementsFound = searchResult.Count;
                metadata.OperationSuccessful = searchResult.Count > 0;

                if (context.MethodName.Contains("GetCustomProperties"))
                {
                    metadata.ActionPerformed = "customPropertiesRetrieved";
                    // Count total properties - use estimate based on available data
                    metadata.PropertiesCount = searchResult.Items?.Count ?? 0;
                }
                else if (context.MethodName.Contains("SetCustomProperty"))
                {
                    metadata.ActionPerformed = "customPropertySet";
                    metadata.PropertiesCount = 1; // One property was set
                    // Property ID and value would need to be captured from the request context
                }
            }

            return metadata;
        }
    }
}