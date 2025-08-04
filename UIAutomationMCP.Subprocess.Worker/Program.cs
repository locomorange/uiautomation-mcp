using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Subprocess.Worker.Helpers;
using UIAutomationMCP.Core.Configuration;
using UIAutomationMCP.Subprocess.Core.Extensions;
using UIAutomationMCP.Subprocess.Core.Infrastructure;
using UIAutomationMCP.Subprocess.Core.Abstractions;
using UIAutomationMCP.Subprocess.Worker.Operations.ElementSearch;
using UIAutomationMCP.Subprocess.Worker.Operations.Invoke;
using UIAutomationMCP.Subprocess.Worker.Operations.Toggle;
using UIAutomationMCP.Subprocess.Worker.Operations.Value;
using UIAutomationMCP.Subprocess.Worker.Operations.Grid;
using UIAutomationMCP.Subprocess.Worker.Operations.Selection;
using UIAutomationMCP.Subprocess.Worker.Operations.Text;
using UIAutomationMCP.Subprocess.Worker.Operations.Layout;
using UIAutomationMCP.Subprocess.Worker.Operations.Transform;
using UIAutomationMCP.Subprocess.Worker.Operations.Window;
using UIAutomationMCP.Subprocess.Worker.Operations.Range;
using UIAutomationMCP.Subprocess.Worker.Operations.TreeNavigation;
using UIAutomationMCP.Subprocess.Worker.Operations.Focus;
using UIAutomationMCP.Subprocess.Core.Helpers;
using UIAutomationMCP.Models.Logging;
using UIAutomationMCP.Models.Requests;

namespace UIAutomationMCP.Subprocess.Worker
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

                // Wait for maximum 5 seconds for UI Automation check
                if (task.Wait(5000))
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
                    errorReason = "UI Automation initialization could not complete within 5 seconds. Consider increasing the initialization timeout if system is slow.";
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
                // Send environment error information via MCP relay
                await ProcessLogRelay.LogErrorAsync("Worker.Environment",
                    $"UI Automation is not available in this environment: {errorReason}",
                    "worker",
                    data: new Dictionary<string, object?>
                    {
                        ["platform"] = Environment.OSVersion.ToString(),
                        ["userInteractive"] = Environment.UserInteractive,
                        ["is64BitOS"] = Environment.Is64BitOperatingSystem,
                        ["currentDirectory"] = Environment.CurrentDirectory
                    });

                // Additional diagnostics for common issues
                try
                {
                    var desktop = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop");
                    await ProcessLogRelay.LogInfoAsync("Worker.Environment",
                        $"Desktop registry key accessible: {desktop != null}", "worker");
                    desktop?.Close();
                }
                catch (Exception regEx)
                {
                    await ProcessLogRelay.LogErrorAsync("Worker.Environment",
                        "Registry access failed", "worker", regEx);
                }

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
            // TEMPORARY: Add console error logging for debugging
            builder.Logging.AddConsole(options =>
            {
                options.LogToStandardErrorThreshold = LogLevel.Debug;
            });
            builder.Logging.SetMinimumLevel(LogLevel.Debug);

            // Register helper services
            builder.Services.AddSingleton<UIAutomationMCP.Subprocess.Core.Services.ElementFinderService>();

            // Register UI Automation operations using new extension methods
            // Element operations
            builder.Services.AddOperation<InvokeElementOperation, InvokeElementRequest>();
            builder.Services.AddOperation<ToggleElementOperation, ToggleElementRequest>();
            builder.Services.AddOperation<SetToggleStateOperation, SetToggleStateRequest>();
            builder.Services.AddOperation<SetElementValueOperation, SetElementValueRequest>();
            builder.Services.AddOperation<SetFocusOperation, SetFocusRequest>();

            // Search and tree operations
            builder.Services.AddOperation<SearchElementsOperation, SearchElementsRequest>();
            builder.Services.AddOperation<GetElementTreeOperation, GetElementTreeRequest>();

            // Grid operations
            builder.Services.AddOperation<GetGridItemOperation, GetGridItemRequest>();
            builder.Services.AddOperation<GetRowHeaderOperation, GetRowHeaderRequest>();
            builder.Services.AddOperation<GetColumnHeaderOperation, GetColumnHeaderRequest>();

            // Selection operations
            builder.Services.AddOperation<SelectElementOperation, SelectElementRequest>();
            builder.Services.AddOperation<AddToSelectionOperation, AddToSelectionRequest>();
            builder.Services.AddOperation<RemoveFromSelectionOperation, RemoveFromSelectionRequest>();
            builder.Services.AddOperation<ClearSelectionOperation, ClearSelectionRequest>();
            builder.Services.AddOperation<SelectItemOperation, SelectItemRequest>();

            // Text operations
            builder.Services.AddOperation<SelectTextOperation, SelectTextRequest>();
            builder.Services.AddOperation<SetTextOperation, SetTextRequest>();
            builder.Services.AddOperation<FindTextOperation, FindTextRequest>();
            builder.Services.AddOperation<GetTextAttributesOperation, GetTextAttributesRequest>();

            // Layout operations
            builder.Services.AddOperation<ScrollElementOperation, ScrollElementRequest>();
            builder.Services.AddOperation<ExpandCollapseElementOperation, ExpandCollapseElementRequest>();
            builder.Services.AddOperation<DockElementOperation, DockElementRequest>();
            builder.Services.AddOperation<SetScrollPercentOperation, SetScrollPercentRequest>();
            builder.Services.AddOperation<ScrollElementIntoViewOperation, ScrollElementIntoViewRequest>();

            // Transform operations
            builder.Services.AddOperation<MoveElementOperation, MoveElementRequest>();
            builder.Services.AddOperation<ResizeElementOperation, ResizeElementRequest>();
            builder.Services.AddOperation<RotateElementOperation, RotateElementRequest>();

            // Window operations
            builder.Services.AddOperation<WindowActionOperation, WindowActionRequest>();
            builder.Services.AddOperation<WaitForInputIdleOperation, WaitForInputIdleRequest>();

            // Range operations
            builder.Services.AddOperation<SetRangeValueOperation, SetRangeValueRequest>();

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

