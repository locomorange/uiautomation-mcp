using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UIAutomationMCP.Models;

namespace UIAutomationMCP.Server.Services
{
    public interface IApplicationLauncher
    {
        Task<ProcessResult> LaunchApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null, int timeoutSeconds = 60, CancellationToken cancellationToken = default);
    }

    public class ApplicationLauncher : IApplicationLauncher
    {
        private readonly ILogger<ApplicationLauncher> _logger;

        public ApplicationLauncher(ILogger<ApplicationLauncher> logger)
        {
            _logger = logger;
        }


        public async Task<ProcessResult> LaunchApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
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

                // プロセスIDを先に取得（プロセスが終了する前に）
                var processId = process.Id;
                string processName;
                try
                {
                    processName = process.ProcessName;
                }
                catch (InvalidOperationException)
                {
                    // プロセスが既に終了している場合でも、プロセス名を取得しようとする
                    processName = Path.GetFileNameWithoutExtension(applicationPath);
                }

                _logger.LogInformation("Process started: ProcessId={ProcessId}, ProcessName={ProcessName}", processId, processName);

                // プロセスが安定するまで少し待機
                await Task.Delay(500, cancellationToken);

                // プロセス状態をチェック（プロセスIDは起動時の値を保持）
                var hasExited = process.HasExited;

                _logger.LogInformation("Application launched: ProcessId={ProcessId}, ProcessName={ProcessName}, HasExited={HasExited}",
                    processId, processName, hasExited);

                return new ProcessResult
                {
                    Success = true,
                    ProcessId = processId,  // 起動時のプロセスIDを常に返す
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
