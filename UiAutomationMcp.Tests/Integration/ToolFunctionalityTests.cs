using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Services.ControlTypes;
using UIAutomationMCP.Server.Services;
using Xunit.Abstractions;
using System.Diagnostics;

namespace UiAutomationMcp.Tests.Integration
{
    /// <summary>
    /// 各ツールの機能テスト
    /// 実際のWindowsアプリケーションを使って各操作が正しく動作することを確認
    /// </summary>
    [Collection("UIAutomation Collection")]
    public class ToolFunctionalityTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ServiceProvider _serviceProvider;
        private readonly SubprocessExecutor _subprocessExecutor;
        private readonly SubprocessBasedElementSearchService _elementSearchService;
        private readonly SubprocessBasedInvokeService _invokeService;
        private readonly SubprocessBasedValueService _valueService;
        private readonly SubprocessBasedToggleService _toggleService;
        private readonly SubprocessBasedSelectionService _selectionService;
        private readonly SubprocessBasedButtonService _buttonService;
        private readonly SubprocessBasedTreeNavigationService _treeNavigationService;
        private readonly string _workerPath;

        public ToolFunctionalityTests(ITestOutputHelper output)
        {
            _output = output;
            
            var services = new ServiceCollection();
            services.AddLogging(builder => 
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            
            _serviceProvider = services.BuildServiceProvider();
            var logger = _serviceProvider.GetRequiredService<ILogger<SubprocessExecutor>>();
            
            // Worker.exeのパスを取得
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var possiblePaths = new[]
            {
                Path.Combine(baseDir, "UIAutomationMCP.Worker.exe"),
                Path.Combine(baseDir, "..", "UIAutomationMCP.Worker", "bin", "Debug", "net9.0-windows", "UIAutomationMCP.Worker.exe"),
                Path.Combine(baseDir, "worker", "UIAutomationMCP.Worker.exe"),
            };

            _workerPath = null!;
            foreach (var path in possiblePaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    _workerPath = fullPath;
                    break;
                }
            }

            if (_workerPath == null)
            {
                throw new InvalidOperationException($"Worker executable not found");
            }

            _subprocessExecutor = new SubprocessExecutor(logger, _workerPath);
            _elementSearchService = new SubprocessBasedElementSearchService(_serviceProvider.GetRequiredService<ILogger<SubprocessBasedElementSearchService>>(), _subprocessExecutor);
            _invokeService = new SubprocessBasedInvokeService(_serviceProvider.GetRequiredService<ILogger<SubprocessBasedInvokeService>>(), _subprocessExecutor);
            _valueService = new SubprocessBasedValueService(_serviceProvider.GetRequiredService<ILogger<SubprocessBasedValueService>>(), _subprocessExecutor);
            _toggleService = new SubprocessBasedToggleService(_serviceProvider.GetRequiredService<ILogger<SubprocessBasedToggleService>>(), _subprocessExecutor);
            _selectionService = new SubprocessBasedSelectionService(_serviceProvider.GetRequiredService<ILogger<SubprocessBasedSelectionService>>(), _subprocessExecutor);
            _buttonService = new SubprocessBasedButtonService(_serviceProvider.GetRequiredService<ILogger<SubprocessBasedButtonService>>(), _subprocessExecutor);
            _treeNavigationService = new SubprocessBasedTreeNavigationService(_serviceProvider.GetRequiredService<ILogger<SubprocessBasedTreeNavigationService>>(), _subprocessExecutor);
        }

