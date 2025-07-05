using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationWorker.Helpers;

namespace UiAutomationWorker.PatternExecutors
{
    /// <summary>
    /// レイアウト関連パターン（ExpandCollapse、Scroll、Transform、Dock）を実行するクラス
    /// </summary>
    public class LayoutPatternExecutor : BasePatternExecutor
    {
        public LayoutPatternExecutor(
            ILogger<LayoutPatternExecutor> logger,
            AutomationHelper automationHelper)
            : base(logger, automationHelper)
        {
        }

        /// <summary>
        /// ExpandCollapse操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteExpandCollapseAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("ExpandCollapse");
                }

                // ExpandCollapsePatternの取得
                if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var patternObj) && 
                    patternObj is ExpandCollapsePattern expandCollapsePattern)
                {
                    var currentState = expandCollapsePattern.Current.ExpandCollapseState;

                    // expand パラメータの処理
                    var shouldExpand = true; // デフォルトは展開
                    if (operation.Parameters.TryGetValue("expand", out var expandObj))
                    {
                        if (expandObj is bool expandBool)
                        {
                            shouldExpand = expandBool;
                        }
                        else if (bool.TryParse(expandObj?.ToString(), out var expandParsed))
                        {
                            shouldExpand = expandParsed;
                        }
                        else
                        {
                            // トグル動作（現在の状態と逆にする）
                            shouldExpand = currentState != ExpandCollapseState.Expanded;
                        }
                    }
                    else
                    {
                        // expand が指定されていない場合もトグル動作
                        shouldExpand = currentState != ExpandCollapseState.Expanded;
                    }

                    _logger.LogInformation("[LayoutPatternExecutor] ExpandCollapse - Element: {ElementName}, Current: {CurrentState}, Target: {TargetAction}", 
                        SafeGetElementName(element), currentState, shouldExpand ? "Expand" : "Collapse");

                    if (shouldExpand && currentState != ExpandCollapseState.Expanded)
                    {
                        expandCollapsePattern.Expand();
                        return new WorkerResult
                        {
                            Success = true,
                            Data = "Element expanded successfully"
                        };
                    }
                    else if (!shouldExpand && currentState != ExpandCollapseState.Collapsed)
                    {
                        expandCollapsePattern.Collapse();
                        return new WorkerResult
                        {
                            Success = true,
                            Data = "Element collapsed successfully"
                        };
                    }
                    else
                    {
                        return new WorkerResult
                        {
                            Success = true,
                            Data = $"Element is already in the desired state: {currentState}"
                        };
                    }
                }
                else
                {
                    return CreatePatternNotSupportedResult("ExpandCollapsePattern");
                }
            }, "ExpandCollapse");
        }

        /// <summary>
        /// Scroll操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteScrollAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("Scroll");
                }

                // ScrollPatternの取得
                if (element.TryGetCurrentPattern(ScrollPattern.Pattern, out var patternObj) && 
                    patternObj is ScrollPattern scrollPattern)
                {
                    // direction パラメータの処理
                    if (operation.Parameters.TryGetValue("direction", out var directionObj) && 
                        directionObj?.ToString() is string direction && !string.IsNullOrEmpty(direction))
                    {
                        return ExecuteScrollByDirection(scrollPattern, direction);
                    }

                    // horizontal/vertical パラメータの処理
                    double horizontal = 0, vertical = 0;
                    var hasHorizontal = operation.Parameters.TryGetValue("horizontal", out var horizontalObj) && 
                                        double.TryParse(horizontalObj?.ToString(), out horizontal);
                    var hasVertical = operation.Parameters.TryGetValue("vertical", out var verticalObj) && 
                                    double.TryParse(verticalObj?.ToString(), out vertical);

                    if (hasHorizontal || hasVertical)
                    {
                        var horizontalPercent = hasHorizontal ? Math.Clamp(horizontal, 0, 100) : scrollPattern.Current.HorizontalScrollPercent;
                        var verticalPercent = hasVertical ? Math.Clamp(vertical, 0, 100) : scrollPattern.Current.VerticalScrollPercent;

                        scrollPattern.SetScrollPercent(horizontalPercent, verticalPercent);

                        return new WorkerResult
                        {
                            Success = true,
                            Data = $"Scrolled to horizontal: {horizontalPercent}%, vertical: {verticalPercent}%"
                        };
                    }

                    return new WorkerResult
                    {
                        Success = false,
                        Error = "No scroll parameters specified. Use 'direction' or 'horizontal'/'vertical' parameters."
                    };
                }
                else
                {
                    return CreatePatternNotSupportedResult("ScrollPattern");
                }
            }, "Scroll");
        }

        /// <summary>
        /// ScrollIntoView操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteScrollIntoViewAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("ScrollIntoView");
                }

                // ScrollItemPatternの取得
                if (element.TryGetCurrentPattern(ScrollItemPattern.Pattern, out var patternObj) && 
                    patternObj is ScrollItemPattern scrollItemPattern)
                {
                    _logger.LogInformation("[LayoutPatternExecutor] Scrolling element into view: {ElementName}", 
                        SafeGetElementName(element));

                    scrollItemPattern.ScrollIntoView();

                    return new WorkerResult
                    {
                        Success = true,
                        Data = "Element scrolled into view successfully"
                    };
                }
                else
                {
                    return CreatePatternNotSupportedResult("ScrollItemPattern");
                }
            }, "ScrollIntoView");
        }

        /// <summary>
        /// Transform操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteTransformAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("Transform");
                }

                // TransformPatternの取得
                if (element.TryGetCurrentPattern(TransformPattern.Pattern, out var patternObj) && 
                    patternObj is TransformPattern transformPattern)
                {
                    // action パラメータの取得
                    if (!operation.Parameters.TryGetValue("action", out var actionObj) || 
                        actionObj?.ToString() is not string action || string.IsNullOrEmpty(action))
                    {
                        return CreateParameterMissingResult("Action", "Transform");
                    }

                    return action.ToLowerInvariant() switch
                    {
                        "move" => ExecuteTransformMove(transformPattern, operation),
                        "resize" => ExecuteTransformResize(transformPattern, operation),
                        "rotate" => ExecuteTransformRotate(transformPattern, operation),
                        _ => new WorkerResult
                        {
                            Success = false,
                            Error = $"Unknown transform action: {action}. Supported actions: move, resize, rotate"
                        }
                    };
                }
                else
                {
                    return CreatePatternNotSupportedResult("TransformPattern");
                }
            }, "Transform");
        }

        /// <summary>
        /// Dock操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteDockAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("Dock");
                }

                // DockPatternの取得
                if (element.TryGetCurrentPattern(DockPattern.Pattern, out var patternObj) && 
                    patternObj is DockPattern dockPattern)
                {
                    // position パラメータの取得
                    if (!operation.Parameters.TryGetValue("position", out var positionObj) || 
                        positionObj?.ToString() is not string position || string.IsNullOrEmpty(position))
                    {
                        return CreateParameterMissingResult("Position", "Dock");
                    }

                    var dockPosition = position.ToLowerInvariant() switch
                    {
                        "top" => DockPosition.Top,
                        "bottom" => DockPosition.Bottom,
                        "left" => DockPosition.Left,
                        "right" => DockPosition.Right,
                        "fill" => DockPosition.Fill,
                        "none" => DockPosition.None,
                        _ => (DockPosition?)null
                    };

                    if (dockPosition == null)
                    {
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"Invalid dock position: {position}. Supported positions: top, bottom, left, right, fill, none"
                        };
                    }

                    _logger.LogInformation("[LayoutPatternExecutor] Docking element: {ElementName} to position: {Position}", 
                        SafeGetElementName(element), dockPosition);

                    dockPattern.SetDockPosition(dockPosition.Value);

                    return new WorkerResult
                    {
                        Success = true,
                        Data = $"Element docked to {dockPosition} position successfully"
                    };
                }
                else
                {
                    return CreatePatternNotSupportedResult("DockPattern");
                }
            }, "Dock");
        }

        #region Private Helper Methods


        /// <summary>
        /// 方向指定によるスクロール
        /// </summary>
        private WorkerResult ExecuteScrollByDirection(ScrollPattern scrollPattern, string direction)
        {
            try
            {
                switch (direction.ToLowerInvariant())
                {
                    case "up":
                        scrollPattern.ScrollVertical(ScrollAmount.SmallDecrement);
                        return new WorkerResult { Success = true, Data = "Scrolled up" };

                    case "down":
                        scrollPattern.ScrollVertical(ScrollAmount.SmallIncrement);
                        return new WorkerResult { Success = true, Data = "Scrolled down" };

                    case "left":
                        scrollPattern.ScrollHorizontal(ScrollAmount.SmallDecrement);
                        return new WorkerResult { Success = true, Data = "Scrolled left" };

                    case "right":
                        scrollPattern.ScrollHorizontal(ScrollAmount.SmallIncrement);
                        return new WorkerResult { Success = true, Data = "Scrolled right" };

                    default:
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"Invalid scroll direction: {direction}. Supported directions: up, down, left, right"
                        };
                }
            }
            catch (Exception ex)
            {
                return new WorkerResult
                {
                    Success = false,
                    Error = $"Scroll by direction failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 移動変換の実行
        /// </summary>
        private WorkerResult ExecuteTransformMove(TransformPattern transformPattern, WorkerOperation operation)
        {
            try
            {
                double x = 0, y = 0;
                var hasX = operation.Parameters.TryGetValue("x", out var xObj) && double.TryParse(xObj?.ToString(), out x);
                var hasY = operation.Parameters.TryGetValue("y", out var yObj) && double.TryParse(yObj?.ToString(), out y);

                if (!hasX || !hasY)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "Both x and y parameters are required for move operation"
                    };
                }

                if (!transformPattern.Current.CanMove)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "Element cannot be moved"
                    };
                }

                transformPattern.Move(x, y);
                return new WorkerResult
                {
                    Success = true,
                    Data = $"Element moved to position ({x}, {y})"
                };
            }
            catch (Exception ex)
            {
                return new WorkerResult
                {
                    Success = false,
                    Error = $"Move operation failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// リサイズ変換の実行
        /// </summary>
        private WorkerResult ExecuteTransformResize(TransformPattern transformPattern, WorkerOperation operation)
        {
            try
            {
                double width = 0, height = 0;
                var hasWidth = operation.Parameters.TryGetValue("width", out var widthObj) && double.TryParse(widthObj?.ToString(), out width);
                var hasHeight = operation.Parameters.TryGetValue("height", out var heightObj) && double.TryParse(heightObj?.ToString(), out height);

                if (!hasWidth || !hasHeight)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "Both width and height parameters are required for resize operation"
                    };
                }

                if (!transformPattern.Current.CanResize)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "Element cannot be resized"
                    };
                }

                transformPattern.Resize(width, height);
                return new WorkerResult
                {
                    Success = true,
                    Data = $"Element resized to {width} x {height}"
                };
            }
            catch (Exception ex)
            {
                return new WorkerResult
                {
                    Success = false,
                    Error = $"Resize operation failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 回転変換の実行
        /// </summary>
        private WorkerResult ExecuteTransformRotate(TransformPattern transformPattern, WorkerOperation operation)
        {
            try
            {
                double degrees = 0;
                var hasDegrees = operation.Parameters.TryGetValue("degrees", out var degreesObj) && double.TryParse(degreesObj?.ToString(), out degrees);

                if (!hasDegrees)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "Degrees parameter is required for rotate operation"
                    };
                }

                if (!transformPattern.Current.CanRotate)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "Element cannot be rotated"
                    };
                }

                transformPattern.Rotate(degrees);
                return new WorkerResult
                {
                    Success = true,
                    Data = $"Element rotated by {degrees} degrees"
                };
            }
            catch (Exception ex)
            {
                return new WorkerResult
                {
                    Success = false,
                    Error = $"Rotate operation failed: {ex.Message}"
                };
            }
        }


        #endregion
    }
}
