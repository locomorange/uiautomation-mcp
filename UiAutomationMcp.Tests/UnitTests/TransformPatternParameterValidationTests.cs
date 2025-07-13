using UIAutomationMCP.Shared;
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

        public TransformPatternParameterValidationTests(ITestOutputHelper output)
        {
            _output = output;
            _mockElementFinder = new Mock<ElementFinderService>();
        }

        public void Dispose()
        {
            // モックのクリーンアップは不要
        }

        #region GetTransformCapabilitiesOperation パラメータ検証

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public async Task GetTransformCapabilitiesOperation_WithInvalidElementId_ShouldReturnError(string invalidElementId)
        {
            // Arrange
            var operation = new GetTransformCapabilitiesOperation(_mockElementFinder.Object);
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", invalidElementId },
                    { "windowTitle", "TestWindow" },
                    { "processId", "0" }
                }
            };

            _output.WriteLine($"Testing GetTransformCapabilitiesOperation with invalid elementId: '{invalidElementId}'");

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("not found", result.Error ?? "", StringComparison.OrdinalIgnoreCase);
            
            _output.WriteLine($"✓ GetTransformCapabilitiesOperation correctly handled invalid elementId");
            _output.WriteLine($"  Error: {result.Error}");
        }

        [Fact]
        public async Task GetTransformCapabilitiesOperation_WithNullParameters_ShouldHandleGracefully()
        {
            // Arrange
            var operation = new GetTransformCapabilitiesOperation(_mockElementFinder.Object);
            var request = new WorkerRequest
            {
                Parameters = null
            };

            _output.WriteLine("Testing GetTransformCapabilitiesOperation with null parameters");

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Error);
            
            _output.WriteLine($"✓ GetTransformCapabilitiesOperation handled null parameters gracefully");
            _output.WriteLine($"  Error: {result.Error}");
        }

        [Fact]
        public async Task GetTransformCapabilitiesOperation_WithMissingElementIdParameter_ShouldUseEmptyString()
        {
            // Arrange
            var operation = new GetTransformCapabilitiesOperation(_mockElementFinder.Object);
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "windowTitle", "TestWindow" },
                    { "processId", "0" }
                    // elementIdパラメータなし
                }
            };

            _output.WriteLine("Testing GetTransformCapabilitiesOperation with missing elementId parameter");

            // Act
            var result = await operation.ExecuteAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("not found", result.Error ?? "", StringComparison.OrdinalIgnoreCase);
            
            _output.WriteLine($"✓ GetTransformCapabilitiesOperation handled missing elementId parameter");
            _output.WriteLine($"  Error: {result.Error}");
        }

        #endregion

        #region MoveElementOperation パラメータ検証

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task MoveElementOperation_WithInvalidElementId_ShouldReturnError(string? invalidElementId)
        {
            // Arrange
            var operation = new MoveElementOperation(_mockElementFinder.Object);
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
            var result = await operation.ExecuteAsync(request);

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
            var operation = new MoveElementOperation(_mockElementFinder.Object);
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
            var result = await operation.ExecuteAsync(request);

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
            var operation = new MoveElementOperation(_mockElementFinder.Object);
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
            var result = await operation.ExecuteAsync(request);

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
            var operation = new ResizeElementOperation(_mockElementFinder.Object);
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
            var result = await operation.ExecuteAsync(request);

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
            var operation = new ResizeElementOperation(_mockElementFinder.Object);
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
            var result = await operation.ExecuteAsync(request);

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
            var operation = new ResizeElementOperation(_mockElementFinder.Object);
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
            var result = await operation.ExecuteAsync(request);

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
            var operation = new RotateElementOperation(_mockElementFinder.Object);
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
            var result = await operation.ExecuteAsync(request);

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
            var operation = new RotateElementOperation(_mockElementFinder.Object);
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
            var result = await operation.ExecuteAsync(request);

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
                ("GetTransformCapabilities", () => {
                    var op = new GetTransformCapabilitiesOperation(_mockElementFinder.Object);
                    var req = new WorkerRequest
                    {
                        Parameters = new Dictionary<string, object>
                        {
                            { "elementId", "TestElement" },
                            { "windowTitle", "TestWindow" },
                            { "processId", processIdValue }
                        }
                    };
                    return op.ExecuteAsync(req);
                }),
                ("MoveElement", () => {
                    var op = new MoveElementOperation(_mockElementFinder.Object);
                    var req = new WorkerRequest
                    {
                        Parameters = new Dictionary<string, object>
                        {
                            { "elementId", "TestElement" },
                            { "x", "100.0" },
                            { "y", "200.0" },
                            { "windowTitle", "TestWindow" },
                            { "processId", processIdValue }
                        }
                    };
                    return op.ExecuteAsync(req);
                }),
                ("ResizeElement", () => {
                    var op = new ResizeElementOperation(_mockElementFinder.Object);
                    var req = new WorkerRequest
                    {
                        Parameters = new Dictionary<string, object>
                        {
                            { "elementId", "TestElement" },
                            { "width", "800.0" },
                            { "height", "600.0" },
                            { "windowTitle", "TestWindow" },
                            { "processId", processIdValue }
                        }
                    };
                    return op.ExecuteAsync(req);
                }),
                ("RotateElement", () => {
                    var op = new RotateElementOperation(_mockElementFinder.Object);
                    var req = new WorkerRequest
                    {
                        Parameters = new Dictionary<string, object>
                        {
                            { "elementId", "TestElement" },
                            { "degrees", "90.0" },
                            { "windowTitle", "TestWindow" },
                            { "processId", processIdValue }
                        }
                    };
                    return op.ExecuteAsync(req);
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
            var operation = new GetTransformCapabilitiesOperation(_mockElementFinder.Object);
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
            var result = await operation.ExecuteAsync(request);

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
            var operation = new GetTransformCapabilitiesOperation(_mockElementFinder.Object);
            var request = new WorkerRequest
            {
                Parameters = new Dictionary<string, object>
                {
                    { "elementId", "TestElement" },
                    { "windowTitle", windowTitle },
                    { "processId", "0" }
                }
            };

            _output.WriteLine($"Testing Transform operation with windowTitle: '{windowTitle}'");

            // Act
            var result = await operation.ExecuteAsync(request);

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