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
    public class SynchronizedInputService : BaseUIAutomationService<SynchronizedInputServiceMetadata>, ISynchronizedInputService
    {
        public SynchronizedInputService(IProcessManager processManager, ILogger<SynchronizedInputService> logger)
            : base(processManager, logger)
        {
        }

        protected override string GetOperationType() => "synchronizedInput";

        public async Task<ServerEnhancedResponse<ElementSearchResult>> StartListeningAsync(string? automationId = null, string? name = null, string inputType = "", string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30)
        {
            var request = new StartSynchronizedInputRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                InputType = inputType,
                WindowHandle = windowHandle
            };

            return await ExecuteServiceOperationAsync<StartSynchronizedInputRequest, ElementSearchResult>(
                "StartSynchronizedInput",
                request,
                nameof(StartListeningAsync),
                timeoutSeconds,
                ValidateStartListeningRequest
            );
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> CancelAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30)
        {
            var request = new CancelSynchronizedInputRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                WindowHandle = windowHandle
            };

            return await ExecuteServiceOperationAsync<CancelSynchronizedInputRequest, ElementSearchResult>(
                "CancelSynchronizedInput",
                request,
                nameof(CancelAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        private static ValidationResult ValidateStartListeningRequest(StartSynchronizedInputRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                errors.Add("Either AutomationId or Name is required for element identification");
            }

            if (string.IsNullOrWhiteSpace(request.InputType))
            {
                errors.Add("Input type is required and cannot be empty");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
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

        protected override SynchronizedInputServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);

            if (data is ElementSearchResult searchResult)
            {
                metadata.OperationSuccessful = searchResult.Success;

                if (context.MethodName.Contains("StartListening"))
                {
                    metadata.ActionPerformed = "inputSynchronized";
                }
                else if (context.MethodName.Contains("Cancel"))
                {
                    metadata.ActionPerformed = "inputCanceled";
                }
            }

            return metadata;
        }
    }
}