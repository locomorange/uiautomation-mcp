using System.Text.Json.Serialization;

namespace UIAutomationMCP.Models
{
    /// <summary>
    /// Base class for all typed event data
    /// </summary>
    [JsonDerivedType(typeof(FocusEventData), "Focus")]
    [JsonDerivedType(typeof(InvokeEventData), "Invoke")]
    [JsonDerivedType(typeof(SelectionEventData), "Selection")]
    [JsonDerivedType(typeof(TextChangedEventData), "Text")]
    [JsonDerivedType(typeof(PropertyChangedEventData), "Property")]
    [JsonDerivedType(typeof(StructureChangedEventData), "Structure")]
    [JsonDerivedType(typeof(GenericEventData), "Generic")]
    public abstract class TypedEventData
    {
        public string EventType { get; protected set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string SourceElement { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Focus change event data
    /// </summary>
    public class FocusEventData : TypedEventData
    {
        public FocusEventData()
        {
            EventType = "Focus";
        }
    }

    /// <summary>
    /// Invoke pattern event data
    /// </summary>
    public class InvokeEventData : TypedEventData
    {
        public string EventId { get; set; } = string.Empty;

        public InvokeEventData()
        {
            EventType = "Invoke";
        }
    }

    /// <summary>
    /// Selection pattern event data
    /// </summary>
    public class SelectionEventData : TypedEventData
    {
        public string EventId { get; set; } = string.Empty;

        public SelectionEventData()
        {
            EventType = "Selection";
        }
    }

    /// <summary>
    /// Text change event data
    /// </summary>
    public class TextChangedEventData : TypedEventData
    {
        public string EventId { get; set; } = string.Empty;

        public TextChangedEventData()
        {
            EventType = "Text";
        }
    }

    /// <summary>
    /// Property change event data
    /// </summary>
    public class PropertyChangedEventData : TypedEventData
    {
        public string PropertyId { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;

        public PropertyChangedEventData()
        {
            EventType = "Property";
        }
    }

    /// <summary>
    /// Structure change event data
    /// </summary>
    public class StructureChangedEventData : TypedEventData
    {
        public string StructureChangeType { get; set; } = string.Empty;
        public int[] RuntimeId { get; set; } = Array.Empty<int>();

        public StructureChangedEventData()
        {
            EventType = "Structure";
        }
    }

    /// <summary>
    /// Generic automation event data for other event types
    /// </summary>
    public class GenericEventData : TypedEventData
    {
        public string EventId { get; set; } = string.Empty;

        public GenericEventData()
        {
            EventType = "Generic";
        }

        public GenericEventData(string eventType)
        {
            EventType = eventType;
        }
    }
}
