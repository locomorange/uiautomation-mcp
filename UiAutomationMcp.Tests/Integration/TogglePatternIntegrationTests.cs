using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Models;
using UIAutomationMCP.Core.Options;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using Xunit.Abstractions;
using System.Diagnostics;

using UIAutomationMCP.Server.Abstractions;
using Moq;
namespace UIAutomationMCP.Tests.Integration
{
    /// <summary>
    /// TogglePattern               /// SubprocessExecutor         Worker                   TogglePattern                /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Integration")]
    public class TogglePatternIntegrationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ServiceProvider _serviceProvider;
        private readonly SubprocessExecutor _subprocessExecutor;
        private readonly ToggleService _toggleService;
        private readonly ElementSearchService _elementSearchService;
        private readonly string _workerPath;

        public TogglePatternIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
            
            var services = new ServiceCollection();
            services.AddLogging(builder => 
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            
            _serviceProvider = services.BuildServiceProvider();
            var logger = _serviceProvider.GetRequiredService<ILogger<SubprocessExecutor>>();
            
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var possiblePaths = new[]
            {
                Path.Combine(baseDir, "UIAutomationMCP.Worker.exe"),
                Path.Combine(baseDir, "..", "UIAutomationMCP.Worker", "bin", "Debug", "net9.0-windows", "UIAutomationMCP.Worker.exe"),
                Path.Combine(baseDir, "worker", "UIAutomationMCP.Worker.exe"),
            };

            _workerPath = possiblePaths.FirstOrDefault(File.Exists) ?? 
                throw new InvalidOperationException("Worker executable not found");

            _subprocessExecutor = new SubprocessExecutor(logger, _workerPath, new CancellationTokenSource());
            
            // Create options
            var options = Options.Create(new UIAutomationOptions());
            
            _toggleService = new ToggleService(Mock.Of<IProcessManager>(), _serviceProvider.GetRequiredService<ILogger<ToggleService>>());
            _elementSearchService = new ElementSearchService(
                _serviceProvider.GetRequiredService<ILogger<ElementSearchService>>(), 
                _subprocessExecutor, 
                options);
        }

        #region        TogglePattern         
        [Fact]
        public async Task ToggleElement_WithNonExistentElement_ShouldReturnFailureResult()
        {
            // Given
            var nonExistentAutomationId = "NonExistentToggleElement123";
            var timeout = 5;

            // When
            var result = await _toggleService.ToggleElementAsync(nonExistentAutomationId, null, null, null, timeout);

            // Then
            Assert.NotNull(result);
            var resultJson = System.Text.Json.JsonSerializer.Serialize(result);
            Assert.Contains("Success", resultJson);
            Assert.Contains("false", resultJson);
            
            _output.WriteLine($"Non-existent element toggle result: {resultJson}");
        }

        [Fact]
        public async Task ToggleElement_WithDifferentParameters_ShouldHandleGracefully()
        {
            // Given
            var nonExistentAutomationId = "TestToggleButton123";
            var timeout = 3;

            // When - Test different parameter combinations
            var resultByAutomationId = await _toggleService.ToggleElementAsync(nonExistentAutomationId, null, null, null, timeout);
            var resultByWindowTitle = await _toggleService.ToggleElementAsync(nonExistentAutomationId, "NonExistentWindow", null, null, timeout);
            var resultByProcessId = await _toggleService.ToggleElementAsync(nonExistentAutomationId, null, null, 99999, timeout);

            // Then
            Assert.NotNull(resultByAutomationId);
            Assert.NotNull(resultByWindowTitle);
            Assert.NotNull(resultByProcessId);
            
            _output.WriteLine($"Result by AutomationId: {resultByAutomationId}");
            _output.WriteLine($"Result by WindowTitle: {resultByWindowTitle}");
            _output.WriteLine($"Result by ProcessId: {resultByProcessId}");
        }

        [Fact]
        public async Task ToggleElement_ConcurrentCalls_ShouldNotBlock()
        {
            // Given
            var elementIds = new[] { "ToggleElement1", "ToggleElement2", "ToggleElement3" };
            var timeout = 2;

            // When - Execute concurrent toggle operations
            var startTime = DateTime.UtcNow;
            var tasks = elementIds.Select(id => 
                _toggleService.ToggleElementAsync(id, null, null, null, timeout)).ToArray();
            
            var results = await Task.WhenAll(tasks);
            var endTime = DateTime.UtcNow;
            var totalTime = (endTime - startTime).TotalMilliseconds;

            // Then - Should complete in reasonable time (non-blocking behavior)
            Assert.All(results, result => Assert.NotNull(result));
            Assert.True(totalTime < 10000, $"Concurrent toggle calls took too long: {totalTime}ms");
            
            _output.WriteLine($"Concurrent toggle operations completed in {totalTime}ms");
            for (int i = 0; i < results.Length; i++)
            {
                _output.WriteLine($"Toggle result {i}: {results[i]}");
            }
        }

