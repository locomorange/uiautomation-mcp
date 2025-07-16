using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using UIAutomationMCP.Shared.Configuration;
using UIAutomationMCP.Worker.Services;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Operations.Invoke;
using UIAutomationMCP.Worker.Operations.Toggle;
using UIAutomationMCP.Worker.Operations.Value;
using UIAutomationMCP.Worker.Operations.ElementSearch;
using UIAutomationMCP.Worker.Operations.ControlTypeInfo;
using UIAutomationMCP.Worker.Operations.Grid;
using UIAutomationMCP.Worker.Operations.ElementInspection;
using UIAutomationMCP.Worker.Operations.Layout;
using UIAutomationMCP.Worker.Operations.MultipleView;
using UIAutomationMCP.Worker.Operations.Range;
using UIAutomationMCP.Worker.Operations.Selection;
using UIAutomationMCP.Worker.Operations.Table;
using UIAutomationMCP.Worker.Operations.Text;
using UIAutomationMCP.Worker.Operations.TreeNavigation;
using UIAutomationMCP.Worker.Operations.Window;
using UIAutomationMCP.Worker.Operations.Transform;
using UIAutomationMCP.Worker.Operations.VirtualizedItem;
using UIAutomationMCP.Worker.Operations.ItemContainer;
using UIAutomationMCP.Worker.Operations.SynchronizedInput;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Shared.Validation;
using System.Text.Json;

namespace UIAutomationMCP.Worker
{
    class Program
    {
        /// <summary>
        /// Check if UI Automation is available in the current environment
        /// This method performs a quick check and returns immediately on failure
        /// </summary>
        private static bool IsUIAutomationAvailable(out string errorReason)
        {
            errorReason = "";
            try
            {
                // Quick timeout-based check
                var task = Task.Run(() =>
                {
                    try
                    {
                        var rootElement = System.Windows.Automation.AutomationElement.RootElement;
                        if (rootElement == null)
                        {
                            return (false, "AutomationElement.RootElement returned null");
                        }
                        
                        // Try to get a basic property to ensure it's accessible
                        var name = rootElement.Current.Name;
                        return (true, "");
                    }
                    catch (TypeInitializationException ex)
                    {
                        return (false, $"UI Automation type initialization failed: {ex.Message}");
                    }
                    catch (System.ComponentModel.Win32Exception ex)
                    {
                        return (false, $"Win32 error: {ex.Message}");
                    }
                    catch (InvalidOperationException ex)
                    {
                        return (false, $"Invalid operation: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Unexpected error: {ex.Message}");
                    }
                });

                // Wait for maximum 2 seconds for UI Automation check
                if (task.Wait(2000))
                {
                    var (success, reason) = task.Result;
                    if (!success)
                    {
                        errorReason = reason;
                    }
                    return success;
                }
                else
                {
                    errorReason = "UI Automation initialization timed out after 2 seconds";
                    return false;
                }
            }
            catch (Exception ex)
            {
                errorReason = $"Failed to check UI Automation availability: {ex.Message}";
                return false;
            }
        }

        static async Task Main(string[] args)
        {
            // Early UI Automation availability check
            if (!IsUIAutomationAvailable(out string errorReason))
            {
                Console.Error.WriteLine($"UI Automation is not available in this environment: {errorReason}");
                Environment.Exit(1);
                return;
            }

            var builder = Host.CreateApplicationBuilder(args);

            // Configure Options Pattern using AOT-compatible smart binding
            builder.Services.AddSingleton<UIAutomationOptions>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                return UIAutomationOptionsBuilder.Build(configuration);
            });

            // Add custom validation
            builder.Services.AddSingleton<IValidateOptions<UIAutomationOptions>, UIAutomationOptionsValidator>();

            // Configure logging - disable console logging to avoid interference with JSON responses
            builder.Logging.ClearProviders();
            // Add file logging for debugging but avoid console output pollution
            builder.Logging.AddProvider(new DebugLoggerProvider());
            builder.Logging.SetMinimumLevel(LogLevel.Debug);

            // Register helper services
            builder.Services.AddSingleton<ElementFinderService>();
            builder.Services.AddSingleton<FindElementsCacheService>();

