using UIAutomationMCP.Models.Abstractions;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Server.Abstractions;
using UIAutomationMCP.Core.Abstractions;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Validation;

namespace UIAutomationMCP.Server.Services
{
    public class TreeNavigationService : BaseUIAutomationService<TreeNavigationServiceMetadata>, ITreeNavigationService
    {
        public TreeNavigationService(IProcessManager processManager, ILogger<TreeNavigationService> logger)
            : base(processManager, logger)
        {
        }

        protected override string GetOperationType() => "treeNavigation";

        public async Task<ServerEnhancedResponse<TreeNavigationResult>> GetChildrenAsync(string elementId, int? processId = null, int timeoutSeconds = 30)
        {
            var request = new GetChildrenRequest
            {
                AutomationId = elementId,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<GetChildrenRequest, TreeNavigationResult>(
                "GetChildren",
                request,
                nameof(GetChildrenAsync),
                timeoutSeconds,
                ValidateGetChildrenRequest
            );
        }

        public async Task<ServerEnhancedResponse<TreeNavigationResult>> GetParentAsync(string elementId, int? processId = null, int timeoutSeconds = 30)
        {
            var request = new GetParentRequest
            {
                AutomationId = elementId,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<GetParentRequest, TreeNavigationResult>(
                "GetParent",
                request,
                nameof(GetParentAsync),
                timeoutSeconds,
                ValidateGetParentRequest
            );
        }

        public async Task<ServerEnhancedResponse<TreeNavigationResult>> GetSiblingsAsync(string elementId, int? processId = null, int timeoutSeconds = 30)
        {
            var request = new GetSiblingsRequest
            {
                AutomationId = elementId,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<GetSiblingsRequest, TreeNavigationResult>(
                "GetSiblings",
                request,
                nameof(GetSiblingsAsync),
                timeoutSeconds,
                ValidateGetSiblingsRequest
            );
        }

        public async Task<ServerEnhancedResponse<TreeNavigationResult>> GetDescendantsAsync(string elementId, int? processId = null, int timeoutSeconds = 30)
        {
            var request = new GetDescendantsRequest
            {
                AutomationId = elementId,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<GetDescendantsRequest, TreeNavigationResult>(
                "GetDescendants",
                request,
                nameof(GetDescendantsAsync),
                timeoutSeconds,
                ValidateGetDescendantsRequest
            );
        }

        public async Task<ServerEnhancedResponse<TreeNavigationResult>> GetAncestorsAsync(string elementId, int? processId = null, int timeoutSeconds = 30)
        {
            var request = new GetAncestorsRequest
            {
                AutomationId = elementId,
                ProcessId = processId
            };

            return await ExecuteServiceOperationAsync<GetAncestorsRequest, TreeNavigationResult>(
                "GetAncestors",
                request,
                nameof(GetAncestorsAsync),
                timeoutSeconds,
                ValidateGetAncestorsRequest
            );
        }

        public async Task<ServerEnhancedResponse<ElementTreeResult>> GetElementTreeAsync(int? processId = null, int maxDepth = 3, int timeoutSeconds = 60)
        {
            var request = new GetElementTreeRequest
            {
                ProcessId = processId,
                MaxDepth = maxDepth
            };

            return await ExecuteServiceOperationAsync<GetElementTreeRequest, ElementTreeResult>(
                "GetElementTree",
                request,
                nameof(GetElementTreeAsync),
                timeoutSeconds,
                ValidateGetElementTreeRequest
            );
        }

        private static ValidationResult ValidateGetChildrenRequest(GetChildrenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId))
            {
                return ValidationResult.Failure("Element ID is required and cannot be empty");
            }

            return ValidationResult.Success;
        }

        private static ValidationResult ValidateGetParentRequest(GetParentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId))
            {
                return ValidationResult.Failure("Element ID is required and cannot be empty");
            }

            return ValidationResult.Success;
        }

        private static ValidationResult ValidateGetSiblingsRequest(GetSiblingsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId))
            {
                return ValidationResult.Failure("Element ID is required and cannot be empty");
            }

            return ValidationResult.Success;
        }

        private static ValidationResult ValidateGetDescendantsRequest(GetDescendantsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId))
            {
                return ValidationResult.Failure("Element ID is required and cannot be empty");
            }

            return ValidationResult.Success;
        }

        private static ValidationResult ValidateGetAncestorsRequest(GetAncestorsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId))
            {
                return ValidationResult.Failure("Element ID is required and cannot be empty");
            }

            return ValidationResult.Success;
        }

        private static ValidationResult ValidateGetElementTreeRequest(GetElementTreeRequest request)
        {
            if (request.MaxDepth <= 0)
            {
                return ValidationResult.Failure("MaxDepth must be greater than 0");
            }

            return ValidationResult.Success;
        }

        protected override TreeNavigationServiceMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);
            metadata.SourceElementId = "extracted_from_request"; // Would need request context to get actual value

            if (data is TreeNavigationResult navResult)
            {
                if (context.MethodName.Contains("GetChildren"))
                {
                    metadata.ActionPerformed = "childrenRetrieved";
                    metadata.ElementsFound = navResult.Elements?.Count ?? 0;
                }
                else if (context.MethodName.Contains("GetParent"))
                {
                    metadata.ActionPerformed = "parentRetrieved";
                    metadata.ElementsFound = navResult.Elements?.Count ?? 0;
                }
                else if (context.MethodName.Contains("GetSiblings"))
                {
                    metadata.ActionPerformed = "siblingsRetrieved";
                    metadata.ElementsFound = navResult.Elements?.Count ?? 0;
                }
                else if (context.MethodName.Contains("GetDescendants"))
                {
                    metadata.ActionPerformed = "descendantsRetrieved";
                    metadata.ElementsFound = navResult.Elements?.Count ?? 0;
                }
                else if (context.MethodName.Contains("GetAncestors"))
                {
                    metadata.ActionPerformed = "ancestorsRetrieved";
                    metadata.ElementsFound = navResult.Elements?.Count ?? 0;
                }

                metadata.NavigationSuccessful = navResult.Elements?.Count > 0;
            }
            else if (data is ElementTreeResult treeResult)
            {
                metadata.ActionPerformed = "elementTreeRetrieved";
                metadata.ElementsFound = treeResult.TotalElements;
                metadata.NavigationSuccessful = treeResult.TotalElements > 0;
            }

            return metadata;
        }
    }
}