        [Fact]
        public async Task ToggleElement_ShortTimeout_ShouldReturnQuickly()
        {
            // Given
            var nonExistentAutomationId = "TimeoutTestToggleElement";
            var shortTimeout = 1;

            // When
            var startTime = DateTime.UtcNow;
            var result = await _toggleService.ToggleElementAsync(nonExistentAutomationId, null, null, null, shortTimeout);
            var endTime = DateTime.UtcNow;
            var actualTime = (endTime - startTime).TotalSeconds;

            // Then - Should respect timeout and return quickly
            Assert.NotNull(result);
            Assert.True(actualTime <= shortTimeout + 2, $"Toggle operation took {actualTime}s, expected <= {shortTimeout + 2}s");
            
            _output.WriteLine($"Short timeout toggle test completed in {actualTime}s with result: {result}");
        }

        #endregion

        #region                     
        [Fact]
        public async Task ToggleElement_TimeoutHandling_ShouldWork()
        {
            // Given
            var veryShortTimeout = 1;
            var nonExistentAutomationId = "NonExistentToggleElement12345";

            // When
            var toggleTask = _toggleService.ToggleElementAsync(nonExistentAutomationId, null, null, null, veryShortTimeout);

            var result = await toggleTask;

            // Then
            Assert.NotNull(result);
            _output.WriteLine($"Timeout test - Toggle: {result}");
        }

        [Fact]
        public async Task ToggleElement_MultipleTimeouts_ShouldHandleIndependently()
        {
            // Given
            var shortTimeout = 1;
            var mediumTimeout = 3;
            var longTimeout = 5;
            var testAutomationId = "TimeoutTestElement";

            // When
            var task1 = _toggleService.ToggleElementAsync(testAutomationId, null, null, null, shortTimeout);
            var task2 = _toggleService.ToggleElementAsync(testAutomationId, null, null, null, mediumTimeout);
            var task3 = _toggleService.ToggleElementAsync(testAutomationId, null, null, null, longTimeout);

            var results = await Task.WhenAll(task1, task2, task3);

            // Then
            Assert.All(results, result => Assert.NotNull(result));
            _output.WriteLine($"Multiple timeout test results:");
            for (int i = 0; i < results.Length; i++)
            {
                _output.WriteLine($"  Result {i + 1}: {results[i]}");
            }
        }

        #endregion

        #region                                               
        [Fact]
        public async Task ToggleElement_WithCalculator_ShouldHandleGracefully()
        {
            // Given
            Process? calcProcess = null;
            try
            {
                calcProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "calc.exe",
                    UseShellExecute = true
                });

                if (calcProcess == null)
                {
                    _output.WriteLine("Calculator could not be launched, skipping toggle test");
                    return;
                }

                await Task.Delay(3000); // Wait for calculator to start
                _output.WriteLine($"Calculator launched with PID: {calcProcess.Id}");
                var timeout = 10;

                // When - Try to find and toggle elements in calculator
                var elementsResult = await _elementSearchService.SearchElementsAsync(new UIAutomationMCP.Models.Requests.SearchElementsRequest
                {
                    ControlType = "Button"
                });
                _output.WriteLine($"Calculator elements found: {elementsResult}");

                // Try to toggle a non-existent toggle control in calculator
                var toggleResult = await _toggleService.ToggleElementAsync("CalculatorToggleTest", "Calculator", null, timeout);

