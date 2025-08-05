using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace UIAutomationMCP.Server.Helpers
{
    /// <summary>
    /// Centralized utility for resolving file system paths, including symbolic links,
    /// and Worker/Monitor executable paths in both development and production environments.
    /// </summary>
    public static class ExecutablePathResolver
    {
        private const string WorkerProjectName = "UIAutomationMCP.Subprocess.Worker";
        private const string MonitorProjectName = "UIAutomationMCP.Subprocess.Monitor";
        private const string WorkerExecutableName = "UIAutomationMCP.Subprocess.Worker.exe";
        private const string MonitorExecutableName = "UIAutomationMCP.Subprocess.Monitor.exe";
        private const string WorkerCsprojName = "UIAutomationMCP.Subprocess.Worker.csproj";
        private const string WorkerLegacyCsprojName = "UIAutomationMCP.Worker.csproj"; // Legacy name used in SubprocessExecutor

        /// <summary>
        /// Gets the real physical path of the current executable, resolving any symbolic links.
        /// </summary>
        public static string GetExecutableRealPath()
        {
            // For Native AOT, Environment.ProcessPath is the most reliable
            var executablePath = Environment.ProcessPath
                ?? throw new InvalidOperationException("Unable to determine executable path");

            var fileInfo = new FileInfo(executablePath);

            // Resolve symbolic links recursively (returnFinalTarget: true)
            var resolvedTarget = fileInfo.ResolveLinkTarget(returnFinalTarget: true);

            // Return the directory of the resolved executable
            return Path.GetDirectoryName(resolvedTarget?.FullName ?? fileInfo.FullName)!;
        }

        /// <summary>
        /// Resolves the path to the Worker executable or project directory.
        /// </summary>
        /// <param name="baseDirectory">Base directory to start search from. Uses executable's real path if null.</param>
        /// <returns>Path to Worker executable, DLL, or project directory; null if not found.</returns>
        public static string? ResolveWorkerPath(string? baseDirectory = null)
        {
            return ResolveExecutablePath(WorkerProjectName, WorkerExecutableName, WorkerCsprojName, baseDirectory);
        }

        /// <summary>
        /// Resolves the path to the Monitor executable or project directory.
        /// </summary>
        /// <param name="baseDirectory">Base directory to start search from. Uses executable's real path if null.</param>
        /// <returns>Path to Monitor executable, DLL, or project directory; null if not found.</returns>
        public static string? ResolveMonitorPath(string? baseDirectory = null)
        {
            return ResolveExecutablePath(MonitorProjectName, MonitorExecutableName, $"{MonitorProjectName}.csproj", baseDirectory);
        }

        /// <summary>
        /// Generic method to resolve any executable path based on project name and executable name.
        /// </summary>
        /// <param name="projectName">Name of the project (e.g., "UIAutomationMCP.Subprocess.Worker")</param>
        /// <param name="executableName">Name of the executable (e.g., "UIAutomationMCP.Subprocess.Worker.exe")</param>
        /// <param name="baseDirectory">Base directory to start search from. Uses executable's real path if null.</param>
        /// <returns>Path to executable, DLL, or project directory; null if not found.</returns>
        public static string? ResolveGenericExecutablePath(string projectName, string executableName, string? baseDirectory = null)
        {
            var csprojName = $"{projectName}.csproj";
            return ResolveExecutablePath(projectName, executableName, csprojName, baseDirectory);
        }

        /// <summary>
        /// Determines if the current environment is development based on the base directory.
        /// </summary>
        /// <param name="baseDirectory">Directory to check</param>
        /// <returns>True if in development environment (contains bin\Debug or bin\Release)</returns>
        public static bool IsDevEnvironment(string baseDirectory)
        {
            return baseDirectory.Contains("bin\\Debug") || baseDirectory.Contains("bin\\Release");
        }

        /// <summary>
        /// Finds the solution directory by navigating up from the start path.
        /// </summary>
        /// <param name="startPath">Path to start navigation from</param>
        /// <returns>Solution directory path, or null if not found</returns>
        public static string? FindSolutionDirectory(string startPath)
        {
            if (IsDevEnvironment(startPath))
            {
                // For development: startPath is like C:\...\UIAutomationMCP.Server\bin\Debug\net9.0-windows
                // Navigate up 4 levels to get to UIAutomationMCP solution directory
                return Directory.GetParent(startPath)?.Parent?.Parent?.Parent?.FullName;
            }

            return null;
        }

        /// <summary>
        /// Determines the configuration (Debug/Release) from the base directory.
        /// </summary>
        /// <param name="baseDirectory">Base directory to analyze</param>
        /// <returns>"Debug" or "Release"</returns>
        public static string GetConfiguration(string baseDirectory)
        {
            return baseDirectory.Contains("Debug") ? "Debug" : "Release";
        }

        /// <summary>
        /// Checks if a path represents a project directory for development use.
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <param name="csprojName">Name of the csproj file to look for</param>
        /// <returns>True if directory contains the specified csproj file</returns>
        public static bool IsProjectDirectory(string path, string csprojName)
        {
            if (!Directory.Exists(path))
                return false;

            // Check for the primary csproj name
            if (File.Exists(Path.Combine(path, csprojName)))
                return true;

            // For Worker, also check legacy csproj name
            if (csprojName == WorkerCsprojName && File.Exists(Path.Combine(path, WorkerLegacyCsprojName)))
                return true;

            return false;
        }

        /// <summary>
        /// Checks if a path represents the Worker project directory for development use.
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True if directory contains Worker csproj file</returns>
        public static bool IsWorkerProjectDirectory(string path)
        {
            return IsProjectDirectory(path, WorkerCsprojName);
        }

        /// <summary>
        /// Checks if a path represents the Monitor project directory for development use.
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True if directory contains Monitor csproj file</returns>
        public static bool IsMonitorProjectDirectory(string path)
        {
            return IsProjectDirectory(path, $"{MonitorProjectName}.csproj");
        }

        /// <summary>
        /// Checks if a path represents any .NET project directory for development use.
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True if directory contains any .csproj file</returns>
        public static bool IsAnyProjectDirectory(string path)
        {
            if (!Directory.Exists(path))
                return false;

            return Directory.GetFiles(path, "*.csproj").Length > 0;
        }

        /// <summary>
        /// Core method that resolves executable paths for Worker or Monitor.
        /// </summary>
        private static string? ResolveExecutablePath(string projectName, string executableName, string csprojName, string? baseDirectory)
        {
            var actualBaseDir = baseDirectory ?? GetExecutableRealPath();
            var isDev = IsDevEnvironment(actualBaseDir);

            if (isDev)
            {
                return ResolveDevPath(projectName, executableName, csprojName, actualBaseDir);
            }
            else
            {
                return ResolveProductionPath(executableName, actualBaseDir);
            }
        }

        /// <summary>
        /// Resolves executable path in development environment.
        /// </summary>
        private static string? ResolveDevPath(string projectName, string executableName, string csprojName, string baseDir)
        {
            var solutionDir = FindSolutionDirectory(baseDir);
            if (solutionDir == null)
                return null;

            var config = GetConfiguration(baseDir);

            // Try different path combinations for development builds
            var devPaths = new[]
            {
                // Built executable with runtime identifier
                Path.Combine(solutionDir, projectName, "bin", config, "net9.0-windows", "win-x64", executableName),
                // Built executable without runtime identifier  
                Path.Combine(solutionDir, projectName, "bin", config, "net9.0-windows", executableName),
                // Project directory for 'dotnet run'
                Path.Combine(solutionDir, projectName)
            };

            foreach (var path in devPaths)
            {
                if (File.Exists(path) || IsProjectDirectory(path, csprojName))
                {
                    return path;
                }
            }

            return null;
        }

        /// <summary>
        /// Resolves executable path in production environment.
        /// </summary>
        private static string? ResolveProductionPath(string executableName, string baseDir)
        {
            var parentDir = Directory.GetParent(baseDir);

            var productionPaths = new[]
            {
                // Same directory as server
                Path.Combine(baseDir, executableName),
                // Subdirectory under current directory
                Path.Combine(baseDir, Path.GetFileNameWithoutExtension(executableName), executableName),
                // Parent directory structure
                Path.Combine(parentDir?.FullName ?? baseDir, Path.GetFileNameWithoutExtension(executableName), executableName),
                // Grandparent directory structure
                Path.Combine(parentDir?.Parent?.FullName ?? baseDir, Path.GetFileNameWithoutExtension(executableName), executableName)
            };

            return productionPaths.FirstOrDefault(File.Exists);
        }

        /// <summary>
        /// Gets all paths that were searched during resolution (for error reporting).
        /// </summary>
        /// <param name="projectName">Project name (Worker or Monitor)</param>
        /// <param name="baseDirectory">Base directory used for search</param>
        /// <returns>List of all paths that were checked</returns>
        public static List<string> GetSearchedPaths(string projectName, string? baseDirectory = null)
        {
            var actualBaseDir = baseDirectory ?? GetExecutableRealPath();
            var isDev = IsDevEnvironment(actualBaseDir);
            var executableName = projectName == WorkerProjectName ? WorkerExecutableName : MonitorExecutableName;
            var searchedPaths = new List<string>();

            if (isDev)
            {
                var solutionDir = FindSolutionDirectory(actualBaseDir);
                if (solutionDir != null)
                {
                    var config = GetConfiguration(actualBaseDir);
                    searchedPaths.AddRange(new[]
                    {
                        Path.Combine(solutionDir, projectName, "bin", config, "net9.0-windows", "win-x64", executableName),
                        Path.Combine(solutionDir, projectName, "bin", config, "net9.0-windows", executableName),
                        Path.Combine(solutionDir, projectName)
                    });
                }
            }
            else
            {
                var parentDir = Directory.GetParent(actualBaseDir);
                searchedPaths.AddRange(new[]
                {
                    Path.Combine(actualBaseDir, executableName),
                    Path.Combine(actualBaseDir, Path.GetFileNameWithoutExtension(executableName), executableName),
                    Path.Combine(parentDir?.FullName ?? actualBaseDir, Path.GetFileNameWithoutExtension(executableName), executableName),
                    Path.Combine(parentDir?.Parent?.FullName ?? actualBaseDir, Path.GetFileNameWithoutExtension(executableName), executableName)
                });
            }

            return searchedPaths;
        }

        /// <summary>
        /// Generic method to get all paths that were searched during resolution (for error reporting).
        /// </summary>
        /// <param name="projectName">Project name (e.g., "UIAutomationMCP.Subprocess.Worker")</param>
        /// <param name="executableName">Executable name (e.g., "UIAutomationMCP.Subprocess.Worker.exe")</param>
        /// <param name="baseDirectory">Base directory used for search</param>
        /// <returns>List of all paths that were checked</returns>
        public static List<string> GetGenericSearchedPaths(string projectName, string executableName, string? baseDirectory = null)
        {
            var actualBaseDir = baseDirectory ?? GetExecutableRealPath();
            var isDev = IsDevEnvironment(actualBaseDir);
            var searchedPaths = new List<string>();

            if (isDev)
            {
                var solutionDir = FindSolutionDirectory(actualBaseDir);
                if (solutionDir != null)
                {
                    var config = GetConfiguration(actualBaseDir);
                    searchedPaths.AddRange(new[]
                    {
                        Path.Combine(solutionDir, projectName, "bin", config, "net9.0-windows", "win-x64", executableName),
                        Path.Combine(solutionDir, projectName, "bin", config, "net9.0-windows", executableName),
                        Path.Combine(solutionDir, projectName)
                    });
                }
            }
            else
            {
                var parentDir = Directory.GetParent(actualBaseDir);
                searchedPaths.AddRange(new[]
                {
                    Path.Combine(actualBaseDir, executableName),
                    Path.Combine(actualBaseDir, Path.GetFileNameWithoutExtension(executableName), executableName),
                    Path.Combine(parentDir?.FullName ?? actualBaseDir, Path.GetFileNameWithoutExtension(executableName), executableName),
                    Path.Combine(parentDir?.Parent?.FullName ?? actualBaseDir, Path.GetFileNameWithoutExtension(executableName), executableName)
                });
            }

            return searchedPaths;
        }
    }
}
