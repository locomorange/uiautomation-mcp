using System;
using System.ComponentModel;
using System.Windows.Automation;
using System.Threading.Tasks;
using System.Threading;

namespace UIAutomationMCP.Worker.Helpers
{
    /// <summary>
    /// Helper class for UI Automation environment checks and error handling
    /// Note: This assumes UI Automation availability was pre-checked at Worker startup
    /// </summary>
    public static class UIAutomationEnvironment
    {
        /// <summary>
        /// Check if UI Automation is available and properly initialized
        /// This is a fast check that assumes the Worker startup validation passed
        /// </summary>
        public static bool IsAvailable => CheckAvailabilityFast();

        /// <summary>
        /// Get a descriptive error message when UI Automation is not available
        /// </summary>
        public static string UnavailabilityReason { get; private set; } = "";

        private static bool CheckAvailabilityFast()
        {
            try
            {
                // Quick check - if Worker started successfully, UI Automation should be available
                // Just verify we can access basic automation without deep operations
                var rootElement = AutomationElement.RootElement;
                return rootElement != null;
            }
            catch (Exception ex)
            {
                UnavailabilityReason = $"UI Automation fast check failed: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Execute a UI Automation operation with basic error handling
        /// Note: Does not prevent hangs - relies on Server-side timeout
        /// </summary>
        public static T ExecuteWithErrorHandling<T>(Func<T> operation, string operationName)
        {
            try
            {
                return operation();
            }
            catch (TypeInitializationException ex)
            {
                throw new InvalidOperationException($"Operation '{operationName}' failed: UI Automation type initialization error. {ex.Message}");
            }
            catch (Win32Exception ex)
            {
                throw new InvalidOperationException($"Operation '{operationName}' failed: Win32 error. {ex.Message}");
            }
            catch (ElementNotAvailableException ex)
            {
                throw new InvalidOperationException($"Operation '{operationName}' failed: Element not available. {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Operation '{operationName}' failed: Invalid argument. {ex.Message}");
            }
            catch (Exception ex) when (ex.Message.Contains("AutomationElement"))
            {
                throw new InvalidOperationException($"Operation '{operationName}' failed: UI Automation element error. {ex.Message}");
            }
        }

        /// <summary>
        /// Execute a UI Automation operation with timeout protection
        /// </summary>
        public static async Task<T> ExecuteWithTimeoutAsync<T>(Func<Task<T>> operation, string operationName, int timeoutSeconds = 8)
        {
            if (!IsAvailable)
            {
                throw new InvalidOperationException($"UI Automation is not available: {UnavailabilityReason}");
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            
            try
            {
                var task = operation();
                var completedTask = await Task.WhenAny(task, Task.Delay(Timeout.Infinite, cts.Token));
                
                if (completedTask == task)
                {
                    return await task;
                }
                else
                {
                    throw new TimeoutException($"Operation '{operationName}' timed out after {timeoutSeconds} seconds");
                }
            }
            catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
            {
                throw new TimeoutException($"Operation '{operationName}' timed out after {timeoutSeconds} seconds");
            }
            catch (TypeInitializationException ex)
            {
                throw new InvalidOperationException($"Operation '{operationName}' failed: UI Automation type initialization error. {ex.Message}");
            }
            catch (Win32Exception ex)
            {
                throw new InvalidOperationException($"Operation '{operationName}' failed: Win32 error. {ex.Message}");
            }
            catch (ElementNotAvailableException ex)
            {
                throw new InvalidOperationException($"Operation '{operationName}' failed: Element not available. {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Operation '{operationName}' failed: Invalid argument. {ex.Message}");
            }
            catch (Exception ex) when (ex.Message.Contains("AutomationElement"))
            {
                throw new InvalidOperationException($"Operation '{operationName}' failed: UI Automation element error. {ex.Message}");
            }
        }

        /// <summary>
        /// Execute a synchronous UI Automation operation with timeout
        /// </summary>
        public static T ExecuteWithTimeout<T>(Func<T> operation, string operationName, int timeoutSeconds = 8)
        {
            return ExecuteWithTimeoutAsync(() => Task.Run(operation), operationName, timeoutSeconds).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Execute a UI Automation operation with basic error handling (void return)
        /// </summary>
        public static void ExecuteWithErrorHandling(Action operation, string operationName)
        {
            ExecuteWithErrorHandling<object?>(() =>
            {
                operation();
                return null;
            }, operationName);
        }
    }
}
