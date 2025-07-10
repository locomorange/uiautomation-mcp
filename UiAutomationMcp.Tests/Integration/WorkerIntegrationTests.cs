using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Services;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using Xunit.Abstractions;

namespace UiAutomationMcp.Tests.Integration
{
    /// <summary>
    /// 統合テスト - 新しいWorkerServiceとkeyed DIパターンをテスト
    /// </summary>
    [Collection("UIAutomation Collection")]
    public class WorkerIntegrationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ILogger<WorkerService> _logger;
        private readonly ServiceProvider _serviceProvider;
        private readonly WorkerService _workerService;

        public WorkerIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
            
            // テスト用のサービスコンテナをセットアップ
            var services = new ServiceCollection();
            
            // ロガーを追加
            services.AddLogging(builder => 
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            
            // ヘルパーサービスを追加
            services.AddSingleton<ElementFinderService>();
            
            // テスト用のモック操作を追加
            services.AddKeyedSingleton<IUIAutomationOperation, TestMockOperation>("TestMock");
            
            // WorkerServiceを追加
            services.AddSingleton<WorkerService>();
            
            _serviceProvider = services.BuildServiceProvider();
            _logger = _serviceProvider.GetRequiredService<ILogger<WorkerService>>();
            _workerService = _serviceProvider.GetRequiredService<WorkerService>();
        }

        [Fact]
        public void WorkerService_ShouldCreateSuccessfully()
        {
            // Act & Assert
            Assert.NotNull(_workerService);
            Assert.NotNull(_serviceProvider);
            _output.WriteLine("WorkerService created successfully with new keyed DI architecture");
        }

        [Fact]
        public void ServiceProvider_ShouldHaveElementFinderService()
        {
            // Act
            var elementFinder = _serviceProvider.GetService<ElementFinderService>();
            
            // Assert
            Assert.NotNull(elementFinder);
            _output.WriteLine("ElementFinderService is properly registered");
        }

        [Fact]
        public void ServiceProvider_ShouldResolveKeyedOperations()
        {
            // Act
            var operation = _serviceProvider.GetKeyedService<IUIAutomationOperation>("TestMock");
            
            // Assert
            Assert.NotNull(operation);
            Assert.IsType<TestMockOperation>(operation);
            _output.WriteLine("Keyed service resolution works correctly");
        }

        [Fact]
        public void ServiceProvider_ShouldReturnNullForUnknownOperation()
        {
            // Act
            var operation = _serviceProvider.GetKeyedService<IUIAutomationOperation>("UnknownOperation");
            
            // Assert
            Assert.Null(operation);
            _output.WriteLine("Unknown operation correctly returns null");
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }
    }

    /// <summary>
    /// テスト用のモック操作クラス
    /// </summary>
    public class TestMockOperation : IUIAutomationOperation
    {
        public async Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            await Task.Delay(1); // 非同期操作をシミュレート
            return new OperationResult 
            { 
                Success = true, 
                Data = "Test operation completed successfully" 
            };
        }
    }
}