using Microsoft.Extensions.Logging;
using System.Windows.Automation;
using UiAutomationMcpServer.Services;
using UiAutomationMcpServer.Helpers;

namespace UiAutomationMcpServer.Tools
{
    /// <summary>
    /// アプリケーションの状態遷移を追跡・記録するためのクラス
    /// 電卓アプリなどの状態探索で使用
    /// </summary>
    public class StateTransitionTracker
    {
        private readonly ILogger<StateTransitionTracker> _logger;
        private readonly DisplayValueExtractor _displayValueExtractor;
        
        private readonly Dictionary<string, AppState> _knownStates = new();
        private readonly List<StateTransition> _transitions = new();
        private string? _currentStateId;

        public StateTransitionTracker(
            ILogger<StateTransitionTracker> logger,
            DisplayValueExtractor displayValueExtractor)
        {
            _logger = logger;
            _displayValueExtractor = displayValueExtractor;
        }

        /// <summary>
        /// 現在のアプリケーション状態をキャプチャ
        /// </summary>
        public async Task<string> CaptureCurrentStateAsync(
            AutomationElement rootElement,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("[StateTracker] Capturing current application state");

                var state = new AppState
                {
                    Timestamp = DateTime.UtcNow,
                    DisplayElements = await CaptureDisplayElementsAsync(rootElement, cancellationToken),
                    ButtonStates = await CaptureButtonStatesAsync(rootElement, cancellationToken),
                    MenuStates = await CaptureMenuStatesAsync(rootElement, cancellationToken)
                };

                var stateId = ComputeStateId(state);
                
                if (!_knownStates.ContainsKey(stateId))
                {
                    _knownStates[stateId] = state;
                    _logger.LogInformation("[StateTracker] New state discovered: {StateId}", stateId);
                }
                else
                {
                    _logger.LogDebug("[StateTracker] Known state: {StateId}", stateId);
                }

                _currentStateId = stateId;
                return stateId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[StateTracker] Failed to capture application state");
                throw;
            }
        }

        /// <summary>
        /// 状態遷移を記録
        /// </summary>
        public void RecordTransition(string fromStateId, string toStateId, string action, bool successful)
        {
            var transition = new StateTransition
            {
                FromStateId = fromStateId,
                ToStateId = toStateId,
                Action = action,
                Successful = successful,
                Timestamp = DateTime.UtcNow
            };

            _transitions.Add(transition);
            
            _logger.LogInformation("[StateTracker] Transition recorded: {FromState} --[{Action}]--> {ToState} (Success: {Success})",
                fromStateId, action, toStateId, successful);
        }

