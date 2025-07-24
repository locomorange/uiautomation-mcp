using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Validation;
using UIAutomationMCP.Shared.Metadata;
using UIAutomationMCP.Shared.Abstractions;
using UIAutomationMCP.Server.Infrastructure;

namespace UIAutomationMCP.Server.Services
{
    public interface IApplicationLauncher
    {
        Task<ServerEnhancedResponse<ProcessLaunchResponse>> LaunchWin32ApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null, int timeoutSeconds = 60, CancellationToken cancellationToken = default);
        Task<ServerEnhancedResponse<ProcessLaunchResponse>> LaunchUWPApplicationAsync(string appsFolderPath, int timeoutSeconds = 60, CancellationToken cancellationToken = default);
        Task<ServerEnhancedResponse<ProcessLaunchResponse>> LaunchApplicationByNameAsync(string applicationName, int timeoutSeconds = 60, CancellationToken cancellationToken = default);
    }

    public class ApplicationLauncher : BaseUIAutomationService<ApplicationLauncherMetadata>, IApplicationLauncher
    {
        public ApplicationLauncher(IOperationExecutor executor, ILogger<ApplicationLauncher> logger)
            : base(executor, logger)
        {
        }

        protected override string GetOperationType() => "application";

        public async Task<ServerEnhancedResponse<ProcessLaunchResponse>> LaunchWin32ApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
        {
            var request = new LaunchWin32ApplicationRequest
            {
                ApplicationPath = applicationPath,
                Arguments = arguments,
                WorkingDirectory = workingDirectory
            };

            return await ExecuteServiceOperationAsync<LaunchWin32ApplicationRequest, ProcessLaunchResponse>(
                "LaunchWin32Application",
                request,
                nameof(LaunchWin32ApplicationAsync),
                timeoutSeconds,
                ValidateLaunchWin32ApplicationRequest
            );
        }

        public async Task<ServerEnhancedResponse<ProcessLaunchResponse>> LaunchUWPApplicationAsync(string appsFolderPath, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
        {
            var request = new LaunchUWPApplicationRequest
            {
                AppsFolderPath = appsFolderPath
            };

            return await ExecuteServiceOperationAsync<LaunchUWPApplicationRequest, ProcessLaunchResponse>(
                "LaunchUWPApplication",
                request,
                nameof(LaunchUWPApplicationAsync),
                timeoutSeconds,
                ValidateLaunchUWPApplicationRequest
            );
        }

        public async Task<ServerEnhancedResponse<ProcessLaunchResponse>> LaunchApplicationByNameAsync(string applicationName, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
        {
            var request = new LaunchApplicationByNameRequest
            {
                ApplicationName = applicationName
            };

            return await ExecuteServiceOperationAsync<LaunchApplicationByNameRequest, ProcessLaunchResponse>(
                "LaunchApplicationByName",
                request,
                nameof(LaunchApplicationByNameAsync),
                timeoutSeconds,
                ValidateLaunchApplicationByNameRequest
            );
        }

        private static ValidationResult ValidateLaunchWin32ApplicationRequest(LaunchWin32ApplicationRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.ApplicationPath))
            {
                errors.Add("ApplicationPath is required");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        private static ValidationResult ValidateLaunchUWPApplicationRequest(LaunchUWPApplicationRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.AppsFolderPath))
            {
                errors.Add("AppsFolderPath is required");
            }
            else if (!request.AppsFolderPath.StartsWith("shell:AppsFolder\\", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("Invalid UWP app path. Must start with 'shell:AppsFolder\\'");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        private static ValidationResult ValidateLaunchApplicationByNameRequest(LaunchApplicationByNameRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.ApplicationName))
            {
                errors.Add("ApplicationName is required");
            }

            return errors.Count > 0 ? ValidationResult.Failure(errors) : ValidationResult.Success;
        }

        protected override ApplicationLauncherMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);

            if (data is ProcessLaunchResponse processResponse)
            {
                metadata.OperationSuccessful = processResponse.Success;
                metadata.ProcessId = processResponse.ProcessId;
                metadata.ProcessName = processResponse.ProcessName;
                metadata.HasExited = processResponse.HasExited;
                metadata.WindowTitle = processResponse.WindowTitle;

                if (context.MethodName.Contains("LaunchWin32Application"))
                {
                    metadata.ActionPerformed = "applicationLaunched";
                }
                else if (context.MethodName.Contains("LaunchUWPApplication"))
                {
                    metadata.ActionPerformed = "uwpApplicationLaunched";
                }
                else if (context.MethodName.Contains("LaunchApplicationByName"))
                {
                    metadata.ActionPerformed = "applicationLaunchedByName";
                }
            }

            return metadata;
        }

    }

    // Request classes for application launcher operations
    public class LaunchWin32ApplicationRequest
    {
        public string ApplicationPath { get; set; } = "";
        public string? Arguments { get; set; }
        public string? WorkingDirectory { get; set; }
    }

    public class LaunchUWPApplicationRequest
    {
        public string AppsFolderPath { get; set; } = "";
    }

    public class LaunchApplicationByNameRequest
    {
        public string ApplicationName { get; set; } = "";
    }
}