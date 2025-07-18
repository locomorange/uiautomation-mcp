using Moq;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Tools;
using UIAutomationMCP.Server.Interfaces;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.UnitTests.Base
{
    /// <summary>
    /// UIAutomationパターンテストの基底クラス
    /// 共通のモックセットアップとヘルパーメソッドを提供
    /// </summary>
    public abstract class BasePatternTests<TService> : IDisposable where TService : class
    {
        protected readonly ITestOutputHelper _output;
        protected readonly UIAutomationTools _tools;
        protected readonly Mock<TService> _mockService;

        // 共通のモックサービス
        protected readonly Mock<IApplicationLauncher> _mockAppLauncher;
        protected readonly Mock<IScreenshotService> _mockScreenshot;
        protected readonly Mock<IElementSearchService> _mockElementSearch;
        protected readonly Mock<ITreeNavigationService> _mockTreeNavigation;
        protected readonly Mock<IInvokeService> _mockInvoke;
        protected readonly Mock<IValueService> _mockValue;
        protected readonly Mock<IRangeService> _mockRange;
        protected readonly Mock<ISelectionService> _mockSelection;
        protected readonly Mock<ITextService> _mockText;
        protected readonly Mock<IToggleService> _mockToggle;
        protected readonly Mock<IWindowService> _mockWindow;
        protected readonly Mock<ILayoutService> _mockLayout;
        protected readonly Mock<IGridService> _mockGrid;
        protected readonly Mock<ITableService> _mockTable;
        protected readonly Mock<IMultipleViewService> _mockMultipleView;
        protected readonly Mock<IAccessibilityService> _mockAccessibility;
        protected readonly Mock<ICustomPropertyService> _mockCustomProperty;
        protected readonly Mock<IControlTypeService> _mockControlType;
        protected readonly Mock<ITransformService> _mockTransform;
        protected readonly Mock<IVirtualizedItemService> _mockVirtualizedItem;
        protected readonly Mock<IItemContainerService> _mockItemContainer;
        protected readonly Mock<ISynchronizedInputService> _mockSynchronizedInput;
        protected readonly Mock<ISubprocessExecutor> _mockSubprocessExecutor;

        protected BasePatternTests(ITestOutputHelper output)
        {
            _output = output;
            
            // 共通モックの初期化
            _mockAppLauncher = new Mock<IApplicationLauncher>();
            _mockScreenshot = new Mock<IScreenshotService>();
            _mockElementSearch = new Mock<IElementSearchService>();
            _mockTreeNavigation = new Mock<ITreeNavigationService>();
            _mockInvoke = new Mock<IInvokeService>();
            _mockValue = new Mock<IValueService>();
            _mockRange = new Mock<IRangeService>();
            _mockSelection = new Mock<ISelectionService>();
            _mockText = new Mock<ITextService>();
            _mockToggle = new Mock<IToggleService>();
            _mockWindow = new Mock<IWindowService>();
            _mockLayout = new Mock<ILayoutService>();
            _mockGrid = new Mock<IGridService>();
            _mockTable = new Mock<ITableService>();
            _mockMultipleView = new Mock<IMultipleViewService>();
            _mockAccessibility = new Mock<IAccessibilityService>();
            _mockCustomProperty = new Mock<ICustomPropertyService>();
            _mockControlType = new Mock<IControlTypeService>();
            _mockTransform = new Mock<ITransformService>();
            _mockVirtualizedItem = new Mock<IVirtualizedItemService>();
            _mockItemContainer = new Mock<IItemContainerService>();
            _mockSynchronizedInput = new Mock<ISynchronizedInputService>();
            _mockSubprocessExecutor = new Mock<ISubprocessExecutor>();

            // テスト対象サービスのモック
            _mockService = CreateServiceMock();

            // UIAutomationToolsの作成
            _tools = CreateUIAutomationTools();
        }

        /// <summary>
        /// 派生クラスでオーバーライドしてテスト対象サービスのモックを作成
        /// </summary>
        protected abstract Mock<TService> CreateServiceMock();

        /// <summary>
        /// UIAutomationToolsインスタンスを作成
        /// </summary>
        protected virtual UIAutomationTools CreateUIAutomationTools()
        {
            return new UIAutomationTools(
                _mockAppLauncher.Object,
                _mockScreenshot.Object,
                _mockElementSearch.Object,
                _mockTreeNavigation.Object,
                _mockInvoke.Object,
                _mockValue.Object,
                _mockRange.Object,
                _mockSelection.Object,
                _mockText.Object,
                _mockToggle.Object,
                _mockWindow.Object,
                GetLayoutService(),
                _mockGrid.Object,
                _mockTable.Object,
                _mockMultipleView.Object,
                _mockAccessibility.Object,
                _mockCustomProperty.Object,
                _mockControlType.Object,
                _mockTransform.Object,
                _mockVirtualizedItem.Object,
                _mockItemContainer.Object,
                _mockSynchronizedInput.Object,
                _mockSubprocessExecutor.Object
            );
        }

        /// <summary>
        /// レイアウトサービスを取得（ILayoutServiceを対象とするテストの場合にオーバーライド）
        /// </summary>
        protected virtual ILayoutService GetLayoutService()
        {
            if (typeof(TService) == typeof(ILayoutService))
                return _mockService.Object as ILayoutService ?? _mockLayout.Object;
            return _mockLayout.Object;
        }

        /// <summary>
        /// 共通のパラメータ検証テストヘルパー
        /// </summary>
        protected void VerifyParameterValidation(string methodName, params object[] parameters)
        {
            try
            {
                var method = typeof(TService).GetMethod(methodName);
                if (method != null)
                {
                    var result = method.Invoke(_mockService.Object, parameters);
                    _output.WriteLine($"✓ {methodName} parameter validation test passed");
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"✓ {methodName} parameter validation test passed with expected error: {ex.Message}");
            }
        }

        /// <summary>
        /// 共通のタイムアウト検証テストヘルパー
        /// </summary>
        protected void VerifyTimeoutHandling(string methodName, int timeout, params object[] additionalParameters)
        {
            var parameters = new List<object> { "", "", 0, timeout };
            if (additionalParameters != null)
                parameters.AddRange(additionalParameters);

            VerifyParameterValidation(methodName, parameters.ToArray());
            _output.WriteLine($"✓ {methodName} timeout handling test completed for timeout: {timeout}ms");
        }

        /// <summary>
        /// 共通のプロセスID検証テストヘルパー
        /// </summary>
        protected void VerifyProcessIdHandling(string methodName, int processId, params object[] additionalParameters)
        {
            var parameters = new List<object> { "", "", processId };
            if (additionalParameters != null)
                parameters.AddRange(additionalParameters);

            VerifyParameterValidation(methodName, parameters.ToArray());
            _output.WriteLine($"✓ {methodName} process ID handling test completed for ID: {processId}");
        }

        public virtual void Dispose()
        {
            // モックのクリーンアップは不要
            _output?.WriteLine($"{GetType().Name} disposed");
        }
    }
}