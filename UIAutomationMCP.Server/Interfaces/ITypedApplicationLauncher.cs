using UIAutomationMCP.Shared;

namespace UIAutomationMCP.Server.Interfaces
{
    /// <summary>
    /// Type-safe interface for application launching operations
    /// </summary>
    public interface ITypedApplicationLauncher
    {
        /// <summary>
        /// Launch a Win32 application by executable path
        /// </summary>
        Task<ProcessLaunchResponse> LaunchWin32ApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null, int timeoutSeconds = 60, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Launch a UWP application by shell:AppsFolder path
        /// </summary>
        Task<ProcessLaunchResponse> LaunchUWPApplicationAsync(string appsFolderPath, int timeoutSeconds = 60, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Launch an application by name
        /// </summary>
        Task<ProcessLaunchResponse> LaunchApplicationByNameAsync(string applicationName, int timeoutSeconds = 60, CancellationToken cancellationToken = default);
    }
}