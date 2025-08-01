using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Tools;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.E2E
{
    /// <summary>
    /// E2E                -                           /// </summary>
    public abstract class BaseE2ETest : IDisposable
    {
        protected readonly ITestOutputHelper Output;
        protected readonly IServiceProvider ServiceProvider;
        protected readonly UIAutomationTools Tools;
        private bool _disposed;

        protected BaseE2ETest(ITestOutputHelper output)
        {
            Output = output;
            ServiceProvider = MCPToolsE2ETests.CreateServiceProvider();
            Tools = ServiceProvider.GetRequiredService<UIAutomationTools>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                try
                {
                    // Dispose ServiceProvider if it implements IDisposable
                    if (ServiceProvider is IDisposable serviceProviderDisposable)
                    {
                        serviceProviderDisposable.Dispose();
                    }

                    // SubprocessExecutor      Dispose                                  GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    //                Worker                     
                    CleanupRemainingWorkerProcesses();
                }
                catch (Exception ex)
                {
                    Output.WriteLine($"Cleanup error in {GetType().Name}: {ex.Message}");
                }
            }

            _disposed = true;
        }

        private void CleanupRemainingWorkerProcesses()
        {
            try
            {
                Output.WriteLine("Starting cleanup of remaining Worker processes...");
                // Use the new ProcessCleanupHelper for more robust cleanup
                ProcessCleanupHelper.CleanupProcessesByName("UIAutomationMCP.Worker", Output, 3000)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Error during Worker process cleanup: {ex.Message}");
            }
        }
    }
}

