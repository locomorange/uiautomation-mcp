using Microsoft.Extensions.Logging;
using Moq;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using Xunit.Abstractions;
using UIAutomationMCP.Models.Abstractions;
using UIAutomationMCP.Server.Abstractions;

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
        private readonly Mock<IOperationExecutor> _mockProcessManager;
        private readonly TableService _tableService;

        public TableServiceTests(ITestOutputHelper output)
        {
            _output = output;
            _mockLogger = new Mock<ILogger<TableService>>();
            _mockProcessManager = new Mock<IOperationExecutor>();
            
            _tableService = new TableService(Mock.Of<IProcessManager>(), _mockLogger.Object);
        }

        #region GetRowOrColumnMajorAsync Tests (COMMENTED OUT - Method doesn't exist in ITableService)

        /*
        // NOTE: GetRowOrColumnMajorAsync method doesn't exist in the current ITableService interface
        // These tests are commented out until the method is implemented
        
        [Fact]
        public async Task GetRowOrColumnMajorAsync_WhenCalled_ShouldExecuteCorrectOperation()
        {
            // Arrange
            var elementId = "dataGrid1";
            var expectedResult = new ActionResult { Success = true };

            _mockProcessManager.Setup(e => e.ExecuteAsync<GetRowOrColumnMajorRequest, ActionResult>("GetRowOrColumnMajor", It.IsAny<GetRowOrColumnMajorRequest>(), 30))
                .Returns(Task.FromResult(ServiceOperationResult<ActionResult>.FromSuccess(expectedResult)));

            // Act
            var result = await _tableService.GetRowOrColumnMajorAsync(automationId: elementId, timeoutSeconds: 30);

            // Assert
            Assert.NotNull(result);
            _mockProcessManager.Verify(e => e.ExecuteAsync<GetRowOrColumnMajorRequest, ActionResult>("GetRowOrColumnMajor", 
                It.Is<GetRowOrColumnMajorRequest>(p => 
                    p.AutomationId == elementId), 30), Times.Once);
            
            _output.WriteLine("GetRowOrColumnMajorAsync service test passed - Correct subprocess execution verified");
        }

        [Fact]
        public async Task GetRowOrColumnMajorAsync_WithNullParameters_ShouldHandleGracefully()
        {
            // Arrange
            var elementId = "table1";
            var expectedResult = new ActionResult { Success = true };

            _mockProcessManager.Setup(e => e.ExecuteAsync<GetRowOrColumnMajorRequest, ActionResult>("GetRowOrColumnMajor", It.IsAny<GetRowOrColumnMajorRequest>(), 30))
                .Returns(Task.FromResult(ServiceOperationResult<ActionResult>.FromSuccess(expectedResult)));

            // Act
            var result = await _tableService.GetRowOrColumnMajorAsync(automationId: elementId, timeoutSeconds: 30);

            // Assert
            Assert.NotNull(result);
            _mockProcessManager.Verify(e => e.ExecuteAsync<GetRowOrColumnMajorRequest, ActionResult>("GetRowOrColumnMajor", 
                It.Is<GetRowOrColumnMajorRequest>(p => 
                    p.AutomationId == elementId &&
                    p.WindowTitle == "" &&
                    p.ProcessId == 0), 30), Times.Once);
            
            _output.WriteLine("GetRowOrColumnMajorAsync null parameters test passed");
        }

        [Fact]
        public async Task GetRowOrColumnMajorAsync_WhenExecutorThrows_ShouldReturnErrorResult()
        {
            // Arrange
            var elementId = "failingTable";
            var windowTitle = "Test Window";
            var expectedError = "TablePattern not supported";

            _mockProcessManager.Setup(e => e.ExecuteAsync<GetRowOrColumnMajorRequest, ActionResult>("GetRowOrColumnMajor", It.IsAny<GetRowOrColumnMajorRequest>(), 30))
                .ThrowsAsync(new InvalidOperationException(expectedError));

            // Act
            var result = await _tableService.GetRowOrColumnMajorAsync(automationId: elementId, name: windowTitle, timeoutSeconds: 30);

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
            var expectedResult = new ActionResult { Success = true };

            _mockProcessManager.Setup(e => e.ExecuteAsync<GetRowOrColumnMajorRequest, ActionResult>("GetRowOrColumnMajor", It.IsAny<GetRowOrColumnMajorRequest>(), 30))
                .Returns(Task.FromResult(ServiceOperationResult<ActionResult>.FromSuccess(expectedResult)));

            // Act
            var result = await _tableService.GetRowOrColumnMajorAsync(automationId: elementId, name: "Test Window", timeoutSeconds: 30);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(expectedValue, result.Data.Value);
            
            _output.WriteLine($"GetRowOrColumnMajorAsync test passed for value: {expectedValue}");
        }
        */

        #endregion

        #region GetTableInfoAsync Tests (COMMENTED OUT - Method doesn't exist in ITableService)

        /*
        // NOTE: GetTableInfoAsync method doesn't exist in the current ITableService interface
        // These tests are commented out until the method is implemented

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

            _mockProcessManager.Setup(e => e.ExecuteAsync<GetTableInfoRequest, TableInfoResult>("GetTableInfo", It.IsAny<GetTableInfoRequest>(), 30))
                .Returns(Task.FromResult(ServiceOperationResult<ElementSearchResult>.FromSuccess(expectedResult)));

            // Act
            var result = await _tableService.GetTableInfoAsync(elementId, windowTitle, processId, 30);

            // Assert
            Assert.NotNull(result);
            _mockProcessManager.Verify(e => e.ExecuteAsync<GetTableInfoRequest, TableInfoResult>("GetTableInfo", 
                It.Is<GetTableInfoRequest>(p => 
                    p.AutomationId == elementId &&
                    p.WindowTitle == windowTitle &&
                    p.ProcessId == processId), 30), Times.Once);
            
            _output.WriteLine("GetTableInfoAsync service test passed");
        }
        */

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
                Elements = new List<UIAutomationMCP.Models.ElementInfo>
                {
                    new UIAutomationMCP.Models.ElementInfo { Name = "Row1" },
                    new UIAutomationMCP.Models.ElementInfo { Name = "Row2" },
                    new UIAutomationMCP.Models.ElementInfo { Name = "Row3" }
                }
            };

            _mockProcessManager.Setup(e => e.ExecuteAsync<GetRowHeadersRequest, ElementSearchResult>("GetRowHeaders", It.IsAny<GetRowHeadersRequest>(), 30))
                .Returns(Task.FromResult(ServiceOperationResult<ElementSearchResult>.FromSuccess(expectedResult)));

            // Act
            var result = await _tableService.GetRowHeadersAsync(automationId: elementId, timeoutSeconds: 30);

            // Assert
            Assert.NotNull(result);
            _mockProcessManager.Verify(e => e.ExecuteAsync<GetRowHeadersRequest, ElementSearchResult>("GetRowHeaders", 
                It.Is<GetRowHeadersRequest>(p => 
                    p.AutomationId == elementId), 30), Times.Once);
            
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
                Elements = new List<UIAutomationMCP.Models.ElementInfo>
                {
                    new UIAutomationMCP.Models.ElementInfo { Name = "Col1" },
                    new UIAutomationMCP.Models.ElementInfo { Name = "Col2" },
                    new UIAutomationMCP.Models.ElementInfo { Name = "Col3" }
                }
            };

            _mockProcessManager.Setup(e => e.ExecuteAsync<GetColumnHeadersRequest, ElementSearchResult>("GetColumnHeaders", It.IsAny<GetColumnHeadersRequest>(), 30))
                .Returns(Task.FromResult(ServiceOperationResult<ElementSearchResult>.FromSuccess(expectedResult)));

            // Act
            var result = await _tableService.GetColumnHeadersAsync(automationId: elementId, timeoutSeconds: 30);

            // Assert
            Assert.NotNull(result);
            _mockProcessManager.Verify(e => e.ExecuteAsync<GetColumnHeadersRequest, ElementSearchResult>("GetColumnHeaders", 
                It.Is<GetColumnHeadersRequest>(p => 
                    p.AutomationId == elementId), 30), Times.Once);
            
            _output.WriteLine("GetColumnHeadersAsync service test passed");
        }

        #endregion

        #region Microsoft Specification Compliance Tests

        /// <summary>
        /// Microsoft              ablePattern Required Members
        /// - GetColumnHeaders() method
        /// - GetRowHeaders() method
        /// - GetColumnHeaderItems() method  
        /// - GetRowHeaderItems() method
        /// NOTE: RowOrColumnMajor property tests are commented out as the method doesn't exist in current interface
        /// </summary>
        [Fact]
        public async Task TableService_ShouldImplementAllRequiredMembers()
        {
            // Arrange
            var elementId = "compliantTable";
            var processId = 1234;

            // Setup mock responses for all available members
            _mockProcessManager.Setup(e => e.ExecuteAsync<GetColumnHeadersRequest, ElementSearchResult>("GetColumnHeaders", It.IsAny<GetColumnHeadersRequest>(), 30))
                .Returns(Task.FromResult(ServiceOperationResult<ElementSearchResult>.FromSuccess(new ElementSearchResult 
                { 
                    Success = true,
                    Elements = new List<UIAutomationMCP.Models.ElementInfo>
                    {
                        new UIAutomationMCP.Models.ElementInfo { Name = "Header1" },
                        new UIAutomationMCP.Models.ElementInfo { Name = "Header2" }
                    }
                })));
            
            _mockProcessManager.Setup(e => e.ExecuteAsync<GetRowHeadersRequest, ElementSearchResult>("GetRowHeaders", It.IsAny<GetRowHeadersRequest>(), 30))
                .Returns(Task.FromResult(ServiceOperationResult<ElementSearchResult>.FromSuccess(new ElementSearchResult 
                { 
                    Success = true,
                    Elements = new List<UIAutomationMCP.Models.ElementInfo>
                    {
                        new UIAutomationMCP.Models.ElementInfo { Name = "Row1" },
                        new UIAutomationMCP.Models.ElementInfo { Name = "Row2" }
                    }
                })));

            _mockProcessManager.Setup(e => e.ExecuteAsync<GetColumnHeaderItemsRequest, ElementSearchResult>("GetColumnHeaderItems", It.IsAny<GetColumnHeaderItemsRequest>(), 30))
                .Returns(Task.FromResult(ServiceOperationResult<ElementSearchResult>.FromSuccess(new ElementSearchResult { Success = true })));

            _mockProcessManager.Setup(e => e.ExecuteAsync<GetRowHeaderItemsRequest, ElementSearchResult>("GetRowHeaderItems", It.IsAny<GetRowHeaderItemsRequest>(), 30))
                .Returns(Task.FromResult(ServiceOperationResult<ElementSearchResult>.FromSuccess(new ElementSearchResult { Success = true })));

            // Act & Assert - Test all implemented members
            var columnHeadersResult = await _tableService.GetColumnHeadersAsync(automationId: elementId, timeoutSeconds: 30);
            var rowHeadersResult = await _tableService.GetRowHeadersAsync(automationId: elementId, timeoutSeconds: 30);
            var columnHeaderItemsResult = await _tableService.GetColumnHeaderItemsAsync(automationId: elementId, timeoutSeconds: 30);
            var rowHeaderItemsResult = await _tableService.GetRowHeaderItemsAsync(automationId: elementId, timeoutSeconds: 30);

            Assert.NotNull(columnHeadersResult);
            Assert.NotNull(rowHeadersResult);
            Assert.NotNull(columnHeaderItemsResult);
            Assert.NotNull(rowHeaderItemsResult);

            // Verify all operations were called
            _mockProcessManager.Verify(e => e.ExecuteAsync<GetColumnHeadersRequest, ElementSearchResult>("GetColumnHeaders", It.IsAny<GetColumnHeadersRequest>(), 30), Times.Once);
            _mockProcessManager.Verify(e => e.ExecuteAsync<GetRowHeadersRequest, ElementSearchResult>("GetRowHeaders", It.IsAny<GetRowHeadersRequest>(), 30), Times.Once);
            _mockProcessManager.Verify(e => e.ExecuteAsync<GetColumnHeaderItemsRequest, ElementSearchResult>("GetColumnHeaderItems", It.IsAny<GetColumnHeaderItemsRequest>(), 30), Times.Once);
            _mockProcessManager.Verify(e => e.ExecuteAsync<GetRowHeaderItemsRequest, ElementSearchResult>("GetRowHeaderItems", It.IsAny<GetRowHeaderItemsRequest>(), 30), Times.Once);

            _output.WriteLine("Microsoft specification compliance test passed - All implemented TablePattern members working");
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
            var expectedResult = new ElementSearchResult { Success = true };

            _mockProcessManager.Setup(e => e.ExecuteAsync<GetRowHeadersRequest, ElementSearchResult>("GetRowHeaders", It.IsAny<GetRowHeadersRequest>(), timeout))
                .Returns(Task.FromResult(ServiceOperationResult<ElementSearchResult>.FromSuccess(expectedResult)));

            // Act
            var result = await _tableService.GetRowHeadersAsync(automationId: elementId, timeoutSeconds: timeout);

            // Assert
            Assert.NotNull(result);
            _mockProcessManager.Verify(e => e.ExecuteAsync<GetRowHeadersRequest, ElementSearchResult>("GetRowHeaders", It.IsAny<GetRowHeadersRequest>(), timeout), Times.Once);
            
            _output.WriteLine($"Timeout parameter test passed for {timeout} seconds");
        }

        #endregion

        #region Logging Verification Tests (COMMENTED OUT - Method doesn't exist in ITableService)

        /*
        // NOTE: GetRowOrColumnMajorAsync method doesn't exist in the current ITableService interface
        // These logging tests are commented out until the method is implemented
        
        [Fact]
        public async Task GetRowOrColumnMajorAsync_ShouldLogCorrectly()
        {
            // Arrange
            var elementId = "logTest";
            var expectedResult = new ActionResult { Success = true };

            _mockProcessManager.Setup(e => e.ExecuteAsync<GetRowOrColumnMajorRequest, ActionResult>("GetRowOrColumnMajor", It.IsAny<GetRowOrColumnMajorRequest>(), 30))
                .Returns(Task.FromResult(ServiceOperationResult<ActionResult>.FromSuccess(expectedResult)));

            // Act
            var result = await _tableService.GetRowOrColumnMajorAsync(automationId: elementId, name: "Test", timeoutSeconds: 30);

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

            _mockProcessManager.Setup(e => e.ExecuteAsync<GetRowOrColumnMajorRequest, ActionResult>("GetRowOrColumnMajor", It.IsAny<GetRowOrColumnMajorRequest>(), 30))
                .ThrowsAsync(error);

            // Act
            var result = await _tableService.GetRowOrColumnMajorAsync(automationId: elementId, name: "Test", timeoutSeconds: 30);

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
        */

        #endregion

        public void Dispose()
        {
            _output?.WriteLine("TableServiceTests disposed");
            GC.SuppressFinalize(this);
        }
    }
}

