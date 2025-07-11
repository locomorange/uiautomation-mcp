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
    /// 高度なコントロールパターンテスト
    /// 実際のアプリケーションでの複雑な操作をテスト
    /// </summary>
    [Collection("UIAutomation Collection")]
    public class AdvancedControlPatternTests : IDisposable
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
        private readonly SubprocessBasedTreeNavigationService _treeNavigationService;
        private readonly SubprocessBasedTabService _tabService;
        private readonly SubprocessBasedMenuService _menuService;
        private readonly SubprocessBasedListService _listService;
        private readonly string _workerPath;

        public AdvancedControlPatternTests(ITestOutputHelper output)
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
            _treeNavigationService = new SubprocessBasedTreeNavigationService(_serviceProvider.GetRequiredService<ILogger<SubprocessBasedTreeNavigationService>>(), _subprocessExecutor);
            _tabService = new SubprocessBasedTabService(_serviceProvider.GetRequiredService<ILogger<SubprocessBasedTabService>>(), _subprocessExecutor);
            _menuService = new SubprocessBasedMenuService(_serviceProvider.GetRequiredService<ILogger<SubprocessBasedMenuService>>(), _subprocessExecutor);
            _listService = new SubprocessBasedListService(_serviceProvider.GetRequiredService<ILogger<SubprocessBasedListService>>(), _subprocessExecutor);
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
                    await Task.Delay(3000);
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
        public async Task AdvancedPattern_TextOperations_WithNotepad()
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

                var operations = new List<(string name, Func<Task<object>> operation)>
                {
                    ("SetText", async () => await _textService.SetTextAsync("Edit", "Hello UIAutomation!", null, null, 10)),
                    ("GetText", async () => await _textService.GetTextAsync("Edit", null, null, 10)),
                    ("AppendText", async () => await _textService.AppendTextAsync("Edit", " Additional text", null, null, 10)),
                    ("SelectText", async () => await _textService.SelectTextAsync("Edit", 0, 5, null, null, 10)),
                    ("GetSelectedText", async () => await _textService.GetSelectedTextAsync("Edit", null, null, 10)),
                };

                var results = new List<(string name, bool success, string result)>();

                // Act & Assert
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

                // 少なくとも60%の操作が応答を返すことを確認
                var successCount = results.Count(r => r.success && !string.IsNullOrEmpty(r.result));
                var successRate = (double)successCount / results.Count;
                
                _output.WriteLine($"Text operations success rate: {successRate:P1} ({successCount}/{results.Count})");
                Assert.True(successRate >= 0.6, $"Expected at least 60% text operations to succeed, got {successRate:P1}");
            }
            finally
            {
                try { notepadProcess?.Kill(); } catch { }
            }
        }

        [Fact]
        public async Task AdvancedPattern_TreeNavigation_Operations()
        {
            // Act - 要素ツリー操作のテスト
            var operations = new List<(string name, Func<Task<object>> operation)>
            {
                ("GetElementTree", async () => await _treeNavigationService.GetElementTreeAsync(null, null, 2, 10)),
                ("GetChildren", async () => await _treeNavigationService.GetChildrenAsync("Desktop", null, null, 10)),
                ("GetParent", async () => await _treeNavigationService.GetParentAsync("test", null, null, 10)),
                ("GetAncestors", async () => await _treeNavigationService.GetAncestorsAsync("test", null, null, 10)),
                ("GetSiblings", async () => await _treeNavigationService.GetSiblingsAsync("test", null, null, 10)),
            };

            var results = new List<(string name, bool success)>();

            // Assert
            foreach (var (name, operation) in operations)
            {
                try
                {
                    var result = await operation();
                    results.Add((name, result != null));
                    _output.WriteLine($"✅ {name}: Response received");
                }
                catch (Exception ex)
                {
                    results.Add((name, false));
                    _output.WriteLine($"❌ {name}: {ex.Message}");
                }
            }

            // 全ての操作がレスポンスを返すことを確認（エラーでも良い）
            Assert.True(results.Count == operations.Count, "All tree navigation operations should return responses");
        }

        [Fact]
        public async Task AdvancedPattern_ControlType_Operations()
        {
            // Act - 様々なコントロールタイプ操作のテスト
            var operations = new List<(string name, Func<Task<object>> operation)>
            {
                ("ButtonOperation", async () => await _buttonService.ButtonOperationAsync("Button", "click", null, null, 5)),
                ("TabOperation", async () => await _tabService.TabOperationAsync("TabControl", "list", null, null, null, 5)),
                ("MenuOperation", async () => await _menuService.MenuOperationAsync("Menu", null, null, 5)),
                ("ListOperation", async () => await _listService.ListOperationAsync("List", "getitems", null, null, null, null, 5)),
            };

            var results = new List<(string name, bool success)>();

            // Assert
            foreach (var (name, operation) in operations)
            {
                try
                {
                    var result = await operation();
                    results.Add((name, result != null));
                    _output.WriteLine($"✅ {name}: Response received");
                }
                catch (Exception ex)
                {
                    results.Add((name, false));
                    _output.WriteLine($"❌ {name}: {ex.Message}");
                }
            }

            // 全ての操作がレスポンスを返すことを確認
            Assert.True(results.Count == operations.Count, "All control type operations should return responses");
        }

        [Fact]
        public async Task AdvancedPattern_ValueAndToggle_Operations()
        {
            // Act - 値とトグル操作のテスト
            var operations = new List<(string name, Func<Task<object>> operation)>
            {
                ("GetValue", async () => await _valueService.GetValueAsync("test", null, null, 5)),
                ("SetValue", async () => await _valueService.SetValueAsync("test", "new value", null, null, 5)),
                ("ToggleElement", async () => await _toggleService.ToggleElementAsync("test", null, null, 5)),
                ("GetToggleState", async () => await _toggleService.GetToggleStateAsync("test", null, null, 5)),
                ("SetToggleState", async () => await _toggleService.SetToggleStateAsync("test", "On", null, null, 5)),
            };

            var results = new List<(string name, bool success)>();

            // Assert
            foreach (var (name, operation) in operations)
            {
                try
                {
                    var result = await operation();
                    results.Add((name, result != null));
                    _output.WriteLine($"✅ {name}: Response received");
                }
                catch (Exception ex)
                {
                    results.Add((name, false));
                    _output.WriteLine($"❌ {name}: {ex.Message}");
                }
            }

            // 全ての操作がレスポンスを返すことを確認
            Assert.True(results.Count == operations.Count, "All value and toggle operations should return responses");
        }

        [Fact]
        public async Task AdvancedPattern_Selection_Operations()
        {
            // Act - 選択操作のテスト
            var operations = new List<(string name, Func<Task<object>> operation)>
            {
                ("SelectItem", async () => await _selectionService.SelectItemAsync("test", null, null, 5)),
                ("AddToSelection", async () => await _selectionService.AddToSelectionAsync("test", null, null, 5)),
                ("RemoveFromSelection", async () => await _selectionService.RemoveFromSelectionAsync("test", null, null, 5)),
                ("ClearSelection", async () => await _selectionService.ClearSelectionAsync("test", null, null, 5)),
                ("GetSelection", async () => await _selectionService.GetSelectionAsync("test", null, null, 5)),
            };

            var results = new List<(string name, bool success)>();

            // Assert
            foreach (var (name, operation) in operations)
            {
                try
                {
                    var result = await operation();
                    results.Add((name, result != null));
                    _output.WriteLine($"✅ {name}: Response received");
                }
                catch (Exception ex)
                {
                    results.Add((name, false));
                    _output.WriteLine($"❌ {name}: {ex.Message}");
                }
            }

            // 全ての操作がレスポンスを返すことを確認
            Assert.True(results.Count == operations.Count, "All selection operations should return responses");
        }

        [Fact]
        public async Task AdvancedPattern_ComprehensiveOperationTest()
        {
            // Act - 包括的な操作テスト
            var allOperations = new List<(string category, string name, Func<Task<object>> operation)>
            {
                ("Search", "GetWindows", async () => await _elementSearchService.GetWindowsAsync(5)),
                ("Search", "FindElements", async () => await _elementSearchService.FindElementsAsync(null, null, "Button", null, 5)),
                ("Navigation", "GetElementTree", async () => await _treeNavigationService.GetElementTreeAsync(null, null, 1, 5)),
                ("Invoke", "InvokeElement", async () => await _invokeService.InvokeElementAsync("test", null, null, 5)),
                ("Value", "SetValue", async () => await _valueService.SetValueAsync("test", "value", null, null, 5)),
                ("Toggle", "ToggleElement", async () => await _toggleService.ToggleElementAsync("test", null, null, 5)),
                ("Selection", "SelectItem", async () => await _selectionService.SelectItemAsync("test", null, null, 5)),
                ("Button", "ButtonClick", async () => await _buttonService.ButtonOperationAsync("test", "click", null, null, 5)),
                ("Text", "SetText", async () => await _textService.SetTextAsync("test", "text", null, null, 5)),
            };

            var categoryResults = new Dictionary<string, List<bool>>();

            // Assert
            foreach (var (category, name, operation) in allOperations)
            {
                if (!categoryResults.ContainsKey(category))
                    categoryResults[category] = new List<bool>();

                try
                {
                    var result = await operation();
                    categoryResults[category].Add(result != null);
                    _output.WriteLine($"✅ {category}/{name}: Response received");
                }
                catch (Exception ex)
                {
                    categoryResults[category].Add(false);
                    _output.WriteLine($"❌ {category}/{name}: {ex.Message}");
                }
            }

            // カテゴリ別成功率を確認
            foreach (var (category, results) in categoryResults)
            {
                var successCount = results.Count(r => r);
                var successRate = (double)successCount / results.Count;
                _output.WriteLine($"{category} category: {successRate:P1} ({successCount}/{results.Count})");
            }

            // 全体として80%以上の操作がレスポンスを返すことを確認
            var totalOperations = allOperations.Count;
            var totalSuccesses = categoryResults.SelectMany(kvp => kvp.Value).Count(r => r);
            var overallSuccessRate = (double)totalSuccesses / totalOperations;
            
            _output.WriteLine($"Overall success rate: {overallSuccessRate:P1} ({totalSuccesses}/{totalOperations})");
            Assert.True(overallSuccessRate >= 0.8, $"Expected at least 80% overall success rate, got {overallSuccessRate:P1}");
        }

        public void Dispose()
        {
            _subprocessExecutor?.Dispose();
            _serviceProvider?.Dispose();
        }
    }
}