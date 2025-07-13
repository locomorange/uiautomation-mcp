using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using UIAutomationMCP.Server.Tools;
using Xunit;

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
                // Use existing instance
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

        public Task DisposeAsync()
        {
            // Close WinUI 3 Gallery if we launched it
            if (_winUI3GalleryProcess != null && !_winUI3GalleryProcess.HasExited)
            {
                try
                {
                    _winUI3GalleryProcess.CloseMainWindow();
                    if (!_winUI3GalleryProcess.WaitForExit(5000))
                    {
                        _winUI3GalleryProcess.Kill();
                    }
                }
                catch
                {
                    // Ignore errors during cleanup
                }
                finally
                {
                    _winUI3GalleryProcess.Dispose();
                }
            }
            
            return Task.CompletedTask;
        }
    }
}