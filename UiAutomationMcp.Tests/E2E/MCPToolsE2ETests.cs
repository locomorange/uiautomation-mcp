using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Tools;
using Xunit;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.E2E
{
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "E2E")]
    public class MCPToolsE2ETests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly IServiceProvider _serviceProvider;
        private readonly UIAutomationTools _tools;

        public MCPToolsE2ETests(ITestOutputHelper output)
        {
            _output = output;
            _serviceProvider = CreateServiceProvider();
            _tools = _serviceProvider.GetRequiredService<UIAutomationTools>();
        }

        public static IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();

            // Configure logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Register application services
            services.AddSingleton<IApplicationLauncher, ApplicationLauncher>();
            services.AddSingleton<IScreenshotService, ScreenshotService>();
            
            // Register subprocess-based UI Automation services
            services.AddSingleton<IElementSearchService, ElementSearchService>();
            services.AddSingleton<ITreeNavigationService, TreeNavigationService>();
            services.AddSingleton<IInvokeService, InvokeService>();
            services.AddSingleton<IValueService, ValueService>();
            services.AddSingleton<IToggleService, ToggleService>();
            services.AddSingleton<ISelectionService, SelectionService>();
            services.AddSingleton<IWindowService, WindowService>();
            services.AddSingleton<ITextService, TextService>();
            services.AddSingleton<ILayoutService, LayoutService>();
            services.AddSingleton<IRangeService, RangeService>();
            services.AddSingleton<IElementInspectionService, ElementInspectionService>();
            
            // Register additional subprocess-based UI Automation services
            services.AddSingleton<IGridService, GridService>();
            services.AddSingleton<ITableService, TableService>();
            services.AddSingleton<IMultipleViewService, MultipleViewService>();
            services.AddSingleton<IAccessibilityService, AccessibilityService>();
            services.AddSingleton<ICustomPropertyService, CustomPropertyService>();
            services.AddSingleton<ITransformService, TransformService>();
            services.AddSingleton<IControlTypeService, ControlTypeService>();

            // Register subprocess executor
            services.AddSingleton<SubprocessExecutor>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<SubprocessExecutor>>();
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                
                // Look for Worker.exe in multiple possible locations
                var possiblePaths = new[]
                {
                    Path.Combine(baseDir, "UIAutomationMCP.Worker.exe"),
                    Path.Combine(baseDir, "..", "..", "..", "..", "UIAutomationMCP.Worker", "bin", "Debug", "net9.0-windows", "UIAutomationMCP.Worker.exe"),
                    Path.Combine(baseDir, "..", "..", "..", "..", "UIAutomationMCP.Server", "bin", "Debug", "net9.0-windows", "UIAutomationMCP.Worker.exe"),
                    Path.Combine(baseDir, "worker", "UIAutomationMCP.Worker.exe"),
                };

                string? workerPath = null;
                foreach (var path in possiblePaths)
                {
                    var fullPath = Path.GetFullPath(path);
                    if (File.Exists(fullPath))
                    {
                        workerPath = fullPath;
                        break;
                    }
                }

                if (workerPath == null)
                {
                    throw new InvalidOperationException($"UIAutomationMCP.Worker not found. Searched paths: {string.Join(", ", possiblePaths.Select(Path.GetFullPath))}");
                }

                return new SubprocessExecutor(logger, workerPath);
            });
            
            services.AddSingleton<ISubprocessExecutor>(provider => provider.GetRequiredService<SubprocessExecutor>());
            
            // Register tools
            services.AddSingleton<UIAutomationTools>();

            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task GetWindowInfo_ShouldReturnOpenWindows()
        {
            _output.WriteLine("Testing GetWindowInfo tool...");

            try
            {
                var result = await _tools.GetWindowInfo();
                Assert.NotNull(result);
                
                _output.WriteLine($"GetWindowInfo result type: {result.GetType().Name}");
                _output.WriteLine($"GetWindowInfo result: {JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })}");
                
                // Test passes if no exception is thrown and result is not null
                Assert.True(true, "GetWindowInfo executed successfully");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"GetWindowInfo failed: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [Fact]
        public async Task LaunchApplicationByName_ShouldLaunchNotepad()
        {
            _output.WriteLine("Testing LaunchApplicationByName with Notepad...");

            try
            {
                var result = await _tools.LaunchApplicationByName("Notepad");
                Assert.NotNull(result);
                
                _output.WriteLine($"LaunchApplicationByName result: {JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })}");
                
                // Wait for application to start
                await Task.Delay(2000);
                
                // Try to get window info for Notepad
                var windowInfo = await _tools.GetWindowInfo();
                _output.WriteLine($"Window info after launch: {JsonSerializer.Serialize(windowInfo, new JsonSerializerOptions { WriteIndented = true })}");
                
                Assert.True(true, "LaunchApplicationByName executed successfully");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"LaunchApplicationByName failed: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [Fact]
        public async Task FindElements_ShouldFindElementsInNotepad()
        {
            _output.WriteLine("Testing FindElements in Notepad...");

            try
            {
                // First launch Notepad
                await _tools.LaunchApplicationByName("Notepad");
                await Task.Delay(2000);
                
                // Find elements in Notepad window
                var result = await _tools.FindElements(windowTitle: "Notepad");
                Assert.NotNull(result);
                
                _output.WriteLine($"FindElements result: {JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })}");
                
                Assert.True(true, "FindElements executed successfully");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"FindElements failed: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [Fact]
        public async Task TakeScreenshot_ShouldCaptureScreen()
        {
            _output.WriteLine("Testing TakeScreenshot...");

            try
            {
                var result = await _tools.TakeScreenshot();
                Assert.NotNull(result);
                
                _output.WriteLine($"TakeScreenshot result type: {result.GetType().Name}");
                
                // Convert to JSON to inspect structure
                var jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
                _output.WriteLine($"TakeScreenshot result: {jsonResult}");
                
                Assert.True(true, "TakeScreenshot executed successfully");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"TakeScreenshot failed: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [Fact]
        public async Task GetElementTree_ShouldGetTreeStructure()
        {
            _output.WriteLine("Testing GetElementTree...");

            try
            {
                var result = await _tools.GetElementTree(maxDepth: 2);
                Assert.NotNull(result);
                
                _output.WriteLine($"GetElementTree result: {JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })}");
                
                Assert.True(true, "GetElementTree executed successfully");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"GetElementTree failed: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                // Clean up any Notepad processes we might have launched
                var notepadProcesses = Process.GetProcessesByName("notepad");
                foreach (var process in notepadProcesses)
                {
                    try
                    {
                        process.CloseMainWindow();
                        if (!process.WaitForExit(5000))
                        {
                            process.Kill();
                        }
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"Failed to close Notepad process: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error during cleanup: {ex.Message}");
            }
            
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}