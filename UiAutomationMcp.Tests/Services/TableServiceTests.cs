using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Shared.Results;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.Services
{
    /// <summary>
    /// Mock-based tests for ITableService implementations
    /// Tests the service layer without direct UI Automation dependencies
    /// Tests Microsoft UI Automation TablePattern specification compliance
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class TableServiceTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ILogger<TableService>> _mockLogger;
        private readonly Mock<ISubprocessExecutor> _mockExecutor;
        private readonly TableService _tableService;

        public TableServiceTests(ITestOutputHelper output)
        {
            _output = output;
            _mockLogger = new Mock<ILogger<TableService>>();
            _mockExecutor = new Mock<ISubprocessExecutor>();
            
            _tableService = new TableService(_mockLogger.Object, _mockExecutor.Object);
        }

        #region GetRowOrColumnMajorAsync Tests

        [Fact]
        public async Task GetRowOrColumnMajorAsync_WhenCalled_ShouldExecuteCorrectOperation()
        {
            // Arrange
            var elementId = "dataGrid1";
            var windowTitle = "Test Window";
            var processId = 1234;
            var expectedResult = new PropertyResult { Success = true };

            _mockExecutor.Setup(e => e.ExecuteAsync<PropertyResult>("GetRowOrColumnMajor", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tableService.GetRowOrColumnMajorAsync(elementId, windowTitle, processId, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<PropertyResult>("GetRowOrColumnMajor", 
                It.Is<Dictionary<string, object>>(p => 
                    p["elementId"].ToString() == elementId &&
                    p["windowTitle"].ToString() == windowTitle &&
                    p["processId"].ToString() == processId.ToString()), 30), Times.Once);
            
            _output.WriteLine("GetRowOrColumnMajorAsync service test passed - Correct subprocess execution verified");
        }

        [Fact]
        public async Task GetRowOrColumnMajorAsync_WithNullParameters_ShouldHandleGracefully()
        {
            // Arrange
            var elementId = "table1";
            var expectedResult = new PropertyResult { Success = true };

            _mockExecutor.Setup(e => e.ExecuteAsync<PropertyResult>("GetRowOrColumnMajor", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tableService.GetRowOrColumnMajorAsync(elementId, null, null, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<PropertyResult>("GetRowOrColumnMajor", 
                It.Is<Dictionary<string, object>>(p => 
                    p["elementId"].ToString() == elementId &&
                    p["windowTitle"].ToString() == "" &&
                    p["processId"].ToString() == "0"), 30), Times.Once);
            
            _output.WriteLine("GetRowOrColumnMajorAsync null parameters test passed");
        }

        [Fact]
        public async Task GetRowOrColumnMajorAsync_WhenExecutorThrows_ShouldReturnErrorResult()
        {
            // Arrange
            var elementId = "failingTable";
            var windowTitle = "Test Window";
            var expectedError = "TablePattern not supported";

            _mockExecutor.Setup(e => e.ExecuteAsync<PropertyResult>("GetRowOrColumnMajor", It.IsAny<Dictionary<string, object>>(), 30))
                .ThrowsAsync(new InvalidOperationException(expectedError));

            // Act
            var result = await _tableService.GetRowOrColumnMajorAsync(elementId, windowTitle, null, 30);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(expectedError, result.ErrorMessage);
            
            _output.WriteLine("GetRowOrColumnMajorAsync error handling test passed");
        }

        [Theory]
        [InlineData("RowMajor")]
        [InlineData("ColumnMajor")]
        [InlineData("Indeterminate")]
        public async Task GetRowOrColumnMajorAsync_WithAllValidValues_ShouldReturnCorrectly(string expectedValue)
        {
            // Arrange
            var elementId = "testTable";
            var expectedResult = new PropertyResult { Success = true };

            _mockExecutor.Setup(e => e.ExecuteAsync<PropertyResult>("GetRowOrColumnMajor", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tableService.GetRowOrColumnMajorAsync(elementId, "Test Window", 1234, 30);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(expectedValue, result.Data.Value);
            
            _output.WriteLine($"GetRowOrColumnMajorAsync test passed for value: {expectedValue}");
        }

        #endregion

        #region GetTableInfoAsync Tests

        [Fact]
        public async Task GetTableInfoAsync_WhenCalled_ShouldExecuteCorrectOperation()
        {
            // Arrange
            var elementId = "dataGrid1";
            var windowTitle = "Test Window";
            var processId = 1234;
            var expectedResult = new TableInfoResult 
            { 
                Success = true, 
                RowCount = 10, 
                ColumnCount = 5, 
                RowOrColumnMajor = "RowMajor" 
            };

            _mockExecutor.Setup(e => e.ExecuteAsync<TableInfoResult>("GetTableInfo", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tableService.GetTableInfoAsync(elementId, windowTitle, processId, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<TableInfoResult>("GetTableInfo", 
                It.Is<Dictionary<string, object>>(p => 
                    p["elementId"].ToString() == elementId &&
                    p["windowTitle"].ToString() == windowTitle &&
                    p["processId"].ToString() == processId.ToString()), 30), Times.Once);
            
            _output.WriteLine("GetTableInfoAsync service test passed");
        }

        #endregion

        #region GetRowHeadersAsync Tests

        [Fact]
        public async Task GetRowHeadersAsync_WhenCalled_ShouldExecuteCorrectOperation()
        {
            // Arrange
            var elementId = "table1";
            var expectedResult = new ElementSearchResult 
            { 
                Success = true,
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo { Name = "Row1" },
                    new UIAutomationMCP.Shared.ElementInfo { Name = "Row2" },
                    new UIAutomationMCP.Shared.ElementInfo { Name = "Row3" }
                }
            };

            _mockExecutor.Setup(e => e.ExecuteAsync<ElementSearchResult>("GetRowHeaders", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tableService.GetRowHeadersAsync(elementId, "Test Window", 1234, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<ElementSearchResult>("GetRowHeaders", 
                It.Is<Dictionary<string, object>>(p => 
                    p["elementId"].ToString() == elementId), 30), Times.Once);
            
            _output.WriteLine("GetRowHeadersAsync service test passed");
        }

        #endregion

        #region GetColumnHeadersAsync Tests

        [Fact]
        public async Task GetColumnHeadersAsync_WhenCalled_ShouldExecuteCorrectOperation()
        {
            // Arrange
            var elementId = "table1";
            var expectedResult = new ElementSearchResult 
            { 
                Success = true,
                Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                {
                    new UIAutomationMCP.Shared.ElementInfo { Name = "Col1" },
                    new UIAutomationMCP.Shared.ElementInfo { Name = "Col2" },
                    new UIAutomationMCP.Shared.ElementInfo { Name = "Col3" }
                }
            };

            _mockExecutor.Setup(e => e.ExecuteAsync<ElementSearchResult>("GetColumnHeaders", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tableService.GetColumnHeadersAsync(elementId, "Test Window", 1234, 30);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<ElementSearchResult>("GetColumnHeaders", 
                It.Is<Dictionary<string, object>>(p => 
                    p["elementId"].ToString() == elementId), 30), Times.Once);
            
            _output.WriteLine("GetColumnHeadersAsync service test passed");
        }

        #endregion

        #region Microsoft Specification Compliance Tests

        /// <summary>
        /// Microsoft仕様準拠テスト：TablePattern Required Members
        /// - RowOrColumnMajor property (tested above)
        /// - GetColumnHeaders() method
        /// - GetRowHeaders() method
        /// </summary>
        [Fact]
        public async Task TableService_ShouldImplementAllRequiredMembers()
        {
            // Arrange
            var elementId = "compliantTable";
            var windowTitle = "Test App";
            var processId = 1234;

            // Setup mock responses for all required members
            _mockExecutor.Setup(e => e.ExecuteAsync<PropertyResult>("GetRowOrColumnMajor", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(new PropertyResult { Success = true }));
            
            _mockExecutor.Setup(e => e.ExecuteAsync<ElementSearchResult>("GetColumnHeaders", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(new ElementSearchResult 
                { 
                    Success = true,
                    Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                    {
                        new UIAutomationMCP.Shared.ElementInfo { Name = "Header1" },
                        new UIAutomationMCP.Shared.ElementInfo { Name = "Header2" }
                    }
                }));
            
            _mockExecutor.Setup(e => e.ExecuteAsync<ElementSearchResult>("GetRowHeaders", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(new ElementSearchResult 
                { 
                    Success = true,
                    Elements = new List<UIAutomationMCP.Shared.ElementInfo>
                    {
                        new UIAutomationMCP.Shared.ElementInfo { Name = "Row1" },
                        new UIAutomationMCP.Shared.ElementInfo { Name = "Row2" }
                    }
                }));

            // Act & Assert - Test all required members
            var rowOrColumnResult = await _tableService.GetRowOrColumnMajorAsync(elementId, windowTitle, processId, 30);
            var columnHeadersResult = await _tableService.GetColumnHeadersAsync(elementId, windowTitle, processId, 30);
            var rowHeadersResult = await _tableService.GetRowHeadersAsync(elementId, windowTitle, processId, 30);

            Assert.NotNull(rowOrColumnResult);
            Assert.NotNull(columnHeadersResult);
            Assert.NotNull(rowHeadersResult);

            // Verify all operations were called
            _mockExecutor.Verify(e => e.ExecuteAsync<PropertyResult>("GetRowOrColumnMajor", It.IsAny<Dictionary<string, object>>(), 30), Times.Once);
            _mockExecutor.Verify(e => e.ExecuteAsync<ElementSearchResult>("GetColumnHeaders", It.IsAny<Dictionary<string, object>>(), 30), Times.Once);
            _mockExecutor.Verify(e => e.ExecuteAsync<ElementSearchResult>("GetRowHeaders", It.IsAny<Dictionary<string, object>>(), 30), Times.Once);

            _output.WriteLine("Microsoft specification compliance test passed - All required TablePattern members implemented");
        }

        [Theory]
        [InlineData(5)]
        [InlineData(15)]
        [InlineData(30)]
        [InlineData(60)]
        public async Task TableService_ShouldRespectTimeoutParameter(int timeout)
        {
            // Arrange
            var elementId = "timeoutTest";
            var expectedResult = new PropertyResult { Success = true };

            _mockExecutor.Setup(e => e.ExecuteAsync<PropertyResult>("GetRowOrColumnMajor", It.IsAny<Dictionary<string, object>>(), timeout))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tableService.GetRowOrColumnMajorAsync(elementId, "Test", 1234, timeout);

            // Assert
            Assert.NotNull(result);
            _mockExecutor.Verify(e => e.ExecuteAsync<PropertyResult>("GetRowOrColumnMajor", It.IsAny<Dictionary<string, object>>(), timeout), Times.Once);
            
            _output.WriteLine($"Timeout parameter test passed for {timeout} seconds");
        }

        #endregion

        #region Logging Verification Tests

        [Fact]
        public async Task GetRowOrColumnMajorAsync_ShouldLogCorrectly()
        {
            // Arrange
            var elementId = "logTest";
            var expectedResult = new PropertyResult { Success = true };

            _mockExecutor.Setup(e => e.ExecuteAsync<PropertyResult>("GetRowOrColumnMajor", It.IsAny<Dictionary<string, object>>(), 30))
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tableService.GetRowOrColumnMajorAsync(elementId, "Test", 1234, 30);

            // Assert
            Assert.NotNull(result);
            
            // Verify logging calls were made
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting RowOrColumnMajor property for element")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("RowOrColumnMajor property retrieved successfully")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _output.WriteLine("Logging verification test passed");
        }

        [Fact]
        public async Task GetRowOrColumnMajorAsync_WhenError_ShouldLogError()
        {
            // Arrange
            var elementId = "errorTest";
            var error = new InvalidOperationException("Test error");

            _mockExecutor.Setup(e => e.ExecuteAsync<PropertyResult>("GetRowOrColumnMajor", It.IsAny<Dictionary<string, object>>(), 30))
                .ThrowsAsync(error);

            // Act
            var result = await _tableService.GetRowOrColumnMajorAsync(elementId, "Test", 1234, 30);

            // Assert
            Assert.NotNull(result);
            
            // Verify error logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to get RowOrColumnMajor property")),
                    error,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _output.WriteLine("Error logging verification test passed");
        }

        #endregion

        public void Dispose()
        {
            _output?.WriteLine("TableServiceTests disposed");
            GC.SuppressFinalize(this);
        }
    }
}