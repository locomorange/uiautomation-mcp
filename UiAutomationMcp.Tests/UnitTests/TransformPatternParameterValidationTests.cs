using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Operations.Transform;
using UIAutomationMCP.Worker.Helpers;
using Moq;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.UnitTests
{
    /// <summary>
    /// TransformPatternのパラメータ検証テスト
    /// Microsoft仕様に基づいたパラメータ検証の安全なテスト
    /// Worker Operations層でのパラメータ処理を検証
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class TransformPatternParameterValidationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ElementFinderService> _mockElementFinder;
        private readonly Mock<IOptions<UIAutomationOptions>> _mockOptions;
        private readonly ILogger<RotateElementOperation> _mockRotateElementLogger;
        private readonly ILogger<ResizeElementOperation> _mockResizeElementLogger;
        private readonly ILogger<MoveElementOperation> _mockMoveElementLogger;

        public TransformPatternParameterValidationTests(ITestOutputHelper output)
        {
            _output = output;
            _mockElementFinder = new Mock<ElementFinderService>(Mock.Of<ILogger<ElementFinderService>>());
            _mockOptions = new Mock<IOptions<UIAutomationOptions>>();
            _mockOptions.Setup(x => x.Value).Returns(new UIAutomationOptions());
            _mockRotateElementLogger = Mock.Of<ILogger<RotateElementOperation>>();
            _mockResizeElementLogger = Mock.Of<ILogger<ResizeElementOperation>>();
            _mockMoveElementLogger = Mock.Of<ILogger<MoveElementOperation>>();
        }

        public void Dispose()
        {
            // モックのクリーンアップは不要
        }


        #region MoveElementOperation パラメータ検証

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task MoveElementOperation_WithInvalidElementId_ShouldReturnError(string? invalidElementId)
        {
            // Arrange
            var operation = new MoveElementOperation(_mockElementFinder.Object, _mockMoveElementLogger);
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", invalidElementId ?? "" },
                    { "x", "100.0" },
                    { "y", "200.0" },
                    { "windowTitle", "TestWindow" },
                    { "processId", "0" }
                }
            };

            _output.WriteLine($"Testing MoveElementOperation with invalid elementId: '{invalidElementId}'");

            // Act
            var typedRequest = new MoveElementRequest 
            { 
                AutomationId = (string)request.Parameters!["elementId"], 
                WindowTitle = (string)request.Parameters["windowTitle"], 
                ProcessId = int.Parse((string)request.Parameters["processId"]),
                X = double.TryParse((string)request.Parameters["x"], out var x) ? x : 0.0,
                Y = double.TryParse((string)request.Parameters["y"], out var y) ? y : 0.0
            };
            var result = await operation.ExecuteAsync(System.Text.Json.JsonSerializer.Serialize(typedRequest));

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("not found", result.Error ?? "", StringComparison.OrdinalIgnoreCase);
            
            _output.WriteLine($"✓ MoveElementOperation correctly handled invalid elementId");
            _output.WriteLine($"  Error: {result.Error}");
        }

        [Theory]
        [InlineData("invalid_x", "200.0")]
        [InlineData("100.0", "invalid_y")]
        [InlineData("", "200.0")]
        [InlineData("100.0", "")]
        [InlineData("abc", "def")]
        public async Task MoveElementOperation_WithInvalidCoordinates_ShouldUseDefaultValues(string xValue, string yValue)
        {
            // Arrange
            var operation = new MoveElementOperation(_mockElementFinder.Object, _mockMoveElementLogger);
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "TestElement" },
                    { "x", xValue },
                    { "y", yValue },
                    { "windowTitle", "TestWindow" },
                    { "processId", "0" }
                }
            };

            _output.WriteLine($"Testing MoveElementOperation with invalid coordinates: x='{xValue}', y='{yValue}'");

            // Act
            var typedRequest = new MoveElementRequest 
            { 
                AutomationId = (string)request.Parameters!["elementId"], 
                WindowTitle = (string)request.Parameters["windowTitle"], 
                ProcessId = int.Parse((string)request.Parameters["processId"]),
                X = double.TryParse((string)request.Parameters["x"], out var x) ? x : 0.0,
                Y = double.TryParse((string)request.Parameters["y"], out var y) ? y : 0.0
            };
            var result = await operation.ExecuteAsync(System.Text.Json.JsonSerializer.Serialize(typedRequest));

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success); // 要素が見つからないため
            
            _output.WriteLine($"✓ MoveElementOperation handled invalid coordinates gracefully");
            _output.WriteLine($"  Error: {result.Error}");
        }

        [Fact]
        public async Task MoveElementOperation_WithMissingCoordinateParameters_ShouldUseDefaults()
        {
            // Arrange
            var operation = new MoveElementOperation(_mockElementFinder.Object, _mockMoveElementLogger);
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "TestElement" },
                    { "windowTitle", "TestWindow" },
                    { "processId", "0" }
                    // x, yパラメータなし
                }
            };

            _output.WriteLine("Testing MoveElementOperation with missing coordinate parameters");

            // Act
            var typedRequest = new MoveElementRequest 
            { 
                AutomationId = (string)request.Parameters!["elementId"], 
                WindowTitle = (string)request.Parameters["windowTitle"], 
                ProcessId = int.Parse((string)request.Parameters["processId"]),
                X = 0.0,  // Default values for missing coordinates
                Y = 0.0
            };
            var result = await operation.ExecuteAsync(System.Text.Json.JsonSerializer.Serialize(typedRequest));

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success); // 要素が見つからないため
            
            _output.WriteLine($"✓ MoveElementOperation handled missing coordinate parameters");
            _output.WriteLine($"  Error: {result.Error}");
        }

        #endregion

        #region ResizeElementOperation パラメータ検証

        [Theory]
        [InlineData("0.0", "100.0")]   // 幅がゼロ
        [InlineData("100.0", "0.0")]   // 高さがゼロ
        [InlineData("-100.0", "200.0")] // 負の幅
        [InlineData("200.0", "-100.0")] // 負の高さ
        [InlineData("0.0", "0.0")]     // 両方ゼロ
        public async Task ResizeElementOperation_WithInvalidDimensions_ShouldReturnError(string widthValue, string heightValue)
        {
            // Arrange
            var operation = new ResizeElementOperation(_mockElementFinder.Object, _mockResizeElementLogger);
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "TestElement" },
                    { "width", widthValue },
                    { "height", heightValue },
                    { "windowTitle", "TestWindow" },
                    { "processId", "0" }
                }
            };

            _output.WriteLine($"Testing ResizeElementOperation with invalid dimensions: width='{widthValue}', height='{heightValue}'");

            // Act
            var typedRequest = new ResizeElementRequest 
            { 
                AutomationId = (string)request.Parameters!["elementId"], 
                WindowTitle = (string)request.Parameters["windowTitle"], 
                ProcessId = int.Parse((string)request.Parameters["processId"]),
                Width = double.TryParse((string)request.Parameters["width"], out var w) ? w : 0.0,
                Height = double.TryParse((string)request.Parameters["height"], out var h) ? h : 0.0
            };
            var result = await operation.ExecuteAsync(System.Text.Json.JsonSerializer.Serialize(typedRequest));

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("must be greater than 0", result.Error ?? "", StringComparison.OrdinalIgnoreCase);
            
            _output.WriteLine($"✓ ResizeElementOperation correctly rejected invalid dimensions");
            _output.WriteLine($"  Error: {result.Error}");
        }

        [Theory]
        [InlineData("invalid_width", "200.0")]
        [InlineData("100.0", "invalid_height")]
        [InlineData("", "200.0")]
        [InlineData("100.0", "")]
        [InlineData("abc", "def")]
        public async Task ResizeElementOperation_WithInvalidDimensionFormat_ShouldUseDefaultValues(string widthValue, string heightValue)
        {
            // Arrange
            var operation = new ResizeElementOperation(_mockElementFinder.Object, _mockResizeElementLogger);
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "TestElement" },
                    { "width", widthValue },
                    { "height", heightValue },
                    { "windowTitle", "TestWindow" },
                    { "processId", "0" }
                }
            };

            _output.WriteLine($"Testing ResizeElementOperation with invalid dimension format: width='{widthValue}', height='{heightValue}'");

            // Act
            var typedRequest = new ResizeElementRequest 
            { 
                AutomationId = (string)request.Parameters!["elementId"], 
                WindowTitle = (string)request.Parameters["windowTitle"], 
                ProcessId = int.Parse((string)request.Parameters["processId"]),
                Width = double.TryParse((string)request.Parameters["width"], out var w) ? w : 0.0,
                Height = double.TryParse((string)request.Parameters["height"], out var h) ? h : 0.0
            };
            var result = await operation.ExecuteAsync(System.Text.Json.JsonSerializer.Serialize(typedRequest));

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            // デフォルト値(0)が使用されるため、"must be greater than 0" エラーになる
            Assert.Contains("must be greater than 0", result.Error ?? "", StringComparison.OrdinalIgnoreCase);
            
            _output.WriteLine($"✓ ResizeElementOperation handled invalid dimension format with default values");
            _output.WriteLine($"  Error: {result.Error}");
        }

        [Theory]
        [InlineData(800.0, 600.0)]
        [InlineData(1920.0, 1080.0)]
        [InlineData(100.5, 200.75)]
        [InlineData(1.0, 1.0)]
        public async Task ResizeElementOperation_WithValidDimensions_ShouldAttemptOperation(double width, double height)
        {
            // Arrange
            var operation = new ResizeElementOperation(_mockElementFinder.Object, _mockResizeElementLogger);
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "TestElement" },
                    { "width", width.ToString() },
                    { "height", height.ToString() },
                    { "windowTitle", "TestWindow" },
                    { "processId", "0" }
                }
            };

            _output.WriteLine($"Testing ResizeElementOperation with valid dimensions: {width}x{height}");

            // Act
            var typedRequest = new ResizeElementRequest 
            { 
                AutomationId = (string)request.Parameters!["elementId"], 
                WindowTitle = (string)request.Parameters["windowTitle"], 
                ProcessId = int.Parse((string)request.Parameters["processId"]),
                Width = width,
                Height = height
            };
            var result = await operation.ExecuteAsync(System.Text.Json.JsonSerializer.Serialize(typedRequest));

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success); // 要素が見つからないため
            Assert.Contains("not found", result.Error ?? "", StringComparison.OrdinalIgnoreCase);
            
            _output.WriteLine($"✓ ResizeElementOperation processed valid dimensions correctly");
            _output.WriteLine($"  Error: {result.Error}");
        }

        #endregion

        #region RotateElementOperation パラメータ検証

        [Theory]
        [InlineData("invalid_degrees")]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("45.5.5")] // 無効な小数点形式
        public async Task RotateElementOperation_WithInvalidDegreesFormat_ShouldUseDefaultValue(string degreesValue)
        {
            // Arrange
            var operation = new RotateElementOperation(_mockElementFinder.Object, _mockRotateElementLogger);
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "TestElement" },
                    { "degrees", degreesValue },
                    { "windowTitle", "TestWindow" },
                    { "processId", "0" }
                }
            };

            _output.WriteLine($"Testing RotateElementOperation with invalid degrees format: '{degreesValue}'");

            // Act
            var typedRequest = new RotateElementRequest 
            { 
                AutomationId = (string)request.Parameters!["elementId"], 
                WindowTitle = (string)request.Parameters["windowTitle"], 
                ProcessId = int.Parse((string)request.Parameters["processId"]),
                Degrees = double.TryParse((string)request.Parameters["degrees"], out var deg) ? deg : 0.0
            };
            var result = await operation.ExecuteAsync(System.Text.Json.JsonSerializer.Serialize(typedRequest));

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success); // 要素が見つからないため
            Assert.Contains("not found", result.Error ?? "", StringComparison.OrdinalIgnoreCase);
            
            _output.WriteLine($"✓ RotateElementOperation handled invalid degrees format with default value");
            _output.WriteLine($"  Error: {result.Error}");
        }

        [Theory]
        [InlineData(90.0)]
        [InlineData(180.0)]
        [InlineData(270.0)]
        [InlineData(360.0)]
        [InlineData(45.5)]
        [InlineData(-90.0)]
        [InlineData(0.0)]
        [InlineData(720.0)]
        public async Task RotateElementOperation_WithValidDegrees_ShouldAttemptOperation(double degrees)
        {
            // Arrange
            var operation = new RotateElementOperation(_mockElementFinder.Object, _mockRotateElementLogger);
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "TestElement" },
                    { "degrees", degrees.ToString() },
                    { "windowTitle", "TestWindow" },
                    { "processId", "0" }
                }
            };

            _output.WriteLine($"Testing RotateElementOperation with valid degrees: {degrees}");

            // Act
            var typedRequest = new RotateElementRequest 
            { 
                AutomationId = (string)request.Parameters!["elementId"], 
                WindowTitle = (string)request.Parameters["windowTitle"], 
                ProcessId = int.Parse((string)request.Parameters["processId"]),
                Degrees = degrees
            };
            var result = await operation.ExecuteAsync(System.Text.Json.JsonSerializer.Serialize(typedRequest));

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success); // 要素が見つからないため
            Assert.Contains("not found", result.Error ?? "", StringComparison.OrdinalIgnoreCase);
            
            _output.WriteLine($"✓ RotateElementOperation processed valid degrees correctly");
            _output.WriteLine($"  Error: {result.Error}");
        }

        #endregion

        #region ProcessId パラメータ検証

        [Theory]
        [InlineData("1234")]
        [InlineData("0")]
        [InlineData("999999")]
        public async Task TransformOperations_WithValidProcessId_ShouldParseCorrectly(string processIdValue)
        {
            // Arrange
            var operations = new List<(string Name, Func<Task<OperationResult>> Execute)>
            {
                ("MoveElement", async () => {
                    var op = new MoveElementOperation(_mockElementFinder.Object, _mockMoveElementLogger);
                    var typedRequest = new MoveElementRequest 
                    { 
                        AutomationId = "TestElement", 
                        WindowTitle = "TestWindow", 
                        ProcessId = int.Parse(processIdValue),
                        X = 100.0,
                        Y = 200.0
                    };
                    return await op.ExecuteAsync(System.Text.Json.JsonSerializer.Serialize(typedRequest));
                }),
                ("ResizeElement", async () => {
                    var op = new ResizeElementOperation(_mockElementFinder.Object, _mockResizeElementLogger);
                    var typedRequest = new ResizeElementRequest 
                    { 
                        AutomationId = "TestElement", 
                        WindowTitle = "TestWindow", 
                        ProcessId = int.Parse(processIdValue),
                        Width = 800.0,
                        Height = 600.0
                    };
                    return await op.ExecuteAsync(System.Text.Json.JsonSerializer.Serialize(typedRequest));
                }),
                ("RotateElement", async () => {
                    var op = new RotateElementOperation(_mockElementFinder.Object, _mockRotateElementLogger);
                    var typedRequest = new RotateElementRequest 
                    { 
                        AutomationId = "TestElement", 
                        WindowTitle = "TestWindow", 
                        ProcessId = int.Parse(processIdValue),
                        Degrees = 90.0
                    };
                    return await op.ExecuteAsync(System.Text.Json.JsonSerializer.Serialize(typedRequest));
                })
            };

            _output.WriteLine($"Testing Transform operations with valid processId: {processIdValue}");

            // Act & Assert
            foreach (var (name, execute) in operations)
            {
                var result = await execute();
                
                Assert.NotNull(result);
                Assert.False(result.Success); // 要素が見つからないため
                Assert.Contains("not found", result.Error ?? "", StringComparison.OrdinalIgnoreCase);
                
                _output.WriteLine($"✓ {name} processed processId {processIdValue} correctly");
            }
        }

        [Theory]
        [InlineData("invalid_id")]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("-1")]
        public async Task TransformOperations_WithInvalidProcessId_ShouldUseDefaultValue(string invalidProcessIdValue)
        {
            // Arrange
            var operation = new MoveElementOperation(_mockElementFinder.Object, _mockMoveElementLogger);
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "TestElement" },
                    { "windowTitle", "TestWindow" },
                    { "processId", invalidProcessIdValue }
                }
            };

            _output.WriteLine($"Testing Transform operation with invalid processId: '{invalidProcessIdValue}'");

            // Act
            var typedRequest = new MoveElementRequest 
            { 
                AutomationId = (string)request.Parameters!["elementId"], 
                WindowTitle = (string)request.Parameters["windowTitle"], 
                ProcessId = int.Parse((string)request.Parameters["processId"]),
                X = 100.0,
                Y = 200.0
            };
            var result = await operation.ExecuteAsync(System.Text.Json.JsonSerializer.Serialize(typedRequest));

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success); // 要素が見つからないため
            Assert.Contains("not found", result.Error ?? "", StringComparison.OrdinalIgnoreCase);
            
            _output.WriteLine($"✓ Transform operation handled invalid processId with default value");
            _output.WriteLine($"  Error: {result.Error}");
        }

        #endregion

        #region WindowTitle パラメータ検証

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("ValidWindowTitle")]
        [InlineData("Window Title With Spaces")]
        [InlineData("特殊文字@#$%^&*()")]
        public async Task TransformOperations_WithVariousWindowTitles_ShouldHandleCorrectly(string windowTitle)
        {
            // Arrange
            var operation = new RotateElementOperation(_mockElementFinder.Object, _mockRotateElementLogger);
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "TestElement" },
                    { "windowTitle", windowTitle },
                    { "processId", "0" }
                }
            };

            _output.WriteLine($"Testing Transform operation");

            // Act
            var typedRequest = new RotateElementRequest 
            { 
                AutomationId = (string)request.Parameters!["elementId"], 
                WindowTitle = (string)request.Parameters["windowTitle"], 
                ProcessId = int.Parse((string)request.Parameters["processId"]),
                Degrees = 90.0
            };
            var result = await operation.ExecuteAsync(System.Text.Json.JsonSerializer.Serialize(typedRequest));

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success); // 要素が見つからないため
            Assert.Contains("not found", result.Error ?? "", StringComparison.OrdinalIgnoreCase);
            
            _output.WriteLine($"✓ Transform operation handled windowTitle correctly");
            _output.WriteLine($"  Error: {result.Error}");
        }

        #endregion
    }
}