using UIAutomationMCP.Models.Abstractions;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Validation;
using UIAutomationMCP.Core.Abstractions;
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

            // Validate the request
            var validation = ValidateLaunchWin32ApplicationRequest(request);
            if (!validation.IsValid)
            {
                var operationId = Guid.NewGuid().ToString("N")[..8];
                return new ServerEnhancedResponse<ProcessLaunchResponse>
                {
                    Success = false,
                    ErrorMessage = string.Join("; ", validation.Errors),
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        OperationId = operationId,
                        ServerExecutedAt = DateTime.UtcNow
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = nameof(LaunchWin32ApplicationAsync),
                        TimeoutSeconds = timeoutSeconds
                    }
                };
            }

            try
            {
                var processResult = await ExecuteLaunchWin32ApplicationOperation(request, cancellationToken);
                var operationId = Guid.NewGuid().ToString("N")[..8];

                var context = new ServiceContext(nameof(LaunchWin32ApplicationAsync), timeoutSeconds);
                var metadata = CreateSuccessMetadata(processResult, context);

                return new ServerEnhancedResponse<ProcessLaunchResponse>
                {
                    Success = processResult.Success,
                    Data = processResult,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        OperationId = operationId,
                        ServerExecutedAt = DateTime.UtcNow,
                        ServerLogs = new List<string> { $"Successfully launched Win32 application: {applicationPath}" }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = nameof(LaunchWin32ApplicationAsync),
                        TimeoutSeconds = timeoutSeconds
                    }
                };
            }
            catch (Exception ex)
            {
                var operationId = Guid.NewGuid().ToString("N")[..8];
                return new ServerEnhancedResponse<ProcessLaunchResponse>
                {
                    Success = false,
                    ErrorMessage = $"Error launching Win32 application '{applicationPath}': {ex.Message}",
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        OperationId = operationId,
                        ServerExecutedAt = DateTime.UtcNow
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = nameof(LaunchWin32ApplicationAsync),
                        TimeoutSeconds = timeoutSeconds
                    }
                };
            }
        }

        public async Task<ServerEnhancedResponse<ProcessLaunchResponse>> LaunchUWPApplicationAsync(string appsFolderPath, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
        {
            var request = new LaunchUWPApplicationRequest
            {
                AppsFolderPath = appsFolderPath
            };

            // Validate the request
            var validation = ValidateLaunchUWPApplicationRequest(request);
            if (!validation.IsValid)
            {
                var operationId = Guid.NewGuid().ToString("N")[..8];
                return new ServerEnhancedResponse<ProcessLaunchResponse>
                {
                    Success = false,
                    ErrorMessage = string.Join("; ", validation.Errors),
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        OperationId = operationId,
                        ServerExecutedAt = DateTime.UtcNow
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = nameof(LaunchUWPApplicationAsync),
                        TimeoutSeconds = timeoutSeconds
                    }
                };
            }

            try
            {
                var processResult = await ExecuteLaunchUWPApplicationOperation(request, cancellationToken);
                var operationId = Guid.NewGuid().ToString("N")[..8];

                var context = new ServiceContext(nameof(LaunchUWPApplicationAsync), timeoutSeconds);
                var metadata = CreateSuccessMetadata(processResult, context);

                return new ServerEnhancedResponse<ProcessLaunchResponse>
                {
                    Success = processResult.Success,
                    Data = processResult,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        OperationId = operationId,
                        ServerExecutedAt = DateTime.UtcNow,
                        ServerLogs = new List<string> { $"Successfully launched UWP application: {appsFolderPath}" }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = nameof(LaunchUWPApplicationAsync),
                        TimeoutSeconds = timeoutSeconds
                    }
                };
            }
            catch (Exception ex)
            {
                var operationId = Guid.NewGuid().ToString("N")[..8];
                return new ServerEnhancedResponse<ProcessLaunchResponse>
                {
                    Success = false,
                    ErrorMessage = $"Error launching UWP application '{appsFolderPath}': {ex.Message}",
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        OperationId = operationId,
                        ServerExecutedAt = DateTime.UtcNow
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = nameof(LaunchUWPApplicationAsync),
                        TimeoutSeconds = timeoutSeconds
                    }
                };
            }
        }

        public async Task<ServerEnhancedResponse<ProcessLaunchResponse>> LaunchApplicationByNameAsync(string applicationName, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
        {
            var request = new LaunchApplicationByNameRequest
            {
                ApplicationName = applicationName
            };

            // Validate the request
            var validation = ValidateLaunchApplicationByNameRequest(request);
            if (!validation.IsValid)
            {
                var operationId = Guid.NewGuid().ToString("N")[..8];
                return new ServerEnhancedResponse<ProcessLaunchResponse>
                {
                    Success = false,
                    ErrorMessage = string.Join("; ", validation.Errors),
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        OperationId = operationId,
                        ServerExecutedAt = DateTime.UtcNow
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = nameof(LaunchApplicationByNameAsync),
                        TimeoutSeconds = timeoutSeconds
                    }
                };
            }

            try
            {
                var processResult = await ExecuteLaunchApplicationByNameOperation(request, cancellationToken);
                var operationId = Guid.NewGuid().ToString("N")[..8];

                var context = new ServiceContext(nameof(LaunchApplicationByNameAsync), timeoutSeconds);
                var metadata = CreateSuccessMetadata(processResult, context);

                return new ServerEnhancedResponse<ProcessLaunchResponse>
                {
                    Success = processResult.Success,
                    Data = processResult,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        OperationId = operationId,
                        ServerExecutedAt = DateTime.UtcNow,
                        ServerLogs = new List<string> { $"Successfully launched application: {applicationName}" }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = nameof(LaunchApplicationByNameAsync),
                        TimeoutSeconds = timeoutSeconds
                    }
                };
            }
            catch (Exception ex)
            {
                var operationId = Guid.NewGuid().ToString("N")[..8];
                return new ServerEnhancedResponse<ProcessLaunchResponse>
                {
                    Success = false,
                    ErrorMessage = $"Error launching application '{applicationName}': {ex.Message}",
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        OperationId = operationId,
                        ServerExecutedAt = DateTime.UtcNow
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = nameof(LaunchApplicationByNameAsync),
                        TimeoutSeconds = timeoutSeconds
                    }
                };
            }
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
            else if (!request.AppsFolderPath.StartsWith("shell:AppsFolder\\", StringComparison.OrdinalIgnoreCase) && 
                     !request.AppsFolderPath.StartsWith("shell:AppsFolder/", StringComparison.OrdinalIgnoreCase))
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

        private async Task<ProcessLaunchResponse> ExecuteLaunchApplicationByNameOperation(LaunchApplicationByNameRequest request, CancellationToken cancellationToken)
        {
            try
            {
                // Try to launch the application by name using shell execute
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = request.ApplicationName,
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                var process = System.Diagnostics.Process.Start(processInfo);
                
                if (process == null)
                {
                    return new ProcessLaunchResponse
                    {
                        Success = false,
                        ProcessId = 0,
                        ProcessName = request.ApplicationName,
                        HasExited = true,
                        WindowTitle = "",
                        Error = $"Failed to launch application: {request.ApplicationName}"
                    };
                }

                // Wait a bit for the process to initialize
                await Task.Delay(1000, cancellationToken);

                // Try to get the main window title
                string windowTitle = "";
                try
                {
                    if (!process.HasExited && process.MainWindowHandle != IntPtr.Zero)
                    {
                        windowTitle = process.MainWindowTitle;
                    }
                }
                catch
                {
                    // Ignore errors getting window title
                }

                return new ProcessLaunchResponse
                {
                    Success = true,
                    ProcessId = process.Id,
                    ProcessName = process.ProcessName,
                    HasExited = process.HasExited,
                    WindowTitle = windowTitle
                };
            }
            catch (Exception ex)
            {
                return new ProcessLaunchResponse
                {
                    Success = false,
                    ProcessId = 0,
                    ProcessName = request.ApplicationName,
                    HasExited = true,
                    WindowTitle = "",
                    Error = $"Error launching application '{request.ApplicationName}': {ex.Message}"
                };
            }
        }

        private async Task<ProcessLaunchResponse> ExecuteLaunchWin32ApplicationOperation(LaunchWin32ApplicationRequest request, CancellationToken cancellationToken)
        {
            try
            {
                // Try to launch the Win32 application by path
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = request.ApplicationPath,
                    Arguments = request.Arguments ?? "",
                    WorkingDirectory = request.WorkingDirectory ?? "",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                var process = System.Diagnostics.Process.Start(processInfo);
                
                if (process == null)
                {
                    return new ProcessLaunchResponse
                    {
                        Success = false,
                        ProcessId = 0,
                        ProcessName = System.IO.Path.GetFileNameWithoutExtension(request.ApplicationPath),
                        HasExited = true,
                        WindowTitle = "",
                        Error = $"Failed to launch application: {request.ApplicationPath}"
                    };
                }

                // Wait a bit for the process to initialize
                await Task.Delay(1000, cancellationToken);

                // Try to get the main window title
                string windowTitle = "";
                try
                {
                    if (!process.HasExited && process.MainWindowHandle != IntPtr.Zero)
                    {
                        windowTitle = process.MainWindowTitle;
                    }
                }
                catch
                {
                    // Ignore errors getting window title
                }

                return new ProcessLaunchResponse
                {
                    Success = true,
                    ProcessId = process.Id,
                    ProcessName = process.ProcessName,
                    HasExited = process.HasExited,
                    WindowTitle = windowTitle
                };
            }
            catch (Exception ex)
            {
                return new ProcessLaunchResponse
                {
                    Success = false,
                    ProcessId = 0,
                    ProcessName = System.IO.Path.GetFileNameWithoutExtension(request.ApplicationPath),
                    HasExited = true,
                    WindowTitle = "",
                    Error = $"Error launching application '{request.ApplicationPath}': {ex.Message}"
                };
            }
        }

        private async Task<ProcessLaunchResponse> ExecuteLaunchUWPApplicationOperation(LaunchUWPApplicationRequest request, CancellationToken cancellationToken)
        {
            try
            {
                // Launch UWP application using shell:AppsFolder path
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = request.AppsFolderPath,
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                var process = System.Diagnostics.Process.Start(processInfo);
                
                if (process == null)
                {
                    return new ProcessLaunchResponse
                    {
                        Success = false,
                        ProcessId = 0,
                        ProcessName = "UWP App",
                        HasExited = true,
                        WindowTitle = "",
                        Error = $"Failed to launch UWP application: {request.AppsFolderPath}"
                    };
                }

                // Wait a bit for the UWP app to launch
                await Task.Delay(2000, cancellationToken);

                // Note: For UWP apps, we can't easily get the actual app process
                // The explorer process launches the UWP app and exits
                return new ProcessLaunchResponse
                {
                    Success = true,
                    ProcessId = process.Id,
                    ProcessName = "UWP App",
                    HasExited = process.HasExited,
                    WindowTitle = ""
                };
            }
            catch (Exception ex)
            {
                return new ProcessLaunchResponse
                {
                    Success = false,
                    ProcessId = 0,
                    ProcessName = "UWP App",
                    HasExited = true,
                    WindowTitle = "",
                    Error = $"Error launching UWP application '{request.AppsFolderPath}': {ex.Message}"
                };
            }
        }

    }
}