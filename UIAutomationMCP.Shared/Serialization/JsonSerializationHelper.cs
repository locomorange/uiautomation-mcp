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
            WriteIndented = false,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
        });

        // Worker Request/Response serialization
        public static string SerializeWorkerRequest(WorkerRequest request)
        {
            try
            {
                return JsonSerializer.Serialize(request, _context.WorkerRequest);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to serialize WorkerRequest: {ex.Message}", ex);
            }
        }

        public static WorkerRequest? DeserializeWorkerRequest(string json)
        {
            try
            {
                if (string.IsNullOrEmpty(json))
                    throw new ArgumentException("JSON string cannot be null or empty");
                
                return JsonSerializer.Deserialize(json, _context.WorkerRequest);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to deserialize WorkerRequest from JSON: {json}. Error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error deserializing WorkerRequest: {ex.Message}", ex);
            }
        }

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
            try
            {
                if (string.IsNullOrEmpty(json))
                    throw new ArgumentException("JSON string cannot be null or empty");

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
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to deserialize WorkerResponse<{typeof(T).Name}> from JSON: {json}. Error: {ex.Message}", ex);
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidOperationException($"Failed to cast response data to type {typeof(T).Name}. JSON: {json}. Error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error deserializing WorkerResponse<{typeof(T).Name}>: {ex.Message}", ex);
            }
        }

        // Object serialization (for unknown types)
        public static string SerializeObject(object obj)
        {
            try
            {
                return obj switch
                {
                    Dictionary<string, object> dict => JsonSerializer.Serialize(dict, _context.DictionaryStringObject),
                    List<Dictionary<string, object>> list => JsonSerializer.Serialize(list, _context.ListDictionaryStringObject),
                    WorkerRequest req => JsonSerializer.Serialize(req, _context.WorkerRequest),
                    _ => JsonSerializer.Serialize(obj, _context.Object)
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to serialize object of type {obj?.GetType().Name ?? "null"}: {ex.Message}", ex);
            }
        }

        public static T? DeserializeObject<T>(string json)
        {
            try
            {
                if (string.IsNullOrEmpty(json))
                    throw new ArgumentException("JSON string cannot be null or empty");

                return typeof(T) switch
                {
                    // Common types
                    Type t when t == typeof(Dictionary<string, object>) => (T?)(object?)JsonSerializer.Deserialize(json, _context.DictionaryStringObject),
                    Type t when t == typeof(List<Dictionary<string, object>>) => (T?)(object?)JsonSerializer.Deserialize(json, _context.ListDictionaryStringObject),
                    Type t when t == typeof(WorkerRequest) => (T?)(object?)JsonSerializer.Deserialize(json, _context.WorkerRequest),
                    
                    // Try result types first
                    _ when GetResultTypeInfo<T>() is JsonTypeInfo<T> resultTypeInfo => JsonSerializer.Deserialize(json, resultTypeInfo),
                    
                    // Try request types
                    _ when GetRequestTypeInfo<T>() is JsonTypeInfo<T> requestTypeInfo => JsonSerializer.Deserialize(json, requestTypeInfo),
                    
                    // Unsupported type
                    _ => throw new NotSupportedException($"Type {typeof(T)} is not supported for deserialization")
                };
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to deserialize JSON to type {typeof(T).Name}. JSON: {json}. Error: {ex.Message}", ex);
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidOperationException($"Failed to cast deserialized object to type {typeof(T).Name}. JSON: {json}. Error: {ex.Message}", ex);
            }
            catch (NotSupportedException)
            {
                throw; // Re-throw NotSupportedException as-is
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error deserializing to type {typeof(T).Name}: {ex.Message}", ex);
            }
        }

        // Helper method to get type info for different WorkerResponse types
        private static JsonTypeInfo<WorkerResponse<T>>? GetWorkerResponseTypeInfo<T>()
        {
            return typeof(T) switch
            {
                Type t when t == typeof(Dictionary<string, object>) => (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseDictionaryStringObject,
                Type t when t == typeof(List<Dictionary<string, object>>) => (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseListDictionaryStringObject,
                Type t when t == typeof(ScreenshotResult) => (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseScreenshotResult,
                Type t when t == typeof(BaseOperationResult) => (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseBaseOperationResult,
                Type t when t == typeof(ElementSearchResult) => (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseElementSearchResult,
                Type t when t == typeof(ActionResult) => (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseActionResult,
                Type t when t == typeof(ElementValueResult) => (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseElementValueResult,
                Type t when t == typeof(WindowInteractionStateResult) => (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseWindowInteractionStateResult,
                Type t when t == typeof(WindowCapabilitiesResult) => (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseWindowCapabilitiesResult,
                Type t when t == typeof(FindItemResult) => (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseFindItemResult,
                Type t when t == typeof(ProcessResult) => (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseProcessResult,
                Type t when t == typeof(ErrorResult) => (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseErrorResult,
                Type t when t == typeof(UniversalResponse) => (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseUniversalResponse,
                Type t when t == typeof(object) => (JsonTypeInfo<WorkerResponse<T>>)(object)_context.WorkerResponseObject,
                _ => null
            };
        }
        
        // Helper method to get type info for result types
        private static JsonTypeInfo<T>? GetResultTypeInfo<T>()
        {
            return typeof(T) switch
            {
                Type t when t == typeof(ScreenshotResult) => (JsonTypeInfo<T>)(object)_context.ScreenshotResult,
                Type t when t == typeof(BaseOperationResult) => (JsonTypeInfo<T>)(object)_context.BaseOperationResult,
                Type t when t == typeof(ElementSearchResult) => (JsonTypeInfo<T>)(object)_context.ElementSearchResult,
                Type t when t == typeof(ActionResult) => (JsonTypeInfo<T>)(object)_context.ActionResult,
                Type t when t == typeof(ElementValueResult) => (JsonTypeInfo<T>)(object)_context.ElementValueResult,
                Type t when t == typeof(WindowInteractionStateResult) => (JsonTypeInfo<T>)(object)_context.WindowInteractionStateResult,
                Type t when t == typeof(WindowCapabilitiesResult) => (JsonTypeInfo<T>)(object)_context.WindowCapabilitiesResult,
                Type t when t == typeof(FindItemResult) => (JsonTypeInfo<T>)(object)_context.FindItemResult,
                Type t when t == typeof(ProcessResult) => (JsonTypeInfo<T>)(object)_context.ProcessResult,
                Type t when t == typeof(ErrorResult) => (JsonTypeInfo<T>)(object)_context.ErrorResult,
                Type t when t == typeof(UniversalResponse) => (JsonTypeInfo<T>)(object)_context.UniversalResponse,
                Type t when t == typeof(ElementTreeResult) => (JsonTypeInfo<T>)(object)_context.ElementTreeResult,
                Type t when t == typeof(TreeNavigationResult) => (JsonTypeInfo<T>)(object)_context.TreeNavigationResult,
                Type t when t == typeof(DesktopWindowsResult) => (JsonTypeInfo<T>)(object)_context.DesktopWindowsResult,
                Type t when t == typeof(BooleanResult) => (JsonTypeInfo<T>)(object)_context.BooleanResult,
                _ => null
            };
        }

        // Helper method to get type info for typed request classes
        private static JsonTypeInfo<T>? GetRequestTypeInfo<T>()
        {
            return typeof(T) switch
            {
                // Basic operations
                Type t when t == typeof(InvokeElementRequest) => (JsonTypeInfo<T>)(object)_context.InvokeElementRequest,
                Type t when t == typeof(ToggleElementRequest) => (JsonTypeInfo<T>)(object)_context.ToggleElementRequest,
                Type t when t == typeof(GetToggleStateRequest) => (JsonTypeInfo<T>)(object)_context.GetToggleStateRequest,
                Type t when t == typeof(SetToggleStateRequest) => (JsonTypeInfo<T>)(object)_context.SetToggleStateRequest,
                
                // Value operations
                Type t when t == typeof(SetElementValueRequest) => (JsonTypeInfo<T>)(object)_context.SetElementValueRequest,
                Type t when t == typeof(GetElementValueRequest) => (JsonTypeInfo<T>)(object)_context.GetElementValueRequest,
                Type t when t == typeof(IsReadOnlyRequest) => (JsonTypeInfo<T>)(object)_context.IsReadOnlyRequest,
                
                // Element search
                Type t when t == typeof(FindElementsRequest) => (JsonTypeInfo<T>)(object)_context.FindElementsRequest,
                Type t when t == typeof(FindElementsByControlTypeRequest) => (JsonTypeInfo<T>)(object)_context.FindElementsByControlTypeRequest,
                Type t when t == typeof(FindElementsByPatternRequest) => (JsonTypeInfo<T>)(object)_context.FindElementsByPatternRequest,
                
                // Window operations
                Type t when t == typeof(GetDesktopWindowsRequest) => (JsonTypeInfo<T>)(object)_context.GetDesktopWindowsRequest,
                Type t when t == typeof(WindowActionRequest) => (JsonTypeInfo<T>)(object)_context.WindowActionRequest,
                Type t when t == typeof(GetWindowInfoRequest) => (JsonTypeInfo<T>)(object)_context.GetWindowInfoRequest,
                Type t when t == typeof(GetWindowInteractionStateRequest) => (JsonTypeInfo<T>)(object)_context.GetWindowInteractionStateRequest,
                Type t when t == typeof(GetWindowCapabilitiesRequest) => (JsonTypeInfo<T>)(object)_context.GetWindowCapabilitiesRequest,
                
                // Range operations
                Type t when t == typeof(SetRangeValueRequest) => (JsonTypeInfo<T>)(object)_context.SetRangeValueRequest,
                Type t when t == typeof(GetRangeValueRequest) => (JsonTypeInfo<T>)(object)_context.GetRangeValueRequest,
                Type t when t == typeof(GetRangePropertiesRequest) => (JsonTypeInfo<T>)(object)_context.GetRangePropertiesRequest,
                
                // Text operations
                Type t when t == typeof(SetTextRequest) => (JsonTypeInfo<T>)(object)_context.SetTextRequest,
                Type t when t == typeof(GetTextRequest) => (JsonTypeInfo<T>)(object)_context.GetTextRequest,
                Type t when t == typeof(FindTextRequest) => (JsonTypeInfo<T>)(object)_context.FindTextRequest,
                Type t when t == typeof(SelectTextRequest) => (JsonTypeInfo<T>)(object)_context.SelectTextRequest,
                Type t when t == typeof(TraverseTextRequest) => (JsonTypeInfo<T>)(object)_context.TraverseTextRequest,
                
                // Transform operations
                Type t when t == typeof(TransformElementRequest) => (JsonTypeInfo<T>)(object)_context.TransformElementRequest,
                Type t when t == typeof(MoveElementRequest) => (JsonTypeInfo<T>)(object)_context.MoveElementRequest,
                Type t when t == typeof(ResizeElementRequest) => (JsonTypeInfo<T>)(object)_context.ResizeElementRequest,
                Type t when t == typeof(RotateElementRequest) => (JsonTypeInfo<T>)(object)_context.RotateElementRequest,
                Type t when t == typeof(WaitForInputIdleRequest) => (JsonTypeInfo<T>)(object)_context.WaitForInputIdleRequest,
                
                // Grid operations
                Type t when t == typeof(GetGridInfoRequest) => (JsonTypeInfo<T>)(object)_context.GetGridInfoRequest,
                Type t when t == typeof(GetGridItemRequest) => (JsonTypeInfo<T>)(object)_context.GetGridItemRequest,
                Type t when t == typeof(GetColumnHeaderRequest) => (JsonTypeInfo<T>)(object)_context.GetColumnHeaderRequest,
                Type t when t == typeof(GetRowHeaderRequest) => (JsonTypeInfo<T>)(object)_context.GetRowHeaderRequest,
                
                // Layout operations
                Type t when t == typeof(DockElementRequest) => (JsonTypeInfo<T>)(object)_context.DockElementRequest,
                Type t when t == typeof(RealizeVirtualizedItemRequest) => (JsonTypeInfo<T>)(object)_context.RealizeVirtualizedItemRequest,
                Type t when t == typeof(FindItemByPropertyRequest) => (JsonTypeInfo<T>)(object)_context.FindItemByPropertyRequest,
                Type t when t == typeof(StartSynchronizedInputRequest) => (JsonTypeInfo<T>)(object)_context.StartSynchronizedInputRequest,
                Type t when t == typeof(CancelSynchronizedInputRequest) => (JsonTypeInfo<T>)(object)_context.CancelSynchronizedInputRequest,
                Type t when t == typeof(ExpandCollapseElementRequest) => (JsonTypeInfo<T>)(object)_context.ExpandCollapseElementRequest,
                Type t when t == typeof(GetScrollInfoRequest) => (JsonTypeInfo<T>)(object)_context.GetScrollInfoRequest,
                Type t when t == typeof(ScrollElementIntoViewRequest) => (JsonTypeInfo<T>)(object)_context.ScrollElementIntoViewRequest,
                Type t when t == typeof(ScrollElementRequest) => (JsonTypeInfo<T>)(object)_context.ScrollElementRequest,
                Type t when t == typeof(SetScrollPercentRequest) => (JsonTypeInfo<T>)(object)_context.SetScrollPercentRequest,
                
                // Multiple view operations
                Type t when t == typeof(GetAvailableViewsRequest) => (JsonTypeInfo<T>)(object)_context.GetAvailableViewsRequest,
                Type t when t == typeof(GetCurrentViewRequest) => (JsonTypeInfo<T>)(object)_context.GetCurrentViewRequest,
                Type t when t == typeof(GetViewNameRequest) => (JsonTypeInfo<T>)(object)_context.GetViewNameRequest,
                Type t when t == typeof(SetViewRequest) => (JsonTypeInfo<T>)(object)_context.SetViewRequest,
                
                // Table operations
                Type t when t == typeof(GetColumnHeaderItemsRequest) => (JsonTypeInfo<T>)(object)_context.GetColumnHeaderItemsRequest,
                Type t when t == typeof(GetColumnHeadersRequest) => (JsonTypeInfo<T>)(object)_context.GetColumnHeadersRequest,
                Type t when t == typeof(GetRowHeaderItemsRequest) => (JsonTypeInfo<T>)(object)_context.GetRowHeaderItemsRequest,
                Type t when t == typeof(GetRowHeadersRequest) => (JsonTypeInfo<T>)(object)_context.GetRowHeadersRequest,
                Type t when t == typeof(GetRowOrColumnMajorRequest) => (JsonTypeInfo<T>)(object)_context.GetRowOrColumnMajorRequest,
                Type t when t == typeof(GetTableInfoRequest) => (JsonTypeInfo<T>)(object)_context.GetTableInfoRequest,
                
                // Selection operations
                Type t when t == typeof(AddToSelectionRequest) => (JsonTypeInfo<T>)(object)_context.AddToSelectionRequest,
                Type t when t == typeof(CanSelectMultipleRequest) => (JsonTypeInfo<T>)(object)_context.CanSelectMultipleRequest,
                Type t when t == typeof(ClearSelectionRequest) => (JsonTypeInfo<T>)(object)_context.ClearSelectionRequest,
                Type t when t == typeof(GetSelectionContainerRequest) => (JsonTypeInfo<T>)(object)_context.GetSelectionContainerRequest,
                Type t when t == typeof(GetSelectionRequest) => (JsonTypeInfo<T>)(object)_context.GetSelectionRequest,
                Type t when t == typeof(IsSelectedRequest) => (JsonTypeInfo<T>)(object)_context.IsSelectedRequest,
                Type t when t == typeof(IsSelectionRequiredRequest) => (JsonTypeInfo<T>)(object)_context.IsSelectionRequiredRequest,
                Type t when t == typeof(RemoveFromSelectionRequest) => (JsonTypeInfo<T>)(object)_context.RemoveFromSelectionRequest,
                Type t when t == typeof(SelectElementRequest) => (JsonTypeInfo<T>)(object)_context.SelectElementRequest,
                Type t when t == typeof(SelectItemRequest) => (JsonTypeInfo<T>)(object)_context.SelectItemRequest,
                
                // Element inspection
                Type t when t == typeof(GetElementPropertiesRequest) => (JsonTypeInfo<T>)(object)_context.GetElementPropertiesRequest,
                Type t when t == typeof(GetElementPatternsRequest) => (JsonTypeInfo<T>)(object)_context.GetElementPatternsRequest,
                
                // Tree navigation
                Type t when t == typeof(GetAncestorsRequest) => (JsonTypeInfo<T>)(object)_context.GetAncestorsRequest,
                Type t when t == typeof(GetChildrenRequest) => (JsonTypeInfo<T>)(object)_context.GetChildrenRequest,
                Type t when t == typeof(GetDescendantsRequest) => (JsonTypeInfo<T>)(object)_context.GetDescendantsRequest,
                Type t when t == typeof(GetElementTreeRequest) => (JsonTypeInfo<T>)(object)_context.GetElementTreeRequest,
                Type t when t == typeof(GetParentRequest) => (JsonTypeInfo<T>)(object)_context.GetParentRequest,
                Type t when t == typeof(GetSiblingsRequest) => (JsonTypeInfo<T>)(object)_context.GetSiblingsRequest,
                
                _ => null
            };
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
    [JsonSerializable(typeof(WorkerResponse<WindowInteractionStateResult>))]
    [JsonSerializable(typeof(WorkerResponse<WindowCapabilitiesResult>))]
    [JsonSerializable(typeof(WorkerResponse<FindItemResult>))]
    [JsonSerializable(typeof(WorkerResponse<ProcessResult>))]
    [JsonSerializable(typeof(WorkerResponse<DesktopWindowsResult>))]
    [JsonSerializable(typeof(WorkerResponse<ElementTreeResult>))]
    [JsonSerializable(typeof(WorkerResponse<BooleanResult>))]
    [JsonSerializable(typeof(WorkerResponse<ErrorResult>))]
    [JsonSerializable(typeof(WorkerResponse<UniversalResponse>))]
    [JsonSerializable(typeof(Dictionary<string, object>))]
    [JsonSerializable(typeof(List<Dictionary<string, object>>))]
    // Result types
    [JsonSerializable(typeof(ScreenshotResult))]
    [JsonSerializable(typeof(ProcessResult))]
    [JsonSerializable(typeof(BaseOperationResult))]
    [JsonSerializable(typeof(ElementSearchResult))]
    [JsonSerializable(typeof(ElementPropertiesResult))]
    [JsonSerializable(typeof(PatternsInfoResult))]
    [JsonSerializable(typeof(ActionResult))]
    [JsonSerializable(typeof(ElementValueResult))]
    [JsonSerializable(typeof(ErrorResult))]
    [JsonSerializable(typeof(UniversalResponse))]
    [JsonSerializable(typeof(GridInfoResult))]
    [JsonSerializable(typeof(TableInfoResult))]
    [JsonSerializable(typeof(ScrollInfoResult))]
    [JsonSerializable(typeof(SelectionInfoResult))]
    [JsonSerializable(typeof(TextInfoResult))]
    [JsonSerializable(typeof(TransformCapabilitiesResult))]
    [JsonSerializable(typeof(WindowInfoResult))]
    [JsonSerializable(typeof(WindowInteractionStateResult))]
    [JsonSerializable(typeof(WindowCapabilitiesResult))]
    [JsonSerializable(typeof(DesktopWindowsResult))]
    [JsonSerializable(typeof(ElementTreeResult))]
    [JsonSerializable(typeof(BooleanResult))]
    [JsonSerializable(typeof(FindItemResult))]
    // Basic types
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(double))]
    [JsonSerializable(typeof(object))]
    [JsonSerializable(typeof(List<string>))]
    [JsonSerializable(typeof(string[]))]
    // JSON element type for parameter processing
    [JsonSerializable(typeof(System.Text.Json.JsonElement))]
    [JsonSerializable(typeof(System.Text.Json.JsonDocument))]
    // MCP Response Models
    [JsonSerializable(typeof(ProcessLaunchResponse))]
    [JsonSerializable(typeof(UIOperationResponse))]
    [JsonSerializable(typeof(ElementSearchResponse))]
    // Typed Request Classes
    [JsonSerializable(typeof(TypedWorkerRequest))]
    [JsonSerializable(typeof(ElementTargetRequest))]
    [JsonSerializable(typeof(InvokeElementRequest))]
    [JsonSerializable(typeof(ToggleElementRequest))]
    [JsonSerializable(typeof(GetToggleStateRequest))]
    [JsonSerializable(typeof(SetToggleStateRequest))]
    [JsonSerializable(typeof(SetElementValueRequest))]
    [JsonSerializable(typeof(GetElementValueRequest))]
    [JsonSerializable(typeof(IsReadOnlyRequest))]
    [JsonSerializable(typeof(FindElementsRequest))]
    [JsonSerializable(typeof(FindElementsByControlTypeRequest))]
    [JsonSerializable(typeof(FindElementsByPatternRequest))]
    [JsonSerializable(typeof(GetDesktopWindowsRequest))]
    [JsonSerializable(typeof(WindowActionRequest))]
    [JsonSerializable(typeof(GetWindowInfoRequest))]
    [JsonSerializable(typeof(GetWindowInteractionStateRequest))]
    [JsonSerializable(typeof(GetWindowCapabilitiesRequest))]
    [JsonSerializable(typeof(SetRangeValueRequest))]
    [JsonSerializable(typeof(GetRangeValueRequest))]
    [JsonSerializable(typeof(GetRangePropertiesRequest))]
    [JsonSerializable(typeof(SetTextRequest))]
    [JsonSerializable(typeof(GetTextRequest))]
    [JsonSerializable(typeof(FindTextRequest))]
    [JsonSerializable(typeof(SelectTextRequest))]
    [JsonSerializable(typeof(TraverseTextRequest))]
    [JsonSerializable(typeof(TransformElementRequest))]
    [JsonSerializable(typeof(MoveElementRequest))]
    [JsonSerializable(typeof(ResizeElementRequest))]
    [JsonSerializable(typeof(RotateElementRequest))]
    [JsonSerializable(typeof(WaitForInputIdleRequest))]
    [JsonSerializable(typeof(GetGridInfoRequest))]
    [JsonSerializable(typeof(GetGridItemRequest))]
    [JsonSerializable(typeof(GetColumnHeaderRequest))]
    [JsonSerializable(typeof(GetRowHeaderRequest))]
    [JsonSerializable(typeof(DockElementRequest))]
    [JsonSerializable(typeof(RealizeVirtualizedItemRequest))]
    [JsonSerializable(typeof(FindItemByPropertyRequest))]
    [JsonSerializable(typeof(StartSynchronizedInputRequest))]
    [JsonSerializable(typeof(CancelSynchronizedInputRequest))]
    [JsonSerializable(typeof(ExpandCollapseElementRequest))]
    [JsonSerializable(typeof(GetScrollInfoRequest))]
    [JsonSerializable(typeof(ScrollElementIntoViewRequest))]
    [JsonSerializable(typeof(ScrollElementRequest))]
    [JsonSerializable(typeof(SetScrollPercentRequest))]
    [JsonSerializable(typeof(GetAvailableViewsRequest))]
    [JsonSerializable(typeof(GetCurrentViewRequest))]
    [JsonSerializable(typeof(GetViewNameRequest))]
    [JsonSerializable(typeof(SetViewRequest))]
    [JsonSerializable(typeof(GetColumnHeaderItemsRequest))]
    [JsonSerializable(typeof(GetColumnHeadersRequest))]
    [JsonSerializable(typeof(GetRowHeaderItemsRequest))]
    [JsonSerializable(typeof(GetRowHeadersRequest))]
    [JsonSerializable(typeof(GetRowOrColumnMajorRequest))]
    [JsonSerializable(typeof(GetTableInfoRequest))]
    [JsonSerializable(typeof(AddToSelectionRequest))]
    [JsonSerializable(typeof(CanSelectMultipleRequest))]
    [JsonSerializable(typeof(ClearSelectionRequest))]
    [JsonSerializable(typeof(GetSelectionContainerRequest))]
    [JsonSerializable(typeof(GetSelectionRequest))]
    [JsonSerializable(typeof(IsSelectedRequest))]
    [JsonSerializable(typeof(IsSelectionRequiredRequest))]
    [JsonSerializable(typeof(RemoveFromSelectionRequest))]
    [JsonSerializable(typeof(SelectElementRequest))]
    [JsonSerializable(typeof(SelectItemRequest))]
    [JsonSerializable(typeof(GetElementPropertiesRequest))]
    [JsonSerializable(typeof(GetElementPatternsRequest))]
    [JsonSerializable(typeof(GetAncestorsRequest))]
    [JsonSerializable(typeof(GetChildrenRequest))]
    [JsonSerializable(typeof(GetDescendantsRequest))]
    [JsonSerializable(typeof(GetElementTreeRequest))]
    [JsonSerializable(typeof(GetParentRequest))]
    [JsonSerializable(typeof(GetSiblingsRequest))]
    // ServerEnhanced types
    [JsonSerializable(typeof(ServerEnhancedResponse<ElementTreeResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<TreeNavigationResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<ElementSearchResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<ProcessResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<ActionResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<ElementValueResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<WindowInfoResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<WindowInteractionStateResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<WindowCapabilitiesResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<ScreenshotResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<BaseOperationResult>))]
    [JsonSerializable(typeof(ServerExecutionInfo))]
    [JsonSerializable(typeof(RequestMetadata))]
    [JsonSourceGenerationOptions(
        WriteIndented = false, 
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals)]
    internal partial class UIAutomationJsonContext : JsonSerializerContext
    {
    }
}