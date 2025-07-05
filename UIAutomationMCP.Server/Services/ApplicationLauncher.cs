using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UiAutomationMcp.Models;

namespace UiAutomationMcpServer.Services
{
    public interface IApplicationLauncher
    {
        Task<ProcessResult> LaunchApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null);
    }

    public class ApplicationLauncher : IApplicationLauncher
    {
        private readonly ILogger<ApplicationLauncher> _logger;

        public ApplicationLauncher(ILogger<ApplicationLauncher> logger)
        {
            _logger = logger;
        }


        public async Task<ProcessResult> LaunchApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null)
        {
            try
            {
                _logger.LogInformation("Launching application: {ApplicationPath}", applicationPath);

                if (!File.Exists(applicationPath))
                {
                    return new ProcessResult { Success = false, Error = $"Application not found: {applicationPath}" };
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = applicationPath,
                    Arguments = arguments ?? "",
                    WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(applicationPath) ?? "",
                    UseShellExecute = true
                };

                var process = Process.Start(startInfo);
                if (process == null)
                {
                    return new ProcessResult { Success = false, Error = "Failed to start process" };
                }

                await Task.Delay(1000);

                var hasExited = process.HasExited;
                var processId = hasExited ? 0 : process.Id;
                var processName = hasExited ? "" : process.ProcessName;

                _logger.LogInformation("Application launched: ProcessId={ProcessId}, ProcessName={ProcessName}, HasExited={HasExited}",
                    processId, processName, hasExited);

                return new ProcessResult
                {
                    Success = true,
                    ProcessId = processId,
                    ProcessName = processName,
                    HasExited = hasExited
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error launching application: {ApplicationPath}", applicationPath);
                return new ProcessResult { Success = false, Error = ex.Message };
            }
        }

    }
}
