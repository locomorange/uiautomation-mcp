using MessagePack;
using MessagePack.Resolvers;

namespace UIAutomationMCP.Models.Serialization
{
    /// <summary>
    /// MessagePack serialization helper for internal subprocess communication
    /// Provides binary serialization to solve Japanese character encoding issues
    /// </summary>
    public static class MessagePackSerializationHelper
    {
        private static readonly MessagePackSerializerOptions _options;
        
        static MessagePackSerializationHelper()
        {
            // Create a composite resolver that includes both standard types and contract-less resolver
            var resolver = CompositeResolver.Create(
                // Contract-less resolver for classes with MessagePackObject attribute
                ContractlessStandardResolver.Instance,
                // Built-in formatters for standard types
                StandardResolver.Instance
            );
            
            _options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
        }

        /// <summary>
        /// Serialize an object to MessagePack binary format
        /// </summary>
        /// <typeparam name="T">The type to serialize</typeparam>
        /// <param name="obj">The object to serialize</param>
        /// <returns>MessagePack binary data</returns>
        /// <exception cref="MessagePackSerializationException">Thrown when serialization fails</exception>
        public static byte[] Serialize<T>(T obj)
        {
            try
            {
                return MessagePackSerializer.Serialize(obj, _options);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to serialize object of type {typeof(T).Name} to MessagePack: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deserialize MessagePack binary data to an object
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="data">The MessagePack binary data</param>
        /// <returns>The deserialized object</returns>
        /// <exception cref="MessagePackSerializationException">Thrown when deserialization fails</exception>
        public static T? Deserialize<T>(byte[] data)
        {
            try
            {
                if (data == null || data.Length == 0)
                    throw new ArgumentException("MessagePack data cannot be null or empty");

                return MessagePackSerializer.Deserialize<T>(data, _options);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to deserialize MessagePack data to type {typeof(T).Name}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deserialize MessagePack binary data to an object (ReadOnlyMemory version)
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="data">The MessagePack binary data</param>
        /// <returns>The deserialized object</returns>
        /// <exception cref="MessagePackSerializationException">Thrown when deserialization fails</exception>
        public static T? Deserialize<T>(ReadOnlyMemory<byte> data)
        {
            try
            {
                if (data.IsEmpty)
                    throw new ArgumentException("MessagePack data cannot be empty");

                return MessagePackSerializer.Deserialize<T>(data, _options);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to deserialize MessagePack data to type {typeof(T).Name}: {ex.Message}", ex);
            }
        }
    }
}