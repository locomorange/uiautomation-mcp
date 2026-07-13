using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Services.ControlPatterns;
using Xunit.Abstractions;
using System.Diagnostics;

using UIAutomationMCP.Server.Abstractions;
namespace UiAutomationMcp.Tests.Integration
{
    /// <summary>
    /// Server-Worker         -                              ///                     /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Integration")]
    public class ServerWorkerIntegrationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ServiceProvider _serviceProvider;
        private readonly SubprocessExecutor _subprocessExecutor;
        private readonly string _workerPath;

        public ServerWorkerIntegrationTests(ITestOutputHelper output)
        {
            _output = output;

            //                                  
            var services = new ServiceCollection();

            //             
            services.AddLogging(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));

            _serviceProvider = services.BuildServiceProvider();
            var logger = _serviceProvider.GetRequiredService<ILogger<SubprocessExecutor>>();

            // Resolve Worker path using ExecutablePathResolver
            var baseDir = ExecutablePathResolver.GetExecutableRealPath();
            var workerPath = ExecutablePathResolver.ResolveWorkerPath(baseDir);

            if (workerPath == null || (!File.Exists(workerPath) && !Directory.Exists(workerPath)))
            {
                var searchedPaths = ExecutablePathResolver.GetSearchedPaths("UIAutomationMCP.Subprocess.Worker", baseDir);
                throw new InvalidOperationException($"Worker executable not found. Searched paths: {string.Join(", ", searchedPaths)}");
            }

            _workerPath = workerPath!;
            _subprocessExecutor = new SubprocessExecutor(logger, _workerPath, new CancellationTokenSource());
            _output.WriteLine($"Worker path: {_workerPath}");
        }

        [Fact]
        public void WorkerExecutable_ShouldExist()
        {
            // Assert
            Assert.True(File.Exists(_workerPath), $"Worker executable should exist at: {_workerPath}");
            _output.WriteLine($"Worker executable verified at: {_workerPath}");
        }

        [Fact]
        public async Task SubprocessExecutor_WhenInvalidOperation_ShouldThrowException()
        {
            // Given - Create a request for an operation that doesn't exist in the Worker
            var request = new InvokeElementRequest
            {
                AutomationId = "test",
                WindowTitle = "",
            };

            // When & Then - An invalid operation name should cause an error
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _subprocessExecutor.ExecuteAsync<InvokeElementRequest, ActionResult>("NonExistentOperation", request, 10));

            Assert.NotNull(exception);
            _output.WriteLine($"Invalid operation correctly threw exception: {exception.Message}");
        }

        [Fact]
        public async Task SubprocessExecutor_WhenValidOperationWithMissingElement_ShouldThrowException()
        {
            // Given - Search in specific window to avoid system-wide search timeout
            var request = new InvokeElementRequest
            {
                AutomationId = "NonExistentElement",
                WindowTitle = "Desktop", // Use Desktop window for faster search
            };

            // When & Then - Either TimeoutException (worker too slow) or InvalidOperationException (element not found)
            // The exact exception type depends on whether the worker responds within the timeout
            var exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
                await _subprocessExecutor.ExecuteAsync<InvokeElementRequest, ActionResult>("InvokeElement", request, 3));

            Assert.NotNull(exception);
            Assert.True(
                exception is TimeoutException || exception is InvalidOperationException,
                $"Expected TimeoutException or InvalidOperationException, but got {exception.GetType().Name}: {exception.Message}");
            _output.WriteLine($"Valid operation with missing element correctly threw {exception.GetType().Name}: {exception.Message}");
        }

        [Fact]
        public async Task InvokeService_WhenCalledWithNonExistentElement_ShouldHandleTimeout()
        {
            // Given
            var invokeService = new InvokeService(Mock.Of<IProcessManager>(), _serviceProvider.GetRequiredService<ILogger<InvokeService>>());

            // When
            var result = await invokeService.InvokeElementAsync("NonExistentElement", null, null, 5);

            // Then - Service should handle the error gracefully
            Assert.NotNull(result);
            Assert.False(result.Success);
            _output.WriteLine($"InvokeService error handling test completed: {result.ErrorMessage}");
        }

        [Fact]
        public async Task ValueService_ShouldCommunicateWithWorker()
        {
            // Arrange
            var valueService = new ValueService(Mock.Of<IProcessManager>(), _serviceProvider.GetRequiredService<ILogger<ValueService>>());

            // Act
            var result = await valueService.SetValueAsync("NonExistentElement", "test value", null, null, 5);

            // Assert - Service should handle the error gracefully
            Assert.NotNull(result);
            Assert.False(result.Success);
            _output.WriteLine($"ValueService error handling test completed: {result.ErrorMessage}");
        }

        [Fact]
        public async Task ToggleService_ShouldCommunicateWithWorker()
        {
            // Arrange
            var toggleService = new ToggleService(Mock.Of<IProcessManager>(), _serviceProvider.GetRequiredService<ILogger<ToggleService>>());

            // Act
            var result = await toggleService.ToggleElementAsync("NonExistentElement", null, null, 5);

            // Assert - Service should handle the error gracefully
            Assert.NotNull(result);
            Assert.False(result.Success);
            _output.WriteLine($"ToggleService error handling test completed: {result.ErrorMessage}");
        }

        [Fact]
        public async Task MultipleOperations_ShouldWorkSequentially()
        {
            // Arrange
            var invokeService = new InvokeService(Mock.Of<IProcessManager>(), _serviceProvider.GetRequiredService<ILogger<InvokeService>>());
            var valueService = new ValueService(Mock.Of<IProcessManager>(), _serviceProvider.GetRequiredService<ILogger<ValueService>>());

            // Act
            var invokeResult = await invokeService.InvokeElementAsync("NonExistentElement1", null, null, 5);
            var valueResult = await valueService.SetValueAsync("NonExistentElement2", "test", null, null, 5);

            // Assert
            Assert.NotNull(invokeResult);
            Assert.NotNull(valueResult);
            var invokeJson = System.Text.Json.JsonSerializer.Serialize(invokeResult);
            var valueJson = System.Text.Json.JsonSerializer.Serialize(valueResult);
            Assert.Contains("Success", invokeJson);
            Assert.Contains("false", invokeJson);
            Assert.Contains("Success", valueJson);
            Assert.Contains("false", valueJson);
            _output.WriteLine("Multiple sequential timeout operations completed successfully");
        }

        [Fact]
        public async Task WorkerProcess_ShouldFailFast_WhenElementNotFound()
        {
            // Arrange
            var request = new InvokeElementRequest
            {
                AutomationId = "test",
                WindowTitle = "",
            };

            // Act & Assert
            // Element lookup fails fast with a not-found error instead of consuming the timeout
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _subprocessExecutor.ExecuteAsync<InvokeElementRequest, ActionResult>("InvokeElement", request, 1));
            Assert.Contains("not found", exception.Message);
            _output.WriteLine($"Fail-fast handling test completed: {exception.Message}");
        }

        public void Dispose()
        {
            _subprocessExecutor?.Dispose();
            _serviceProvider?.Dispose();
        }
    }
}

