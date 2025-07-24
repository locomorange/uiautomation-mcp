using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.EventMonitor
{
    /// <summary>
    /// MS Learn ベストプラクティスに従った一時的なイベント監視操作
    /// 指定期間中のイベントを収集して返す
    /// </summary>
    public class MonitorEventsOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<MonitorEventsOperation> _logger;
        private readonly List<EventData> _capturedEvents = new();
        private readonly object _eventLock = new();

        public MonitorEventsOperation(
            ElementFinderService elementFinderService,
            ILogger<MonitorEventsOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public async Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var request = JsonSerializationHelper.Deserialize<MonitorEventsRequest>(parametersJson);
                if (request == null)
                {
                    return new OperationResult 
                    { 
                        Success = false, 
                        Error = "Failed to deserialize MonitorEventsRequest",
                        Data = new EventMonitoringResult()
                    };
                }

                _logger.LogInformation($"Starting event monitoring for {string.Join(", ", request.EventTypes)} for {request.Duration} seconds");

                // MS Learn推奨: イベントハンドラは別スレッドで処理される
                var eventHandlers = new List<IDisposable>();
                
                try
                {
                    // 各イベントタイプに対してハンドラを登録
                    foreach (var eventType in request.EventTypes)
                    {
                        var handler = RegisterEventHandler(eventType, request);
                        if (handler != null)
                        {
                            eventHandlers.Add(handler);
                        }
                    }

                    // 指定期間待機
                    await Task.Delay(TimeSpan.FromSeconds(request.Duration));

                    var result = new EventMonitoringResult
                    {
                        Success = true,
                        EventType = string.Join(", ", request.EventTypes),
                        Duration = request.Duration,
                        ElementId = request.AutomationId,
                        WindowTitle = request.WindowTitle,
                        ProcessId = request.ProcessId,
                        CapturedEvents = new List<EventData>(_capturedEvents)
                    };

                    return new OperationResult 
                    { 
                        Success = true, 
                        Data = result
                    };
                }
                finally
                {
                    // MS Learn推奨: 必ずイベントハンドラを解除
                    foreach (var handler in eventHandlers)
                    {
                        handler?.Dispose();
                    }
                    _logger.LogInformation("All event handlers disposed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MonitorEvents operation failed");
                return new OperationResult 
                { 
                    Success = false, 
                    Error = $"MonitorEvents failed: {ex.Message}",
                    Data = new EventMonitoringResult()
                };
            }
        }

        /// <summary>
        /// MS Learnベストプラクティス: イベントタイプに応じた適切なハンドラ登録
        /// </summary>
        private IDisposable? RegisterEventHandler(string eventType, MonitorEventsRequest request)
        {
            try
            {
                AutomationElement? targetElement = null;

                // 特定要素の監視が指定されている場合
                if (!string.IsNullOrEmpty(request.AutomationId) || !string.IsNullOrEmpty(request.Name))
                {
                    targetElement = _elementFinderService.FindElement(
                        automationId: request.AutomationId,
                        name: request.Name,
                        controlType: request.ControlType,
                        windowTitle: request.WindowTitle,
                        processId: request.ProcessId);

                    if (targetElement == null)
                    {
                        _logger.LogWarning($"Target element not found for event monitoring: AutomationId={request.AutomationId}, Name={request.Name}");
                        // 要素が見つからなくてもデスクトップ全体の監視は続行
                    }
                }

                // MS Learn推奨: イベントタイプに応じて適切なハンドラを選択
                return eventType.ToLower() switch
                {
                    "focus" => RegisterFocusChangedHandler(targetElement ?? AutomationElement.RootElement),
                    "invoke" => RegisterInvokeHandler(targetElement ?? AutomationElement.RootElement),
                    "selection" => RegisterSelectionHandler(targetElement ?? AutomationElement.RootElement),
                    "text" => RegisterTextChangedHandler(targetElement ?? AutomationElement.RootElement),
                    "property" => RegisterPropertyChangedHandler(targetElement ?? AutomationElement.RootElement),
                    "structure" => RegisterStructureChangedHandler(targetElement ?? AutomationElement.RootElement),
                    _ => RegisterGenericAutomationEventHandler(targetElement ?? AutomationElement.RootElement, eventType)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to register event handler for {eventType}");
                return null;
            }
        }

        /// <summary>
        /// MS Learn推奨: フォーカス変更イベントハンドラ
        /// </summary>
        private IDisposable RegisterFocusChangedHandler(AutomationElement element)
        {
            AutomationFocusChangedEventHandler handler = (sender, e) =>
            {
                lock (_eventLock)
                {
                    _capturedEvents.Add(new EventData
                    {
                        EventType = "Focus",
                        Timestamp = DateTime.UtcNow,
                        SourceElement = "Focus changed to element",
                        EventDataProperties = new Dictionary<string, object>()
                    });
                }
            };

            Automation.AddAutomationFocusChangedEventHandler(handler);

            return new ActionDisposable(() =>
            {
                try
                {
                    Automation.RemoveAutomationFocusChangedEventHandler(handler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove focus changed event handler");
                }
            });
        }

        /// <summary>
        /// MS Learn推奨: Invokeイベントハンドラ
        /// </summary>
        private IDisposable RegisterInvokeHandler(AutomationElement element)
        {
            AutomationEventHandler handler = (sender, e) =>
            {
                lock (_eventLock)
                {
                    _capturedEvents.Add(new EventData
                    {
                        EventType = "Invoke",
                        Timestamp = DateTime.UtcNow,
                        SourceElement = GetElementDescription(sender as AutomationElement),
                        EventDataProperties = new Dictionary<string, object>
                        {
                            ["EventId"] = e.EventId.ProgrammaticName
                        }
                    });
                }
            };

            Automation.AddAutomationEventHandler(
                InvokePattern.InvokedEvent,
                element,
                TreeScope.Subtree,
                handler);

            return new ActionDisposable(() =>
            {
                try
                {
                    Automation.RemoveAutomationEventHandler(
                        InvokePattern.InvokedEvent,
                        element,
                        handler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove invoke event handler");
                }
            });
        }

        /// <summary>
        /// MS Learn推奨: 選択イベントハンドラ
        /// </summary>
        private IDisposable RegisterSelectionHandler(AutomationElement element)
        {
            AutomationEventHandler handler = (sender, e) =>
            {
                lock (_eventLock)
                {
                    _capturedEvents.Add(new EventData
                    {
                        EventType = "Selection",
                        Timestamp = DateTime.UtcNow,
                        SourceElement = GetElementDescription(sender as AutomationElement),
                        EventDataProperties = new Dictionary<string, object>
                        {
                            ["EventId"] = e.EventId.ProgrammaticName
                        }
                    });
                }
            };

            // SelectionItemPattern.ElementSelectedEvent
            Automation.AddAutomationEventHandler(
                SelectionItemPattern.ElementSelectedEvent,
                element,
                TreeScope.Subtree,
                handler);

            return new ActionDisposable(() =>
            {
                try
                {
                    Automation.RemoveAutomationEventHandler(
                        SelectionItemPattern.ElementSelectedEvent,
                        element,
                        handler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove selection event handler");
                }
            });
        }

        /// <summary>
        /// MS Learn推奨: テキスト変更イベントハンドラ
        /// </summary>
        private IDisposable RegisterTextChangedHandler(AutomationElement element)
        {
            AutomationEventHandler handler = (sender, e) =>
            {
                lock (_eventLock)
                {
                    _capturedEvents.Add(new EventData
                    {
                        EventType = "Text",
                        Timestamp = DateTime.UtcNow,
                        SourceElement = GetElementDescription(sender as AutomationElement),
                        EventDataProperties = new Dictionary<string, object>
                        {
                            ["EventId"] = e.EventId.ProgrammaticName
                        }
                    });
                }
            };

            Automation.AddAutomationEventHandler(
                TextPattern.TextChangedEvent,
                element,
                TreeScope.Subtree,
                handler);

            return new ActionDisposable(() =>
            {
                try
                {
                    Automation.RemoveAutomationEventHandler(
                        TextPattern.TextChangedEvent,
                        element,
                        handler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove text changed event handler");
                }
            });
        }

        /// <summary>
        /// MS Learn推奨: プロパティ変更イベントハンドラ
        /// </summary>
        private IDisposable RegisterPropertyChangedHandler(AutomationElement element)
        {
            AutomationPropertyChangedEventHandler handler = (sender, e) =>
            {
                lock (_eventLock)
                {
                    _capturedEvents.Add(new EventData
                    {
                        EventType = "Property",
                        Timestamp = DateTime.UtcNow,
                        SourceElement = GetElementDescription(sender as AutomationElement),
                        EventDataProperties = new Dictionary<string, object>
                        {
                            ["PropertyId"] = e.Property.ProgrammaticName,
                            ["NewValue"] = e.NewValue?.ToString() ?? "null",
                            ["OldValue"] = e.OldValue?.ToString() ?? "null"
                        }
                    });
                }
            };

            // 主要なプロパティを監視
            var properties = new[]
            {
                AutomationElement.NameProperty,
                AutomationElement.IsEnabledProperty,
                ValuePattern.ValueProperty,
                TogglePattern.ToggleStateProperty
            };

            Automation.AddAutomationPropertyChangedEventHandler(
                element,
                TreeScope.Subtree,
                handler,
                properties);

            return new ActionDisposable(() =>
            {
                try
                {
                    Automation.RemoveAutomationPropertyChangedEventHandler(
                        element,
                        handler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove property changed event handler");
                }
            });
        }

        /// <summary>
        /// MS Learn推奨: 構造変更イベントハンドラ
        /// </summary>
        private IDisposable RegisterStructureChangedHandler(AutomationElement element)
        {
            StructureChangedEventHandler handler = (sender, e) =>
            {
                lock (_eventLock)
                {
                    _capturedEvents.Add(new EventData
                    {
                        EventType = "Structure",
                        Timestamp = DateTime.UtcNow,
                        SourceElement = GetElementDescription(sender as AutomationElement),
                        EventDataProperties = new Dictionary<string, object>
                        {
                            ["StructureChangeType"] = e.StructureChangeType.ToString(),
                            ["RuntimeId"] = e.GetRuntimeId()?.ToArray() ?? Array.Empty<int>()
                        }
                    });
                }
            };

            Automation.AddStructureChangedEventHandler(
                element,
                TreeScope.Subtree,
                handler);

            return new ActionDisposable(() =>
            {
                try
                {
                    Automation.RemoveStructureChangedEventHandler(
                        element,
                        handler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove structure changed event handler");
                }
            });
        }

        /// <summary>
        /// その他のAutomationEventの汎用ハンドラ
        /// </summary>
        private IDisposable? RegisterGenericAutomationEventHandler(AutomationElement element, string eventType)
        {
            // 既知のイベントタイプのマッピング
            var eventMap = new Dictionary<string, AutomationEvent>
            {
                ["window.opened"] = WindowPattern.WindowOpenedEvent,
                ["window.closed"] = WindowPattern.WindowClosedEvent,
                ["menu.opened"] = AutomationElement.MenuOpenedEvent,
                ["menu.closed"] = AutomationElement.MenuClosedEvent,
                ["tooltip.opened"] = AutomationElement.ToolTipOpenedEvent,
                ["tooltip.closed"] = AutomationElement.ToolTipClosedEvent
            };

            if (!eventMap.TryGetValue(eventType.ToLower(), out var automationEvent))
            {
                _logger.LogWarning($"Unknown event type: {eventType}");
                return null;
            }

            AutomationEventHandler handler = (sender, e) =>
            {
                lock (_eventLock)
                {
                    _capturedEvents.Add(new EventData
                    {
                        EventType = eventType,
                        Timestamp = DateTime.UtcNow,
                        SourceElement = GetElementDescription(sender as AutomationElement),
                        EventDataProperties = new Dictionary<string, object>
                        {
                            ["EventId"] = e.EventId.ProgrammaticName
                        }
                    });
                }
            };

            Automation.AddAutomationEventHandler(
                automationEvent,
                element,
                TreeScope.Subtree,
                handler);

            return new ActionDisposable(() =>
            {
                try
                {
                    Automation.RemoveAutomationEventHandler(
                        automationEvent,
                        element,
                        handler);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to remove {eventType} event handler");
                }
            });
        }

        /// <summary>
        /// 要素の説明文を取得
        /// </summary>
        private string GetElementDescription(AutomationElement? element)
        {
            if (element == null) return "Unknown element";

            try
            {
                var name = element.GetCurrentPropertyValue(AutomationElement.NameProperty)?.ToString() ?? "";
                var controlType = element.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty)?.ToString() ?? "";
                var automationId = element.GetCurrentPropertyValue(AutomationElement.AutomationIdProperty)?.ToString() ?? "";
                
                return $"{controlType} '{name}' (AutomationId: {automationId})";
            }
            catch
            {
                return "Element description unavailable";
            }
        }
    }

    /// <summary>
    /// IDisposableの汎用実装
    /// </summary>
    internal class ActionDisposable : IDisposable
    {
        private readonly Action _disposeAction;
        private bool _disposed = false;

        public ActionDisposable(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposeAction?.Invoke();
                _disposed = true;
            }
        }
    }
}
