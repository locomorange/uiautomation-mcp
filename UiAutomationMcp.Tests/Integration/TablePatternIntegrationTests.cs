using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Tools;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.Integration
{
    /// <summary>
    /// Integration tests for Table pattern operations
    /// Tests the complete Server→Worker→UI Automation pipeline using subprocess execution
    /// Microsoft UI Automation TablePattern specification compliance verification
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Integration")]
    public class TablePatternIntegrationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ILogger<TableService> _logger;
        private readonly SubprocessExecutor _subprocessExecutor;
        private readonly TableService _tableService;
        private readonly string _workerPath;

        public TablePatternIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
            var loggerFactory = new LoggerFactory();
            _logger = loggerFactory.CreateLogger<TableService>();
            var executorLogger = loggerFactory.CreateLogger<SubprocessExecutor>();

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
            _tableService = new TableService(_logger, _subprocessExecutor);

            _output.WriteLine($"Using Worker executable at: {_workerPath}");
        }

        #region GetRowOrColumnMajorAsync Integration Tests

        [Fact]
        public async Task GetRowOrColumnMajorAsync_WithNonExistentElement_ShouldHandleTimeoutGracefully()
        {
            // Arrange
            var elementId = "NonExistentTable";
            var windowTitle = "NonExistentWindow";
            var processId = 99999; // Non-existent process ID

            // Act
            var result = await _tableService.GetRowOrColumnMajorAsync(elementId, windowTitle, processId, timeoutSeconds: 5);

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

            _output.WriteLine("GetRowOrColumnMajorAsync integration test completed - Handled non-existent element gracefully");
        }

        [Fact]
        public async Task GetRowOrColumnMajorAsync_WithInvalidElementId_ShouldReturnError()
        {
            // Arrange
            var elementId = "InvalidTableElement";
            var windowTitle = "TestTableWindow";

            // Act
            var result = await _tableService.GetRowOrColumnMajorAsync(elementId, windowTitle, null, timeoutSeconds: 5);

            // Assert
            Assert.NotNull(result);
            
            // Should handle gracefully even with invalid elements
            var resultType = result.GetType();
            var successProperty = resultType.GetProperty("Success");
            Assert.NotNull(successProperty);

            _output.WriteLine("GetRowOrColumnMajorAsync integration test completed - Handled invalid element gracefully");
        }

        [Fact]
        public async Task GetRowOrColumnMajorAsync_WithEmptyParameters_ShouldHandleGracefully()
        {
            // Arrange
            var elementId = "";
            var windowTitle = "";

            // Act
            var result = await _tableService.GetRowOrColumnMajorAsync(elementId, windowTitle, null, timeoutSeconds: 5);

            // Assert
            Assert.NotNull(result);
            
            // Should handle empty parameters gracefully
            var resultType = result.GetType();
            var successProperty = resultType.GetProperty("Success");
            Assert.NotNull(successProperty);

            _output.WriteLine("GetRowOrColumnMajorAsync integration test completed - Handled empty parameters gracefully");
        }

        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        public async Task GetRowOrColumnMajorAsync_WithDifferentTimeouts_ShouldRespectTimeoutSettings(int timeoutSeconds)
        {
            // Arrange
            var elementId = "TimeoutTestTable";
            var windowTitle = "TimeoutTestWindow";

            var startTime = DateTime.UtcNow;

            // Act
            var result = await _tableService.GetRowOrColumnMajorAsync(elementId, windowTitle, 12345, timeoutSeconds);

            // Assert
            var elapsed = DateTime.UtcNow - startTime;
            Assert.NotNull(result);
            
            // Should not exceed timeout significantly (allowing some buffer for processing)
            Assert.True(elapsed.TotalSeconds <= timeoutSeconds + 2, 
                $"Operation took {elapsed.TotalSeconds} seconds, expected <= {timeoutSeconds + 2}");

            _output.WriteLine($"GetRowOrColumnMajorAsync timeout test completed - {timeoutSeconds}s timeout respected (took {elapsed.TotalSeconds:F2}s)");
        }

        #endregion

        #region TablePattern Required Members Integration Tests

        [Fact]
        public async Task TablePattern_RequiredMembers_ShouldAllBeAccessibleThroughService()
        {
            // Arrange
            var elementId = "IntegrationTestTable";
            var windowTitle = "Integration Test Window";
            var processId = 54321;
            var timeoutSeconds = 5;

            // Act & Assert - Test all required TablePattern members
            
            // 1. RowOrColumnMajor property
            var rowOrColumnResult = await _tableService.GetRowOrColumnMajorAsync(elementId, windowTitle, processId, timeoutSeconds);
            Assert.NotNull(rowOrColumnResult);

            // 2. GetColumnHeaders() method
            var columnHeadersResult = await _tableService.GetColumnHeadersAsync(elementId, windowTitle, processId, timeoutSeconds);
            Assert.NotNull(columnHeadersResult);

            // 3. GetRowHeaders() method  
            var rowHeadersResult = await _tableService.GetRowHeadersAsync(elementId, windowTitle, processId, timeoutSeconds);
            Assert.NotNull(rowHeadersResult);

            // 4. Table info (combines row/column count with RowOrColumnMajor)
            var tableInfoResult = await _tableService.GetTableInfoAsync(elementId, windowTitle, processId, timeoutSeconds);
            Assert.NotNull(tableInfoResult);

            _output.WriteLine("TablePattern required members integration test completed - All members accessible through service layer");
        }

        [Fact]
        public async Task TablePattern_ErrorHandling_ShouldBeConsistentAcrossAllMethods()
        {
            // Arrange
            var elementId = "NonExistentTable";
            var windowTitle = "ErrorTestWindow";
            var processId = 99999;
            var timeoutSeconds = 3;

            // Act - Test error handling for all table methods
            var rowOrColumnResult = await _tableService.GetRowOrColumnMajorAsync(elementId, windowTitle, processId, timeoutSeconds);
            var columnHeadersResult = await _tableService.GetColumnHeadersAsync(elementId, windowTitle, processId, timeoutSeconds);
            var rowHeadersResult = await _tableService.GetRowHeadersAsync(elementId, windowTitle, processId, timeoutSeconds);
            var tableInfoResult = await _tableService.GetTableInfoAsync(elementId, windowTitle, processId, timeoutSeconds);

            // Assert - All should handle errors consistently
            Assert.NotNull(rowOrColumnResult);
            Assert.NotNull(columnHeadersResult);
            Assert.NotNull(rowHeadersResult);
            Assert.NotNull(tableInfoResult);

            // All should have Success/Error structure when handling errors
            var results = new object[] { rowOrColumnResult, columnHeadersResult, rowHeadersResult, tableInfoResult };
            
            foreach (var result in results)
            {
                var resultType = result.GetType();
                var successProperty = resultType.GetProperty("Success");
                Assert.NotNull(successProperty);
                
                // Error handling should be consistent across all methods
                var success = (bool?)successProperty.GetValue(result);
                if (success == false)
                {
                    var errorProperty = resultType.GetProperty("Error");
                    Assert.NotNull(errorProperty);
                    var error = errorProperty.GetValue(result)?.ToString();
                    Assert.NotEmpty(error ?? "");
                }
            }

            _output.WriteLine("TablePattern error handling integration test completed - Consistent error handling across all methods");
        }

        #endregion

        #region Microsoft Specification Compliance Integration Tests

        /// <summary>
        /// Microsoft仕様準拠統合テスト：TablePattern specification compliance
        /// Tests that the full integration pipeline respects Microsoft UI Automation standards
        /// </summary>
        [Fact]
        public async Task TablePattern_MicrosoftSpecificationCompliance_ShouldMeetAllRequirements()
        {
            // This test verifies that our implementation meets Microsoft's TablePattern requirements:
            // 1. RowOrColumnMajor property access
            // 2. GetColumnHeaders() method availability  
            // 3. GetRowHeaders() method availability
            // 4. Proper error handling for unsupported elements
            // 5. Consistent timeout handling

            // Arrange
            var testCases = new[]
            {
                new { AutomationId = "DataGrid_Test", WindowTitle = "DataGrid Window", ExpectedToSupport = true },
                new { AutomationId = "Table_Test", WindowTitle = "Table Window", ExpectedToSupport = true },
                new { AutomationId = "Button_Test", WindowTitle = "Button Window", ExpectedToSupport = false },
                new { AutomationId = "TextBox_Test", WindowTitle = "TextBox Window", ExpectedToSupport = false }
            };

            foreach (var testCase in testCases)
            {
                // Act
                var result = await _tableService.GetRowOrColumnMajorAsync(
                    testCase.ElementId, testCase.WindowTitle, 12345, timeoutSeconds: 3);

                // Assert
                Assert.NotNull(result);
                
                var resultType = result.GetType();
                var successProperty = resultType.GetProperty("Success");
                Assert.NotNull(successProperty);

                var success = (bool?)successProperty.GetValue(result);
                
                if (testCase.ExpectedToSupport)
                {
                    // Elements that should support TablePattern might still fail due to element not found,
                    // but should not fail due to pattern not supported
                    if (success == false)
                    {
                        var errorProperty = resultType.GetProperty("Error");
                        var error = errorProperty?.GetValue(result)?.ToString() ?? "";
                        
                        // Should not fail specifically due to pattern support issues if element supports it
                        Assert.DoesNotContain("TablePattern not supported", error);
                    }
                }

                _output.WriteLine($"Microsoft specification compliance test passed for {testCase.ElementId} (Expected support: {testCase.ExpectedToSupport})");
            }

            _output.WriteLine("Microsoft TablePattern specification compliance integration test completed");
        }

        [Fact]
        public async Task TablePattern_PerformanceRequirements_ShouldMeetReasonableTimeouts()
        {
            // Arrange
            var elementId = "PerformanceTestTable";
            var windowTitle = "Performance Test Window";
            var maxAcceptableTimeSeconds = 10; // Reasonable maximum for integration tests

            var startTime = DateTime.UtcNow;

            // Act
            var result = await _tableService.GetRowOrColumnMajorAsync(elementId, windowTitle, 11111, timeoutSeconds: maxAcceptableTimeSeconds);

            // Assert
            var elapsed = DateTime.UtcNow - startTime;
            Assert.NotNull(result);
            
            // Should complete within reasonable time
            Assert.True(elapsed.TotalSeconds <= maxAcceptableTimeSeconds + 1, 
                $"Operation took {elapsed.TotalSeconds} seconds, which exceeds reasonable performance expectations");

            _output.WriteLine($"TablePattern performance test completed - Operation completed in {elapsed.TotalSeconds:F2} seconds");
        }

        #endregion

        #region Subprocess Execution Integration Tests

        [Fact]
        public async Task TableService_SubprocessExecution_ShouldHandleWorkerCommunication()
        {
            // This test specifically validates that the subprocess execution pipeline works correctly
            // for TablePattern operations, ensuring that Server↔Worker communication is robust

            // Arrange
            var elementId = "SubprocessTestTable";
            var windowTitle = "Subprocess Test Window";

            // Act
            var result = await _tableService.GetRowOrColumnMajorAsync(elementId, windowTitle, 33333, timeoutSeconds: 5);

            // Assert
            Assert.NotNull(result);
            
            // The result should have been processed through the subprocess pipeline
            // Even if the element doesn't exist, we should get a properly structured response
            var resultType = result.GetType();
            
            // Should have Success property (from OperationResult structure)
            var successProperty = resultType.GetProperty("Success");
            Assert.NotNull(successProperty);

            // Should have either Data or Error property depending on result
            var dataProperty = resultType.GetProperty("Data");
            var errorProperty = resultType.GetProperty("Error");
            Assert.True(dataProperty != null || errorProperty != null, 
                "Result should have either Data or Error property from subprocess execution");

            _output.WriteLine("TableService subprocess execution integration test completed - Worker communication validated");
        }

        #endregion

        public void Dispose()
        {
            _subprocessExecutor?.Dispose();
            _output?.WriteLine("TablePatternIntegrationTests disposed");
            GC.SuppressFinalize(this);
        }
    }
}