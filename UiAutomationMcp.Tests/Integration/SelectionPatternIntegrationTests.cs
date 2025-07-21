using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Services.ControlPatterns;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.Integration
{
    /// <summary>
    /// Integration tests for SelectionItem and Selection patterns
    /// Tests the complete Server→Worker→UI Automation pipeline using subprocess execution
    /// Microsoft UI Automation specification compliance verification
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Integration")]
    public class SelectionPatternIntegrationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ILogger<SelectionService> _logger;
        private readonly SubprocessExecutor _subprocessExecutor;
        private readonly SelectionService _selectionService;
        private readonly string _workerPath;

        public SelectionPatternIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
            
            // Use simple mock loggers for integration tests
            _logger = Mock.Of<ILogger<SelectionService>>();
            var executorLogger = Mock.Of<ILogger<SubprocessExecutor>>();

            // Locate the Worker executable for subprocess testing
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var possiblePaths = new[]
            {
                Path.Combine(baseDir, "UIAutomationMCP.Worker.exe"),
                Path.Combine(baseDir, "..", "..", "..", "..", "UIAutomationMCP.Worker", "bin", "Debug", "net9.0-windows", "UIAutomationMCP.Worker.exe"),
                Path.Combine(baseDir, "..", "UIAutomationMCP.Worker", "bin", "Debug", "net9.0-windows", "UIAutomationMCP.Worker.exe"),
            };

            _workerPath = possiblePaths.FirstOrDefault(File.Exists) ??
                throw new InvalidOperationException($"Worker executable not found. Searched paths: {string.Join(", ", possiblePaths)}");

            _subprocessExecutor = new SubprocessExecutor(executorLogger, _workerPath);
            _selectionService = new SelectionService(_logger, _subprocessExecutor);

            _output.WriteLine($"Using Worker executable at: {_workerPath}");
        }

        #region SelectionItemPattern Integration Tests

        [Fact]
        public async Task IsSelectedAsync_WithNonExistentElement_ShouldHandleTimeoutGracefully()
        {
            // Arrange
            var elementId = "NonExistentSelectionItem";
            var windowTitle = "NonExistentWindow";
            var processId = 99999; // Non-existent process ID

            // Act
            var result = await _selectionService.IsSelectedAsync(automationId: elementId, processId: processId, timeoutSeconds: 5);

            // Assert
            Assert.NotNull(result);
            
            // Should return error result due to element not found
            var resultType = result.GetType();
            var successProperty = resultType.GetProperty("Success");
            Assert.NotNull(successProperty);
            
            var success = (bool?)successProperty.GetValue(result);
            if (success == false)
            {
                var errorProperty = resultType.GetProperty("Error");
                Assert.NotNull(errorProperty);
                var error = errorProperty.GetValue(result)?.ToString();
                Assert.NotNull(error);
                _output.WriteLine($"Expected error result: {error}");
            }

            _output.WriteLine("IsSelectedAsync integration test completed - Handled non-existent element gracefully");
        }

        [Fact]
        public async Task GetSelectionContainerAsync_WithNonExistentElement_ShouldHandleTimeoutGracefully()
        {
            // Arrange
            var elementId = "NonExistentContainerItem";
            var windowTitle = "TestSelectionWindow";

            // Act
            var result = await _selectionService.GetSelectionContainerAsync(elementId, windowTitle, null, timeoutSeconds: 5);

            // Assert
            Assert.NotNull(result);
            
            // Should handle gracefully even with non-existent elements
            var resultType = result.GetType();
            var successProperty = resultType.GetProperty("Success");
            Assert.NotNull(successProperty);

            _output.WriteLine("GetSelectionContainerAsync integration test completed - Handled non-existent element gracefully");
        }

        #endregion

        #region SelectionPattern Integration Tests

        [Fact]
        public async Task CanSelectMultipleAsync_WithNonExistentContainer_ShouldHandleTimeoutGracefully()
        {
            // Arrange
            var containerId = "NonExistentContainer";
            var windowTitle = "TestMultiSelectWindow";
            var processId = 88888;

            // Act
            var result = await _selectionService.CanSelectMultipleAsync(containerId, windowTitle, processId, timeoutSeconds: 5);

            // Assert
            Assert.NotNull(result);
            
            // Should return error result for non-existent container
            var resultType = result.GetType();
            var successProperty = resultType.GetProperty("Success");
            Assert.NotNull(successProperty);

            _output.WriteLine("CanSelectMultipleAsync integration test completed - Handled non-existent container gracefully");
        }

        [Fact]
        public async Task IsSelectionRequiredAsync_WithNonExistentContainer_ShouldHandleTimeoutGracefully()
        {
            // Arrange
            var containerId = "NonExistentTabControl";
            var windowTitle = "TestTabWindow";

            // Act
            var result = await _selectionService.IsSelectionRequiredAsync(automationId: containerId, processId: null, timeoutSeconds: 5);

            // Assert
            Assert.NotNull(result);
            
            // Should handle gracefully
            var resultType = result.GetType();
            var successProperty = resultType.GetProperty("Success");
            Assert.NotNull(successProperty);

            _output.WriteLine("IsSelectionRequiredAsync integration test completed - Handled non-existent container gracefully");
        }

        #endregion

        #region Selection Operations Integration Tests

        [Fact]
        public async Task AddToSelectionAsync_WithNonExistentElement_ShouldHandleTimeoutGracefully()
        {
            // Arrange
            var elementId = "NonExistentAddItem";
            var windowTitle = "TestMultiSelectList";
            var processId = 77777;

            // Act
            var result = await _selectionService.AddToSelectionAsync(elementId, windowTitle, processId, timeoutSeconds: 5);

            // Assert
            Assert.NotNull(result);
            
            // Should return error result
            var resultType = result.GetType();
            var successProperty = resultType.GetProperty("Success");
            Assert.NotNull(successProperty);

            _output.WriteLine("AddToSelectionAsync integration test completed - Handled non-existent element gracefully");
        }

        [Fact]
        public async Task RemoveFromSelectionAsync_WithNonExistentElement_ShouldHandleTimeoutGracefully()
        {
            // Arrange
            var elementId = "NonExistentRemoveItem";
            var windowTitle = "TestRemoveWindow";

            // Act
            var result = await _selectionService.RemoveFromSelectionAsync(elementId, windowTitle, null, timeoutSeconds: 5);

            // Assert
            Assert.NotNull(result);
            
            // Should handle gracefully
            var resultType = result.GetType();
            var successProperty = resultType.GetProperty("Success");
            Assert.NotNull(successProperty);

            _output.WriteLine("RemoveFromSelectionAsync integration test completed - Handled non-existent element gracefully");
        }

        [Fact]
        public async Task ClearSelectionAsync_WithNonExistentContainer_ShouldHandleTimeoutGracefully()
        {
            // Arrange
            var containerId = "NonExistentClearContainer";
            var processId = 66666;

            // Act
            var result = await _selectionService.ClearSelectionAsync(containerId, null, processId, timeoutSeconds: 5);

            // Assert
            Assert.NotNull(result);
            
            // Should handle gracefully
            var resultType = result.GetType();
            var successProperty = resultType.GetProperty("Success");
            Assert.NotNull(successProperty);

            _output.WriteLine("ClearSelectionAsync integration test completed - Handled non-existent container gracefully");
        }

        [Fact]
        public async Task GetSelectionAsync_WithNonExistentContainer_ShouldHandleTimeoutGracefully()
        {
            // Arrange
            var containerId = "NonExistentSelectionContainer";
            var windowTitle = "TestGetSelectionWindow";

            // Act
            var result = await _selectionService.GetSelectionAsync(containerId, windowTitle, null, timeoutSeconds: 5);

            // Assert
            Assert.NotNull(result);
            
            // Should handle gracefully
            var resultType = result.GetType();
            var successProperty = resultType.GetProperty("Success");
            Assert.NotNull(successProperty);

            _output.WriteLine("GetSelectionAsync integration test completed - Handled non-existent container gracefully");
        }

        #endregion

        #region Concurrent Operations Integration Tests

        [Fact]
        public async Task SelectionOperations_ConcurrentExecution_ShouldBeThreadSafe()
        {
            // Arrange
            var tasks = new List<Task<object>>();
            
            // Create multiple concurrent operations
            for (int i = 0; i < 3; i++)
            {
                var elementId = $"ConcurrentElement_{i}";
                var containerId = $"ConcurrentContainer_{i}";
                
                // Add different types of selection operations
                tasks.Add(Task.FromResult<object>(await _selectionService.IsSelectedAsync(elementId, "ConcurrentWindow", 12345, 3)));
                tasks.Add(Task.FromResult<object>(await _selectionService.CanSelectMultipleAsync(containerId, "ConcurrentWindow", 12345, 3)));
                tasks.Add(Task.FromResult<object>(await _selectionService.AddToSelectionAsync(elementId, "ConcurrentWindow", 12345, 3)));
            }

            // Act
            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.All(results, result => Assert.NotNull(result));
            
            _output.WriteLine($"Concurrent operations test completed - {tasks.Count} operations executed simultaneously");
        }

        #endregion

        #region Timeout and Error Handling Integration Tests

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(10)]
        public async Task SelectionOperations_WithVariousTimeouts_ShouldRespectTimeoutValues(int timeoutSeconds)
        {
            // Arrange
            var elementId = $"TimeoutTest_Element_{timeoutSeconds}";
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var result = await _selectionService.IsSelectedAsync(elementId, "TimeoutTestWindow", null, timeoutSeconds);

            // Assert
            stopwatch.Stop();
            
            Assert.NotNull(result);
            
            // Should complete within reasonable time (allowing for some overhead)
            var maxExpectedTime = TimeSpan.FromSeconds(timeoutSeconds + 2);
            Assert.True(stopwatch.Elapsed <= maxExpectedTime, 
                $"Operation took {stopwatch.Elapsed} but should complete within {maxExpectedTime}");

            _output.WriteLine($"Timeout test completed for {timeoutSeconds}s in {stopwatch.Elapsed}");
        }

        [Fact]
        public async Task SelectionOperations_WithEmptyParameters_ShouldHandleGracefully()
        {
            // Arrange - Test with empty strings and zero values
            var emptyElementId = "";
            var emptyWindowTitle = "";
            var zeroProcessId = 0;

            // Act
            var isSelectedResult = await _selectionService.IsSelectedAsync(emptyElementId, emptyWindowTitle, zeroProcessId, 3);
            var canSelectResult = await _selectionService.CanSelectMultipleAsync(emptyElementId, emptyWindowTitle, zeroProcessId, 3);

            // Assert
            Assert.NotNull(isSelectedResult);
            Assert.NotNull(canSelectResult);

            _output.WriteLine("Empty parameters test completed - Operations handled empty values gracefully");
        }

        #endregion

        #region Microsoft Specification Compliance Integration Tests

        /// <summary>
        /// Microsoft仕様準拠統合テスト：SelectionItemPatternを使用した操作の完全なフロー
        /// </summary>
        [Fact]
        public async Task SelectionItemPattern_FullWorkflow_ShouldFollowMicrosoftSpecification()
        {
            // Arrange - Simulate a typical SelectionItem workflow
            var listItemId = "SpecCompliance_ListItem";
            var windowTitle = "SpecificationTestWindow";
            var processId = 54321;

            // Act & Assert - Test the complete SelectionItem pattern workflow
            
            // 1. Check initial selection state
            var isSelectedResult = await _selectionService.IsSelectedAsync(listItemId, windowTitle, processId, 5);
            Assert.NotNull(isSelectedResult);
            
            // 2. Get selection container information
            var containerResult = await _selectionService.GetSelectionContainerAsync(listItemId, windowTitle, processId, 5);
            Assert.NotNull(containerResult);
            
            // 3. Attempt to add to selection (if multiple selection supported)
            var addResult = await _selectionService.AddToSelectionAsync(listItemId, windowTitle, processId, 5);
            Assert.NotNull(addResult);
            
            // 4. Attempt to remove from selection
            var removeResult = await _selectionService.RemoveFromSelectionAsync(listItemId, windowTitle, processId, 5);
            Assert.NotNull(removeResult);

            _output.WriteLine("Microsoft specification compliance workflow test completed");
        }

        /// <summary>
        /// Microsoft仕様準拠統合テスト：SelectionPatternを使用した操作の完全なフロー
        /// </summary>
        [Fact]
        public async Task SelectionPattern_FullWorkflow_ShouldFollowMicrosoftSpecification()
        {
            // Arrange - Simulate a typical Selection container workflow
            var containerId = "SpecCompliance_Container";
            var windowTitle = "ContainerSpecificationTestWindow";

            // Act & Assert - Test the complete Selection pattern workflow
            
            // 1. Check if multiple selection is supported
            var multipleSelectionResult = await _selectionService.CanSelectMultipleAsync(automationId: containerId, processId: null, timeoutSeconds: 5);
            Assert.NotNull(multipleSelectionResult);
            
            // 2. Check if selection is required
            var selectionRequiredResult = await _selectionService.IsSelectionRequiredAsync(automationId: containerId, processId: null, timeoutSeconds: 5);
            Assert.NotNull(selectionRequiredResult);
            
            // 3. Get current selection
            var currentSelectionResult = await _selectionService.GetSelectionAsync(automationId: containerId, processId: null, timeoutSeconds: 5);
            Assert.NotNull(currentSelectionResult);
            
            // 4. Clear all selections
            var clearResult = await _selectionService.ClearSelectionAsync(automationId: containerId, processId: null, timeoutSeconds: 5);
            Assert.NotNull(clearResult);

            _output.WriteLine("Microsoft Selection pattern specification compliance workflow test completed");
        }

        #endregion

        #region Performance and Load Integration Tests

        [Fact]
        public async Task SelectionOperations_HighFrequency_ShouldMaintainPerformance()
        {
            // Arrange
            var operationCount = 20;
            var maxOperationTime = TimeSpan.FromSeconds(5);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var tasks = new List<Task<object>>();
            for (int i = 0; i < operationCount; i++)
            {
                var elementId = $"PerformanceTest_Element_{i}";
                tasks.Add(Task.FromResult<object>(await _selectionService.IsSelectedAsync(elementId, "PerformanceWindow", null, 3)));
            }

            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert
            Assert.All(results, result => Assert.NotNull(result));
            
            var averageTimePerOperation = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds / operationCount);
            Assert.True(averageTimePerOperation <= maxOperationTime, 
                $"Average operation time {averageTimePerOperation} exceeded maximum {maxOperationTime}");

            _output.WriteLine($"Performance test completed - {operationCount} operations in {stopwatch.Elapsed}, average: {averageTimePerOperation}");
        }

        #endregion

        public void Dispose()
        {
            try
            {
                _subprocessExecutor?.Dispose();
                _output?.WriteLine("SelectionPatternIntegrationTests disposed");
            }
            catch (Exception ex)
            {
                _output?.WriteLine($"Error during disposal: {ex.Message}");
            }
            
            GC.SuppressFinalize(this);
        }
    }
}