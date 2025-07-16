using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ItemContainer
{
    public class FindItemByPropertyOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public FindItemByPropertyOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<FindItemResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<FindItemByPropertyRequest>(_options);
            if (typedRequest == null)
            {
                return Task.FromResult(new OperationResult<FindItemResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format. Expected FindItemByPropertyRequest.",
                    Data = new FindItemResult { Found = false }
                });
            }
            
            var containerId = typedRequest.ContainerId;
            var propertyName = typedRequest.PropertyName;
            var value = typedRequest.Value;
            var startAfterId = typedRequest.StartAfterId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var container = _elementFinderService.FindElementById(containerId, windowTitle, processId);
            if (container == null)
                return Task.FromResult(new OperationResult<FindItemResult> 
                { 
                    Success = false, 
                    Error = "Container element not found",
                    Data = new FindItemResult { Found = false }
                });

            if (!container.TryGetCurrentPattern(ItemContainerPattern.Pattern, out var pattern) || pattern is not ItemContainerPattern itemContainerPattern)
                return Task.FromResult(new OperationResult<FindItemResult> 
                { 
                    Success = false, 
                    Error = "ItemContainerPattern not supported on this container",
                    Data = new FindItemResult { Found = false }
                });

            try
            {
                AutomationElement? startAfterElement = null;
                if (!string.IsNullOrEmpty(startAfterId))
                {
                    startAfterElement = _elementFinderService.FindElementById(startAfterId, windowTitle, processId);
                }

                AutomationElement? foundElement = null;
                AutomationProperty? property = null;
                object? propertyValue = null;

                // Determine which property to search by
                if (!string.IsNullOrEmpty(propertyName))
                {
                    property = GetAutomationProperty(propertyName);
                    if (property == null)
                    {
                        return Task.FromResult(new OperationResult<FindItemResult> 
                        { 
                            Success = false, 
                            Error = $"Unknown property name: {propertyName}",
                            Data = new FindItemResult { Found = false }
                        });
                    }

                    // Convert value to appropriate type
                    propertyValue = ConvertPropertyValue(property, value);
                }

                // Find item using the pattern
                foundElement = itemContainerPattern.FindItemByProperty(startAfterElement, property, propertyValue);

                var result = new FindItemResult
                {
                    Found = foundElement != null,
                    SearchDetails = new Dictionary<string, object>
                    {
                        ["ContainerId"] = containerId,
                        ["PropertyName"] = propertyName ?? "Any",
                        ["Value"] = value ?? "Any",
                        ["StartAfterId"] = startAfterId ?? "None"
                    }
                };

                if (foundElement != null)
                {
                    var elementInfo = _elementFinderService.GetElementBasicInfo(foundElement);
                    var bounds = foundElement.Current.BoundingRectangle;
                    
                    result.FoundElementId = elementInfo.AutomationId;
                    result.FoundElementName = elementInfo.Name;
                    result.FoundElementType = elementInfo.ControlType;
                    result.FoundElementBounds = bounds.IsEmpty ? null : new Dictionary<string, double>
                    {
                        ["left"] = bounds.Left,
                        ["top"] = bounds.Top,
                        ["width"] = bounds.Width,
                        ["height"] = bounds.Height
                    };
                }
                
                return Task.FromResult(new OperationResult<FindItemResult> 
                { 
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<FindItemResult> 
                { 
                    Success = false, 
                    Error = $"Failed to find item: {ex.Message}",
                    Data = new FindItemResult { Found = false }
                });
            }
        }

        private AutomationProperty? GetAutomationProperty(string propertyName)
        {
            return propertyName.ToLower() switch
            {
                "name" => AutomationElement.NameProperty,
                "automationid" => AutomationElement.AutomationIdProperty,
                "controltype" => AutomationElement.ControlTypeProperty,
                "classname" => AutomationElement.ClassNameProperty,
                "value" => ValuePattern.ValueProperty,
                "helptext" => AutomationElement.HelpTextProperty,
                "accesskey" => AutomationElement.AccessKeyProperty,
                "acceleratorkey" => AutomationElement.AcceleratorKeyProperty,
                "itemstatus" => AutomationElement.ItemStatusProperty,
                "itemtype" => AutomationElement.ItemTypeProperty,
                _ => null
            };
        }

        private object? ConvertPropertyValue(AutomationProperty property, string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            if (property == AutomationElement.ControlTypeProperty)
            {
                // Convert control type string to ControlType object
                return value.ToLower() switch
                {
                    "button" => ControlType.Button,
                    "checkbox" => ControlType.CheckBox,
                    "combobox" => ControlType.ComboBox,
                    "edit" => ControlType.Edit,
                    "list" => ControlType.List,
                    "listitem" => ControlType.ListItem,
                    "menu" => ControlType.Menu,
                    "menuitem" => ControlType.MenuItem,
                    "radiobutton" => ControlType.RadioButton,
                    "text" => ControlType.Text,
                    "tree" => ControlType.Tree,
                    "treeitem" => ControlType.TreeItem,
                    "window" => ControlType.Window,
                    _ => value
                };
            }

            return value;
        }

        Task<OperationResult> IUIAutomationOperation.ExecuteAsync(WorkerRequest request)
        {
            var typedResult = ExecuteAsync(request);
            return Task.FromResult(new OperationResult
            {
                Success = typedResult.Result.Success,
                Error = typedResult.Result.Error,
                Data = typedResult.Result.Data,
                ExecutionSeconds = typedResult.Result.ExecutionSeconds
            });
        }
    }
}