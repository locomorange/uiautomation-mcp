using UIAutomationMCP.Shared.Options;

namespace UIAutomationMCP.Shared.Requests
{
    /// <summary>
    /// TypedWorkerRequestに設定デフォルト値を適用するためのクラス
    /// </summary>
    public static class RequestConfigurationDefaults
    {
        /// <summary>
        /// 設定値をTypedRequestに適用
        /// </summary>
        public static void ApplyConfigurationDefaults(TypedWorkerRequest typedRequest, UIAutomationOptions options)
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
    }
}