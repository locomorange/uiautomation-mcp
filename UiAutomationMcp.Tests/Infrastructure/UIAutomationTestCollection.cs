using System.Diagnostics;
using Xunit;

namespace UiAutomationMcp.Tests.Infrastructure
{
    /// <summary>
    ///                  -                                 
    /// </summary>
    [CollectionDefinition("UIAutomationTestCollection")]
    public class UIAutomationTestCollection : ICollectionFixture<UIAutomationTestFixture>
    {
        // Collection marker class
    }

    /// <summary>
    ///                 -                                  
    /// </summary>
    public class UIAutomationTestFixture : IDisposable
    {
        public UIAutomationTestFixture()
        {
            //                             CleanupWorkerProcesses();
            
            //                                            
            for (int i = 0; i < 3; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            
            //                                 
            System.Threading.Thread.Sleep(500);
        }

        public void Dispose()
        {
            try
            {
                //                                            
                for (int i = 0; i < 3; i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                
                //         Worker                                 CleanupWorkerProcesses();
                
                //                            System.Threading.Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fixture disposal error: {ex.Message}");
            }
        }
        
        private void CleanupWorkerProcesses()
        {
            try
            {
                // Kill any running UIAutomationMCP.Worker processes
                var workerProcesses = Process.GetProcessesByName("UIAutomationMCP.Worker");
                foreach (var process in workerProcesses)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill(entireProcessTree: true);
                            process.WaitForExit(2000);
                        }
                        process.Dispose();
                    }
                    catch
                    {
                        // Ignore errors
                    }
                }
                
                // dotnet.exe           UIAutomationMCP.Worker.dll                      
                var dotnetProcesses = Process.GetProcessesByName("dotnet");
                foreach (var process in dotnetProcesses)
                {
                    try
                    {
                        // Check if this dotnet process is running UIAutomationMCP.Worker
                        var commandLine = GetProcessCommandLine(process);
                        if (!string.IsNullOrEmpty(commandLine) && commandLine.Contains("UIAutomationMCP.Worker"))
                        {
                            if (!process.HasExited)
                            {
                                process.Kill(entireProcessTree: true);
                                process.WaitForExit(2000);
                            }
                        }
                        process.Dispose();
                    }
                    catch
                    {
                        // Ignore errors
                    }
                }
            }
            catch (Exception ex)
            {
                // Log cleanup error
                Console.WriteLine($"Worker process cleanup error: {ex.Message}");
            }
        }
        
        private string? GetProcessCommandLine(Process process)
        {
            try
            {
                // Windows Management Instrumentation query
                using (var searcher = new System.Management.ManagementObjectSearcher(
                    $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj["CommandLine"]?.ToString();
                    }
                }
            }
            catch
            {
                // WMI query failed
            }
            return null;
        }
    }
}

