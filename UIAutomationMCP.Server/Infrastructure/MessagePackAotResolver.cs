using System.Runtime.CompilerServices;

namespace UIAutomationMCP.Server.Infrastructure
{
    /// <summary>
    /// MessagePack AOT resolver configuration
    /// 
    /// DEPRECATED: This class is deprecated and no longer functional. MessagePack support has been removed.
    /// JSON-UTF-8 serialization is now used instead of MessagePack.
    /// </summary>
    [Obsolete("MessagePackAotResolver is deprecated and no longer functional. MessagePack support has been removed.", true)]
    public static class MessagePackAotResolver
    {
        private static bool _initialized = false;
        private static readonly object _lock = new object();

        /// <summary>
        /// Initialize MessagePack resolver for Native AOT compatibility
        /// This method is deprecated and no longer does anything
        /// </summary>
        [ModuleInitializer]
        [Obsolete("MessagePack initialization is no longer needed", true)]
        public static void Initialize()
        {
            if (_initialized) return;

            lock (_lock)
            {
                if (_initialized) return;
                // No-op: MessagePack support removed
                _initialized = true;
            }
        }

        /// <summary>
        /// Get configured MessagePack options for explicit use
        /// </summary>
        [Obsolete("MessagePack is no longer supported", true)]
        public static object GetOptions()
        {
            throw new NotSupportedException("MessagePack is no longer supported. Use JSON-UTF-8 serialization instead.");
        }
    }
}
