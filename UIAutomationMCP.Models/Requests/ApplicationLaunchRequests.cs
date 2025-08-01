namespace UIAutomationMCP.Models.Requests
{
    /// <summary>
    /// Request for launching Win32 applications by path
    /// </summary>
    public class LaunchWin32ApplicationRequest
    {
        public string ApplicationPath { get; set; } = "";
        public string? Arguments { get; set; }
        public string? WorkingDirectory { get; set; }
    }

    /// <summary>
    /// Request for launching UWP applications by apps folder path
    /// </summary>
    public class LaunchUWPApplicationRequest
    {
        public string AppsFolderPath { get; set; } = "";
    }

    /// <summary>
    /// Request for launching applications by display name
    /// </summary>
    public class LaunchApplicationByNameRequest
    {
        public string ApplicationName { get; set; } = "";
    }
}