#if FALSE // Disabled - ProcessTimeoutManager no longer exists in new architecture
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Services;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.Services
{
    /// <summary>
    /// Simple tests for ProcessTimeoutManager functionality
    /// Tests basic functionality without complex process operations
    /// </summary>
    public class SimpleProcessManagerTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ILogger<ProcessTimeoutManager> _logger;
        private readonly ProcessTimeoutManager _processManager;

        public SimpleProcessManagerTests(ITestOutputHelper output)
        {
            _output = output;
            
            var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            _logger = loggerFactory.CreateLogger<ProcessTimeoutManager>();
            
            _processManager = new ProcessTimeoutManager(_logger);
        }

        [Fact]
        public void ProcessTimeoutManager_ShouldCreateSuccessfully()
        {
            // Act & Assert
            Assert.NotNull(_processManager);
            _output.WriteLine("ProcessTimeoutManager created successfully");
        }

        [Fact]
        public void ProcessTimeoutManager_ShouldImplementIDisposable()
        {
            // Act & Assert
            Assert.IsAssignableFrom<IDisposable>(_processManager);
            _output.WriteLine("ProcessTimeoutManager implements IDisposable");
        }

        [Fact]
        public async Task ExecuteWithTimeoutAsync_WithNonExistentFile_ShouldReturnFailure()
        {
            // Arrange
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "definitely_does_not_exist_12345.exe"
            };

            // Act
            var result = await _processManager.ExecuteWithTimeoutAsync(
                processStartInfo,
                "",
                5,
                "TestNonExistent");

            // Assert
            _output.WriteLine($"Result: Success={result.Success}, Error='{result.Error}'");
            Assert.False(result.Success);
            Assert.NotNull(result.Error);
            Assert.Contains("not found", result.Error);
            Assert.False(result.TimedOut);
        }

        [Fact]
        public void TerminateAllActiveProcesses_ShouldNotThrow()
        {
            // Act & Assert - Should not throw even with no active processes
            var exception = Record.Exception(() => _processManager.TerminateAllActiveProcesses());
            
            Assert.Null(exception);
            _output.WriteLine("TerminateAllActiveProcesses completed without exceptions");
        }

        [Fact]
        public void Dispose_ShouldNotThrow()
        {
            // Arrange
            var manager = new ProcessTimeoutManager(_logger);

            // Act & Assert
            var exception = Record.Exception(() => manager.Dispose());
            
            Assert.Null(exception);
            _output.WriteLine("Dispose completed without exceptions");
        }

        [Fact]
        public void KillProcessTree_WithNullProcess_ShouldNotThrow()
        {
            // Act & Assert
            var exception = Record.Exception(() => _processManager.KillProcessTree(null, "TestNull"));
            
            Assert.Null(exception);
            _output.WriteLine("KillProcessTree with null process completed safely");
        }

        public void Dispose()
        {
            try
            {
                _processManager?.Dispose();
            }
            catch
            {
                // Ignore disposal errors in tests
            }
        }
    }
}
#endif
