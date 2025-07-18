using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using UIAutomationMCP.Server.Tools;
using Xunit;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.E2E
{
    [CollectionDefinition("WinUI3GalleryTestCollection")]
    public class WinUI3GalleryTestCollection : ICollectionFixture<WinUI3GalleryTestFixture>
    {
    }

    public class WinUI3GalleryTestFixture : IAsyncLifetime
    {
        private Process? _winUI3GalleryProcess;
        private const string WindowTitle = "WinUI 3 Gallery";
        private const int LaunchTimeoutSeconds = 30;
        
        public async Task InitializeAsync()
        {
            // Check if WinUI 3 Gallery is already running
            var existingProcesses = Process.GetProcessesByName("WinUIGallery3")
                .Concat(Process.GetProcessesByName("WinUI3Gallery"))
                .Concat(Process.GetProcessesByName("WinUI 3 Gallery"));
            
            if (existingProcesses.Any())
            {
                // Use existing instance - don't set _winUI3GalleryProcess so we won't try to close it
                return;
            }

            // Launch WinUI 3 Gallery
            var serviceProvider = MCPToolsE2ETests.CreateServiceProvider();
            var tools = serviceProvider.GetRequiredService<UIAutomationTools>();
            
            try
            {
                // Try to launch by name
                await tools.LaunchApplicationByName("WinUI 3 Gallery", timeoutSeconds: LaunchTimeoutSeconds);
                
                // Wait a bit for the app to fully initialize
                await Task.Delay(3000);
                
                // Find the launched process
                _winUI3GalleryProcess = Process.GetProcessesByName("WinUIGallery3")
                    .Concat(Process.GetProcessesByName("WinUI3Gallery"))
                    .Concat(Process.GetProcessesByName("WinUI 3 Gallery"))
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Failed to launch WinUI 3 Gallery. Please ensure it is installed from the Microsoft Store.", ex);
            }
            finally
            {
                if (serviceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        public async Task DisposeAsync()
        {
            // Close WinUI 3 Gallery if we launched it
            if (_winUI3GalleryProcess != null && !_winUI3GalleryProcess.HasExited)
            {
                try
                {
                    // Use ProcessCleanupHelper for more robust cleanup
                    await ProcessCleanupHelper.CleanupProcess(
                        _winUI3GalleryProcess,
                        new TestOutputHelper(), // Simple output helper for logging
                        "WinUI 3 Gallery",
                        8000);
                }
                catch (Exception ex)
                {
                    // Log the error but don't throw during cleanup
                    Console.WriteLine($"Error during WinUI 3 Gallery cleanup: {ex.Message}");
                }
            }
        }
    }

    // Simple test output helper for fixture cleanup logging
    internal class TestOutputHelper : ITestOutputHelper
    {
        public void WriteLine(string message)
        {
            Console.WriteLine($"[WinUI3GalleryTestFixture] {message}");
        }

        public void WriteLine(string format, params object[] args)
        {
            Console.WriteLine($"[WinUI3GalleryTestFixture] {format}", args);
        }
    }
}