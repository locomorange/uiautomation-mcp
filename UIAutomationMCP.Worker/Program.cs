using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared.Configuration;
using UIAutomationMCP.Worker.Services;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Operations.ElementSearch;
using UIAutomationMCP.Worker.Operations.Invoke;
using UIAutomationMCP.Worker.Operations.Toggle;
using UIAutomationMCP.Worker.Operations.Value;
using UIAutomationMCP.Worker.Operations.Grid;
using UIAutomationMCP.Worker.Operations.Table;
using UIAutomationMCP.Worker.Operations.Selection;
using UIAutomationMCP.Worker.Operations.ElementInspection;
using UIAutomationMCP.Worker.Operations.ControlTypeInfo;
using UIAutomationMCP.Worker.Operations.Text;
using UIAutomationMCP.Worker.Helpers;

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

            // Tools Level Serialization: Configuration is now handled at Server level only
            // Worker operations receive pre-configured typed requests via JSON

            // Configure logging - disable console logging to avoid interference with JSON responses
            builder.Logging.ClearProviders();
            // Add file logging for debugging but avoid console output pollution
            builder.Logging.AddProvider(new DebugLoggerProvider());
            builder.Logging.SetMinimumLevel(LogLevel.Debug);

            // Register helper services
            builder.Services.AddSingleton<ElementFinderService>();
            builder.Services.AddSingleton<FindElementsCacheService>();

            // Register operations as keyed services - Tools Level Serialization pattern
            // Only operations updated to use ExecuteAsync(string parametersJson) interface
            builder.Services.AddKeyedTransient<IUIAutomationOperation, FindElementsOperation>("FindElements");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, InvokeElementOperation>("InvokeElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetDesktopWindowsOperation>("GetDesktopWindows");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, ToggleElementOperation>("ToggleElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetToggleStateOperation>("GetToggleState");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SetToggleStateOperation>("SetToggleState");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SetElementValueOperation>("SetElementValue");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetElementValueOperation>("GetElementValue");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, IsReadOnlyOperation>("IsReadOnly");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetGridInfoOperation>("GetGridInfo");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetGridItemOperation>("GetGridItem");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetRowHeaderOperation>("GetRowHeader");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetTableInfoOperation>("GetTableInfo");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetColumnHeadersOperation>("GetColumnHeaders");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetRowHeadersOperation>("GetRowHeaders");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SelectElementOperation>("SelectElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, AddToSelectionOperation>("AddToSelection");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, RemoveFromSelectionOperation>("RemoveFromSelection");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, ClearSelectionOperation>("ClearSelection");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetElementPropertiesOperation>("GetElementProperties");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetElementPatternsOperation>("GetElementPatterns");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, ValidateControlTypePatternsOperation>("ValidateControlTypePatterns");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, FindElementsByPatternOperation>("FindElementsByPattern");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SelectTextOperation>("SelectText");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetTextOperation>("GetText");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SetTextOperation>("SetText");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, FindTextOperation>("FindText");
            
            // TODO: Phase 6 - Update remaining operations to new interface
            // The following operations need to be updated to use string parametersJson:
            /*
            builder.Services.AddKeyedTransient<IUIAutomationOperation, InvokeElementOperation>("InvokeElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, ToggleElementOperation>("ToggleElement");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetToggleStateOperation>("GetToggleState");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SetToggleStateOperation>("SetToggleState");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, SetElementValueOperation>("SetElementValue");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetElementValueOperation>("GetElementValue");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, IsReadOnlyOperation>("IsReadOnly");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetDesktopWindowsOperation>("GetDesktopWindows");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, ValidateControlTypePatternsOperation>("ValidateControlTypePatterns");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, FindElementsByPatternOperation>("FindElementsByPattern");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetRowHeaderOperation>("GetRowHeader");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetGridInfoOperation>("GetGridInfo");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetGridItemOperation>("GetGridItem");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetElementPropertiesOperation>("GetElementProperties");
            builder.Services.AddKeyedTransient<IUIAutomationOperation, GetElementPatternsOperation>("GetElementPatterns");
            */ 

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