        /// <summary>
        /// 表示要素の状態をキャプチャ
        /// </summary>
        private async Task<Dictionary<string, string>> CaptureDisplayElementsAsync(
            AutomationElement rootElement,
            CancellationToken cancellationToken)
        {
            var displayElements = new Dictionary<string, string>();

            try
            {
                // 電卓の表示部を探す
                var displayCondition = new OrCondition(
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "CalculatorResults"),
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "Display"),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text)
                );

                var displays = await rootElement.FindAllAsync(TreeScope.Descendants, displayCondition, cancellationToken);

                foreach (AutomationElement display in displays)
                {
                    try
                    {
                        var automationId = await display.GetCurrentPropertyAsync<string>(
                            AutomationElement.AutomationIdProperty, cancellationToken);
                        
                        var (success, value, method) = await _displayValueExtractor.ExtractDisplayValueAsync(display, cancellationToken);
                        
                        if (success && !string.IsNullOrEmpty(value))
                        {
                            var key = !string.IsNullOrEmpty(automationId) ? automationId : "UnknownDisplay";
                            displayElements[key] = value;
                            _logger.LogDebug("[StateTracker] Display element captured: {Key} = {Value} (Method: {Method})", 
                                key, value, method);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "[StateTracker] Failed to capture display element");
                    }
                }

                // 履歴やメモリ表示も探す
                var historyCondition = new OrCondition(
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "History"),
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "Memory"),
                    new PropertyCondition(AutomationElement.NameProperty, "履歴"),
                    new PropertyCondition(AutomationElement.NameProperty, "メモリ")
                );

                var historyElements = await rootElement.FindAllAsync(TreeScope.Descendants, historyCondition, cancellationToken);

                foreach (AutomationElement historyElement in historyElements)
                {
                    try
                    {
                        var name = await historyElement.GetNameAsync(cancellationToken);
                        var (success, value, method) = await _displayValueExtractor.ExtractDisplayValueAsync(historyElement, cancellationToken);
                        
                        if (success && !string.IsNullOrEmpty(value))
                        {
                            displayElements[$"History_{name}"] = value;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "[StateTracker] Failed to capture history element");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[StateTracker] Failed to capture display elements");
            }

            return displayElements;
        }

        /// <summary>
        /// ボタンの状態をキャプチャ
        /// </summary>
        private async Task<Dictionary<string, ButtonState>> CaptureButtonStatesAsync(
            AutomationElement rootElement,
            CancellationToken cancellationToken)
        {
            var buttonStates = new Dictionary<string, ButtonState>();

            try
            {
                var buttonCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button);
                var buttons = await rootElement.FindAllAsync(TreeScope.Descendants, buttonCondition, cancellationToken);

                foreach (AutomationElement button in buttons)
                {
                    try
                    {
                        var automationId = await button.GetCurrentPropertyAsync<string>(
                            AutomationElement.AutomationIdProperty, cancellationToken);
                        
                        if (string.IsNullOrEmpty(automationId))
                            continue;

                        var isEnabled = await button.GetCurrentPropertyAsync<bool>(
                            AutomationElement.IsEnabledProperty, cancellationToken);
                        
                        var isOffscreen = await button.GetCurrentPropertyAsync<bool>(
                            AutomationElement.IsOffscreenProperty, cancellationToken);

                        var name = await button.GetNameAsync(cancellationToken);

                        buttonStates[automationId] = new ButtonState
                        {
                            AutomationId = automationId,
                            Name = name,
                            IsEnabled = isEnabled,
                            IsOffscreen = isOffscreen
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "[StateTracker] Failed to capture button state");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[StateTracker] Failed to capture button states");
            }

            return buttonStates;
        }

        /// <summary>
        /// メニューの状態をキャプチャ
        /// </summary>
        private async Task<Dictionary<string, MenuState>> CaptureMenuStatesAsync(
            AutomationElement rootElement,
            CancellationToken cancellationToken)
        {
            var menuStates = new Dictionary<string, MenuState>();

            try
            {
                var menuCondition = new OrCondition(
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Menu),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.MenuBar),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.MenuItem)
                );

                var menus = await rootElement.FindAllAsync(TreeScope.Descendants, menuCondition, cancellationToken);

                foreach (AutomationElement menu in menus)
                {
                    try
                    {
                        var automationId = await menu.GetCurrentPropertyAsync<string>(
                            AutomationElement.AutomationIdProperty, cancellationToken);
                        
                        if (string.IsNullOrEmpty(automationId))
                            continue;

                        var name = await menu.GetNameAsync(cancellationToken);
                        var isEnabled = await menu.GetCurrentPropertyAsync<bool>(
                            AutomationElement.IsEnabledProperty, cancellationToken);

                        menuStates[automationId] = new MenuState
                        {
                            AutomationId = automationId,
                            Name = name,
                            IsEnabled = isEnabled
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "[StateTracker] Failed to capture menu state");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[StateTracker] Failed to capture menu states");
            }

            return menuStates;
        }

        /// <summary>
        /// 状態IDを計算（状態の一意性判定用）
        /// </summary>
        private string ComputeStateId(AppState state)
        {
            var stateData = new
            {
                Display = state.DisplayElements,
                EnabledButtons = state.ButtonStates.Where(kvp => kvp.Value.IsEnabled).Select(kvp => kvp.Key).OrderBy(x => x),
                EnabledMenus = state.MenuStates.Where(kvp => kvp.Value.IsEnabled).Select(kvp => kvp.Key).OrderBy(x => x)
            };

            var json = System.Text.Json.JsonSerializer.Serialize(stateData);
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(json));
            return Convert.ToHexString(hashBytes)[..16]; // 最初の16文字を使用
        }

        /// <summary>
        /// 状態遷移レポートを生成
        /// </summary>
        public object GenerateTransitionReport()
        {
            return new
            {
                TotalStates = _knownStates.Count,
                TotalTransitions = _transitions.Count,
                SuccessfulTransitions = _transitions.Count(t => t.Successful),
                FailedTransitions = _transitions.Count(t => !t.Successful),
                States = _knownStates.Select(kvp => new 
                {
                    StateId = kvp.Key,
                    Timestamp = kvp.Value.Timestamp,
                    DisplayCount = kvp.Value.DisplayElements.Count,
                    ButtonCount = kvp.Value.ButtonStates.Count,
                    EnabledButtons = kvp.Value.ButtonStates.Count(b => b.Value.IsEnabled)
                }),
                RecentTransitions = _transitions.TakeLast(10).Select(t => new
                {
                    t.FromStateId,
                    t.ToStateId,
                    t.Action,
                    t.Successful,
                    t.Timestamp
                })
            };
        }
    }

    // データ構造
    public class AppState
    {
        public DateTime Timestamp { get; set; }
        public Dictionary<string, string> DisplayElements { get; set; } = new();
        public Dictionary<string, ButtonState> ButtonStates { get; set; } = new();
        public Dictionary<string, MenuState> MenuStates { get; set; } = new();
    }

    public class ButtonState
    {
        public string AutomationId { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsEnabled { get; set; }
        public bool IsOffscreen { get; set; }
    }

    public class MenuState
    {
        public string AutomationId { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsEnabled { get; set; }
    }

    public class StateTransition
    {
        public string FromStateId { get; set; } = "";
        public string ToStateId { get; set; } = "";
        public string Action { get; set; } = "";
        public bool Successful { get; set; }
        public DateTime Timestamp { get; set; }
    }
}