using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using System.Text.Json;

namespace TestServerWorkerCommunication
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Setup logging
            using var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            var logger = loggerFactory.CreateLogger<SubprocessExecutor>();

            // Path to worker executable
            var workerPath = @"C:\Users\yksh\source\clones\win-UIAutomation-mcp\UIAutomationMCP.Worker\bin\Debug\net9.0-windows\UIAutomationMCP.Worker.exe";

            Console.WriteLine("Testing Server-Worker Communication...");
            Console.WriteLine($"Worker Path: {workerPath}");

            try
            {
                using var executor = new SubprocessExecutor(logger, workerPath);

                // Test 1: Invalid operation (should return error)
                Console.WriteLine("\n=== Test 1: Invalid Operation ===");
                try
                {
                    var result1 = await executor.ExecuteAsync<object>("InvalidOperation", null, 5);
                    Console.WriteLine($"Unexpected success: {JsonSerializer.Serialize(result1)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Expected error: {ex.Message}");
                }

                // Test 2: Test registered operation (if any)
                Console.WriteLine("\n=== Test 2: Test Mock Operation ===");
                try
                {
                    var result2 = await executor.ExecuteAsync<object>("TestMock", new Dictionary<string, object>(), 5);
                    Console.WriteLine($"Result: {JsonSerializer.Serialize(result2)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                Console.WriteLine("\n=== Communication Test Complete ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}