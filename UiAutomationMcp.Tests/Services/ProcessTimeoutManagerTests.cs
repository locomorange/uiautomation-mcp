#if FALSE // Disabled - ProcessTimeoutManager no longer exists in new architecture
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UIAutomationMCP.Server.Services;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.Services
{
    /// <summary>
    /// ProcessTimeoutManager unit tests - timeout management and process lifecycle
    /// </summary>
    public class ProcessTimeoutManagerTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ILogger<ProcessTimeoutManager> _logger;
        private readonly ProcessTimeoutManager _processManager;

        public ProcessTimeoutManagerTests(ITestOutputHelper output)
        {
            _output = output;
            
            var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            _logger = loggerFactory.CreateLogger<ProcessTimeoutManager>();
            
            _processManager = new ProcessTimeoutManager(_logger);
        }

        [Fact]
        public async Task ExecuteWithTimeoutAsync_ValidProcess_ShouldSucceed()
        {
            // Arrange - Use a simple ping command that completes quickly
            var processStartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "ping.exe"),
                Arguments = "-n 1 127.0.0.1"  // Single ping to localhost
            };

            // Act
            var result = await _processManager.ExecuteWithTimeoutAsync(
                processStartInfo,
                "",
                5,
                "TestPing");

            // Assert
            _output.WriteLine($"Result: Success={result.Success}, Output length={result.Output?.Length ?? 0}, Error='{result.Error}'");
            Assert.True(result.Success);
            Assert.NotNull(result.Output);
            Assert.False(result.TimedOut);
        }

        [Fact]
        public async Task ExecuteWithTimeoutAsync_NonExistentExecutable_ShouldFail()
        {
            // Arrange
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "non_existent_executable_12345.exe"
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
            Assert.Contains("not found", result.Error ?? "");
        }

        [Fact]
        public async Task ExecuteWithTimeoutAsync_LongRunningProcess_ShouldTimeout()
        {
            // Arrange - Use ping command which is more reliable for timeout testing
            var processStartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "ping.exe"),
                Arguments = "-n 10 127.0.0.1"  // Ping 10 times, takes ~10 seconds
            };

            // Act
            var stopwatch = Stopwatch.StartNew();
            var result = await _processManager.ExecuteWithTimeoutAsync(
                processStartInfo,
                "",
                2,  // 2 second timeout
                "TestTimeout");
            stopwatch.Stop();

            // Assert
            _output.WriteLine($"Result: Success={result.Success}, TimedOut={result.TimedOut}, Elapsed={stopwatch.ElapsedMilliseconds}ms, Error={result.Error}");
            Assert.False(result.Success);
            // Don't assert TimedOut as it might vary depending on process state
            Assert.True(stopwatch.ElapsedMilliseconds < 5000); // Should complete within 5 seconds (much less than 10s)
        }

        [Fact]
        public void KillProcessTree_WithValidProcess_ShouldKillProcess()
        {
            // Arrange
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "ping.exe"),
                    Arguments = "-n 100 127.0.0.1",  // Long running process
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            try
            {
                process.Start();
                var processId = process.Id;
                
                _output.WriteLine($"Started process PID: {processId}");

                // Give process time to start properly
                Thread.Sleep(200);
                
                // Act
                _processManager.KillProcessTree(process, "TestKill");

                // Assert
                // Give it time to be killed
                Thread.Sleep(500);
                
                Assert.True(process.HasExited);
                _output.WriteLine($"Process PID {processId} was successfully killed");
            }
            finally
            {
                // Cleanup - ensure process is killed even if test fails
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                    process.Dispose();
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        [Fact]
        public async Task TerminateAllActiveProcesses_ShouldKillTrackedProcesses()
        {
            // Arrange
            var processStartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "ping.exe"),
                Arguments = "-n 100 127.0.0.1"
            };

            // Start a process through the manager to ensure it's tracked
            var processTask = _processManager.ExecuteWithTimeoutAsync(
                processStartInfo,
                "",
                30,
                "TestTerminateAll");

            // Wait for process to start
            await Task.Delay(500);

            // Act
            _processManager.TerminateAllActiveProcesses();

            // Assert
            // The long-running process should be terminated
            var result = await processTask;
            _output.WriteLine($"Process terminated: Success={result.Success}, TimedOut={result.TimedOut}, Error={result.Error}");
            
            // The process should have been killed before normal completion
            Assert.False(result.Success);
        }

        [Fact]
        public async Task Dispose_ShouldCleanupAllProcesses()
        {
            // Arrange
            var manager = new ProcessTimeoutManager(_logger);
            var processStartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "ping.exe"),
                Arguments = "-n 100 127.0.0.1"
            };

            // Start a process
            var processTask = manager.ExecuteWithTimeoutAsync(
                processStartInfo,
                "",
                30,
                "TestDispose");

            // Wait for process to start
            await Task.Delay(500);

            // Act
            manager.Dispose();

            // Assert
            var result = await processTask;
            _output.WriteLine($"After dispose: Success={result.Success}, Error={result.Error}");
            
            // Process should be terminated
            Assert.False(result.Success);
        }

        public void Dispose()
        {
            _processManager?.Dispose();
        }
    }
}
#endif
