using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Tools;
using Xunit;
using Xunit.Abstractions;
using UIAutomationMCP.Models.Abstractions;
using UIAutomationMCP.Server.Abstractions;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Models.Logging;

namespace UIAutomationMCP.Tests.E2E
{
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "E2E")]
    public class MCPToolsE2ETests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly IServiceProvider _serviceProvider;
        private readonly UIAutomationTools _tools;
        private readonly List<Process> _launchedProcesses = new();

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

            // Register shutdown CancellationTokenSource
            var shutdownCts = new CancellationTokenSource();
            services.AddSingleton(shutdownCts);

            // Register all core UIAutomation services (shared with Program.cs)
            services.AddUIAutomationCoreServices();

            // Register ProcessManager for worker and monitor process management
            services.AddSingleton<ProcessManager>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<ProcessManager>>();
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var baseDir = ExecutablePathResolver.GetExecutableRealPath();

                // Resolve Worker and Monitor paths
                var workerPath = ExecutablePathResolver.ResolveWorkerPath(baseDir);
                var monitorPath = ExecutablePathResolver.ResolveMonitorPath(baseDir);

                if (workerPath == null || (!File.Exists(workerPath) && !Directory.Exists(workerPath)))
                {
                    var searchedPaths = ExecutablePathResolver.GetSearchedPaths("UIAutomationMCP.Subprocess.Worker", baseDir);
                    throw new InvalidOperationException($"UIAutomationMCP.Subprocess.Worker not found. Searched: {string.Join(", ", searchedPaths)}");
                }

                if (monitorPath == null || (!File.Exists(monitorPath) && !Directory.Exists(monitorPath)))
                {
                    var searchedPaths = ExecutablePathResolver.GetSearchedPaths("UIAutomationMCP.Subprocess.Monitor", baseDir);
                    throw new InvalidOperationException($"UIAutomationMCP.Subprocess.Monitor not found. Searched: {string.Join(", ", searchedPaths)}");
                }

                var shutdown = provider.GetRequiredService<CancellationTokenSource>();
                var processManager = new ProcessManager(logger, loggerFactory, shutdown, workerPath, monitorPath);

                // Set MCP log service for subprocess log relay
                var mcpLogService = provider.GetRequiredService<IMcpLogService>();
                processManager.SetMcpLogService(mcpLogService);

                return processManager;
            });

            // Register ProcessManager as both IProcessManager and IOperationExecutor
            services.AddSingleton<IProcessManager>(provider => provider.GetRequiredService<ProcessManager>());
            services.AddSingleton<IOperationExecutor>(provider => provider.GetRequiredService<ProcessManager>());

            // Register tools
            services.AddSingleton<UIAutomationTools>();

            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task SearchElements_Windows_ShouldReturnOpenWindows()
        {
            _output.WriteLine("Testing SearchElements with Window filter...");

            try
            {
                var result = await _tools.SearchElements(controlType: "Window", scope: "children");
                Assert.NotNull(result);

                _output.WriteLine($"SearchElements Windows result type: {result.GetType().Name}");
                _output.WriteLine($"SearchElements Windows result: {JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })}");

                // Test passes if no exception is thrown and result is not null
                Assert.True(true, "SearchElements Windows executed successfully");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"SearchElements Windows failed: {ex.Message}");
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
                // Use IApplicationLauncher directly instead of non-existent LaunchApplicationByName
                var appLauncher = _serviceProvider.GetRequiredService<IApplicationLauncher>();
                var result = await appLauncher.LaunchApplicationAsync("notepad.exe");
                Assert.NotNull(result);
                Assert.True(result.Success, "LaunchApplication should succeed for notepad.exe");
                Assert.True(result.WindowHandle.HasValue, "LaunchApplication should return a window handle for notepad.exe");
                _output.WriteLine($"UsedEventBasedDetection: {result.UsedEventBasedDetection}");

                _output.WriteLine($"LaunchApplicationByName result: {JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })}");

                // Wait for application to start
                await Task.Delay(2000);

                // Track the launched Notepad process
                var notepadProcess = Process.GetProcessesByName("notepad").OrderByDescending(p => p.StartTime).FirstOrDefault();
                if (notepadProcess != null)
                {
                    _launchedProcesses.Add(notepadProcess);
                    _output.WriteLine($"Tracked Notepad process ID: {notepadProcess.Id}");
                }

                // Try to get window info for Notepad
                var windowInfo = await _tools.SearchElements(controlType: "Window", scope: "children");
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
        public async Task SearchElements_ShouldSearchElementsInNotepad()
        {
            _output.WriteLine("Testing SearchElements in Notepad...");

            try
            {
                // First launch Notepad
                // Use IApplicationLauncher directly
                var appLauncher = _serviceProvider.GetRequiredService<IApplicationLauncher>();
                await appLauncher.LaunchApplicationAsync("notepad.exe");
                await Task.Delay(2000);

                // Track the launched Notepad process
                var notepadProcess = Process.GetProcessesByName("notepad").OrderByDescending(p => p.StartTime).FirstOrDefault();
                if (notepadProcess != null)
                {
                    _launchedProcesses.Add(notepadProcess);
                    _output.WriteLine($"Tracked Notepad process ID: {notepadProcess.Id}");
                }

                // Find elements in Notepad window
                var result = await _tools.SearchElements();
                Assert.NotNull(result);

                _output.WriteLine($"SearchElements result: {JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })}");

                Assert.True(true, "SearchElements executed successfully");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"SearchElements failed: {ex.Message}");
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
                // Clean up only the processes we explicitly launched using ProcessCleanupHelper
                var cleanupTasks = _launchedProcesses
                    .Where(p =>
                    {
                        try
                        {
                            return !p.HasExited;
                        }
                        catch
                        {
                            return false; // Process may have already been disposed
                        }
                    })
                    .Select(process =>
                        ProcessCleanupHelper.CleanupProcess(
                            process,
                            _output,
                            $"{process.ProcessName}({process.Id})",
                            5000))
                    .ToList();

                if (cleanupTasks.Any())
                {
                    _output.WriteLine($"Cleaning up {cleanupTasks.Count} launched processes...");
                    Task.WhenAll(cleanupTasks).Wait(TimeSpan.FromSeconds(30));
                }

                _launchedProcesses.Clear();
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error during process cleanup: {ex.Message}");
            }

            try
            {
                if (_serviceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error during service provider cleanup: {ex.Message}");
            }
        }
    }
}