            // Register basic operations as keyed services (working ones only)
            builder.Services.AddKeyedTransient<IUIAutomationOperation, InvokeElementOperation>("InvokeElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, ToggleElementOperation>("ToggleElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetToggleStateOperation>("GetToggleState");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SetToggleStateOperation>("SetToggleState");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SetElementValueOperation>("SetElementValue");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetElementValueOperation>("GetElementValue");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, IsReadOnlyOperation>("IsReadOnly");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, FindElementsOperation>("FindElements");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetDesktopWindowsOperation>("GetDesktopWindows");


            // ControlTypeInfo operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetControlTypeInfoOperation>("GetControlTypeInfo");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, ValidateControlTypePatternsOperation>("ValidateControlTypePatterns");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, FindElementsByControlTypeOperation>("FindElementsByControlType");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, FindElementsByPatternOperation>("FindElementsByPattern");

            // Grid operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetColumnHeaderOperation>("GetColumnHeader");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetRowHeaderOperation>("GetRowHeader");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetGridInfoOperation>("GetGridInfo");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetGridItemOperation>("GetGridItem");

            // ElementInspection operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetElementPropertiesOperation>("GetElementProperties");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetElementPatternsOperation>("GetElementPatterns");

            // Layout operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, DockElementOperation>("DockElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, ExpandCollapseElementOperation>("ExpandCollapseElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, ScrollElementOperation>("ScrollElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, ScrollElementIntoViewOperation>("ScrollElementIntoView");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetScrollInfoOperation>("GetScrollInfo");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SetScrollPercentOperation>("SetScrollPercent");

            // MultipleView operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetAvailableViewsOperation>("GetAvailableViews");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetCurrentViewOperation>("GetCurrentView");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetViewNameOperation>("GetViewName");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SetViewOperation>("SetView");

            // Range operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetRangePropertiesOperation>("GetRangeProperties");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetRangeValueOperation>("GetRangeValue");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SetRangeValueOperation>("SetRangeValue");


            // Selection operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, AddToSelectionOperation>("AddToSelection");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, CanSelectMultipleOperation>("CanSelectMultiple");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, ClearSelectionOperation>("ClearSelection");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetSelectionContainerOperation>("GetSelectionContainer");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetSelectionOperation>("GetSelection");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, IsSelectedOperation>("IsSelected");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, IsSelectionRequiredOperation>("IsSelectionRequired");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, RemoveFromSelectionOperation>("RemoveFromSelection");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SelectElementOperation>("SelectElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SelectItemOperation>("SelectItem");

            // Table operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetColumnHeadersOperation>("GetColumnHeaders");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetRowHeadersOperation>("GetRowHeaders");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetTableInfoOperation>("GetTableInfo");

            // Text operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, AppendTextOperation>("AppendText");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, FindTextOperation>("FindText");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetSelectedTextOperation>("GetSelectedText");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetTextAttributesOperation>("GetTextAttributes");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetTextOperation>("GetText");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetTextSelectionOperation>("GetTextSelection");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SelectTextOperation>("SelectText");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SetTextOperation>("SetText");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, TraverseTextOperation>("TraverseText");

            // TreeNavigation operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetAncestorsOperation>("GetAncestors");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetChildrenOperation>("GetChildren");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetDescendantsOperation>("GetDescendants");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetElementTreeOperation>("GetElementTree");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetParentOperation>("GetParent");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetSiblingsOperation>("GetSiblings");

            // Window operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, TransformElementOperation>("TransformElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, WindowActionOperation>("WindowAction");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetWindowInfoOperation>("GetWindowInfo");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetWindowInteractionStateOperation>("GetWindowInteractionState");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetWindowCapabilitiesOperation>("GetWindowCapabilities");

            // Transform operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetTransformCapabilitiesOperation>("GetTransformCapabilities");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, MoveElementOperation>("MoveElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, ResizeElementOperation>("ResizeElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, RotateElementOperation>("RotateElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, WaitForInputIdleOperation>("WaitForInputIdle");

            // VirtualizedItem operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, RealizeVirtualizedItemOperation>("RealizeVirtualizedItem");

            // ItemContainer operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, FindItemByPropertyOperation>("FindItemByProperty");

            // SynchronizedInput operations
            builder.Services.AddKeyedTransient<IUIAutomationOperation, StartSynchronizedInputOperation>("StartSynchronizedInput");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, CancelSynchronizedInputOperation>("CancelSynchronizedInput");

            // Register Worker service
            builder.Services.AddSingleton<WorkerService>();

            var host = builder.Build();

            // Setup cancellation handling
            using var cts = new CancellationTokenSource();
            
            // Handle console cancellation
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                var workerService = host.Services.GetRequiredService<WorkerService>();
                
                // Run the worker with cancellation support
                var workerTask = workerService.RunAsync();
                var cancellationTask = Task.Delay(Timeout.Infinite, cts.Token);
                
                await Task.WhenAny(workerTask, cancellationTask);
                
                if (cts.Token.IsCancellationRequested)
                {
                    // Cancellation requested
                    return;
                }
                
                await workerTask; // Wait for completion or propagate exception
            }
            catch (OperationCanceledException)
            {
                // Expected during cancellation
            }
            finally
            {
                // Ensure host is disposed
                host.Dispose();
            }
        }
    }
}