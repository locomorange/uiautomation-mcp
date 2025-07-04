using Microsoft.Extensions.Logging;
using Moq;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.Services;
using UiAutomationMcpServer.Services.Patterns;
using Xunit.Abstractions;

namespace UiAutomationMcp.Tests.Patterns
{
    /// <summary>
    /// Comprehensive tests for RangePatternService covering all range value scenarios
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    public class RangePatternServiceTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ILogger<RangePatternService>> _logger;
        private readonly RangePatternService _service;
        private readonly Mock<IUIAutomationWorker> _mockWorker;

        public RangePatternServiceTests(ITestOutputHelper output)
        {
            _output = output;
            _logger = new Mock<ILogger<RangePatternService>>();
            _mockWorker = new Mock<IUIAutomationWorker>();
            _service = new RangePatternService(_logger.Object, _mockWorker.Object);
        }

        [Fact]
        public async Task SetRangeValueAsync_Success_SetsValue()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = true,
                Data = "Range value set to 50"
            };
            _mockWorker.Setup(w => w.SetRangeValueAsync("rangeElement", 50.0, "RangeWindow", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.SetRangeValueAsync("rangeElement", 50.0, "RangeWindow");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Range value set to 50", result.Data);
            _mockWorker.Verify(w => w.SetRangeValueAsync("rangeElement", 50.0, "RangeWindow", null, 20), Times.Once);
            _output.WriteLine($"SetRangeValue test passed: {result.Data}");
        }

        [Fact]
        public async Task GetRangeValueAsync_Success_ReturnsValue()
        {
            // Arrange
            var rangeData = new Dictionary<string, object>
            {
                { "Value", 75.0 },
                { "Minimum", 0.0 },
                { "Maximum", 100.0 },
                { "LargeChange", 10.0 },
                { "SmallChange", 1.0 },
                { "IsReadOnly", false }
            };
            var expectedResult = new OperationResult<Dictionary<string, object>>
            {
                Success = true,
                Data = rangeData
            };
            _mockWorker.Setup(w => w.GetRangeValueAsync("rangeElement", "RangeWindow", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.GetRangeValueAsync("rangeElement", "RangeWindow");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(rangeData, result.Data);
            _mockWorker.Verify(w => w.GetRangeValueAsync("rangeElement", "RangeWindow", null, 20), Times.Once);
            _output.WriteLine($"GetRangeValue test passed: Value={rangeData["Value"]}, Min={rangeData["Minimum"]}, Max={rangeData["Maximum"]}");
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(25.5)]
        [InlineData(50.0)]
        [InlineData(75.75)]
        [InlineData(100.0)]
        public async Task SetRangeValueAsync_VariousValues_Success(double value)
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = true,
                Data = $"Range value set to {value}"
            };
            _mockWorker.Setup(w => w.SetRangeValueAsync("rangeElement", value, null, null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.SetRangeValueAsync("rangeElement", value);

            // Assert
            Assert.True(result.Success);
            Assert.Equal($"Range value set to {value}", result.Data);
            _output.WriteLine($"SetRangeValue {value} test passed: {result.Data}");
        }

        [Theory]
        [InlineData(-10.0)]
        [InlineData(110.0)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public async Task SetRangeValueAsync_InvalidValues_HandlesGracefully(double value)
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = false,
                Error = $"Invalid range value: {value}"
            };
            _mockWorker.Setup(w => w.SetRangeValueAsync("rangeElement", value, null, null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.SetRangeValueAsync("rangeElement", value);

            // Assert
            Assert.False(result.Success);
            Assert.Equal($"Invalid range value: {value}", result.Error);
            _output.WriteLine($"Invalid value {value} test passed: {result.Error}");
        }

        [Fact]
        public async Task GetRangeValueAsync_CompleteRangeInfo_Success()
        {
            // Arrange
            var rangeData = new Dictionary<string, object>
            {
                { "Value", 42.5 },
                { "Minimum", -100.0 },
                { "Maximum", 200.0 },
                { "LargeChange", 25.0 },
                { "SmallChange", 2.5 },
                { "IsReadOnly", true }
            };
            var expectedResult = new OperationResult<Dictionary<string, object>>
            {
                Success = true,
                Data = rangeData
            };
            _mockWorker.Setup(w => w.GetRangeValueAsync("complexRange", null, null, 20))
                      .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _service.GetRangeValueAsync("complexRange");

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            var data = (Dictionary<string, object>)result.Data!;
            Assert.Equal(42.5, Convert.ToDouble(data["Value"]));
            Assert.Equal(-100.0, Convert.ToDouble(data["Minimum"]));
            Assert.Equal(200.0, Convert.ToDouble(data["Maximum"]));
            Assert.Equal(25.0, Convert.ToDouble(data["LargeChange"]));
            Assert.Equal(2.5, Convert.ToDouble(data["SmallChange"]));
            Assert.True(Convert.ToBoolean(data["IsReadOnly"]));
            _output.WriteLine($"Complex range info test passed: Range=[{Convert.ToDouble(data["Minimum"])}, {Convert.ToDouble(data["Maximum"])}], Value={Convert.ToDouble(data["Value"])}");
        }

        [Fact]
        public async Task SetRangeValueAsync_ReadOnlyRange_ReturnsFailure()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = false,
                Error = "Range is read-only and cannot be modified"
            };
            _mockWorker.Setup(w => w.SetRangeValueAsync("readOnlyRange", 50.0, null, null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.SetRangeValueAsync("readOnlyRange", 50.0);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Range is read-only and cannot be modified", result.Error);
            _output.WriteLine($"Read-only range test passed: {result.Error}");
        }

        [Fact]
        public async Task SetRangeValueAsync_WorkerFailure_ReturnsFailure()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = false,
                Error = "Unable to set range value"
            };
            _mockWorker.Setup(w => w.SetRangeValueAsync(It.IsAny<string>(), It.IsAny<double>(), 
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>()))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.SetRangeValueAsync("rangeElement", 50.0, "RangeWindow");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Unable to set range value", result.Error);
            _output.WriteLine($"SetRangeValue failure test passed: {result.Error}");
        }

        [Fact]
        public async Task GetRangeValueAsync_WorkerFailure_ReturnsFailure()
        {
            // Arrange
            var expectedResult = new OperationResult<Dictionary<string, object>>
            {
                Success = false,
                Error = "Unable to get range value"
            };
            _mockWorker.Setup(w => w.GetRangeValueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>()))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.GetRangeValueAsync("rangeElement", "RangeWindow");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Unable to get range value", result.Error);
            _output.WriteLine($"GetRangeValue failure test passed: {result.Error}");
        }

        [Fact]
        public async Task GetRangeValueAsync_MissingElement_ReturnsFailure()
        {
            // Arrange
            var expectedResult = new OperationResult<Dictionary<string, object>>
            {
                Success = false,
                Error = "Range element not found"
            };
            _mockWorker.Setup(w => w.GetRangeValueAsync("nonexistentRange", null, null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.GetRangeValueAsync("nonexistentRange");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Range element not found", result.Error);
            _output.WriteLine($"Missing element test passed: {result.Error}");
        }

        [Theory]
        [InlineData(0.0, 100.0, 50.0)]   // Middle value
        [InlineData(0.0, 100.0, 0.0)]    // Minimum value
        [InlineData(0.0, 100.0, 100.0)]  // Maximum value
        [InlineData(-50.0, 50.0, 0.0)]   // Zero in negative/positive range
        [InlineData(10.5, 20.7, 15.6)]   // Decimal range and value
        public async Task SetRangeValueAsync_ValidRanges_Success(double min, double max, double setValue)
        {
            // First, set up the range info
            var rangeData = new Dictionary<string, object>
            {
                { "Value", setValue },
                { "Minimum", min },
                { "Maximum", max },
                { "LargeChange", (max - min) / 10 },
                { "SmallChange", (max - min) / 100 },
                { "IsReadOnly", false }
            };
            var getRangeResult = new OperationResult<Dictionary<string, object>>
            {
                Success = true,
                Data = rangeData
            };
            _mockWorker.Setup(w => w.GetRangeValueAsync("validRange", null, null, 20))
                      .ReturnsAsync(getRangeResult);

            // Set up the set value result
            var setValueResult = new OperationResult<string>
            {
                Success = true,
                Data = $"Range value set to {setValue} (range: {min} - {max})"
            };
            _mockWorker.Setup(w => w.SetRangeValueAsync("validRange", setValue, null, null, 20))
                      .ReturnsAsync(setValueResult);

            // Act
            var getResult = await _service.GetRangeValueAsync("validRange");
            var setResult = await _service.SetRangeValueAsync("validRange", setValue);

            // Assert
            Assert.True(getResult.Success);
            Assert.True(setResult.Success);
            Assert.Equal(setValue, Convert.ToDouble(((Dictionary<string, object>)getResult.Data!)["Value"]));
            Assert.Equal(min, Convert.ToDouble(((Dictionary<string, object>)getResult.Data!)["Minimum"]));
            Assert.Equal(max, Convert.ToDouble(((Dictionary<string, object>)getResult.Data!)["Maximum"]));
            _output.WriteLine($"Valid range test passed: Set {setValue} in range [{min}, {max}]");
        }

        [Fact]
        public async Task SetRangeValueAsync_PercentageSlider_Success()
        {
            // Arrange
            var expectedResult = new OperationResult<string>
            {
                Success = true,
                Data = "Percentage slider set to 75%"
            };
            _mockWorker.Setup(w => w.SetRangeValueAsync("percentageSlider", 75.0, "SliderWindow", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.SetRangeValueAsync("percentageSlider", 75.0, "SliderWindow");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Percentage slider set to 75%", result.Data);
            _output.WriteLine($"Percentage slider test passed: {result.Data}");
        }

        [Fact]
        public async Task GetRangeValueAsync_VolumeControl_Success()
        {
            // Arrange
            var volumeData = new Dictionary<string, object>
            {
                { "Value", 65.0 },
                { "Minimum", 0.0 },
                { "Maximum", 100.0 },
                { "LargeChange", 10.0 },
                { "SmallChange", 1.0 },
                { "IsReadOnly", false }
            };
            var expectedResult = new OperationResult<Dictionary<string, object>>
            {
                Success = true,
                Data = volumeData
            };
            _mockWorker.Setup(w => w.GetRangeValueAsync("volumeControl", "AudioWindow", null, 20))
                      .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.GetRangeValueAsync("volumeControl", "AudioWindow");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(65.0, Convert.ToDouble(((Dictionary<string, object>)result.Data!)["Value"]));
            Assert.Equal(0.0, Convert.ToDouble(((Dictionary<string, object>)result.Data!)["Minimum"]));
            Assert.Equal(100.0, Convert.ToDouble(((Dictionary<string, object>)result.Data!)["Maximum"]));
            _output.WriteLine($"Volume control test passed: Volume={Convert.ToDouble(((Dictionary<string, object>)result.Data!)["Value"])}%");
        }

        public void Dispose()
        {
            _mockWorker?.Object?.Dispose();
        }
    }
}
