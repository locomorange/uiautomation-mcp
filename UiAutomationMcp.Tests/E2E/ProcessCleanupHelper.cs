using System.Diagnostics;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.E2E
{
    /// <summary>
    /// Helper class for robust process cleanup in E2E tests
    /// </summary>
    public static class ProcessCleanupHelper
    {
        /// <summary>
        /// Safely executes a process with proper cleanup using try-finally pattern
        /// </summary>
        /// <param name="processAction">Action that creates and uses the process</param>
        /// <param name="output">Test output helper for logging</param>
        /// <param name="processName">Name of the process for logging purposes</param>
        /// <param name="timeoutMs">Timeout in milliseconds for process termination</param>
        /// <returns>Task representing the async operation</returns>
        public static async Task ExecuteWithCleanup(
            Func<Task<Process?>> processAction,
            ITestOutputHelper output,
            string processName = "TestProcess",
            int timeoutMs = 5000)
        {
            Process? process = null;
            try
            {
                output.WriteLine($"Starting {processName}...");
                process = await processAction();

                if (process != null)
                {
                    output.WriteLine($"{processName} started successfully (PID: {process.Id})");
                }
            }
            finally
            {
                if (process != null)
                {
                    await CleanupProcess(process, output, processName, timeoutMs);
                }
            }
        }

        /// <summary>
        /// Safely executes a process with proper cleanup using try-finally pattern (synchronous version)
        /// </summary>
        /// <param name="processAction">Action that creates and uses the process</param>
        /// <param name="output">Test output helper for logging</param>
        /// <param name="processName">Name of the process for logging purposes</param>
        /// <param name="timeoutMs">Timeout in milliseconds for process termination</param>
        public static void ExecuteWithCleanup(
            Func<Process?> processAction,
            ITestOutputHelper output,
            string processName = "TestProcess",
            int timeoutMs = 5000)
        {
            Process? process = null;
            try
            {
                output.WriteLine($"Starting {processName}...");
                process = processAction();

                if (process != null)
                {
                    output.WriteLine($"{processName} started successfully (PID: {process.Id})");
                }
            }
            finally
            {
                if (process != null)
                {
                    CleanupProcess(process, output, processName, timeoutMs).Wait();
                }
            }
        }

        /// <summary>
        /// Safely cleans up a process with comprehensive error handling and logging
        /// </summary>
        /// <param name="process">The process to cleanup</param>
        /// <param name="output">Test output helper for logging</param>
        /// <param name="processName">Name of the process for logging purposes</param>
        /// <param name="timeoutMs">Timeout in milliseconds for process termination</param>
        /// <returns>Task representing the async operation</returns>
        public static async Task CleanupProcess(
            Process process,
            ITestOutputHelper output,
            string processName = "Process",
            int timeoutMs = 5000)
        {
            if (process == null)
            {
                output.WriteLine($"No {processName} to cleanup");
                return;
            }

            try
            {
                output.WriteLine($"Starting cleanup of {processName} (PID: {process.Id})...");

                // Check if process has already exited
                if (process.HasExited)
                {
                    output.WriteLine($"{processName} has already exited (Exit code: {process.ExitCode})");
                    return;
                }

                // Attempt graceful termination first
                try
                {
                    output.WriteLine($"Attempting graceful termination of {processName}...");
                    process.CloseMainWindow();

                    // Wait for graceful shutdown
                    if (await WaitForExitAsync(process, timeoutMs / 2))
                    {
                        output.WriteLine($"{processName} terminated gracefully");
                        return;
                    }
                    else
                    {
                        output.WriteLine($"{processName} did not respond to graceful termination, forcing termination...");
                    }
                }
                catch (Exception ex)
                {
                    output.WriteLine($"Error during graceful termination of {processName}: {ex.Message}");
                }

                // Force termination if graceful shutdown failed
                if (!process.HasExited)
                {
                    try
                    {
                        output.WriteLine($"Force killing {processName}...");
                        process.Kill(entireProcessTree: true);

                        // Wait for force termination
                        if (await WaitForExitAsync(process, timeoutMs / 2))
                        {
                            output.WriteLine($"{processName} force terminated successfully");
                        }
                        else
                        {
                            output.WriteLine($"Warning: {processName} may not have terminated properly");
                        }
                    }
                    catch (Exception ex)
                    {
                        output.WriteLine($"Error during force termination of {processName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                output.WriteLine($"Unexpected error during cleanup of {processName}: {ex.Message}");
            }
            finally
            {
                // Always dispose the process object
                try
                {
                    process.Dispose();
                    output.WriteLine($"{processName} process object disposed");
                }
                catch (Exception ex)
                {
                    output.WriteLine($"Error disposing {processName} process object: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Asynchronously waits for a process to exit with a timeout
        /// </summary>
        /// <param name="process">The process to wait for</param>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        /// <returns>True if process exited within timeout, false otherwise</returns>
        private static async Task<bool> WaitForExitAsync(Process process, int timeoutMs)
        {
            try
            {
                var tcs = new TaskCompletionSource<bool>();

                // Set up event handler for process exit
                process.EnableRaisingEvents = true;
                process.Exited += (sender, e) => tcs.TrySetResult(true);

                // If process has already exited, return true immediately
                if (process.HasExited)
                {
                    return true;
                }

                // Wait for either exit or timeout
                var timeoutTask = Task.Delay(timeoutMs);
                var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                return completedTask == tcs.Task;
            }
            catch (Exception)
            {
                // If we can't wait asynchronously, fall back to synchronous wait
                try
                {
                    return process.WaitForExit(timeoutMs);
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Cleans up processes by name - useful for cleaning up orphaned processes
        /// </summary>
        /// <param name="processName">Name of the process to cleanup</param>
        /// <param name="output">Test output helper for logging</param>
        /// <param name="timeoutMs">Timeout in milliseconds for each process termination</param>
        /// <returns>Task representing the async operation</returns>
        public static async Task CleanupProcessesByName(
            string processName,
            ITestOutputHelper output,
            int timeoutMs = 5000)
        {
            try
            {
                output.WriteLine($"Searching for orphaned {processName} processes...");
                var processes = Process.GetProcessesByName(processName);

                if (processes.Length == 0)
                {
                    output.WriteLine($"No {processName} processes found");
                    return;
                }

                output.WriteLine($"Found {processes.Length} {processName} process(es) to cleanup");

                var cleanupTasks = processes.Select(process =>
                    CleanupProcess(process, output, $"{processName}({process.Id})", timeoutMs));

                await Task.WhenAll(cleanupTasks);
                output.WriteLine($"Cleanup of all {processName} processes completed");
            }
            catch (Exception ex)
            {
                output.WriteLine($"Error during cleanup of {processName} processes: {ex.Message}");
            }
        }
    }
}

