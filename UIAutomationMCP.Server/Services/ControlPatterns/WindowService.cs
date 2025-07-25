using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Core.Abstractions;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Core.Validation;
using UIAutomationMCP.Shared.Metadata;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class WindowService : BaseUIAutomationService<WindowServiceMetadata>, IWindowService
    {
        public WindowService(IOperationExecutor executor, ILogger<WindowService> logger)
            : base(executor, logger)
        {
        }

        protected override string GetOperationType() => "window";

        public async Task<ServerEnhancedResponse<ActionResult>> WindowOperationAsync(
            string operation, 
            string? windowTitle = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new WindowActionRequest
            {
                Action = operation,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<WindowActionRequest, ActionResult>(
                "WindowAction",
                request,
                nameof(WindowOperationAsync),
                timeoutSeconds,
                ValidateWindowOperationRequest
            );
        }

        public async Task<object> TransformElementAsync(
            string elementId, 
            string action, 
            double? x = null, 
            double? y = null, 
            double? width = null, 
            double? height = null, 
            string? windowTitle = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new TransformElementRequest
            {
                AutomationId = elementId,
                Action = action,
                X = x ?? 0,
                Y = y ?? 0,
                Width = width ?? 0,
                Height = height ?? 0,
                WindowTitle = windowTitle ?? "",
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<TransformElementRequest, ActionResult>(
                "TransformElement",
                request,
                nameof(TransformElementAsync),
                timeoutSeconds,
                ValidateTransformElementRequest
            );
        }

        public async Task<ServerEnhancedResponse<ActionResult>> SetWindowStateAsync(
            string windowState, 
            string? windowTitle = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new SetWindowStateRequest
            {
                WindowState = windowState,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<SetWindowStateRequest, ActionResult>(
                "SetWindowState",
                request,
                nameof(SetWindowStateAsync),
                timeoutSeconds,
                ValidateSetWindowStateRequest
            );
        }

        public async Task<ServerEnhancedResponse<ActionResult>> MoveWindowAsync(
            int x, 
            int y, 
            string? windowTitle = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new MoveWindowRequest
            {
                X = x,
                Y = y,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<MoveWindowRequest, ActionResult>(
                "MoveWindow",
                request,
                nameof(MoveWindowAsync),
                timeoutSeconds
            );
        }

        public async Task<ServerEnhancedResponse<ActionResult>> ResizeWindowAsync(
            int width, 
            int height, 
            string? windowTitle = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new ResizeWindowRequest
            {
                Width = width,
                Height = height,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<ResizeWindowRequest, ActionResult>(
                "ResizeWindow",
                request,
                nameof(ResizeWindowAsync),
                timeoutSeconds,
                ValidateResizeWindowRequest
            );
        }

        public async Task<ServerEnhancedResponse<BooleanResult>> WaitForInputIdleAsync(
            int timeoutMilliseconds = 10000, 
            string? windowTitle = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new WaitForInputIdleRequest
            {
                TimeoutMilliseconds = timeoutMilliseconds,
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<WaitForInputIdleRequest, BooleanResult>(
                "WaitForInputIdle",
                request,
                nameof(WaitForInputIdleAsync),
                timeoutSeconds,
                ValidateWaitForInputIdleRequest
            );
        }

        public async Task<ServerEnhancedResponse<WindowInteractionStateResult>> GetWindowInteractionStateAsync(
            string? windowTitle = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new GetWindowInfoRequest
            {
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<GetWindowInfoRequest, WindowInteractionStateResult>(
                "GetWindowInteractionState",
                request,
                nameof(GetWindowInteractionStateAsync),
                timeoutSeconds
            );
        }

        public async Task<ServerEnhancedResponse<WindowCapabilitiesResult>> GetWindowCapabilitiesAsync(
            string? windowTitle = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var request = new GetWindowInfoRequest
            {
                WindowTitle = windowTitle,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<GetWindowInfoRequest, WindowCapabilitiesResult>(
                "GetWindowCapabilities",
                request,
                nameof(GetWindowCapabilitiesAsync),
                timeoutSeconds
            );
        }

        private static ValidationResult ValidateWindowOperationRequest(WindowActionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Action))
            {
                return ValidationResult.Failure("Window operation is required and cannot be empty");
            }

            return ValidationResult.Success;
        }

        private static ValidationResult ValidateTransformElementRequest(TransformElementRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.AutomationId))
            {
                errors.Add("Element ID is required and cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(request.Action))
            {
                errors.Add("Transform action is required and cannot be empty");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        private static ValidationResult ValidateSetWindowStateRequest(SetWindowStateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.WindowState))
            {
                return ValidationResult.Failure("Window state is required and cannot be empty");
            }

            return ValidationResult.Success;
        }

        private static ValidationResult ValidateResizeWindowRequest(ResizeWindowRequest request)
        {
            var errors = new List<string>();

            if (request.Width <= 0)
            {
                errors.Add("Width must be greater than 0");
            }

            if (request.Height <= 0)
            {
                errors.Add("Height must be greater than 0");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        private static ValidationResult ValidateWaitForInputIdleRequest(WaitForInputIdleRequest request)
        {
            if (request.TimeoutMilliseconds <= 0)
            {
                return ValidationResult.Failure("Timeout milliseconds must be greater than 0");
            }

            return ValidationResult.Success;
        }

        protected override WindowServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);
            
            if (data is ActionResult)
            {
                metadata.ActionPerformed = context.MethodName.Replace("Async", "").ToLowerInvariant();
                
                // Extract size information for resize operations
                if (context.MethodName.Contains("Resize"))
                {
                    // Size information would need to be extracted from request context
                    // For now, we'll leave these null as we don't have easy access to the request data here
                }
                // Extract position information for move operations
                else if (context.MethodName.Contains("Move"))
                {
                    // Position information would need to be extracted from request context
                }
            }
            else if (data is BooleanResult boolResult)
            {
                metadata.ActionPerformed = "waitForInputIdle";
                metadata.InputIdleAchieved = boolResult.Value;
            }
            else if (data is WindowInteractionStateResult)
            {
                metadata.ActionPerformed = "windowInteractionStateRetrieved";
            }
            else if (data is WindowCapabilitiesResult)
            {
                metadata.ActionPerformed = "windowCapabilitiesRetrieved";
            }
            
            return metadata;
        }
    }
}