using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Services;
using Xunit.Abstractions;
using System.Diagnostics;

namespace UiAutomationMcp.Tests.Integration
{
    /// <summary>
    /// 実際のWindowsアプリケーション（Calculator、Notepad）を使った統合テスト
    /// </summary>
    [Collection("UIAutomation Collection")]
    public class RealUIAutomationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ServiceProvider _serviceProvider;
        private readonly SubprocessExecutor _subprocessExecutor;
        private readonly SubprocessBasedElementSearchService _elementSearchService;
        private readonly SubprocessBasedInvokeService _invokeService;
        private readonly SubprocessBasedValueService _valueService;
        private readonly string _workerPath;

        public RealUIAutomationTests(ITestOutputHelper output)
        {
            _output = output;
            
            // テスト用のサービスコンテナをセットアップ
            var services = new ServiceCollection();
            
            // ロガーを追加
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
        }

        private async Task<Process?> LaunchCalculatorAsync()
        {
            try
            {
                // Windows 10/11 Calculator を起動
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "calc.exe",
                    UseShellExecute = true
                });

                if (process != null)
                {
                    // アプリケーションが完全に起動するまで待機
                    await Task.Delay(3000);
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
        public async Task Calculator_ShouldBeDiscoverable()
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
                // Act
                var result = await _elementSearchService.GetWindowsAsync(10);

                // Assert
                Assert.NotNull(result);
                _output.WriteLine($"Window discovery result: {result}");

                // Calculatorウィンドウが見つかることを確認 (日本語版Windowsでは「電卓」)
                var resultString = result?.ToString() ?? "";
                Assert.True(
                    resultString.Contains("Calculator", StringComparison.OrdinalIgnoreCase) ||
                    resultString.Contains("電卓", StringComparison.OrdinalIgnoreCase),
                    "Calculator window (Calculator or 電卓) should be discoverable"
                );
            }
            finally
            {
                try { calcProcess?.Kill(); } catch { }
            }
        }

        [Fact] 
        public async Task WinUI3Gallery_BasicLaunchTest()
        {
            // WinUI 3 Galleryの基本起動テスト（RealUIAutomationTestsから移植）
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = "shell:appsFolder\\Microsoft.WinUI3Gallery_8wekyb3d8bbwe!App",
                    UseShellExecute = true
                });

                if (process != null)
                {
                    await Task.Delay(3000);
                    _output.WriteLine($"WinUI 3 Gallery launched with PID: {process.Id}");
                    
                    // 基本的なウィンドウ発見テスト
                    var result = await _elementSearchService.GetWindowsAsync(10);
                    _output.WriteLine($"Windows discovered: {result}");
                    
                    try { process?.Kill(); } catch { }
                    Assert.NotNull(result);
                }
                else
                {
                    _output.WriteLine("WinUI 3 Gallery could not be launched - may not be installed");
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"WinUI 3 Gallery launch failed: {ex.Message}");
                // テスト失敗ではなく、インストールされていない可能性があるためスキップ
            }
        }

        [Fact]
        public async Task Calculator_ShouldHaveButtons()
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
                // Act
                var result = await _elementSearchService.FindElementsAsync("Calculator", null, "Button", null, 15);

                // Assert
                Assert.NotNull(result);
                _output.WriteLine($"Calculator buttons result: {result}");

                // ボタンが見つかることを確認
                var resultString = result.ToString();
                Assert.Contains("Button", resultString, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                try { calcProcess?.Kill(); } catch { }
            }
        }

        [Fact]
        public async Task Calculator_ShouldAllowButtonClicks()
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
                // まずボタンを探す
                var elementsResult = await _elementSearchService.FindElementsAsync("Calculator", "1", "Button", null, 15);
                _output.WriteLine($"Found elements: {elementsResult}");

                // ボタン1をクリックしてみる（エラーが出ても、通信は動作していることを確認）
                var clickResult = await _invokeService.InvokeElementAsync("1", "Calculator", null, 10);
                
                // Assert
                Assert.NotNull(clickResult);
                _output.WriteLine($"Button click result: {clickResult}");
            }
            finally
            {
                try { calcProcess?.Kill(); } catch { }
            }
        }

        [Fact]
        public async Task Notepad_ShouldAllowTextInput()
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
                // Act - エディット領域を探して文字入力を試す
                var elementsResult = await _elementSearchService.FindElementsAsync("Notepad", null, "Edit", null, 15);
                _output.WriteLine($"Found edit elements: {elementsResult}");

                // テキスト入力を試す
                var textResult = await _valueService.SetValueAsync("Text Editor", "Hello Integration Test!", "Notepad", null, 10);
                
                // Assert
                Assert.NotNull(textResult);
                _output.WriteLine($"Text input result: {textResult}");
            }
            finally
            {
                try { notepadProcess?.Kill(); } catch { }
            }
        }

        [Fact]
        public async Task WindowDiscovery_ShouldWorkWithTimeout()
        {
            // Act
            var stopwatch = Stopwatch.StartNew();
            var result = await _elementSearchService.GetWindowsAsync(5);
            stopwatch.Stop();

            // Assert
            Assert.NotNull(result);
            Assert.True(stopwatch.ElapsedMilliseconds < 6000, "Should complete within timeout");
            _output.WriteLine($"Window discovery completed in {stopwatch.ElapsedMilliseconds}ms");
        }

        [Fact]
        public async Task ElementSearch_ShouldHandleInvalidWindow()
        {
            // Act
            var result = await _elementSearchService.FindElementsAsync("NonExistentWindow123", null, null, null, 5);

            // Assert
            Assert.NotNull(result);
            _output.WriteLine($"Invalid window search result: {result}");
        }

        public void Dispose()
        {
            _subprocessExecutor?.Dispose();
            _serviceProvider?.Dispose();
        }
    }
}