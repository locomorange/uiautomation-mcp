using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.Encodings.Web;
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
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        /// <summary>
        /// Serializes an object using type inference to automatically select the appropriate JsonTypeInfo
        /// </summary>
        /// <typeparam name="T">The type to serialize</typeparam>
        /// <param name="obj">The object to serialize</param>
        /// <returns>JSON string representation</returns>
        /// <exception cref="NotSupportedException">Thrown when the type is not supported for serialization</exception>
        public static string Serialize<T>(T obj) where T : notnull
        {
            try
            {
                return GetTypeInfo<T>() switch
                {
                    JsonTypeInfo<T> typeInfo => JsonSerializer.Serialize(obj, typeInfo),
                    null => throw new NotSupportedException($"Type {typeof(T).Name} is not supported for serialization")
                };
            }
            catch (NotSupportedException)
            {
                throw; // Re-throw NotSupportedException as-is
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to serialize object of type {typeof(T).Name}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deserializes a JSON string using type inference to automatically select the appropriate JsonTypeInfo
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="json">The JSON string to deserialize</param>
        /// <returns>The deserialized object</returns>
        /// <exception cref="NotSupportedException">Thrown when the type is not supported for deserialization</exception>
        public static T? Deserialize<T>(string json) where T : notnull
        {
            try
            {
                if (string.IsNullOrEmpty(json))
                    throw new ArgumentException("JSON string cannot be null or empty");

                return GetTypeInfo<T>() switch
                {
                    JsonTypeInfo<T> typeInfo => JsonSerializer.Deserialize(json, typeInfo),
                    null => throw new NotSupportedException($"Type {typeof(T).Name} is not supported for deserialization")
                };
            }
            catch (NotSupportedException)
            {
                throw; // Re-throw NotSupportedException as-is
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to deserialize JSON to type {typeof(T).Name}. JSON: {json}. Error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error deserializing to type {typeof(T).Name}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the unified JsonTypeInfo for a given type by consolidating all type mappings
        /// </summary>
        /// <typeparam name="T">The type to get JsonTypeInfo for</typeparam>
        /// <returns>JsonTypeInfo for the specified type, or null if not supported</returns>
        private static JsonTypeInfo<T>? GetTypeInfo<T>()
        {
            return typeof(T) switch
            {
                // WorkerResponse types (consolidated from GetWorkerResponseTypeInfo)
                Type t when t == typeof(WorkerResponse<Dictionary<string, object>>) => (JsonTypeInfo<T>)(object)_context.WorkerResponseDictionaryStringObject,
                Type t when t == typeof(WorkerResponse<List<Dictionary<string, object>>>) => (JsonTypeInfo<T>)(object)_context.WorkerResponseListDictionaryStringObject,
                Type t when t == typeof(WorkerResponse<ScreenshotResult>) => (JsonTypeInfo<T>)(object)_context.WorkerResponseScreenshotResult,
                Type t when t == typeof(WorkerResponse<BaseOperationResult>) => (JsonTypeInfo<T>)(object)_context.WorkerResponseBaseOperationResult,
                Type t when t == typeof(WorkerResponse<ElementSearchResult>) => (JsonTypeInfo<T>)(object)_context.WorkerResponseElementSearchResult,
                Type t when t == typeof(WorkerResponse<ActionResult>) => (JsonTypeInfo<T>)(object)_context.WorkerResponseActionResult,
                Type t when t == typeof(WorkerResponse<WindowActionResult>) => (JsonTypeInfo<T>)(object)_context.WorkerResponseWindowActionResult,
                Type t when t == typeof(WorkerResponse<FindItemResult>) => (JsonTypeInfo<T>)(object)_context.WorkerResponseFindItemResult,
                Type t when t == typeof(WorkerResponse<ProcessResult>) => (JsonTypeInfo<T>)(object)_context.WorkerResponseProcessResult,
                Type t when t == typeof(WorkerResponse<ErrorResult>) => (JsonTypeInfo<T>)(object)_context.WorkerResponseErrorResult,
                Type t when t == typeof(WorkerResponse<UniversalResponse>) => (JsonTypeInfo<T>)(object)_context.WorkerResponseUniversalResponse,
                Type t when t == typeof(WorkerResponse<BooleanResult>) => (JsonTypeInfo<T>)(object)_context.WorkerResponseBooleanResult,
                Type t when t == typeof(WorkerResponse<TextInfoResult>) => (JsonTypeInfo<T>)(object)_context.WorkerResponseTextInfoResult,
                Type t when t == typeof(WorkerResponse<TextAttributesResult>) => (JsonTypeInfo<T>)(object)_context.WorkerResponseTextAttributesResult,
                Type t when t == typeof(WorkerResponse<TextSearchResult>) => (JsonTypeInfo<T>)(object)_context.WorkerResponseTextSearchResult,
                Type t when t == typeof(WorkerResponse<ElementTreeResult>) => (JsonTypeInfo<T>)(object)_context.WorkerResponseElementTreeResult,
                Type t when t == typeof(WorkerResponse<SearchElementsResult>) => (JsonTypeInfo<T>)(object)_context.WorkerResponseSearchElementsResult,
                Type t when t == typeof(WorkerResponse<ElementDetailResult>) => (JsonTypeInfo<T>)(object)_context.WorkerResponseElementDetailResult,
                Type t when t == typeof(WorkerResponse<TableInfoResult>) => (JsonTypeInfo<T>)(object)_context.WorkerResponseTableInfoResult,
                Type t when t == typeof(WorkerResponse<object>) => (JsonTypeInfo<T>)(object)_context.WorkerResponseObject,

                // Result types (consolidated from GetResultTypeInfo)
                Type t when t == typeof(ScreenshotResult) => (JsonTypeInfo<T>)(object)_context.ScreenshotResult,
                Type t when t == typeof(BaseOperationResult) => (JsonTypeInfo<T>)(object)_context.BaseOperationResult,
                Type t when t == typeof(ElementSearchResult) => (JsonTypeInfo<T>)(object)_context.ElementSearchResult,
                Type t when t == typeof(ActionResult) => (JsonTypeInfo<T>)(object)_context.ActionResult,
                Type t when t == typeof(WindowActionResult) => (JsonTypeInfo<T>)(object)_context.WindowActionResult,
                Type t when t == typeof(FindItemResult) => (JsonTypeInfo<T>)(object)_context.FindItemResult,
                Type t when t == typeof(ProcessResult) => (JsonTypeInfo<T>)(object)_context.ProcessResult,
                Type t when t == typeof(ErrorResult) => (JsonTypeInfo<T>)(object)_context.ErrorResult,
                Type t when t == typeof(UniversalResponse) => (JsonTypeInfo<T>)(object)_context.UniversalResponse,
                Type t when t == typeof(ElementTreeResult) => (JsonTypeInfo<T>)(object)_context.ElementTreeResult,
                Type t when t == typeof(TreeNavigationResult) => (JsonTypeInfo<T>)(object)_context.TreeNavigationResult,
                Type t when t == typeof(BooleanResult) => (JsonTypeInfo<T>)(object)_context.BooleanResult,
                Type t when t == typeof(ProcessLaunchResponse) => (JsonTypeInfo<T>)(object)_context.ProcessLaunchResponse,
                Type t when t == typeof(TextInfoResult) => (JsonTypeInfo<T>)(object)_context.TextInfoResult,
                Type t when t == typeof(TextAttributesResult) => (JsonTypeInfo<T>)(object)_context.TextAttributesResult,
                Type t when t == typeof(TextSearchResult) => (JsonTypeInfo<T>)(object)_context.TextSearchResult,
                Type t when t == typeof(TextAttributeRange) => (JsonTypeInfo<T>)(object)_context.TextAttributeRange,
                Type t when t == typeof(TextRangeAttributes) => (JsonTypeInfo<T>)(object)_context.TextRangeAttributes,
                Type t when t == typeof(TextAttributes) => (JsonTypeInfo<T>)(object)_context.TextAttributes,
                Type t when t == typeof(SearchElementsResult) => (JsonTypeInfo<T>)(object)_context.SearchElementsResult,
                Type t when t == typeof(ElementDetailResult) => (JsonTypeInfo<T>)(object)_context.ElementDetailResult,
                Type t when t == typeof(TableInfoResult) => (JsonTypeInfo<T>)(object)_context.TableInfoResult,

                // Request types (consolidated from GetRequestTypeInfo)
                Type t when t == typeof(InvokeElementRequest) => (JsonTypeInfo<T>)(object)_context.InvokeElementRequest,
                Type t when t == typeof(ToggleElementRequest) => (JsonTypeInfo<T>)(object)_context.ToggleElementRequest,
                Type t when t == typeof(GetToggleStateRequest) => (JsonTypeInfo<T>)(object)_context.GetToggleStateRequest,
                Type t when t == typeof(SetToggleStateRequest) => (JsonTypeInfo<T>)(object)_context.SetToggleStateRequest,
                Type t when t == typeof(SetValueRequest) => (JsonTypeInfo<T>)(object)_context.SetValueRequest,
                Type t when t == typeof(FindElementsRequest) => (JsonTypeInfo<T>)(object)_context.FindElementsRequest,
                Type t when t == typeof(WindowActionRequest) => (JsonTypeInfo<T>)(object)_context.WindowActionRequest,
                Type t when t == typeof(GetWindowInfoRequest) => (JsonTypeInfo<T>)(object)_context.GetWindowInfoRequest,
                Type t when t == typeof(GetWindowInteractionStateRequest) => (JsonTypeInfo<T>)(object)_context.GetWindowInteractionStateRequest,
                Type t when t == typeof(GetWindowCapabilitiesRequest) => (JsonTypeInfo<T>)(object)_context.GetWindowCapabilitiesRequest,
                Type t when t == typeof(SetRangeValueRequest) => (JsonTypeInfo<T>)(object)_context.SetRangeValueRequest,
                Type t when t == typeof(GetRangeValueRequest) => (JsonTypeInfo<T>)(object)_context.GetRangeValueRequest,
                Type t when t == typeof(GetRangePropertiesRequest) => (JsonTypeInfo<T>)(object)_context.GetRangePropertiesRequest,
                Type t when t == typeof(SetTextRequest) => (JsonTypeInfo<T>)(object)_context.SetTextRequest,
                Type t when t == typeof(FindTextRequest) => (JsonTypeInfo<T>)(object)_context.FindTextRequest,
                Type t when t == typeof(GetTextAttributesRequest) => (JsonTypeInfo<T>)(object)_context.GetTextAttributesRequest,
                Type t when t == typeof(SelectTextRequest) => (JsonTypeInfo<T>)(object)_context.SelectTextRequest,
                Type t when t == typeof(TraverseTextRequest) => (JsonTypeInfo<T>)(object)_context.TraverseTextRequest,
                Type t when t == typeof(TransformElementRequest) => (JsonTypeInfo<T>)(object)_context.TransformElementRequest,
                Type t when t == typeof(MoveElementRequest) => (JsonTypeInfo<T>)(object)_context.MoveElementRequest,
                Type t when t == typeof(ResizeElementRequest) => (JsonTypeInfo<T>)(object)_context.ResizeElementRequest,
                Type t when t == typeof(RotateElementRequest) => (JsonTypeInfo<T>)(object)_context.RotateElementRequest,
                Type t when t == typeof(WaitForInputIdleRequest) => (JsonTypeInfo<T>)(object)_context.WaitForInputIdleRequest,
                Type t when t == typeof(GetGridInfoRequest) => (JsonTypeInfo<T>)(object)_context.GetGridInfoRequest,
                Type t when t == typeof(GetGridItemRequest) => (JsonTypeInfo<T>)(object)_context.GetGridItemRequest,
                Type t when t == typeof(GetColumnHeaderRequest) => (JsonTypeInfo<T>)(object)_context.GetColumnHeaderRequest,
                Type t when t == typeof(GetRowHeaderRequest) => (JsonTypeInfo<T>)(object)_context.GetRowHeaderRequest,
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
                Type t when t == typeof(GetAvailableViewsRequest) => (JsonTypeInfo<T>)(object)_context.GetAvailableViewsRequest,
                Type t when t == typeof(GetCurrentViewRequest) => (JsonTypeInfo<T>)(object)_context.GetCurrentViewRequest,
                Type t when t == typeof(GetViewNameRequest) => (JsonTypeInfo<T>)(object)_context.GetViewNameRequest,
                Type t when t == typeof(SetViewRequest) => (JsonTypeInfo<T>)(object)_context.SetViewRequest,
                Type t when t == typeof(GetColumnHeaderItemsRequest) => (JsonTypeInfo<T>)(object)_context.GetColumnHeaderItemsRequest,
                Type t when t == typeof(GetColumnHeadersRequest) => (JsonTypeInfo<T>)(object)_context.GetColumnHeadersRequest,
                Type t when t == typeof(GetRowHeaderItemsRequest) => (JsonTypeInfo<T>)(object)_context.GetRowHeaderItemsRequest,
                Type t when t == typeof(GetRowHeadersRequest) => (JsonTypeInfo<T>)(object)_context.GetRowHeadersRequest,
                Type t when t == typeof(GetRowOrColumnMajorRequest) => (JsonTypeInfo<T>)(object)_context.GetRowOrColumnMajorRequest,
                Type t when t == typeof(GetTableInfoRequest) => (JsonTypeInfo<T>)(object)_context.GetTableInfoRequest,
                Type t when t == typeof(AddToSelectionRequest) => (JsonTypeInfo<T>)(object)_context.AddToSelectionRequest,
                Type t when t == typeof(ClearSelectionRequest) => (JsonTypeInfo<T>)(object)_context.ClearSelectionRequest,
                Type t when t == typeof(GetSelectionContainerRequest) => (JsonTypeInfo<T>)(object)_context.GetSelectionContainerRequest,
                Type t when t == typeof(GetSelectionRequest) => (JsonTypeInfo<T>)(object)_context.GetSelectionRequest,
                Type t when t == typeof(RemoveFromSelectionRequest) => (JsonTypeInfo<T>)(object)_context.RemoveFromSelectionRequest,
                Type t when t == typeof(SelectElementRequest) => (JsonTypeInfo<T>)(object)_context.SelectElementRequest,
                Type t when t == typeof(SelectItemRequest) => (JsonTypeInfo<T>)(object)_context.SelectItemRequest,
                Type t when t == typeof(SetFocusRequest) => (JsonTypeInfo<T>)(object)_context.SetFocusRequest,
                Type t when t == typeof(GetElementPropertiesRequest) => (JsonTypeInfo<T>)(object)_context.GetElementPropertiesRequest,
                Type t when t == typeof(GetElementPatternsRequest) => (JsonTypeInfo<T>)(object)_context.GetElementPatternsRequest,
                Type t when t == typeof(ValidateControlTypePatternsRequest) => (JsonTypeInfo<T>)(object)_context.ValidateControlTypePatternsRequest,
                Type t when t == typeof(VerifyAccessibilityRequest) => (JsonTypeInfo<T>)(object)_context.VerifyAccessibilityRequest,
                Type t when t == typeof(GetLabeledByRequest) => (JsonTypeInfo<T>)(object)_context.GetLabeledByRequest,
                Type t when t == typeof(GetDescribedByRequest) => (JsonTypeInfo<T>)(object)_context.GetDescribedByRequest,
                Type t when t == typeof(GetCustomPropertiesRequest) => (JsonTypeInfo<T>)(object)_context.GetCustomPropertiesRequest,
                Type t when t == typeof(SetCustomPropertyRequest) => (JsonTypeInfo<T>)(object)_context.SetCustomPropertyRequest,
                Type t when t == typeof(GetAccessibilityInfoRequest) => (JsonTypeInfo<T>)(object)_context.GetAccessibilityInfoRequest,
                Type t when t == typeof(GetAncestorsRequest) => (JsonTypeInfo<T>)(object)_context.GetAncestorsRequest,
                Type t when t == typeof(GetChildrenRequest) => (JsonTypeInfo<T>)(object)_context.GetChildrenRequest,
                Type t when t == typeof(GetDescendantsRequest) => (JsonTypeInfo<T>)(object)_context.GetDescendantsRequest,
                Type t when t == typeof(GetElementTreeRequest) => (JsonTypeInfo<T>)(object)_context.GetElementTreeRequest,
                Type t when t == typeof(GetParentRequest) => (JsonTypeInfo<T>)(object)_context.GetParentRequest,
                Type t when t == typeof(GetSiblingsRequest) => (JsonTypeInfo<T>)(object)_context.GetSiblingsRequest,
                Type t when t == typeof(SearchElementsRequest) => (JsonTypeInfo<T>)(object)_context.SearchElementsRequest,
                Type t when t == typeof(GetElementDetailsRequest) => (JsonTypeInfo<T>)(object)_context.GetElementDetailsRequest,
                Type t when t == typeof(ElementDetails) => (JsonTypeInfo<T>)(object)_context.ElementDetails,

                // ServerEnhancedResponse types
                Type t when t == typeof(ServerEnhancedResponse<ElementTreeResult>) => (JsonTypeInfo<T>)(object)_context.ServerEnhancedResponseElementTreeResult,
                Type t when t == typeof(ServerEnhancedResponse<TreeNavigationResult>) => (JsonTypeInfo<T>)(object)_context.ServerEnhancedResponseTreeNavigationResult,
                Type t when t == typeof(ServerEnhancedResponse<ElementSearchResult>) => (JsonTypeInfo<T>)(object)_context.ServerEnhancedResponseElementSearchResult,
                Type t when t == typeof(ServerEnhancedResponse<ProcessResult>) => (JsonTypeInfo<T>)(object)_context.ServerEnhancedResponseProcessResult,
                Type t when t == typeof(ServerEnhancedResponse<ActionResult>) => (JsonTypeInfo<T>)(object)_context.ServerEnhancedResponseActionResult,
                Type t when t == typeof(ServerEnhancedResponse<ScreenshotResult>) => (JsonTypeInfo<T>)(object)_context.ServerEnhancedResponseScreenshotResult,
                Type t when t == typeof(ServerEnhancedResponse<BaseOperationResult>) => (JsonTypeInfo<T>)(object)_context.ServerEnhancedResponseBaseOperationResult,
                Type t when t == typeof(ServerEnhancedResponse<ProcessLaunchResponse>) => (JsonTypeInfo<T>)(object)_context.ServerEnhancedResponseProcessLaunchResponse,
                Type t when t == typeof(ServerEnhancedResponse<BooleanResult>) => (JsonTypeInfo<T>)(object)_context.ServerEnhancedResponseBooleanResult,
                Type t when t == typeof(ServerEnhancedResponse<SearchElementsResult>) => (JsonTypeInfo<T>)(object)_context.ServerEnhancedResponseSearchElementsResult,
                Type t when t == typeof(ServerEnhancedResponse<ElementDetailResult>) => (JsonTypeInfo<T>)(object)_context.ServerEnhancedResponseElementDetailResult,
                Type t when t == typeof(ServerEnhancedResponse<TableInfoResult>) => (JsonTypeInfo<T>)(object)_context.ServerEnhancedResponseTableInfoResult,
                Type t when t == typeof(ServerEnhancedResponse<TextSearchResult>) => (JsonTypeInfo<T>)(object)_context.ServerEnhancedResponseTextSearchResult,
                Type t when t == typeof(ServerEnhancedResponse<TextAttributesResult>) => (JsonTypeInfo<T>)(object)_context.ServerEnhancedResponseTextAttributesResult,

                // Basic types
                Type t when t == typeof(Dictionary<string, object>) => (JsonTypeInfo<T>)(object)_context.DictionaryStringObject,
                Type t when t == typeof(List<Dictionary<string, object>>) => (JsonTypeInfo<T>)(object)_context.ListDictionaryStringObject,
                Type t when t == typeof(WorkerRequest) => (JsonTypeInfo<T>)(object)_context.WorkerRequest,

                // Unsupported type
                _ => null
            };
        }

    }

    #region JSON Source Generation Context
    [JsonSerializable(typeof(WorkerRequest))]
    [JsonSerializable(typeof(WorkerResponse<object>))]
    [JsonSerializable(typeof(WorkerResponse<Dictionary<string, object>>))]
    [JsonSerializable(typeof(WorkerResponse<List<Dictionary<string, object>>>))]
    [JsonSerializable(typeof(WorkerResponse<ScreenshotResult>))]
    [JsonSerializable(typeof(WorkerResponse<BaseOperationResult>))]
    [JsonSerializable(typeof(WorkerResponse<ElementSearchResult>))]
    [JsonSerializable(typeof(WorkerResponse<ActionResult>))]
    [JsonSerializable(typeof(WorkerResponse<WindowActionResult>))]
    [JsonSerializable(typeof(WorkerResponse<FindItemResult>))]
    [JsonSerializable(typeof(WorkerResponse<ProcessResult>))]
    [JsonSerializable(typeof(WorkerResponse<ElementTreeResult>))]
    [JsonSerializable(typeof(WorkerResponse<SearchElementsResult>))]
    [JsonSerializable(typeof(WorkerResponse<ElementDetailResult>))]
    [JsonSerializable(typeof(WorkerResponse<TableInfoResult>))]
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
    [JsonSerializable(typeof(ActionResult))]
    [JsonSerializable(typeof(WindowActionResult))]
    [JsonSerializable(typeof(ErrorResult))]
    [JsonSerializable(typeof(UniversalResponse))]
    [JsonSerializable(typeof(ElementTreeResult))]
    [JsonSerializable(typeof(TreeNavigationResult))]
    [JsonSerializable(typeof(BooleanResult))]
    [JsonSerializable(typeof(FindItemResult))]
    [JsonSerializable(typeof(TextInfoResult))]
    [JsonSerializable(typeof(TextAttributesResult))]
    [JsonSerializable(typeof(TextSearchResult))]
    [JsonSerializable(typeof(TextAttributeRange))]
    [JsonSerializable(typeof(TextRangeAttributes))]
    [JsonSerializable(typeof(TextAttributes))]
    [JsonSerializable(typeof(TableInfoResult))]
    [JsonSerializable(typeof(WorkerResponse<TextInfoResult>))]
    [JsonSerializable(typeof(WorkerResponse<TextAttributesResult>))]
    [JsonSerializable(typeof(WorkerResponse<TextSearchResult>))]
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
    [JsonSerializable(typeof(SetValueRequest))]
    [JsonSerializable(typeof(FindElementsRequest))]
    [JsonSerializable(typeof(WindowActionRequest))]
    [JsonSerializable(typeof(GetWindowInfoRequest))]
    [JsonSerializable(typeof(GetWindowInteractionStateRequest))]
    [JsonSerializable(typeof(GetWindowCapabilitiesRequest))]
    [JsonSerializable(typeof(SetRangeValueRequest))]
    [JsonSerializable(typeof(GetRangeValueRequest))]
    [JsonSerializable(typeof(GetRangePropertiesRequest))]
    [JsonSerializable(typeof(SetTextRequest))]
    [JsonSerializable(typeof(FindTextRequest))]
    [JsonSerializable(typeof(GetTextAttributesRequest))]
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
    [JsonSerializable(typeof(ClearSelectionRequest))]
    [JsonSerializable(typeof(GetSelectionContainerRequest))]
    [JsonSerializable(typeof(GetSelectionRequest))]
    [JsonSerializable(typeof(RemoveFromSelectionRequest))]
    [JsonSerializable(typeof(SelectElementRequest))]
    [JsonSerializable(typeof(SelectItemRequest))]
    [JsonSerializable(typeof(SetFocusRequest))]
    [JsonSerializable(typeof(GetElementPropertiesRequest))]
    [JsonSerializable(typeof(GetElementPatternsRequest))]
    [JsonSerializable(typeof(ValidateControlTypePatternsRequest))]
    [JsonSerializable(typeof(VerifyAccessibilityRequest))]
    [JsonSerializable(typeof(GetLabeledByRequest))]
    [JsonSerializable(typeof(GetDescribedByRequest))]
    [JsonSerializable(typeof(GetCustomPropertiesRequest))]
    [JsonSerializable(typeof(SetCustomPropertyRequest))]
    [JsonSerializable(typeof(GetAccessibilityInfoRequest))]
    [JsonSerializable(typeof(GetAncestorsRequest))]
    [JsonSerializable(typeof(GetChildrenRequest))]
    [JsonSerializable(typeof(GetDescendantsRequest))]
    [JsonSerializable(typeof(GetElementTreeRequest))]
    [JsonSerializable(typeof(GetParentRequest))]
    [JsonSerializable(typeof(GetSiblingsRequest))]
    // New MCP tools
    [JsonSerializable(typeof(SearchElementsRequest))]
    [JsonSerializable(typeof(GetElementDetailsRequest))]
    [JsonSerializable(typeof(SearchElementsResult))]
    [JsonSerializable(typeof(ElementDetailResult))]
    [JsonSerializable(typeof(BasicElementInfo))]
    [JsonSerializable(typeof(SearchMetadata))]
    [JsonSerializable(typeof(DetailMetadata))]
    [JsonSerializable(typeof(ElementDetail))]
    [JsonSerializable(typeof(ElementDetails))]
    [JsonSerializable(typeof(string[]))]
    [JsonSerializable(typeof(List<string>))]
    // ServerEnhanced types
    [JsonSerializable(typeof(ServerEnhancedResponse<ElementTreeResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<TreeNavigationResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<ElementSearchResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<ProcessResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<ActionResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<ScreenshotResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<BaseOperationResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<ProcessLaunchResponse>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<BooleanResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<SearchElementsResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<ElementDetailResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<TableInfoResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<TextSearchResult>))]
    [JsonSerializable(typeof(ServerEnhancedResponse<TextAttributesResult>))]
    [JsonSerializable(typeof(ProcessLaunchResponse))]
    [JsonSerializable(typeof(ServerExecutionInfo))]
    [JsonSerializable(typeof(RequestMetadata))]
    // Pattern Info types
    [JsonSerializable(typeof(ToggleInfo))]
    [JsonSerializable(typeof(RangeInfo))]
    [JsonSerializable(typeof(WindowPatternInfo))]
    [JsonSerializable(typeof(SelectionInfo))]
    [JsonSerializable(typeof(SelectionItemInfo))]
    [JsonSerializable(typeof(GridInfo))]
    [JsonSerializable(typeof(ScrollInfo))]
    [JsonSerializable(typeof(TextInfo))]
    [JsonSerializable(typeof(TransformInfo))]
    [JsonSerializable(typeof(ValueInfo))]
    [JsonSerializable(typeof(ExpandCollapseInfo))]
    [JsonSerializable(typeof(DockInfo))]
    [JsonSerializable(typeof(MultipleViewInfo))]
    [JsonSerializable(typeof(PatternViewInfo))]
    [JsonSerializable(typeof(GridItemInfo))]
    [JsonSerializable(typeof(TableItemInfo))]
    [JsonSerializable(typeof(TableInfo))]
    [JsonSerializable(typeof(InvokeInfo))]
    [JsonSerializable(typeof(ScrollItemInfo))]
    [JsonSerializable(typeof(VirtualizedItemInfo))]
    [JsonSerializable(typeof(ItemContainerInfo))]
    [JsonSerializable(typeof(SynchronizedInputInfo))]
    [JsonSerializable(typeof(AccessibilityInfo))]
    [JsonSerializable(typeof(ElementReference))]
    [JsonSourceGenerationOptions(
        WriteIndented = false, 
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals)]
    internal partial class UIAutomationJsonContext : JsonSerializerContext
    {
    }
    #endregion
}