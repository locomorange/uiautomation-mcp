using System.Text.Json;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;

namespace UIAutomationMCP.Shared
{
    /// <summary>
    /// WorkerRequestの型安全な拡張メソッド
    /// </summary>
    public static class WorkerRequestExtensions
    {
        /// <summary>
        /// WorkerRequestから型安全なリクエストオブジェクトを取得（設定値を適用）
        /// </summary>
        public static T? GetTypedRequest<T>(this WorkerRequest request, IOptions<UIAutomationOptions> options) where T : TypedWorkerRequest
        {
            if (request.Parameters == null)
                return null;

            try
            {
                var json = JsonSerializer.Serialize(request.Parameters);
                var typedRequest = JsonSerializer.Deserialize<T>(json);
                
                if (typedRequest != null)
                {
                    ApplyConfigurationDefaults(typedRequest, options.Value);
                }
                
                return typedRequest;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// 設定値をTypedRequestに適用
        /// </summary>
        private static void ApplyConfigurationDefaults(TypedWorkerRequest typedRequest, UIAutomationOptions options)
        {
            switch (typedRequest)
            {
                case FindElementsRequest findRequest:
                    ApplyElementSearchDefaults(findRequest, options.ElementSearch);
                    break;
                case FindElementsByControlTypeRequest findByTypeRequest:
                    ApplyElementSearchDefaults(findByTypeRequest, options.ElementSearch);
                    break;
                case FindElementsByPatternRequest findByPatternRequest:
                    ApplyElementSearchDefaults(findByPatternRequest, options.ElementSearch);
                    break;
                case GetDesktopWindowsRequest windowsRequest:
                    ApplyWindowOperationDefaults(windowsRequest, options.WindowOperation);
                    break;
                case WindowActionRequest windowActionRequest:
                    ApplyWindowOperationDefaults(windowActionRequest, options.WindowOperation);
                    break;
                case FindTextRequest findTextRequest:
                    ApplyTextOperationDefaults(findTextRequest, options.TextOperation);
                    break;
                case TraverseTextRequest traverseTextRequest:
                    ApplyTextOperationDefaults(traverseTextRequest, options.TextOperation);
                    break;
                case TransformElementRequest transformRequest:
                    ApplyTransformDefaults(transformRequest, options.Transform);
                    break;
                case MoveElementRequest moveRequest:
                    ApplyTransformDefaults(moveRequest, options.Transform);
                    break;
                case ResizeElementRequest resizeRequest:
                    ApplyTransformDefaults(resizeRequest, options.Transform);
                    break;
                case RotateElementRequest rotateRequest:
                    ApplyTransformDefaults(rotateRequest, options.Transform);
                    break;
                case SetRangeValueRequest rangeRequest:
                    ApplyRangeValueDefaults(rangeRequest, options.RangeValue);
                    break;
                case DockElementRequest dockRequest:
                    ApplyLayoutDefaults(dockRequest, options.Layout);
                    break;
                case ExpandCollapseElementRequest expandCollapseRequest:
                    ApplyLayoutDefaults(expandCollapseRequest, options.Layout);
                    break;
                case ScrollElementRequest scrollRequest:
                    ApplyLayoutDefaults(scrollRequest, options.Layout);
                    break;
                case SetScrollPercentRequest scrollPercentRequest:
                    ApplyLayoutDefaults(scrollPercentRequest, options.Layout);
                    break;
            }
        }

        private static void ApplyElementSearchDefaults(FindElementsRequest request, ElementSearchOptions options)
        {
            if (string.IsNullOrEmpty(request.Scope))
                request.Scope = options.DefaultScope;
            if (request.UseCache == default)
                request.UseCache = options.UseCache;
            if (request.UseRegex == default)
                request.UseRegex = options.UseRegex;
            if (request.UseWildcard == default)
                request.UseWildcard = options.UseWildcard;
        }

        private static void ApplyElementSearchDefaults(FindElementsByControlTypeRequest request, ElementSearchOptions options)
        {
            if (string.IsNullOrEmpty(request.Scope))
                request.Scope = options.DefaultScope;
        }

        private static void ApplyElementSearchDefaults(FindElementsByPatternRequest request, ElementSearchOptions options)
        {
            if (string.IsNullOrEmpty(request.Scope))
                request.Scope = options.DefaultScope;
        }

        private static void ApplyWindowOperationDefaults(GetDesktopWindowsRequest request, WindowOperationOptions options)
        {
            if (request.IncludeInvisible == default)
                request.IncludeInvisible = options.IncludeInvisible;
        }

        private static void ApplyWindowOperationDefaults(WindowActionRequest request, WindowOperationOptions options)
        {
            if (string.IsNullOrEmpty(request.Action))
                request.Action = options.DefaultAction;
        }

        private static void ApplyTextOperationDefaults(FindTextRequest request, TextOperationOptions options)
        {
            if (request.IgnoreCase == default)
                request.IgnoreCase = options.DefaultIgnoreCase;
            if (request.Backward == default)
                request.Backward = options.DefaultBackward;
        }

        private static void ApplyTextOperationDefaults(TraverseTextRequest request, TextOperationOptions options)
        {
            if (string.IsNullOrEmpty(request.Direction))
                request.Direction = options.DefaultTraverseUnit;
            if (request.Count == default)
                request.Count = options.DefaultTraverseCount;
        }

        private static void ApplyTransformDefaults(TransformElementRequest request, TransformOptions options)
        {
            if (request.X == default)
                request.X = options.DefaultX;
            if (request.Y == default)
                request.Y = options.DefaultY;
            if (request.Width == default)
                request.Width = options.DefaultWidth;
            if (request.Height == default)
                request.Height = options.DefaultHeight;
        }

        private static void ApplyTransformDefaults(MoveElementRequest request, TransformOptions options)
        {
            if (request.X == default)
                request.X = options.DefaultX;
            if (request.Y == default)
                request.Y = options.DefaultY;
        }

        private static void ApplyTransformDefaults(ResizeElementRequest request, TransformOptions options)
        {
            if (request.Width == default)
                request.Width = options.DefaultWidth;
            if (request.Height == default)
                request.Height = options.DefaultHeight;
        }

        private static void ApplyTransformDefaults(RotateElementRequest request, TransformOptions options)
        {
            if (request.Degrees == default)
                request.Degrees = options.DefaultRotationDegrees;
        }

        private static void ApplyRangeValueDefaults(SetRangeValueRequest request, RangeValueOptions options)
        {
            if (request.Value == default)
                request.Value = options.DefaultValue;
        }

        private static void ApplyLayoutDefaults(DockElementRequest request, LayoutOptions options)
        {
            if (string.IsNullOrEmpty(request.DockPosition))
                request.DockPosition = options.DefaultDockPosition;
        }

        private static void ApplyLayoutDefaults(ExpandCollapseElementRequest request, LayoutOptions options)
        {
            if (string.IsNullOrEmpty(request.Action))
                request.Action = options.DefaultExpandCollapseAction;
        }

        private static void ApplyLayoutDefaults(ScrollElementRequest request, LayoutOptions options)
        {
            if (string.IsNullOrEmpty(request.Direction))
                request.Direction = options.DefaultScrollDirection;
            if (request.Amount == default)
                request.Amount = options.DefaultScrollAmount;
        }

        private static void ApplyLayoutDefaults(SetScrollPercentRequest request, LayoutOptions options)
        {
            if (request.HorizontalPercent == default)
                request.HorizontalPercent = options.DefaultHorizontalScrollPercent;
            if (request.VerticalPercent == default)
                request.VerticalPercent = options.DefaultVerticalScrollPercent;
        }

        /// <summary>
        /// 操作名に基づいて適切な型安全リクエストを取得（設定値を適用）
        /// </summary>
        public static TypedWorkerRequest? GetTypedRequestByOperation(this WorkerRequest request, IOptions<UIAutomationOptions> options)
        {
            return request.Operation switch
            {
                "InvokeElement" => request.GetTypedRequest<InvokeElementRequest>(options),
                "ToggleElement" => request.GetTypedRequest<ToggleElementRequest>(options),
                "GetToggleState" => request.GetTypedRequest<GetToggleStateRequest>(options),
                "SetToggleState" => request.GetTypedRequest<SetToggleStateRequest>(options),
                "SetElementValue" => request.GetTypedRequest<SetElementValueRequest>(options),
                "GetElementValue" => request.GetTypedRequest<GetElementValueRequest>(options),
                "IsReadOnly" => request.GetTypedRequest<IsReadOnlyRequest>(options),
                "FindElements" => request.GetTypedRequest<FindElementsRequest>(options),
                "FindElementsByControlType" => request.GetTypedRequest<FindElementsByControlTypeRequest>(options),
                "FindElementsByPattern" => request.GetTypedRequest<FindElementsByPatternRequest>(options),
                "GetDesktopWindows" => request.GetTypedRequest<GetDesktopWindowsRequest>(options),
                "WindowAction" => request.GetTypedRequest<WindowActionRequest>(options),
                "GetWindowInfo" => request.GetTypedRequest<GetWindowInfoRequest>(options),
                "GetWindowInteractionState" => request.GetTypedRequest<GetWindowInteractionStateRequest>(options),
                "GetWindowCapabilities" => request.GetTypedRequest<GetWindowCapabilitiesRequest>(options),
                "SetRangeValue" => request.GetTypedRequest<SetRangeValueRequest>(options),
                "GetRangeValue" => request.GetTypedRequest<GetRangeValueRequest>(options),
                "GetRangeProperties" => request.GetTypedRequest<GetRangePropertiesRequest>(options),
                "SetText" => request.GetTypedRequest<SetTextRequest>(options),
                "GetText" => request.GetTypedRequest<GetTextRequest>(options),
                "FindText" => request.GetTypedRequest<FindTextRequest>(options),
                "SelectText" => request.GetTypedRequest<SelectTextRequest>(options),
                "TraverseText" => request.GetTypedRequest<TraverseTextRequest>(options),
                "TransformElement" => request.GetTypedRequest<TransformElementRequest>(options),
                "MoveElement" => request.GetTypedRequest<MoveElementRequest>(options),
                "ResizeElement" => request.GetTypedRequest<ResizeElementRequest>(options),
                "RotateElement" => request.GetTypedRequest<RotateElementRequest>(options),
                "WaitForInputIdle" => request.GetTypedRequest<WaitForInputIdleRequest>(options),
                "GetGridInfo" => request.GetTypedRequest<GetGridInfoRequest>(options),
                "GetGridItem" => request.GetTypedRequest<GetGridItemRequest>(options),
                "GetColumnHeader" => request.GetTypedRequest<GetColumnHeaderRequest>(options),
                "GetRowHeader" => request.GetTypedRequest<GetRowHeaderRequest>(options),
                "DockElement" => request.GetTypedRequest<DockElementRequest>(options),
                "ExpandCollapseElement" => request.GetTypedRequest<ExpandCollapseElementRequest>(options),
                "GetScrollInfo" => request.GetTypedRequest<GetScrollInfoRequest>(options),
                "ScrollElementIntoView" => request.GetTypedRequest<ScrollElementIntoViewRequest>(options),
                "ScrollElement" => request.GetTypedRequest<ScrollElementRequest>(options),
                "SetScrollPercent" => request.GetTypedRequest<SetScrollPercentRequest>(options),
                "GetAvailableViews" => request.GetTypedRequest<GetAvailableViewsRequest>(options),
                "GetCurrentView" => request.GetTypedRequest<GetCurrentViewRequest>(options),
                "GetViewName" => request.GetTypedRequest<GetViewNameRequest>(options),
                "SetView" => request.GetTypedRequest<SetViewRequest>(options),
                "AddToSelection" => request.GetTypedRequest<AddToSelectionRequest>(options),
                "CanSelectMultiple" => request.GetTypedRequest<CanSelectMultipleRequest>(options),
                "ClearSelection" => request.GetTypedRequest<ClearSelectionRequest>(options),
                "GetSelectionContainer" => request.GetTypedRequest<GetSelectionContainerRequest>(options),
                "GetSelection" => request.GetTypedRequest<GetSelectionRequest>(options),
                "IsSelected" => request.GetTypedRequest<IsSelectedRequest>(options),
                "IsSelectionRequired" => request.GetTypedRequest<IsSelectionRequiredRequest>(options),
                "RemoveFromSelection" => request.GetTypedRequest<RemoveFromSelectionRequest>(options),
                "SelectElement" => request.GetTypedRequest<SelectElementRequest>(options),
                "SelectItem" => request.GetTypedRequest<SelectItemRequest>(options),
                "GetColumnHeaderItems" => request.GetTypedRequest<GetColumnHeaderItemsRequest>(options),
                "GetColumnHeaders" => request.GetTypedRequest<GetColumnHeadersRequest>(options),
                "GetRowHeaderItems" => request.GetTypedRequest<GetRowHeaderItemsRequest>(options),
                "GetRowHeaders" => request.GetTypedRequest<GetRowHeadersRequest>(options),
                "GetRowOrColumnMajor" => request.GetTypedRequest<GetRowOrColumnMajorRequest>(options),
                "GetTableInfo" => request.GetTypedRequest<GetTableInfoRequest>(options),
                "GetElementProperties" => request.GetTypedRequest<GetElementPropertiesRequest>(options),
                "GetElementPatterns" => request.GetTypedRequest<GetElementPatternsRequest>(options),
                "GetAncestors" => request.GetTypedRequest<GetAncestorsRequest>(options),
                "GetChildren" => request.GetTypedRequest<GetChildrenRequest>(options),
                "GetDescendants" => request.GetTypedRequest<GetDescendantsRequest>(options),
                "GetElementTree" => request.GetTypedRequest<GetElementTreeRequest>(options),
                "GetParent" => request.GetTypedRequest<GetParentRequest>(options),
                "GetSiblings" => request.GetTypedRequest<GetSiblingsRequest>(options),
                _ => null
            };
        }

        /// <summary>
        /// 型安全リクエストからWorkerRequestを作成
        /// </summary>
        public static WorkerRequest ToWorkerRequest(this TypedWorkerRequest typedRequest)
        {
            var parametersJson = JsonSerializer.Serialize(typedRequest);
            var parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(parametersJson);

            return new WorkerRequest
            {
                Operation = typedRequest.Operation,
                Parameters = parameters
            };
        }
    }

    /// <summary>
    /// 型安全なWorkerRequestファクトリ
    /// </summary>
    public static class TypedWorkerRequestFactory
    {
        public static WorkerRequest CreateInvokeElement(string elementId, string windowTitle = "", int? processId = null)
        {
            return new InvokeElementRequest
            {
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId
            }.ToWorkerRequest();
        }

        public static WorkerRequest CreateSetElementValue(string elementId, string value, string windowTitle = "", int? processId = null)
        {
            return new SetElementValueRequest
            {
                ElementId = elementId,
                Value = value,
                WindowTitle = windowTitle,
                ProcessId = processId
            }.ToWorkerRequest();
        }

        public static WorkerRequest CreateFindElements(
            string? windowTitle = null,
            int? processId = null,
            string? searchText = null,
            string? automationId = null,
            string? controlType = null,
            string? className = null,
            string scope = "descendants",
            int timeoutSeconds = 30,
            bool useCache = true,
            bool useRegex = false,
            bool useWildcard = false)
        {
            return new FindElementsRequest
            {
                WindowTitle = windowTitle,
                ProcessId = processId,
                SearchText = searchText,
                AutomationId = automationId,
                ControlType = controlType,
                ClassName = className,
                Scope = scope,
                TimeoutSeconds = timeoutSeconds,
                UseCache = useCache,
                UseRegex = useRegex,
                UseWildcard = useWildcard
            }.ToWorkerRequest();
        }

        public static WorkerRequest CreateWindowAction(string action, string? windowTitle = null, int? processId = null)
        {
            return new WindowActionRequest
            {
                Action = action,
                WindowTitle = windowTitle,
                ProcessId = processId
            }.ToWorkerRequest();
        }

        public static WorkerRequest CreateSetRangeValue(string elementId, double value, string windowTitle = "", int? processId = null)
        {
            return new SetRangeValueRequest
            {
                ElementId = elementId,
                Value = value,
                WindowTitle = windowTitle,
                ProcessId = processId
            }.ToWorkerRequest();
        }

        public static WorkerRequest CreateSelectText(string elementId, int startIndex, int length, string windowTitle = "", int? processId = null)
        {
            return new SelectTextRequest
            {
                ElementId = elementId,
                StartIndex = startIndex,
                Length = length,
                WindowTitle = windowTitle,
                ProcessId = processId
            }.ToWorkerRequest();
        }

        public static WorkerRequest CreateMoveElement(string elementId, double x, double y, string windowTitle = "", int? processId = null)
        {
            return new MoveElementRequest
            {
                ElementId = elementId,
                X = x,
                Y = y,
                WindowTitle = windowTitle,
                ProcessId = processId
            }.ToWorkerRequest();
        }

        public static WorkerRequest CreateWaitForInputIdle(string? windowTitle = null, int? processId = null, int timeoutMilliseconds = 10000)
        {
            return new WaitForInputIdleRequest
            {
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutMilliseconds = timeoutMilliseconds
            }.ToWorkerRequest();
        }
    }
}