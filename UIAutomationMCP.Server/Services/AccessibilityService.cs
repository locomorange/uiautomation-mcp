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
    public class AccessibilityService : BaseUIAutomationService<AccessibilityServiceMetadata>, IAccessibilityService
    {
        public AccessibilityService(IProcessManager processManager, ILogger<AccessibilityService> logger)
            : base(processManager, logger)
        {
        }

        protected override string GetOperationType() => "accessibility";

        public async Task<ServerEnhancedResponse<ElementSearchResult>> VerifyAccessibilityAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 60)
        {
            var request = new VerifyAccessibilityRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                WindowHandle = windowHandle
            };

            return await ExecuteServiceOperationAsync<VerifyAccessibilityRequest, ElementSearchResult>(
                "VerifyAccessibility",
                request,
                nameof(VerifyAccessibilityAsync),
                timeoutSeconds
            );
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> GetLabeledByAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30)
        {
            var request = new GetLabeledByRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                WindowHandle = windowHandle
            };

            return await ExecuteServiceOperationAsync<GetLabeledByRequest, ElementSearchResult>(
                "GetLabeledBy",
                request,
                nameof(GetLabeledByAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> GetDescribedByAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30)
        {
            var request = new GetDescribedByRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                WindowHandle = windowHandle
            };

            return await ExecuteServiceOperationAsync<GetDescribedByRequest, ElementSearchResult>(
                "GetDescribedBy",
                request,
                nameof(GetDescribedByAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> GetAccessibilityInfoAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30)
        {
            var request = new GetAccessibilityInfoRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                WindowTitle = "",
                WindowHandle = windowHandle
            };

            return await ExecuteServiceOperationAsync<GetAccessibilityInfoRequest, ElementSearchResult>(
                "GetAccessibilityInfo",
                request,
                nameof(GetAccessibilityInfoAsync),
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

        protected override AccessibilityServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);

            if (data is ElementSearchResult searchResult)
            {
                if (context.MethodName.Contains("VerifyAccessibility"))
                {
                    metadata.ActionPerformed = "accessibilityVerified";
                    metadata.ElementsFound = searchResult.Count;
                    metadata.VerificationSuccessful = searchResult.Count > 0;
                }
                else if (context.MethodName.Contains("GetLabeledBy"))
                {
                    metadata.ActionPerformed = "labeledByRetrieved";
                    metadata.ElementsFound = searchResult.Count;
                }
                else if (context.MethodName.Contains("GetDescribedBy"))
                {
                    metadata.ActionPerformed = "describedByRetrieved";
                    metadata.ElementsFound = searchResult.Count;
                }
                else if (context.MethodName.Contains("GetAccessibilityInfo"))
                {
                    metadata.ActionPerformed = "accessibilityPropertiesRetrieved";
                    metadata.ElementsFound = searchResult.Count;
                    // Estimate properties count based on elements found
                    metadata.PropertiesCount = searchResult.Items?.Count ?? 0;
                }

                metadata.VerificationSuccessful = searchResult.Count > 0;
            }

            return metadata;
        }
    }
}