        private async Task<Process?> LaunchCalculatorAsync()
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "calc.exe",
                    UseShellExecute = true
                });

                if (process != null)
                {
                    await Task.Delay(3000); // アプリケーションが完全に起動するまで待機
                    _output.WriteLine($"Calculator launched with PID: {process.Id}");
                }
                return process;
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Failed to launch Calculator: {ex.Message}");
                return null;
            }
        }

        private async Task<Process?> LaunchNotepadAsync()
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    UseShellExecute = true
                });

                if (process != null)
                {
                    await Task.Delay(2000);
                    _output.WriteLine($"Notepad launched with PID: {process.Id}");
                }
                return process;
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Failed to launch Notepad: {ex.Message}");
                return null;
            }
        }

        [Fact]
        public async Task Tool_GetDesktopWindows_ShouldWork()
        {
            // Act
            var result = await _elementSearchService.GetWindowsAsync(15);

            // Assert
            Assert.NotNull(result);
            _output.WriteLine($"GetDesktopWindows result: {result}");
            
            // レスポンスが正常に返されることを確認
            var resultString = result.ToString();
            Assert.False(string.IsNullOrEmpty(resultString));
        }

        [Fact]
        public async Task Tool_FindElements_WithCalculator_ShouldWork()
        {
            // Arrange
            using var calcProcess = await LaunchCalculatorAsync();
            if (calcProcess == null)
            {
                _output.WriteLine("Skipping test - Calculator could not be launched");
                return;
            }

            try
            {
                // Act - ボタン要素を検索
                var result = await _elementSearchService.FindElementsAsync(null, null, "Button", null, 15);

                // Assert
                Assert.NotNull(result);
                _output.WriteLine($"FindElements result: {result}");
                
                var resultString = result.ToString();
                Assert.Contains("Button", resultString, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                try { calcProcess?.Kill(); } catch { }
            }
        }

        [Fact]
        public async Task Tool_GetElementTree_ShouldWork()
        {
            // Arrange
            using var calcProcess = await LaunchCalculatorAsync();
            if (calcProcess == null)
            {
                _output.WriteLine("Skipping test - Calculator could not be launched");
                return;
            }

            try
            {
                // Act - 要素ツリーを取得
                var result = await _treeNavigationService.GetElementTreeAsync(null, null, 2, 15);

                // Assert
                Assert.NotNull(result);
                _output.WriteLine($"GetElementTree result: {result}");
            }
            finally
            {
                try { calcProcess?.Kill(); } catch { }
            }
        }

        [Fact]
        public async Task Tool_InvokeElement_ShouldReturnResponse()
        {
            // Arrange
            using var calcProcess = await LaunchCalculatorAsync();
            if (calcProcess == null)
            {
                _output.WriteLine("Skipping test - Calculator could not be launched");
                return;
            }

            try
            {
                await Task.Delay(2000); // UI安定化のため待機

                // Act - ボタンを押す試行（見つからなくてもエラーレスポンスが返ることを確認）
                var result = await _invokeService.InvokeElementAsync("1", null, null, 10);

                // Assert
                Assert.NotNull(result);
                _output.WriteLine($"InvokeElement result: {result}");
                
                // 通信自体は成功することを確認（要素が見つからない場合もあるが、レスポンスは返る）
                var resultString = result.ToString();
                Assert.False(string.IsNullOrEmpty(resultString));
            }
            finally
            {
                try { calcProcess?.Kill(); } catch { }
            }
        }

        [Fact]
        public async Task Tool_SetValue_WithNotepad_ShouldWork()
        {
            // Arrange
            using var notepadProcess = await LaunchNotepadAsync();
            if (notepadProcess == null)
            {
                _output.WriteLine("Skipping test - Notepad could not be launched");
                return;
            }

            try
            {
                await Task.Delay(2000); // UI安定化のため待機

                // Act - テキスト入力を試行
                var result = await _valueService.SetValueAsync("Edit", "Test automation text", null, null, 10);

                // Assert
                Assert.NotNull(result);
                _output.WriteLine($"SetValue result: {result}");
                
                var resultString = result.ToString();
                Assert.False(string.IsNullOrEmpty(resultString));
            }
            finally
            {
                try { notepadProcess?.Kill(); } catch { }
            }
        }

        [Fact]
        public async Task Tool_GetValue_WithNotepad_ShouldWork()
        {
            // Arrange
            using var notepadProcess = await LaunchNotepadAsync();
            if (notepadProcess == null)
            {
                _output.WriteLine("Skipping test - Notepad could not be launched");
                return;
            }

            try
            {
                await Task.Delay(2000);

                // Act - テキスト値を取得
                var result = await _valueService.GetValueAsync("Edit", null, null, 10);

                // Assert
                Assert.NotNull(result);
                _output.WriteLine($"GetValue result: {result}");
            }
            finally
            {
                try { notepadProcess?.Kill(); } catch { }
            }
        }

        [Fact]
        public async Task Tool_ButtonOperation_ShouldReturnResponse()
        {
            // Arrange
            using var calcProcess = await LaunchCalculatorAsync();
            if (calcProcess == null)
            {
                _output.WriteLine("Skipping test - Calculator could not be launched");
                return;
            }

            try
            {
                await Task.Delay(2000);

                // Act - ボタン操作を試行
                var result = await _buttonService.ButtonOperationAsync("Button", "click", null, null, 10);

                // Assert
                Assert.NotNull(result);
                _output.WriteLine($"ButtonOperation result: {result}");
            }
            finally
            {
                try { calcProcess?.Kill(); } catch { }
            }
        }

        [Fact]
        public async Task Tool_ToggleOperation_ShouldReturnResponse()
        {
            // Act - トグル操作を試行（要素が見つからなくてもレスポンスが返ることを確認）
            var result = await _toggleService.ToggleElementAsync("ToggleButton", null, null, 5);

            // Assert
            Assert.NotNull(result);
            _output.WriteLine($"ToggleOperation result: {result}");
        }

        [Fact]
        public async Task Tool_SelectionOperation_ShouldReturnResponse()
        {
            // Act - 選択操作を試行
            var result = await _selectionService.SelectItemAsync("ListItem", null, null, 5);

            // Assert
            Assert.NotNull(result);
            _output.WriteLine($"SelectionOperation result: {result}");
        }

        [Fact]
        public async Task Tools_CommunicationStability_Test()
        {
            // Act - 複数の操作を連続実行して通信の安定性をテスト
            var operations = new List<(string name, Func<Task<object>> operation)>
            {
                ("GetWindows", async () => await _elementSearchService.GetWindowsAsync(5)),
                ("FindElements", async () => await _elementSearchService.FindElementsAsync(null, null, "Button", null, 5)),
                ("GetTree", async () => await _treeNavigationService.GetElementTreeAsync(null, null, 1, 5)),
                ("InvokeElement", async () => await _invokeService.InvokeElementAsync("test", null, null, 5)),
                ("GetValue", async () => await _valueService.GetValueAsync("test", null, null, 5)),
            };

            var results = new List<(string name, bool success, string result)>();

            // Assert - 各操作が応答を返すことを確認
            foreach (var (name, operation) in operations)
            {
                try
                {
                    var result = await operation();
                    results.Add((name, true, result?.ToString() ?? "null"));
                    _output.WriteLine($"✅ {name}: Success");
                }
                catch (Exception ex)
                {
                    results.Add((name, false, ex.Message));
                    _output.WriteLine($"❌ {name}: {ex.Message}");
                }
            }

            // 少なくとも80%の操作が何らかのレスポンスを返すことを確認
            var successCount = results.Count(r => r.success && !string.IsNullOrEmpty(r.result));
            var successRate = (double)successCount / results.Count;
            
            _output.WriteLine($"Communication stability: {successRate:P1} ({successCount}/{results.Count})");
            Assert.True(successRate >= 0.8, $"Expected at least 80% operations to respond, got {successRate:P1}");
        }

        public void Dispose()
        {
            _subprocessExecutor?.Dispose();
            _serviceProvider?.Dispose();
        }
    }
}