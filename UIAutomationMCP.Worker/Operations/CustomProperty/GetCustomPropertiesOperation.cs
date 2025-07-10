using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.CustomProperty
{
    public class GetCustomPropertiesOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetCustomPropertiesOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var propertyNames = request.Parameters?.GetValueOrDefault("propertyNames") as string[] ?? Array.Empty<string>();

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element not found" });

            try
            {
                var customProperties = new Dictionary<string, object>();
                var allProperties = new Dictionary<string, object>();

                // Get all standard automation properties
                var standardProperties = GetStandardProperties(element);
                foreach (var prop in standardProperties)
                {
                    allProperties[prop.Key] = prop.Value;
                }

                // Try to get custom properties by name if specified
                if (propertyNames.Length > 0)
                {
                    foreach (var propertyName in propertyNames)
                    {
                        try
                        {
                            // This is a simplified approach - in reality, custom properties
                            // would need to be registered with AutomationProperty.Register()
                            // and retrieved using element.GetCurrentPropertyValue()
                            
                            // For now, we'll check if the property name matches any known property
                            if (allProperties.ContainsKey(propertyName))
                            {
                                customProperties[propertyName] = allProperties[propertyName];
                            }
                            else
                            {
                                // Try to get property by reflection or known property IDs
                                var propertyValue = TryGetPropertyByName(element, propertyName);
                                if (propertyValue != null)
                                {
                                    customProperties[propertyName] = propertyValue;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            customProperties[propertyName] = $"Error: {ex.Message}";
                        }
                    }
                }
                else
                {
                    // If no specific properties requested, return all available properties
                    customProperties = allProperties;
                }

                // Try to get framework-specific properties
                var frameworkProperties = GetFrameworkSpecificProperties(element);
                foreach (var prop in frameworkProperties)
                {
                    customProperties[$"Framework.{prop.Key}"] = prop.Value;
                }

                var result = new Dictionary<string, object>
                {
                    ["ElementId"] = elementId,
                    ["FrameworkId"] = element.Current.FrameworkId,
                    ["Properties"] = customProperties,
                    ["PropertyCount"] = customProperties.Count,
                    ["AvailableProperties"] = allProperties.Keys.ToList()
                };

                return Task.FromResult(new OperationResult { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error getting custom properties: {ex.Message}" });
            }
        }

        private Dictionary<string, object> GetStandardProperties(AutomationElement element)
        {
            var properties = new Dictionary<string, object>();

            try
            {
                // Basic properties
                properties["Name"] = element.Current.Name ?? "";
                properties["AutomationId"] = element.Current.AutomationId ?? "";
                properties["ControlType"] = element.Current.ControlType.LocalizedControlType ?? "";
                properties["LocalizedControlType"] = element.Current.LocalizedControlType ?? "";
                properties["ClassName"] = element.Current.ClassName ?? "";
                properties["FrameworkId"] = element.Current.FrameworkId ?? "";
                properties["ProcessId"] = element.Current.ProcessId;
                properties["IsEnabled"] = element.Current.IsEnabled;
                properties["IsVisible"] = !element.Current.IsOffscreen;
                properties["IsKeyboardFocusable"] = element.Current.IsKeyboardFocusable;
                properties["HasKeyboardFocus"] = element.Current.HasKeyboardFocus;
                properties["IsContentElement"] = element.Current.IsContentElement;
                properties["IsControlElement"] = element.Current.IsControlElement;
                properties["HelpText"] = element.Current.HelpText ?? "";
                properties["AcceleratorKey"] = element.Current.AcceleratorKey ?? "";
                properties["AccessKey"] = element.Current.AccessKey ?? "";
                properties["ItemStatus"] = element.Current.ItemStatus ?? "";
                properties["ItemType"] = element.Current.ItemType ?? "";
                properties["Orientation"] = element.Current.Orientation.ToString();
                properties["BoundingRectangle"] = new
                {
                    X = element.Current.BoundingRectangle.X,
                    Y = element.Current.BoundingRectangle.Y,
                    Width = element.Current.BoundingRectangle.Width,
                    Height = element.Current.BoundingRectangle.Height
                };
            }
            catch (Exception ex)
            {
                properties["Error"] = $"Error getting standard properties: {ex.Message}";
            }

            return properties;
        }

        private object? TryGetPropertyByName(AutomationElement element, string propertyName)
        {
            try
            {
                // This is a simplified implementation
                // In a real scenario, you would need to map property names to AutomationProperty instances
                switch (propertyName.ToLower())
                {
                    case "runtimeid":
                        return element.Current.RuntimeId;
                    case "nativewndowhandle":
                        return element.Current.NativeWindowHandle;
                    case "culture":
                        return element.Current.Culture;
                    case "ispassword":
                        return element.Current.IsPassword;
                    case "isrequiredforform":
                        return element.Current.IsRequiredForForm;
                    case "clickablepoint":
                        return element.Current.ClickablePoint;
                    default:
                        return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private Dictionary<string, object> GetFrameworkSpecificProperties(AutomationElement element)
        {
            var properties = new Dictionary<string, object>();

            try
            {
                var frameworkId = element.Current.FrameworkId;
                
                switch (frameworkId)
                {
                    case "WPF":
                        properties["WPF.Type"] = "Windows Presentation Foundation";
                        break;
                    case "WinForm":
                        properties["WinForm.Type"] = "Windows Forms";
                        break;
                    case "Win32":
                        properties["Win32.Type"] = "Win32 Native";
                        break;
                    case "DirectUI":
                        properties["DirectUI.Type"] = "Direct UI";
                        break;
                    default:
                        properties["Framework.Type"] = frameworkId ?? "Unknown";
                        break;
                }

                // Try to get additional framework-specific information
                if (element.Current.NativeWindowHandle != IntPtr.Zero)
                {
                    properties["NativeWindowHandle"] = element.Current.NativeWindowHandle.ToString();
                }
            }
            catch (Exception ex)
            {
                properties["FrameworkError"] = ex.Message;
            }

            return properties;
        }
    }
}