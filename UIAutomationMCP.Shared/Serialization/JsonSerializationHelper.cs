using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Serialization
{
    /// <summary>
    /// Centralized JSON serialization helper for Native AOT compatibility
    /// </summary>
    public static class JsonSerializationHelper
    {
        private static readonly UIAutomationJsonContext _context = new(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        });

        // Worker Request/Response serialization
        public static string SerializeWorkerRequest(WorkerRequest request)
            => JsonSerializer.Serialize(request, _context.WorkerRequest);

        public static WorkerRequest? DeserializeWorkerRequest(string json)
            => JsonSerializer.Deserialize(json, _context.WorkerRequest);

        // Generic WorkerResponse serialization with type detection
        public static string SerializeWorkerResponse<T>(WorkerResponse<T> response)
        {
            // Get the specific type info based on T
            JsonTypeInfo<WorkerResponse<T>>? typeInfo = GetWorkerResponseTypeInfo<T>();
            
            if (typeInfo != null)
                return JsonSerializer.Serialize(response, typeInfo);
            
            // Fallback to object type
            var objResponse = new WorkerResponse<object>
            {
                Success = response.Success,
                Data = response.Data,
                Error = response.Error
            };
            return JsonSerializer.Serialize(objResponse, _context.WorkerResponseObject);
        }

        public static WorkerResponse<T>? DeserializeWorkerResponse<T>(string json)
        {
            JsonTypeInfo<WorkerResponse<T>>? typeInfo = GetWorkerResponseTypeInfo<T>();
            
            if (typeInfo != null)
                return JsonSerializer.Deserialize(json, typeInfo);
            
            // Fallback: deserialize as object and convert
            var objResponse = JsonSerializer.Deserialize(json, _context.WorkerResponseObject);
            if (objResponse == null) return null;
            
            return new WorkerResponse<T>
            {
                Success = objResponse.Success,
                Data = (T)objResponse.Data!,
                Error = objResponse.Error
            };
        }

        // Object serialization (for unknown types)
        public static string SerializeObject(object obj)
        {
            return obj switch
            {
                Dictionary<string, object> dict => JsonSerializer.Serialize(dict, _context.DictionaryStringObject),
                List<Dictionary<string, object>> list => JsonSerializer.Serialize(list, _context.ListDictionaryStringObject),
                WorkerRequest req => JsonSerializer.Serialize(req, _context.WorkerRequest),
                _ => JsonSerializer.Serialize(obj, _context.Object)
            };
        }

        public static T? DeserializeObject<T>(string json)
        {
            var type = typeof(T);
            
            if (type == typeof(Dictionary<string, object>))
                return (T?)(object?)JsonSerializer.Deserialize(json, _context.DictionaryStringObject);
            if (type == typeof(List<Dictionary<string, object>>))
                return (T?)(object?)JsonSerializer.Deserialize(json, _context.ListDictionaryStringObject);
            if (type == typeof(WorkerRequest))
                return (T?)(object?)JsonSerializer.Deserialize(json, _context.WorkerRequest);
            
            // For result types
            var typeInfo = GetResultTypeInfo<T>();
            if (typeInfo != null)
                return JsonSerializer.Deserialize(json, typeInfo);
            
            throw new NotSupportedException($"Type {type} is not supported for deserialization");
        }

        // Helper method to get type info for different WorkerResponse types
        private static JsonTypeInfo<WorkerResponse<T>>? GetWorkerResponseTypeInfo<T>()
        {
            var type = typeof(T);
            
            if (type == typeof(Dictionary<string, object>))
                return (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseDictionaryStringObject;
            if (type == typeof(List<Dictionary<string, object>>))
                return (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseListDictionaryStringObject;
            if (type == typeof(ScreenshotResult))
                return (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseScreenshotResult;
            if (type == typeof(BaseOperationResult))
                return (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseBaseOperationResult;
            if (type == typeof(ElementSearchResult))
                return (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseElementSearchResult;
            if (type == typeof(ActionResult))
                return (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseActionResult;
            if (type == typeof(ElementValueResult))
                return (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseElementValueResult;
            if (type == typeof(LegacyPropertiesResult))
                return (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseLegacyPropertiesResult;
            if (type == typeof(LegacyStateResult))
                return (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseLegacyStateResult;
            if (type == typeof(AnnotationInfoResult))
                return (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseAnnotationInfoResult;
            if (type == typeof(AnnotationTargetResult))
                return (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseAnnotationTargetResult;
            if (type == typeof(FindItemResult))
                return (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseFindItemResult;
            if (type == typeof(object))
                return (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseObject;
            
            return null;
        }
        
        // Helper method to get type info for result types
        private static JsonTypeInfo<T>? GetResultTypeInfo<T>()
        {
            var type = typeof(T);
            
            if (type == typeof(ScreenshotResult))
                return (JsonTypeInfo<T>)(object)_context.ScreenshotResult;
            if (type == typeof(BaseOperationResult))
                return (JsonTypeInfo<T>)(object)_context.BaseOperationResult;
            if (type == typeof(ElementSearchResult))
                return (JsonTypeInfo<T>)(object)_context.ElementSearchResult;
            if (type == typeof(ActionResult))
                return (JsonTypeInfo<T>)(object)_context.ActionResult;
            if (type == typeof(ElementValueResult))
                return (JsonTypeInfo<T>)(object)_context.ElementValueResult;
            if (type == typeof(LegacyPropertiesResult))
                return (JsonTypeInfo<T>)(object)_context.LegacyPropertiesResult;
            if (type == typeof(LegacyStateResult))
                return (JsonTypeInfo<T>)(object)_context.LegacyStateResult;
            if (type == typeof(AnnotationInfoResult))
                return (JsonTypeInfo<T>)(object)_context.AnnotationInfoResult;
            if (type == typeof(AnnotationTargetResult))
                return (JsonTypeInfo<T>)(object)_context.AnnotationTargetResult;
            if (type == typeof(FindItemResult))
                return (JsonTypeInfo<T>)(object)_context.FindItemResult;
            
            return null;
        }

    }

    // Source generation context with all types
    [JsonSerializable(typeof(WorkerRequest))]
    [JsonSerializable(typeof(WorkerResponse<object>))]
    [JsonSerializable(typeof(WorkerResponse<Dictionary<string, object>>))]
    [JsonSerializable(typeof(WorkerResponse<List<Dictionary<string, object>>>))]
    [JsonSerializable(typeof(WorkerResponse<ScreenshotResult>))]
    [JsonSerializable(typeof(WorkerResponse<BaseOperationResult>))]
    [JsonSerializable(typeof(WorkerResponse<ElementSearchResult>))]
    [JsonSerializable(typeof(WorkerResponse<ElementPropertiesResult>))]
    [JsonSerializable(typeof(WorkerResponse<PatternsInfoResult>))]
    [JsonSerializable(typeof(WorkerResponse<ActionResult>))]
    [JsonSerializable(typeof(WorkerResponse<ElementValueResult>))]
    [JsonSerializable(typeof(WorkerResponse<GridInfoResult>))]
    [JsonSerializable(typeof(WorkerResponse<TableInfoResult>))]
    [JsonSerializable(typeof(WorkerResponse<ScrollInfoResult>))]
    [JsonSerializable(typeof(WorkerResponse<SelectionInfoResult>))]
    [JsonSerializable(typeof(WorkerResponse<TextInfoResult>))]
    [JsonSerializable(typeof(WorkerResponse<TransformCapabilitiesResult>))]
    [JsonSerializable(typeof(WorkerResponse<WindowInfoResult>))]
    [JsonSerializable(typeof(WorkerResponse<LegacyPropertiesResult>))]
    [JsonSerializable(typeof(WorkerResponse<LegacyStateResult>))]
    [JsonSerializable(typeof(WorkerResponse<AnnotationInfoResult>))]
    [JsonSerializable(typeof(WorkerResponse<AnnotationTargetResult>))]
    [JsonSerializable(typeof(WorkerResponse<FindItemResult>))]
    [JsonSerializable(typeof(Dictionary<string, object>))]
    [JsonSerializable(typeof(List<Dictionary<string, object>>))]
    // Result types
    [JsonSerializable(typeof(ScreenshotResult))]
    [JsonSerializable(typeof(BaseOperationResult))]
    [JsonSerializable(typeof(ElementSearchResult))]
    [JsonSerializable(typeof(ElementPropertiesResult))]
    [JsonSerializable(typeof(PatternsInfoResult))]
    [JsonSerializable(typeof(ActionResult))]
    [JsonSerializable(typeof(ElementValueResult))]
    [JsonSerializable(typeof(GridInfoResult))]
    [JsonSerializable(typeof(TableInfoResult))]
    [JsonSerializable(typeof(ScrollInfoResult))]
    [JsonSerializable(typeof(SelectionInfoResult))]
    [JsonSerializable(typeof(TextInfoResult))]
    [JsonSerializable(typeof(TransformCapabilitiesResult))]
    [JsonSerializable(typeof(WindowInfoResult))]
    [JsonSerializable(typeof(DesktopWindowsResult))]
    [JsonSerializable(typeof(BooleanResult))]
    [JsonSerializable(typeof(LegacyPropertiesResult))]
    [JsonSerializable(typeof(LegacyStateResult))]
    [JsonSerializable(typeof(AnnotationInfoResult))]
    [JsonSerializable(typeof(AnnotationTargetResult))]
    [JsonSerializable(typeof(FindItemResult))]
    // Basic types
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(double))]
    [JsonSerializable(typeof(object))]
    [JsonSerializable(typeof(List<string>))]
    [JsonSerializable(typeof(string[]))]
    [JsonSourceGenerationOptions(
        WriteIndented = false, 
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    internal partial class UIAutomationJsonContext : JsonSerializerContext
    {
    }
}