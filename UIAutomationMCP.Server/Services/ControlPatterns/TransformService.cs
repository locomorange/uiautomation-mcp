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
    public class TransformService : BaseUIAutomationService<TransformServiceMetadata>, ITransformService
    {
        public TransformService(IProcessManager processManager, ILogger<TransformService> logger)
            : base(processManager, logger)
        {
        }

        protected override string GetOperationType() => "transform";

        public async Task<ServerEnhancedResponse<ActionResult>> MoveElementAsync(string? automationId = null, string? name = null, double x = 0, double y = 0, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30)
        {
            var request = new MoveElementRequest
            {
                AutomationId = automationId,
                Name = name,
                X = x,
                Y = y,
                ControlType = controlType,
                WindowHandle = windowHandle
            };

            return await ExecuteServiceOperationAsync<MoveElementRequest, ActionResult>(
                "MoveElement",
                request,
                nameof(MoveElementAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        public async Task<ServerEnhancedResponse<ActionResult>> ResizeElementAsync(string? automationId = null, string? name = null, double width = 100, double height = 100, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30)
        {
            var request = new ResizeElementRequest
            {
                AutomationId = automationId,
                Name = name,
                Width = width,
                Height = height,
                ControlType = controlType,
                WindowHandle = windowHandle
            };

            return await ExecuteServiceOperationAsync<ResizeElementRequest, ActionResult>(
                "ResizeElement",
                request,
                nameof(ResizeElementAsync),
                timeoutSeconds,
                ValidateResizeElementRequest
            );
        }

        public async Task<ServerEnhancedResponse<ActionResult>> RotateElementAsync(string? automationId = null, string? name = null, double degrees = 0, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30)
        {
            var request = new RotateElementRequest
            {
                AutomationId = automationId,
                Name = name,
                Degrees = degrees,
                ControlType = controlType,
                WindowHandle = windowHandle
            };

            return await ExecuteServiceOperationAsync<RotateElementRequest, ActionResult>(
                "RotateElement",
                request,
                nameof(RotateElementAsync),
                timeoutSeconds,
                ValidateElementIdentificationRequest
            );
        }

        public async Task<ServerEnhancedResponse<TransformCapabilitiesResult>> GetTransformCapabilitiesAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30)
        {
            var request = new GetTransformCapabilitiesRequest
            {
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                WindowHandle = windowHandle
            };

            return await ExecuteServiceOperationAsync<GetTransformCapabilitiesRequest, TransformCapabilitiesResult>(
                "GetTransformCapabilities",
                request,
                nameof(GetTransformCapabilitiesAsync),
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

        private static ValidationResult ValidateResizeElementRequest(ResizeElementRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                errors.Add("Either AutomationId or Name is required for element identification");
            }

            if (request.Width <= 0 || request.Height <= 0)
            {
                errors.Add("Width and height must be positive values");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        protected override TransformServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);

            if (data is ActionResult actionResult)
            {
                metadata.OperationSuccessful = actionResult.Success;

                if (context.MethodName.Contains("Move"))
                {
                    metadata.ActionPerformed = "elementMoved";
                }
                else if (context.MethodName.Contains("Resize"))
                {
                    metadata.ActionPerformed = "elementResized";
                }
                else if (context.MethodName.Contains("Rotate"))
                {
                    metadata.ActionPerformed = "elementRotated";
                }
            }
            else if (data is TransformCapabilitiesResult capabilitiesResult)
            {
                metadata.OperationSuccessful = capabilitiesResult.Success;
                metadata.ActionPerformed = "capabilitiesRetrieved";
            }

            return metadata;
        }
    }
}
