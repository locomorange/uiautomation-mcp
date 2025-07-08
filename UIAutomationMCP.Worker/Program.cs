using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Worker.Operations;
using UIAutomationMCP.Worker.Services;
using System.Text.Json;

namespace UIAutomationMCP.Worker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            // Configure logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            // Register UI Automation operations
            builder.Services.AddSingleton<InvokeOperations>();
            builder.Services.AddSingleton<ValueOperations>();
            builder.Services.AddSingleton<ElementSearchOperations>();
            builder.Services.AddSingleton<ElementPropertyOperations>();
            builder.Services.AddSingleton<ToggleOperations>();
            builder.Services.AddSingleton<SelectionOperations>();
            builder.Services.AddSingleton<WindowOperations>();
            builder.Services.AddSingleton<TextOperations>();
            builder.Services.AddSingleton<LayoutOperations>();
            builder.Services.AddSingleton<RangeOperations>();

            // Register Worker service
            builder.Services.AddSingleton<WorkerService>();

            var host = builder.Build();

            var workerService = host.Services.GetRequiredService<WorkerService>();
            await workerService.RunAsync();
        }
    }
}