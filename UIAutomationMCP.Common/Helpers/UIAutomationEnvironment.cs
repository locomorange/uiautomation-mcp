using System;
using System.ComponentModel;
using System.Windows.Automation;

namespace UIAutomationMCP.Common.Helpers
{
    /// <summary>
    /// Helper class for UI Automation environment checks and error handling
    /// Note: This assumes UI Automation availability was pre-checked at Worker startup
    /// </summary>
    public static class UIAutomationEnvironment
    {
        /// <summary>
        /// Check if UI Automation is available and properly initialized
        /// This is a fast check that assumes the Worker startup validation passed
        /// </summary>
        public static bool IsAvailable => CheckAvailabilityFast();

        /// <summary>
        /// Get a descriptive error message when UI Automation is not available
        /// </summary>
        public static string UnavailabilityReason { get; private set; } = "";

        private static bool CheckAvailabilityFast()
        {
            try
            {
                // Quick check - if Worker started successfully, UI Automation should be available
                // Just verify we can access basic automation without deep operations
                var rootElement = AutomationElement.RootElement;
                return rootElement != null;
            }
            catch (Exception ex)
            {
                UnavailabilityReason = $"UI Automation fast check failed: {ex.Message}";
                return false;
            }
        }

    }
}
