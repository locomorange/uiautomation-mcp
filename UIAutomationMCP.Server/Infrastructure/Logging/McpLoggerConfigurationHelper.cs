using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Server.Infrastructure.Logging
{
    /// <summary>
    /// Helper class for configuring MCP logging options from environment variables
    /// </summary>
    public static class McpLoggerConfigurationHelper
    {
        /// <summary>
        /// Create McpLoggerOptions from environment variables
        /// </summary>
        public static McpLoggerOptions CreateFromEnvironment()
        {
            var options = new McpLoggerOptions
            {
                // Production-safe defaults
                EnableNotifications = true,  // MCP notifications are always enabled
                EnableFileOutput = false,    // File logging is OFF by default (production-safe)
                FileOutputPath = "mcp-logs.json",
                FileOutputFormat = "json",
                FileMinimumLevel = LogLevel.Information,
                NotificationMinimumLevel = LogLevel.Information,
                MaxFileSizeMB = 10,
                BackupFileCount = 5
            };

            // MCP_LOG_ENABLE_NOTIFICATIONS (default: true)
            if (bool.TryParse(Environment.GetEnvironmentVariable("MCP_LOG_ENABLE_NOTIFICATIONS"), out var enableNotifications))
            {
                options.EnableNotifications = enableNotifications;
            }

            // MCP_LOG_ENABLE_FILE (default: false - must explicitly enable)
            if (bool.TryParse(Environment.GetEnvironmentVariable("MCP_LOG_ENABLE_FILE"), out var enableFile))
            {
                options.EnableFileOutput = enableFile;
            }

            // MCP_LOG_FILE_PATH (default: "mcp-logs.json")
            var filePath = Environment.GetEnvironmentVariable("MCP_LOG_FILE_PATH");
            if (!string.IsNullOrEmpty(filePath))
            {
                options.FileOutputPath = filePath;
            }

            // MCP_LOG_FILE_FORMAT (default: "json")
            var fileFormat = Environment.GetEnvironmentVariable("MCP_LOG_FILE_FORMAT");
            if (!string.IsNullOrEmpty(fileFormat))
            {
                options.FileOutputFormat = fileFormat;
            }

            // MCP_LOG_FILE_MIN_LEVEL (default: "Information")
            var fileMinLevel = Environment.GetEnvironmentVariable("MCP_LOG_FILE_MIN_LEVEL");
            if (!string.IsNullOrEmpty(fileMinLevel) && Enum.TryParse<LogLevel>(fileMinLevel, true, out var parsedFileLevel))
            {
                options.FileMinimumLevel = parsedFileLevel;
            }

            // MCP_LOG_NOTIFICATION_MIN_LEVEL (default: "Information")
            var notificationMinLevel = Environment.GetEnvironmentVariable("MCP_LOG_NOTIFICATION_MIN_LEVEL");
            if (!string.IsNullOrEmpty(notificationMinLevel) && Enum.TryParse<LogLevel>(notificationMinLevel, true, out var parsedNotificationLevel))
            {
                options.NotificationMinimumLevel = parsedNotificationLevel;
            }

            // MCP_LOG_FILE_MAX_SIZE_MB (default: 10)
            if (int.TryParse(Environment.GetEnvironmentVariable("MCP_LOG_FILE_MAX_SIZE_MB"), out var maxSize) && maxSize > 0)
            {
                options.MaxFileSizeMB = maxSize;
            }

            // MCP_LOG_FILE_BACKUP_COUNT (default: 5)
            if (int.TryParse(Environment.GetEnvironmentVariable("MCP_LOG_FILE_BACKUP_COUNT"), out var backupCount) && backupCount > 0)
            {
                options.BackupFileCount = backupCount;
            }

            return options;
        }

        /// <summary>
        /// Create McpLoggerOptions with common presets
        /// </summary>
        public static class Presets
        {
            /// <summary>
            /// Notifications only (current behavior)
            /// </summary>
            public static McpLoggerOptions NotificationsOnly => new()
            {
                EnableNotifications = true,
                EnableFileOutput = false
            };

            /// <summary>
            /// File output only
            /// </summary>
            public static McpLoggerOptions FileOnly => new()
            {
                EnableNotifications = false,
                EnableFileOutput = true,
                FileOutputPath = "mcp-logs.json",
                FileOutputFormat = "json"
            };

            /// <summary>
            /// Both notifications and file output
            /// </summary>
            public static McpLoggerOptions Both => new()
            {
                EnableNotifications = true,
                EnableFileOutput = true,
                FileOutputPath = "mcp-logs.json",
                FileOutputFormat = "json"
            };

            /// <summary>
            /// Debug mode with text file output
            /// </summary>
            public static McpLoggerOptions DebugMode => new()
            {
                EnableNotifications = true,
                EnableFileOutput = true,
                FileOutputPath = "mcp-debug.log",
                FileOutputFormat = "text",
                FileMinimumLevel = LogLevel.Debug,
                NotificationMinimumLevel = LogLevel.Information
            };

            /// <summary>
            /// Production mode with JSON file output
            /// </summary>
            public static McpLoggerOptions ProductionMode => new()
            {
                EnableNotifications = true,
                EnableFileOutput = true,
                FileOutputPath = "mcp-production.json",
                FileOutputFormat = "json",
                FileMinimumLevel = LogLevel.Information,
                NotificationMinimumLevel = LogLevel.Information,
                MaxFileSizeMB = 50,
                BackupFileCount = 10
            };
        }

        /// <summary>
        /// Print current configuration to console
        /// </summary>
        public static void PrintConfiguration(McpLoggerOptions options)
        {
            Console.WriteLine("=== MCP Logger Configuration ===");
            Console.WriteLine($"Notifications Enabled: {options.EnableNotifications}");
            Console.WriteLine($"File Output Enabled: {options.EnableFileOutput}");

            if (options.EnableFileOutput)
            {
                Console.WriteLine($"File Path: {options.FileOutputPath}");
                Console.WriteLine($"File Format: {options.FileOutputFormat}");
                Console.WriteLine($"File Min Level: {options.FileMinimumLevel}");
                Console.WriteLine($"Max File Size: {options.MaxFileSizeMB}MB");
                Console.WriteLine($"Backup File Count: {options.BackupFileCount}");
            }

            if (options.EnableNotifications)
            {
                Console.WriteLine($"Notification Min Level: {options.NotificationMinimumLevel}");
            }

            Console.WriteLine("================================");
        }

        /// <summary>
        /// Print environment variable examples
        /// </summary>
        public static void PrintEnvironmentVariableExamples()
        {
            Console.WriteLine("=== MCP Logger Environment Variables ===");
            Console.WriteLine("# Enable/disable features");
            Console.WriteLine("MCP_LOG_ENABLE_NOTIFICATIONS=true");
            Console.WriteLine("MCP_LOG_ENABLE_FILE=true");
            Console.WriteLine();
            Console.WriteLine("# File output configuration");
            Console.WriteLine("MCP_LOG_FILE_PATH=mcp-logs.json");
            Console.WriteLine("MCP_LOG_FILE_FORMAT=json        # or 'text'");
            Console.WriteLine("MCP_LOG_FILE_MIN_LEVEL=Information");
            Console.WriteLine("MCP_LOG_NOTIFICATION_MIN_LEVEL=Information");
            Console.WriteLine();
            Console.WriteLine("# File rotation");
            Console.WriteLine("MCP_LOG_FILE_MAX_SIZE_MB=10");
            Console.WriteLine("MCP_LOG_FILE_MAX_COUNT=5");
            Console.WriteLine("=========================================");
        }
    }
}
