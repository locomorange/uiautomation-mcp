namespace UIAutomationMCP.Models.Serialization
{
    /// <summary>
    /// MessagePack serialization helper for internal subprocess communication
    /// 
    /// DEPRECATED: This class is deprecated and no longer functional. Use JsonUtf8SerializationHelper instead for JSON-UTF-8 communication.
    /// MessagePack dependencies have been removed from the project.
    /// </summary>
    [Obsolete("MessagePackSerializationHelper is deprecated and no longer functional. Use JsonUtf8SerializationHelper for JSON-UTF-8 communication.", true)]
    public static class MessagePackSerializationHelper
    {
        /// <summary>
        /// Serialize an object to MessagePack binary format
        /// </summary>
        /// <typeparam name="T">The type to serialize</typeparam>
        /// <param name="obj">The object to serialize</param>
        /// <returns>MessagePack binary data</returns>
        /// <exception cref="NotSupportedException">Always thrown - MessagePack is no longer supported</exception>
        [Obsolete("Use JsonUtf8SerializationHelper.SerializeToUtf8Bytes instead", true)]
        public static byte[] Serialize<T>(T obj)
        {
            throw new NotSupportedException("MessagePack serialization is no longer supported. Use JsonUtf8SerializationHelper.SerializeToUtf8Bytes instead.");
        }

        /// <summary>
        /// Deserialize MessagePack binary data to an object
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="data">The MessagePack binary data</param>
        /// <returns>The deserialized object</returns>
        /// <exception cref="NotSupportedException">Always thrown - MessagePack is no longer supported</exception>
        [Obsolete("Use JsonUtf8SerializationHelper.DeserializeFromUtf8Bytes instead", true)]
        public static T? Deserialize<T>(byte[] data)
        {
            throw new NotSupportedException("MessagePack deserialization is no longer supported. Use JsonUtf8SerializationHelper.DeserializeFromUtf8Bytes instead.");
        }

        /// <summary>
        /// Deserialize MessagePack binary data to an object (ReadOnlyMemory version)
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="data">The MessagePack binary data</param>
        /// <returns>The deserialized object</returns>
        /// <exception cref="NotSupportedException">Always thrown - MessagePack is no longer supported</exception>
        [Obsolete("Use JsonUtf8SerializationHelper.DeserializeFromUtf8Bytes instead", true)]
        public static T? Deserialize<T>(ReadOnlyMemory<byte> data)
        {
            throw new NotSupportedException("MessagePack deserialization is no longer supported. Use JsonUtf8SerializationHelper.DeserializeFromUtf8Bytes instead.");
        }
    }
}
