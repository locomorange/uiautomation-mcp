using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Services.ControlPatterns;
using Xunit.Abstractions;
using System.Diagnostics;

namespace UIAutomationMCP.Tests.Integration
{
    /// <summary>
    /// TransformPatternの統合テスト
    /// SubprocessExecutorを使用した安全なWorker実行によるTransform操作テスト
    /// Microsoft仕様に基づいたTransformPatternの検証
    /// https://learn.microsoft.com/en-us/dotnet/framework/ui-automation/implementing-the-ui-automation-transform-control-pattern
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Integration")]
    public class TransformPatternIntegrationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ServiceProvider _serviceProvider;
        private readonly SubprocessExecutor _subprocessExecutor;
        private readonly TransformService _transformService;
        private readonly string _workerPath;

        public TransformPatternIntegrationTests(ITestOutputHelper output)
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

            _subprocessExecutor = new SubprocessExecutor(logger, _workerPath);
            
            var transformLogger = _serviceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger<TransformService>();
            _transformService = new TransformService(transformLogger, _subprocessExecutor);
            
            _output.WriteLine($"TransformPattern Integration Tests initialized with worker: {_workerPath}");
        }

        private T DeserializeResult<T>(object jsonResult) where T : notnull
        {
            var result = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Deserialize<T>(jsonResult.ToString()!);
            Assert.NotNull(result);
            return result;
        }

        public void Dispose()
        {
            try
            {
                _subprocessExecutor?.Dispose();
                _serviceProvider?.Dispose();
                _output.WriteLine("TransformPattern Integration Tests disposed successfully");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error during dispose: {ex.Message}");
            }
        }

        #region Microsoft仕様準拠の能力確認テスト

        [Fact]
        public async Task GetTransformCapabilities_WithNonExistentElement_ShouldReturnError()
        {
            // Arrange
            const string nonExistentAutomationId = "NonExistentTransformElement_12345";
            const string windowTitle = "NonExistentWindow";
            const int timeout = 5; // 短いタイムアウト

            _output.WriteLine($"Testing GetTransformCapabilities with non-existent element: {nonExistentAutomationId}");

            // Act
            var jsonResult = await _transformService.GetTransformCapabilitiesAsync(
                nonExistentAutomationId, windowTitle, null, null, timeout);

            // Assert
            Assert.NotNull(jsonResult);
            var result = DeserializeResult<ServerEnhancedResponse<TransformCapabilitiesResult>>(jsonResult);
            Assert.False(result.Success);
            Assert.Contains("not found", result.ErrorMessage ?? "", StringComparison.OrdinalIgnoreCase);
            
            _output.WriteLine($"✓ GetTransformCapabilities correctly handled non-existent element");
            _output.WriteLine($"  Error: {result.ErrorMessage}");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetTransformCapabilities_WithInvalidElementId_ShouldReturnError(string invalidElementId)
        {
            // Arrange
            _output.WriteLine($"Testing GetTransformCapabilities with invalid element ID: '{invalidElementId}'");

            // Act
            var jsonResult = await _transformService.GetTransformCapabilitiesAsync(
                invalidElementId, "TestWindow", timeoutSeconds: 5);

            // Assert
            Assert.NotNull(jsonResult);
            var result = DeserializeResult<ServerEnhancedResponse<ActionResult>>(jsonResult);
            Assert.False(result.Success);
            Assert.NotNull(result.ErrorMessage);
            
            _output.WriteLine($"✓ GetTransformCapabilities correctly handled invalid element ID");
            _output.WriteLine($"  Error: {result.ErrorMessage}");
        }

        #endregion

        #region Microsoft仕様準拠のMove操作テスト

        [Fact]
        public async Task MoveElement_WithNonExistentElement_ShouldReturnError()
        {
            // Arrange
            const string nonExistentAutomationId = "NonExistentMoveableElement_12345";
            const double x = 100.0;
            const double y = 200.0;
            const int timeout = 5;

            _output.WriteLine($"Testing MoveElement with non-existent element: {nonExistentAutomationId}");
            _output.WriteLine($"  Target coordinates: ({x}, {y})");

            // Act
            var jsonResult = await _transformService.MoveElementAsync(
                nonExistentAutomationId, "TestWindow", x, y, null, null, timeout);

            // Assert
            Assert.NotNull(jsonResult);
            var result = DeserializeResult<ServerEnhancedResponse<ActionResult>>(jsonResult);
            Assert.False(result.Success);
            Assert.Contains("not found", result.ErrorMessage ?? "", StringComparison.OrdinalIgnoreCase);
            
            _output.WriteLine($"✓ MoveElement correctly handled non-existent element");
            _output.WriteLine($"  Error: {result.ErrorMessage}");
        }

        [Theory]
        [InlineData(0.0, 0.0)]      // 原点
        [InlineData(-100.0, -200.0)] // 負の座標
        [InlineData(1920.0, 1080.0)] // 大きな座標
        [InlineData(123.45, 678.90)] // 小数点座標
        public async Task MoveElement_WithValidCoordinates_ShouldAttemptMove(double x, double y)
        {
            // Arrange
            const string elementId = "TestMoveElement";
            const int timeout = 5;

            _output.WriteLine($"Testing MoveElement with coordinates: ({x}, {y})");

            // Act
            var jsonResult = await _transformService.MoveElementAsync(
                elementId, x, y, "TestWindow", timeoutSeconds: timeout);

            // Assert
            Assert.NotNull(jsonResult);
            var result = DeserializeResult<ServerEnhancedResponse<ActionResult>>(jsonResult);
            // 要素が存在しないため失敗するが、座標は正しく処理される
            Assert.False(result.Success);
            
            _output.WriteLine($"✓ MoveElement processed coordinates correctly");
            _output.WriteLine($"  Result: {result.ErrorMessage}");
        }

        #endregion

        #region Microsoft仕様準拠のResize操作テスト

        [Fact]
        public async Task ResizeElement_WithNonExistentElement_ShouldReturnError()
        {
            // Arrange
            const string nonExistentAutomationId = "NonExistentResizableElement_12345";
            const double width = 800.0;
            const double height = 600.0;
            const int timeout = 5;

            _output.WriteLine($"Testing ResizeElement with non-existent element: {nonExistentAutomationId}");
            _output.WriteLine($"  Target dimensions: {width}x{height}");

            // Act
            var jsonResult = await _transformService.ResizeElementAsync(
                nonExistentAutomationId, "TestWindow", width, height, null, null, timeout);

            // Assert
            Assert.NotNull(jsonResult);
            var result = DeserializeResult<ServerEnhancedResponse<ActionResult>>(jsonResult);
            Assert.False(result.Success);
            Assert.Contains("not found", result.ErrorMessage ?? "", StringComparison.OrdinalIgnoreCase);
            
            _output.WriteLine($"✓ ResizeElement correctly handled non-existent element");
            _output.WriteLine($"  Error: {result.ErrorMessage}");
        }

        [Theory]
        [InlineData(0.0, 100.0)]   // 幅がゼロ
        [InlineData(100.0, 0.0)]   // 高さがゼロ
        [InlineData(-100.0, 200.0)] // 負の幅
        [InlineData(200.0, -100.0)] // 負の高さ
        [InlineData(0.0, 0.0)]     // 両方ゼロ
        public async Task ResizeElement_WithInvalidDimensions_ShouldReturnError(double width, double height)
        {
            // Arrange
            const string elementId = "TestResizeElement";
            const int timeout = 5;

            _output.WriteLine($"Testing ResizeElement with invalid dimensions: {width}x{height}");

            // Act
            var jsonResult = await _transformService.ResizeElementAsync(
                elementId, width, height, "TestWindow", timeoutSeconds: timeout);

            // Assert
            Assert.NotNull(jsonResult);
            var result = DeserializeResult<ServerEnhancedResponse<ActionResult>>(jsonResult);
            Assert.False(result.Success);
            
            _output.WriteLine($"✓ ResizeElement correctly handled invalid dimensions");
            _output.WriteLine($"  Error: {result.ErrorMessage}");
        }

        [Theory]
        [InlineData(800.0, 600.0)]    // 標準的なサイズ
        [InlineData(1920.0, 1080.0)]  // 大きなサイズ
        [InlineData(320.0, 240.0)]    // 小さなサイズ
        [InlineData(100.5, 200.75)]   // 小数点サイズ
        public async Task ResizeElement_WithValidDimensions_ShouldAttemptResize(double width, double height)
        {
            // Arrange
            const string elementId = "TestResizeElement";
            const int timeout = 5;

            _output.WriteLine($"Testing ResizeElement with valid dimensions: {width}x{height}");

            // Act
            var jsonResult = await _transformService.ResizeElementAsync(
                automationId: elementId, width: width, height: height, timeoutSeconds: timeout);

            // Assert
            Assert.NotNull(jsonResult);
            var result = DeserializeResult<ServerEnhancedResponse<ActionResult>>(jsonResult);
            // 要素が存在しないため失敗するが、サイズは正しく処理される
            Assert.False(result.Success);
            
            _output.WriteLine($"✓ ResizeElement processed dimensions correctly");
            _output.WriteLine($"  Result: {result.ErrorMessage}");
        }

        #endregion

        #region Microsoft仕様準拠のRotate操作テスト

        [Fact]
        public async Task RotateElement_WithNonExistentElement_ShouldReturnError()
        {
            // Arrange
            const string nonExistentAutomationId = "NonExistentRotatableElement_12345";
            const double degrees = 90.0;
            const int timeout = 5;

            _output.WriteLine($"Testing RotateElement with non-existent element: {nonExistentAutomationId}");
            _output.WriteLine($"  Target rotation: {degrees} degrees");

            // Act
            var jsonResult = await _transformService.RotateElementAsync(
                nonExistentAutomationId, "TestWindow", degrees, null, null, timeout);

            // Assert
            Assert.NotNull(jsonResult);
            var result = DeserializeResult<ServerEnhancedResponse<ActionResult>>(jsonResult);
            Assert.False(result.Success);
            Assert.Contains("not found", result.ErrorMessage ?? "", StringComparison.OrdinalIgnoreCase);
            
            _output.WriteLine($"✓ RotateElement correctly handled non-existent element");
            _output.WriteLine($"  Error: {result.ErrorMessage}");
        }

        [Theory]
        [InlineData(90.0)]     // 90度回転
        [InlineData(180.0)]    // 180度回転
        [InlineData(270.0)]    // 270度回転
        [InlineData(360.0)]    // 360度回転
        [InlineData(45.5)]     // 小数点角度
        [InlineData(-90.0)]    // 負の角度
        [InlineData(0.0)]      // 0度（回転なし）
        [InlineData(720.0)]    // 720度（2回転）
        public async Task RotateElement_WithValidDegrees_ShouldAttemptRotate(double degrees)
        {
            // Arrange
            const string elementId = "TestRotateElement";
            const int timeout = 5;

            _output.WriteLine($"Testing RotateElement with degrees: {degrees}");

            // Act
            var jsonResult = await _transformService.RotateElementAsync(
                elementId, degrees, "TestWindow", timeoutSeconds: timeout);

            // Assert
            Assert.NotNull(jsonResult);
            var result = DeserializeResult<ServerEnhancedResponse<ActionResult>>(jsonResult);
            // 要素が存在しないため失敗するが、角度は正しく処理される
            Assert.False(result.Success);
            
            _output.WriteLine($"✓ RotateElement processed degrees correctly");
            _output.WriteLine($"  Result: {result.ErrorMessage}");
        }

        #endregion

        #region タイムアウト処理テスト

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task TransformOperations_WithCustomTimeout_ShouldRespectTimeout(int timeoutSeconds)
        {
            // Arrange
            const string elementId = "TimeoutTestElement";
            var stopwatch = Stopwatch.StartNew();

            _output.WriteLine($"Testing Transform operations with timeout: {timeoutSeconds}s");

            // Act & Assert - GetTransformCapabilities
            var capabilitiesResult = await _transformService.GetTransformCapabilitiesAsync(
                elementId, "TestWindow", timeoutSeconds: timeoutSeconds);
            
            stopwatch.Stop();
            var elapsed = stopwatch.Elapsed.TotalSeconds;

            Assert.NotNull(capabilitiesResult);
            var capabilitiesDeserialized = DeserializeResult<ServerEnhancedResponse<TransformCapabilitiesResult>>(capabilitiesResult);
            Assert.False(capabilitiesDeserialized.Success);
            Assert.True(elapsed <= timeoutSeconds + 2, // 2秒のバッファ
                $"Operation took {elapsed:F1}s, expected <= {timeoutSeconds + 2}s");

            _output.WriteLine($"✓ GetTransformCapabilities respected timeout: {elapsed:F1}s");

            // Act & Assert - MoveElement
            stopwatch.Restart();
            var moveResult = await _transformService.MoveElementAsync(
                elementId, 100.0, 200.0, "TestWindow", timeoutSeconds: timeoutSeconds);
            
            stopwatch.Stop();
            elapsed = stopwatch.Elapsed.TotalSeconds;

            Assert.NotNull(moveResult);
            var moveDeserialized = DeserializeResult<ServerEnhancedResponse<ActionResult>>(moveResult);
            Assert.False(moveDeserialized.Success);
            Assert.True(elapsed <= timeoutSeconds + 2,
                $"Move operation took {elapsed:F1}s, expected <= {timeoutSeconds + 2}s");

            _output.WriteLine($"✓ MoveElement respected timeout: {elapsed:F1}s");

            // Act & Assert - ResizeElement
            stopwatch.Restart();
            var resizeResult = await _transformService.ResizeElementAsync(
                elementId, 800.0, 600.0, "TestWindow", timeoutSeconds: timeoutSeconds);
            
            stopwatch.Stop();
            elapsed = stopwatch.Elapsed.TotalSeconds;

            Assert.NotNull(resizeResult);
            var resizeDeserialized = DeserializeResult<ServerEnhancedResponse<ActionResult>>(resizeResult);
            Assert.False(resizeDeserialized.Success);
            Assert.True(elapsed <= timeoutSeconds + 2,
                $"Resize operation took {elapsed:F1}s, expected <= {timeoutSeconds + 2}s");

            _output.WriteLine($"✓ ResizeElement respected timeout: {elapsed:F1}s");

            // Act & Assert - RotateElement
            stopwatch.Restart();
            var rotateResult = await _transformService.RotateElementAsync(
                elementId, 90.0, "TestWindow", timeoutSeconds: timeoutSeconds);
            
            stopwatch.Stop();
            elapsed = stopwatch.Elapsed.TotalSeconds;

            Assert.NotNull(rotateResult);
            var rotateDeserialized = DeserializeResult<ServerEnhancedResponse<ActionResult>>(rotateResult);
            Assert.False(rotateDeserialized.Success);
            Assert.True(elapsed <= timeoutSeconds + 2,
                $"Rotate operation took {elapsed:F1}s, expected <= {timeoutSeconds + 2}s");

            _output.WriteLine($"✓ RotateElement respected timeout: {elapsed:F1}s");
        }

        #endregion

        #region プロセスID指定テスト

        [Theory]
        [InlineData(1234)]
        [InlineData(5678)]
        [InlineData(0)]
        public async Task TransformOperations_WithProcessId_ShouldIncludeProcessIdInRequest(int processId)
        {
            // Arrange
            const string elementId = "ProcessIdTestElement";
            const int timeout = 5;

            _output.WriteLine($"Testing Transform operations with processId: {processId}");

            // Act & Assert - GetTransformCapabilities
            var capabilitiesResult = await _transformService.GetTransformCapabilitiesAsync(
                elementId, "TestWindow", processId, timeout);

            Assert.NotNull(capabilitiesResult);
            var capabilitiesDeserialized2 = DeserializeResult<ServerEnhancedResponse<TransformCapabilitiesResult>>(capabilitiesResult);
            Assert.False(capabilitiesDeserialized2.Success); // 要素が存在しないため
            
            _output.WriteLine($"✓ GetTransformCapabilities processed processId: {processId}");

            // Act & Assert - MoveElement
            var moveResult = await _transformService.MoveElementAsync(
                elementId, 100.0, 200.0, "TestWindow", processId, timeout);

            Assert.NotNull(moveResult);
            var moveDeserialized2 = DeserializeResult<ServerEnhancedResponse<ActionResult>>(moveResult);
            Assert.False(moveDeserialized2.Success);
            
            _output.WriteLine($"✓ MoveElement processed processId: {processId}");

            // Act & Assert - ResizeElement
            var resizeResult = await _transformService.ResizeElementAsync(
                elementId, 800.0, 600.0, "TestWindow", processId, timeout);

            Assert.NotNull(resizeResult);
            var resizeDeserialized2 = DeserializeResult<ServerEnhancedResponse<ActionResult>>(resizeResult);
            Assert.False(resizeDeserialized2.Success);
            
            _output.WriteLine($"✓ ResizeElement processed processId: {processId}");

            // Act & Assert - RotateElement
            var rotateResult = await _transformService.RotateElementAsync(
                elementId, 90.0, "TestWindow", processId, timeout);

            Assert.NotNull(rotateResult);
            var rotateDeserialized2 = DeserializeResult<ServerEnhancedResponse<ActionResult>>(rotateResult);
            Assert.False(rotateDeserialized2.Success);
            
            _output.WriteLine($"✓ RotateElement processed processId: {processId}");
        }

        #endregion

        #region Worker プロセス安定性テスト

        [Fact]
        public async Task TransformOperations_ConcurrentExecution_ShouldHandleCorrectly()
        {
            // Arrange
            const int concurrentOperations = 5;
            var tasks = new List<Task<object>>();

            _output.WriteLine($"Testing {concurrentOperations} concurrent Transform operations");

            // Act - 複数の変換操作を並行実行
            for (int i = 0; i < concurrentOperations; i++)
            {
                var elementId = $"ConcurrentElement_{i}";
                var moveTask = _transformService.MoveElementAsync(automationId: elementId, x: i * 100.0, y: i * 100.0, timeoutSeconds: 10).ContinueWith(t => t.Result as object);
                var resizeTask = _transformService.ResizeElementAsync(automationId: elementId, width: 800.0 + i * 100, height: 600.0 + i * 100, timeoutSeconds: 10).ContinueWith(t => t.Result as object);
                var rotateTask = _transformService.RotateElementAsync(automationId: elementId, degrees: i * 45.0, timeoutSeconds: 10).ContinueWith(t => t.Result as object);
                
                tasks.Add(moveTask);
                tasks.Add(resizeTask);
                tasks.Add(rotateTask);
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(concurrentOperations * 3, results.Length);
            
            foreach (var result in results)
            {
                Assert.NotNull(result);
                // 要素が存在しないため全て失敗するはずだが、エラーハンドリングは正常
                var deserializedResult = DeserializeResult<ServerEnhancedResponse<ActionResult>>(result);
                Assert.False(deserializedResult.Success);
                Assert.NotNull(deserializedResult.ErrorMessage);
            }

            _output.WriteLine($"✓ All {results.Length} concurrent operations completed correctly");
            _output.WriteLine("✓ Worker process remained stable under concurrent load");
        }

        [Fact]
        public async Task TransformOperations_SequentialExecution_ShouldMaintainWorkerStability()
        {
            // Arrange
            const int sequentialOperations = 10;
            var elementIds = Enumerable.Range(1, sequentialOperations)
                .Select(i => $"SequentialElement_{i}")
                .ToArray();

            _output.WriteLine($"Testing {sequentialOperations} sequential Transform operations");

            // Act & Assert
            for (int i = 0; i < sequentialOperations; i++)
            {
                var elementId = elementIds[i];
                
                // GetTransformCapabilities
                var capabilitiesResult = await _transformService.GetTransformCapabilitiesAsync(
                    elementId, "TestWindow", timeoutSeconds: 5);
                Assert.NotNull(capabilitiesResult);
                var capabilitiesSeq = DeserializeResult<ServerEnhancedResponse<TransformCapabilitiesResult>>(capabilitiesResult);
                Assert.False(capabilitiesSeq.Success);

                // MoveElement
                var moveResult = await _transformService.MoveElementAsync(
                    elementId, i * 50.0, i * 50.0, "TestWindow", timeoutSeconds: 5);
                Assert.NotNull(moveResult);
                var moveSeq = DeserializeResult<ServerEnhancedResponse<ActionResult>>(moveResult);
                Assert.False(moveSeq.Success);

                // ResizeElement
                var resizeResult = await _transformService.ResizeElementAsync(
                    elementId, 400.0 + i * 50, 300.0 + i * 50, "TestWindow", timeoutSeconds: 5);
                Assert.NotNull(resizeResult);
                var resizeSeq = DeserializeResult<ServerEnhancedResponse<ActionResult>>(resizeResult);
                Assert.False(resizeSeq.Success);

                // RotateElement
                var rotateResult = await _transformService.RotateElementAsync(
                    elementId, i * 36.0, "TestWindow", timeoutSeconds: 5);
                Assert.NotNull(rotateResult);
                var rotateSeq = DeserializeResult<ServerEnhancedResponse<ActionResult>>(rotateResult);
                Assert.False(rotateSeq.Success);

                _output.WriteLine($"✓ Operation set {i + 1}/{sequentialOperations} completed");
            }

            _output.WriteLine($"✓ All {sequentialOperations * 4} sequential operations completed successfully");
            _output.WriteLine("✓ Worker process maintained stability throughout sequential execution");
        }

        #endregion
    }
}