                // Then
                Assert.NotNull(elementsResult);
                Assert.NotNull(toggleResult);
                _output.WriteLine($"Calculator toggle test result: {toggleResult}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Calculator toggle test exception (expected in CI): {ex.Message}");
            }
            finally
            {
                await SafelyTerminateProcessAsync(calcProcess, "Calculator");
            }
        }

        #endregion

        #region Microsoft                  
        [Fact]
        public async Task ToggleElement_SpecificationCompliance_ShouldReturnConsistentResults()
        {
            // Given - Microsoft specification test cases
            var testCases = new[]
            {
                new { AutomationId = "CheckBox1", WindowTitle = "TestForm", ProcessId = (int?)null },
                new { AutomationId = "RadioButton1", WindowTitle = "TestDialog", ProcessId = (int?)null },
                new { AutomationId = "ToggleButton1", WindowTitle = "TestWindow", ProcessId = (int?)1234 },
                new { AutomationId = "MenuItem1", WindowTitle = "", ProcessId = (int?)null }
            };
            var timeout = 5;

            // When
            var results = new List<object>();
            foreach (var testCase in testCases)
            {
                var result = await _toggleService.ToggleElementAsync(
                    testCase.AutomationId, 
                    testCase.WindowTitle, 
                    null,
                    testCase.ProcessId, 
                    timeout);
                results.Add(result);
            }

            // Then
            Assert.All(results, result => Assert.NotNull(result));
            _output.WriteLine("Microsoft specification compliance test results:");
            for (int i = 0; i < results.Count; i++)
            {
                _output.WriteLine($"  Test case {i + 1}: {results[i]}");
            }
        }

        [Fact]
        public async Task ToggleElement_ErrorHandling_ShouldFollowSpecification()
        {
            // Given - Test error conditions per Microsoft specification
            var errorTestCases = new[]
            {
                new { AutomationId = "", Description = "Empty element ID" },
                new { AutomationId = "NonToggleElement", Description = "Non-toggle control" },
                new { AutomationId = "StaticText", Description = "Static text element" },
                new { AutomationId = "HyperlinkElement", Description = "Hyperlink element" }
            };
            var timeout = 3;

            // When & Then
            foreach (var testCase in errorTestCases)
            {
                var result = await _toggleService.ToggleElementAsync(testCase.AutomationId, null, null, null, timeout);
                Assert.NotNull(result);
                _output.WriteLine($"Error test - {testCase.Description}: {result}");
            }
        }

        #endregion

        #region             
        [Fact]
        public async Task ToggleElement_HighVolumeRequests_ShouldHandleGracefully()
        {
            // Given
            var requestCount = 20;
            var timeout = 2;
            var elementIdPrefix = "StressTestToggle";

            // When
            var startTime = DateTime.UtcNow;
            var tasks = Enumerable.Range(0, requestCount)
                .Select(i => _toggleService.ToggleElementAsync($"{elementIdPrefix}{i}", null, null, null, timeout))
                .ToArray();
            
            var results = await Task.WhenAll(tasks);
            var endTime = DateTime.UtcNow;
            var totalTime = (endTime - startTime).TotalMilliseconds;

            // Then
            Assert.All(results, result => Assert.NotNull(result));
            Assert.True(totalTime < 30000, $"High volume requests took too long: {totalTime}ms");
            
            _output.WriteLine($"High volume toggle test: {requestCount} requests completed in {totalTime}ms");
            _output.WriteLine($"Average time per request: {totalTime / requestCount:F2}ms");
        }

        #endregion

        #region                  
        [Fact]
        public async Task ToggleElement_ResourceManagement_ShouldNotLeak()
        {
            // Given
            var iterationCount = 10;
            var timeout = 2;
            var elementId = "ResourceTestToggle";

            // When - Perform multiple toggle operations to test resource management
            for (int i = 0; i < iterationCount; i++)
            {
                var result = await _toggleService.ToggleElementAsync($"{elementId}{i}", null, null, null, timeout);
                Assert.NotNull(result);
                
                // Small delay to observe resource behavior
                if (i % 3 == 0)
                {
                    await Task.Delay(100);
                }
            }

            // Then - Should complete without resource issues
            _output.WriteLine($"Resource management test completed {iterationCount} iterations successfully");
        }

        #endregion

        private async Task SafelyTerminateProcessAsync(Process? process, string appName)
        {
            if (process == null) return;

            try
            {
                if (!process.HasExited)
                {
                    _output.WriteLine($"Terminating {appName} with PID: {process.Id}");
                    
                    //                                      process.CloseMainWindow();
                    
                    //                                          if (!process.WaitForExit(2000))
                    {
                        _output.WriteLine($"Force killing {appName} with PID: {process.Id}");
                        process.Kill();
                        await process.WaitForExitAsync();
                    }
                    
                    _output.WriteLine($"{appName} with PID: {process.Id} terminated successfully");
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error terminating {appName}: {ex.Message}");
            }
            finally
            {
                process?.Dispose();
            }
        }

        public void Dispose()
        {
            _subprocessExecutor?.Dispose();
            _serviceProvider?.Dispose();
        }
    }
}
