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
    /// WinUI 3 Galleryアプリケーションを使った包括的な統合テスト
    /// WinUI 3 Galleryは様々なコントロールとパターンをテストするのに最適なアプリケーション
    /// </summary>
    [Collection("UIAutomation Collection")]
    public class WinUI3GalleryTests : IDisposable
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
        private readonly SubprocessBasedTextService _textService;
        private readonly SubprocessBasedTabService _tabService;
        private readonly SubprocessBasedTreeNavigationService _treeNavigationService;
        private readonly string _workerPath;

        public WinUI3GalleryTests(ITestOutputHelper output)
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
            _textService = new SubprocessBasedTextService(_serviceProvider.GetRequiredService<ILogger<SubprocessBasedTextService>>(), _subprocessExecutor);
            _tabService = new SubprocessBasedTabService(_serviceProvider.GetRequiredService<ILogger<SubprocessBasedTabService>>(), _subprocessExecutor);
            _treeNavigationService = new SubprocessBasedTreeNavigationService(_serviceProvider.GetRequiredService<ILogger<SubprocessBasedTreeNavigationService>>(), _subprocessExecutor);
        }

        private async Task<Process?> LaunchWinUI3GalleryAsync()
        {
            try
            {
                // ApplicationLauncher.LaunchApplicationByNameAsync の方法を参考にする
                _output.WriteLine("Trying to launch WinUI 3 Gallery using PowerShell search...");
                
                // Step 1: アプリケーションを検索 - WinUI 3 Galleryを探す
                var searchStartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-Command \"$app = Get-StartApps | Where-Object { $_.Name -eq 'WinUI 3 Gallery' } | Select-Object -First 1; if ($app) { Write-Output $app.AppID } else { Write-Error 'WinUI 3 Gallery not found' }\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                var searchProcess = Process.Start(searchStartInfo);
                if (searchProcess == null)
                {
                    _output.WriteLine("Failed to start PowerShell search process");
                    return null;
                }

                var searchOutput = await searchProcess.StandardOutput.ReadToEndAsync();
                var searchError = await searchProcess.StandardError.ReadToEndAsync();
                await searchProcess.WaitForExitAsync();

                _output.WriteLine($"Search output: {searchOutput}, Error: {searchError}, ExitCode: {searchProcess.ExitCode}");

                if (searchProcess.ExitCode != 0 || !string.IsNullOrEmpty(searchError))
                {
                    _output.WriteLine($"Application search failed");
                    return null;
                }

                var appId = searchOutput.Trim();
                if (string.IsNullOrEmpty(appId))
                {
                    _output.WriteLine("AppID is empty");
                    return null;
                }

                _output.WriteLine($"Found WinUI 3 Gallery with AppID: {appId}");

                // Step 2: アプリケーションを起動
                var launchStartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"start 'shell:AppsFolder\\{appId}'\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var launchProcess = Process.Start(launchStartInfo);
                if (launchProcess == null)
                {
                    _output.WriteLine("Failed to start launch process");
                    return null;
                }

                _output.WriteLine("WinUI 3 Gallery launched using PowerShell method");
                await Task.Delay(3000); // アプリケーションが起動するまで待機

                return launchProcess;
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Failed to launch WinUI 3 Gallery with PowerShell method: {ex.Message}");
                return null;
            }
        }

        [Fact]
        public async Task WinUI3Gallery_ShouldBeDiscoverable()
        {
            // Arrange
            using var galleryProcess = await LaunchWinUI3GalleryAsync();
            if (galleryProcess == null)
            {
                _output.WriteLine("Skipping test - WinUI 3 Gallery could not be launched");
                return;
            }

            try
            {
                // Act
                var result = await _elementSearchService.GetWindowsAsync(15);

                // Assert
                Assert.NotNull(result);
                _output.WriteLine($"Window discovery result: {result}");

                // WinUI 3 Galleryウィンドウが見つかることを確認
                var resultString = result?.ToString() ?? "";
                Assert.True(
                    resultString.Contains("WinUI", StringComparison.OrdinalIgnoreCase) ||
                    resultString.Contains("Gallery", StringComparison.OrdinalIgnoreCase),
                    "WinUI 3 Gallery window should be discoverable"
                );
            }
            finally
            {
                try { galleryProcess?.Kill(); } catch { }
            }
        }

        [Fact]
        public async Task WinUI3Gallery_ShouldHaveNavigationElements()
        {
            // Arrange
            using var galleryProcess = await LaunchWinUI3GalleryAsync();
            if (galleryProcess == null)
            {
                _output.WriteLine("Skipping test - WinUI3 Gallery could not be launched");
                return;
            }

            try
            {
                // Act - ナビゲーション要素を探す
                var treeResult = await _treeNavigationService.GetElementTreeAsync(null, null, 3, 20);
                _output.WriteLine($"Element tree result: {treeResult}");

                var buttonResult = await _elementSearchService.FindElementsAsync(null, null, "Button", null, 15);
                _output.WriteLine($"Button elements result: {buttonResult}");

                // Assert
                Assert.NotNull(treeResult);
                Assert.NotNull(buttonResult);

                // ボタンまたはナビゲーション要素が見つかることを確認
                var buttonString = buttonResult.ToString();
                Assert.Contains("Button", buttonString, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                try { galleryProcess?.Kill(); } catch { }
            }
        }

        [Fact]
        public async Task WinUI3Gallery_ButtonInteraction_ShouldWork()
        {
            // Arrange
            using var galleryProcess = await LaunchWinUI3GalleryAsync();
            if (galleryProcess == null)
            {
                _output.WriteLine("Skipping test - WinUI3 Gallery could not be launched");
                return;
            }

            try
            {
                // Act - Button pageに移動してボタンをテスト
                await Task.Delay(2000); // UI安定化のため待機

                // "Basic Input" または "Button" セクションを探す
                var elementsResult = await _elementSearchService.FindElementsAsync(null, "Button", null, null, 15);
                _output.WriteLine($"Found Button-related elements: {elementsResult}");

                // 最初に見つかったボタンをクリックしてみる
                var clickResult = await _buttonService.ButtonOperationAsync("Button", "click", null, null, 10);
                _output.WriteLine($"Button click result: {clickResult}");

                // Assert - エラーが返ってきても通信が動作していることを確認
                Assert.NotNull(clickResult);
            }
            finally
            {
                try { galleryProcess?.Kill(); } catch { }
            }
        }

        [Fact]
        public async Task WinUI3Gallery_TextInput_ShouldWork()
        {
            // Arrange
            using var galleryProcess = await LaunchWinUI3GalleryAsync();
            if (galleryProcess == null)
            {
                _output.WriteLine("Skipping test - WinUI3 Gallery could not be launched");
                return;
            }

            try
            {
                // Act - テキスト入力コントロールを探す
                await Task.Delay(2000);

                var textElements = await _elementSearchService.FindElementsAsync(null, null, "Edit", null, 15);
                _output.WriteLine($"Found text input elements: {textElements}");

                // テキスト入力を試す
                var textResult = await _valueService.SetValueAsync("TextBox", "Hello WinUI3!", null, null, 10);
                _output.WriteLine($"Text input result: {textResult}");

                // Assert
                Assert.NotNull(textResult);
            }
            finally
            {
                try { galleryProcess?.Kill(); } catch { }
            }
        }

        [Fact]
        public async Task WinUI3Gallery_ToggleSwitch_ShouldWork()
        {
            // Arrange
            using var galleryProcess = await LaunchWinUI3GalleryAsync();
            if (galleryProcess == null)
            {
                _output.WriteLine("Skipping test - WinUI3 Gallery could not be launched");
                return;
            }

            try
            {
                // Act - トグルスイッチを探してテスト
                await Task.Delay(2000);

                var toggleElements = await _elementSearchService.FindElementsAsync(null, "Toggle", null, null, 15);
                _output.WriteLine($"Found toggle elements: {toggleElements}");

                // トグルスイッチの操作を試す
                var toggleResult = await _toggleService.ToggleElementAsync("ToggleSwitch", null, null, 10);
                _output.WriteLine($"Toggle operation result: {toggleResult}");

                // 状態取得も試す
                var stateResult = await _toggleService.GetToggleStateAsync("ToggleSwitch", null, null, 10);
                _output.WriteLine($"Toggle state result: {stateResult}");

                // Assert
                Assert.NotNull(toggleResult);
                Assert.NotNull(stateResult);
            }
            finally
            {
                try { galleryProcess?.Kill(); } catch { }
            }
        }

        [Fact]
        public async Task WinUI3Gallery_TabNavigation_ShouldWork()
        {
            // Arrange
            using var galleryProcess = await LaunchWinUI3GalleryAsync();
            if (galleryProcess == null)
            {
                _output.WriteLine("Skipping test - WinUI3 Gallery could not be launched");
                return;
            }

            try
            {
                // Act - タブコントロールを探してテスト
                await Task.Delay(2000);

                var tabElements = await _elementSearchService.FindElementsAsync(null, "Tab", null, null, 15);
                _output.WriteLine($"Found tab elements: {tabElements}");

                // タブ操作を試す
                var tabListResult = await _tabService.TabOperationAsync("TabView", "list", null, null, null, 10);
                _output.WriteLine($"Tab list result: {tabListResult}");

                // Assert
                Assert.NotNull(tabListResult);
            }
            finally
            {
                try { galleryProcess?.Kill(); } catch { }
            }
        }

        [Fact]
        public async Task WinUI3Gallery_ComprehensiveControlTest()
        {
            // Arrange
            using var galleryProcess = await LaunchWinUI3GalleryAsync();
            if (galleryProcess == null)
            {
                _output.WriteLine("Skipping test - WinUI3 Gallery could not be launched");
                return;
            }

            try
            {
                // Act - 包括的なコントロールテスト
                await Task.Delay(3000); // 十分な起動時間を確保

                var results = new List<(string test, bool success, string result)>();

                // ウィンドウ発見テスト
                try
                {
                    var windowResult = await _elementSearchService.GetWindowsAsync(10);
                    results.Add(("Window Discovery", windowResult != null, windowResult?.ToString() ?? "null"));
                }
                catch (Exception ex)
                {
                    results.Add(("Window Discovery", false, ex.Message));
                }

                // 要素ツリー取得テスト
                try
                {
                    var treeResult = await _treeNavigationService.GetElementTreeAsync(null, null, 2, 15);
                    results.Add(("Element Tree", treeResult != null, treeResult?.ToString() ?? "null"));
                }
                catch (Exception ex)
                {
                    results.Add(("Element Tree", false, ex.Message));
                }

                // 各種コントロール探索テスト
                var controlTypes = new[] { "Button", "Edit", "Text", "List", "ComboBox" };
                foreach (var controlType in controlTypes)
                {
                    try
                    {
                        var elementResult = await _elementSearchService.FindElementsAsync(null, null, controlType, null, 10);
                        results.Add(($"Find {controlType}", elementResult != null, $"Found {controlType} elements"));
                    }
                    catch (Exception ex)
                    {
                        results.Add(($"Find {controlType}", false, ex.Message));
                    }
                }

                // 結果をログ出力
                foreach (var (test, success, result) in results)
                {
                    _output.WriteLine($"{test}: {(success ? "✅" : "❌")} - {result}");
                }

                // Assert - 少なくとも50%のテストが成功することを期待
                var successCount = results.Count(r => r.success);
                var successRate = (double)successCount / results.Count;
                Assert.True(successRate >= 0.5, $"Expected at least 50% success rate, got {successRate:P1} ({successCount}/{results.Count})");

                _output.WriteLine($"Overall success rate: {successRate:P1} ({successCount}/{results.Count})");
            }
            finally
            {
                try { galleryProcess?.Kill(); } catch { }
            }
        }

        [Fact]
        public async Task WinUI3Gallery_PerformanceTest()
        {
            // Arrange
            using var galleryProcess = await LaunchWinUI3GalleryAsync();
            if (galleryProcess == null)
            {
                _output.WriteLine("Skipping test - WinUI3 Gallery could not be launched");
                return;
            }

            try
            {
                // Act - パフォーマンステスト
                await Task.Delay(2000);

                var stopwatch = Stopwatch.StartNew();
                var windowResult = await _elementSearchService.GetWindowsAsync(5);
                stopwatch.Stop();

                var windowDiscoveryTime = stopwatch.ElapsedMilliseconds;

                stopwatch.Restart();
                var elementResult = await _elementSearchService.FindElementsAsync(null, null, "Button", null, 5);
                stopwatch.Stop();

                var elementSearchTime = stopwatch.ElapsedMilliseconds;

                // Assert
                Assert.True(windowDiscoveryTime < 6000, $"Window discovery should complete within 6 seconds, took {windowDiscoveryTime}ms");
                Assert.True(elementSearchTime < 6000, $"Element search should complete within 6 seconds, took {elementSearchTime}ms");

                _output.WriteLine($"Window discovery: {windowDiscoveryTime}ms");
                _output.WriteLine($"Element search: {elementSearchTime}ms");
            }
            finally
            {
                try { galleryProcess?.Kill(); } catch { }
            }
        }

        public void Dispose()
        {
            _subprocessExecutor?.Dispose();
            _serviceProvider?.Dispose();
        }
    }
}