using System.Windows.Automation;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.Subprocess.Core.Abstractions;
using UIAutomationMCP.Subprocess.Core.Services;
using UIAutomationMCP.Subprocess.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Subprocess.Worker.Operations.ItemContainer
{
    /// <summary>
    /// Finds an item within a container using ItemContainerPattern.FindItemByProperty.
    /// Useful for efficiently searching virtualized collections (DataGrid, ListView, etc.)
    /// </summary>
    public class FindItemByPropertyOperation : BaseUIAutomationOperation<FindItemByPropertyRequest, FindItemResult>
    {
        public FindItemByPropertyOperation(ElementFinderService elementFinderService, ILogger<FindItemByPropertyOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Task<FindItemResult> ExecuteOperationAsync(FindItemByPropertyRequest request)
        {
            // Find the container element
            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
                RequiredPattern = ItemContainerPattern.Pattern.ProgrammaticName,
                WindowHandle = request.WindowHandle
            };
            var containerElement = _elementFinderService.FindElement(searchCriteria);

            if (containerElement == null)
            {
                var elementIdentifier = !string.IsNullOrWhiteSpace(request.AutomationId) ? request.AutomationId : request.Name ?? "unknown";
                throw new UIAutomationElementNotFoundException("FindItemByProperty", elementIdentifier);
            }

            if (!containerElement.TryGetCurrentPattern(ItemContainerPattern.Pattern, out var patternObj) || patternObj is not ItemContainerPattern itemContainerPattern)
            {
                var elementIdentifier = !string.IsNullOrWhiteSpace(request.AutomationId) ? request.AutomationId : request.Name ?? "unknown";
                throw new UIAutomationInvalidOperationException("FindItemByProperty", elementIdentifier, "ItemContainerPattern not supported");
            }

            // Resolve the property to search by
            AutomationProperty? searchProperty = ResolveProperty(request.PropertyName);

            // Resolve startAfter element if specified
            AutomationElement? startAfter = null;
            if (!string.IsNullOrWhiteSpace(request.StartAfterId))
            {
                var startAfterCriteria = new ElementSearchCriteria
                {
                    AutomationId = request.StartAfterId,
                    WindowHandle = request.WindowHandle
                };
                startAfter = _elementFinderService.FindElement(startAfterCriteria);
            }

            // Execute FindItemByProperty
            var foundElement = itemContainerPattern.FindItemByProperty(
                startAfter,
                searchProperty,
                string.IsNullOrEmpty(request.Value) ? null : request.Value);

            if (foundElement == null)
            {
                return Task.FromResult(new FindItemResult
                {
                    Success = true,
                    FoundElement = null,
                    SearchText = request.Value,
                    TotalMatches = 0,
                    OperationName = "FindItemByProperty"
                });
            }

            // If the found element supports VirtualizedItemPattern, realize it
            try
            {
                if (foundElement.TryGetCurrentPattern(VirtualizedItemPattern.Pattern, out var virtualizedObj) && virtualizedObj is VirtualizedItemPattern virtualizedPattern)
                {
                    virtualizedPattern.Realize();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to realize virtualized item after FindItemByProperty");
            }

            var elementInfo = ElementInfoBuilder.CreateElementInfo(foundElement, includeDetails: false, _logger);

            return Task.FromResult(new FindItemResult
            {
                Success = true,
                FoundElement = elementInfo,
                SearchText = request.Value,
                TotalMatches = 1,
                OperationName = "FindItemByProperty"
            });
        }

        protected override UIAutomationMCP.Core.Validation.ValidationResult ValidateRequest(FindItemByPropertyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("Either AutomationId or Name is required to identify the container element");
            }

            return UIAutomationMCP.Core.Validation.ValidationResult.Success;
        }

        /// <summary>
        /// Resolves a property name string to an AutomationProperty.
        /// Returns null to search all properties.
        /// </summary>
        private static AutomationProperty? ResolveProperty(string? propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return null;

            return propertyName.ToLowerInvariant() switch
            {
                "name" => AutomationElement.NameProperty,
                "automationid" => AutomationElement.AutomationIdProperty,
                "classname" or "class" => AutomationElement.ClassNameProperty,
                "controltype" => AutomationElement.ControlTypeProperty,
                "isenabled" => AutomationElement.IsEnabledProperty,
                "isoffscreen" => AutomationElement.IsOffscreenProperty,
                "helptext" => AutomationElement.HelpTextProperty,
                "acceleratorkey" => AutomationElement.AcceleratorKeyProperty,
                "accesskey" => AutomationElement.AccessKeyProperty,
                "frameworkid" => AutomationElement.FrameworkIdProperty,
                "itemstatus" => AutomationElement.ItemStatusProperty,
                "itemtype" => AutomationElement.ItemTypeProperty,
                _ => null // null = search all properties
            };
        }
    }
}
