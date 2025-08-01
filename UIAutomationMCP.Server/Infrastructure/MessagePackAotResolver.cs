using MessagePack;
using MessagePack.Resolvers;
using System.Runtime.CompilerServices;

namespace UIAutomationMCP.Server.Infrastructure
{
    /// <summary>
    /// MessagePack AOT resolver configuration
    /// This ensures MessagePack serialization works correctly with Native AOT
    /// </summary>
    public static class MessagePackAotResolver
    {
        private static bool _initialized = false;
        private static readonly object _lock = new object();

        /// <summary>
        /// Initialize MessagePack resolver for Native AOT compatibility
        /// This method is called automatically by ModuleInitializer
        /// </summary>
        [ModuleInitializer]
        public static void Initialize()
        {
            if (_initialized) return;
            
            lock (_lock)
            {
                if (_initialized) return;

                try
                {
                    // Create composite resolver with AOT-compatible resolvers
                    // Note: GeneratedResolver will be created by MessagePack source generator
                    var resolver = CompositeResolver.Create(
                        // Built-in resolvers that work with AOT
                        BuiltinResolver.Instance,
                        AttributeFormatterResolver.Instance,
                        // Standard resolver (AOT-compatible subset)
                        StandardResolver.Instance
                    );

                    // Configure MessagePack with AOT-compatible options
                    var options = MessagePackSerializerOptions.Standard
                        .WithResolver(resolver)
                        .WithSecurity(MessagePackSecurity.UntrustedData);

                    MessagePackSerializer.DefaultOptions = options;
                    _initialized = true;
                }
                catch (Exception)
                {
                    // Fallback to default configuration if initialization fails
                    // This ensures the application can still run even if AOT resolver setup fails
                    _initialized = true;
                }
            }
        }

        /// <summary>
        /// Get configured MessagePack options for explicit use
        /// </summary>
        public static MessagePackSerializerOptions GetOptions()
        {
            Initialize();
            return MessagePackSerializer.DefaultOptions;
        }
    }
}