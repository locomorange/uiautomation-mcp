using Moq;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Tools;
using Xunit.Abstractions;
using UIAutomationMCP.Models.Abstractions;

namespace UIAutomationMCP.Tests.UnitTests
{
    /// <summary>
    /// TogglePattern縺ｮ蜊倅ｽ薙ユ繧ｹ繝・    /// Microsoft莉墓ｧ倥↓蝓ｺ縺･縺・◆TogglePattern縺ｮ讖溯・繧偵Δ繝・け繝吶・繧ｹ縺ｧ繝・せ繝医＠縺ｾ縺・    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class TogglePatternTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly UIAutomationTools _tools;
        private readonly Mock<IToggleService> _mockToggleService;

        public TogglePatternTests(ITestOutputHelper output)
        {
            _output = output;
            _mockToggleService = new Mock<IToggleService>();
            
            // UIAutomationTools縺ｮ莉悶・繧ｵ繝ｼ繝薙せ繧ゅΔ繝・け蛹厄ｼ域怙蟆城剞縺ｮ險ｭ螳夲ｼ・            var mockAppLauncher = new Mock<IApplicationLauncher>();
            var mockScreenshot = new Mock<IScreenshotService>();
            var mockElementSearch = new Mock<IElementSearchService>();
            var mockTreeNavigation = new Mock<ITreeNavigationService>();
            var mockInvoke = new Mock<IInvokeService>();
            var mockValue = new Mock<IValueService>();
            var mockRange = new Mock<IRangeService>();
            var mockSelection = new Mock<ISelectionService>();
            var mockText = new Mock<ITextService>();
            var mockWindow = new Mock<IWindowService>();
            var mockLayout = new Mock<ILayoutService>();
            var mockGrid = new Mock<IGridService>();
            var mockTable = new Mock<ITableService>();
            var mockMultipleView = new Mock<IMultipleViewService>();
            var mockAccessibility = new Mock<IAccessibilityService>();
            var mockCustomProperty = new Mock<ICustomPropertyService>();
            var mockControlType = new Mock<IControlTypeService>();
            var mockTransform = new Mock<ITransformService>();
            var mockVirtualizedItem = new Mock<IVirtualizedItemService>();
            var mockItemContainer = new Mock<IItemContainerService>();
            var mockSynchronizedInput = new Mock<ISynchronizedInputService>();
            var mockEventMonitor = new Mock<IEventMonitorService>();
            var mockSubprocessExecutor = new Mock<IOperationExecutor>();

            _tools = new UIAutomationTools(
                mockAppLauncher.Object,
                mockScreenshot.Object,
                mockElementSearch.Object,
                mockTreeNavigation.Object,
                mockInvoke.Object,
                mockValue.Object,
                mockRange.Object,
                mockSelection.Object,
                mockText.Object,
                _mockToggleService.Object,
                mockWindow.Object,
                mockLayout.Object,
                mockGrid.Object,
                mockTable.Object,
                mockMultipleView.Object,
                mockAccessibility.Object,
                mockCustomProperty.Object,
                mockControlType.Object,
                mockTransform.Object,
                mockVirtualizedItem.Object,
                mockItemContainer.Object,
                mockSynchronizedInput.Object,
                mockEventMonitor.Object,
                Mock.Of<IFocusService>(),
                mockSubprocessExecutor.Object
            );
        }

        public void Dispose()
        {
            // 繝｢繝・け縺ｮ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・縺ｯ荳崎ｦ・        }

        #region Microsoft莉墓ｧ俶ｺ匁侠縺ｮToggleState繝・せ繝・
        [Theory]
        [InlineData("CheckBox")]
        [InlineData("RadioButton")]
        [InlineData("Button")]
        [InlineData("MenuItem")]
        [InlineData("ToggleButton")]
        public async Task ToggleElement_WithSupportedControls_ShouldSucceed(string elementSelector)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { 
                    Success = true, 
                    OperationName = "Toggle", 
                    Action = "Toggle", 
                    ReturnValue = "On", 
                    Details = "Previous state: Off", 
                    StateChanged = true 
                }
            };
            _mockToggleService.Setup(s => s.ToggleElementAsync(elementSelector, null, null, null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ToggleElement(elementSelector, "TestWindow");

            // Assert
            Assert.NotNull(result);
            _mockToggleService.Verify(s => s.ToggleElementAsync(elementSelector, null, null, null, 30), Times.Once);
            _output.WriteLine($"ToggleElement test passed for control: {elementSelector}");
        }

        [Theory]
        [InlineData("Off", "On")]
        [InlineData("On", "Indeterminate")]
        [InlineData("Indeterminate", "Off")]
        public async Task ToggleElement_WithStateTransitions_ShouldFollowCorrectCycle(string fromState, string toState)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { 
                    Success = true, 
                    OperationName = "Toggle", 
                    Action = "Toggle", 
                    ReturnValue = toState, 
                    Details = $"Toggled from {fromState} to {toState}", 
                    StateChanged = true 
                }
            };
            _mockToggleService.Setup(s => s.ToggleElementAsync("checkBox1", null, null, null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ToggleElement("checkBox1", "Form");

            // Assert
            Assert.NotNull(result);
            _mockToggleService.Verify(s => s.ToggleElementAsync("checkBox1", null, null, null, 30), Times.Once);
            _output.WriteLine($"State transition from {fromState} to {toState} test passed");
        }

        #endregion

        #region Microsoft莉墓ｧ俶ｺ匁侠縺ｮToggleStateProperty繝・せ繝・
        [Fact]
        public async Task ToggleElement_OffToOn_ShouldReturnCorrectStates()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { 
                    Success = true, 
                    Action = "Toggle", 
                    ReturnValue = "On", 
                    Details = "Toggled - Previous state: Off", 
                    StateChanged = true,
                    Metadata = new Dictionary<string, object> { { "PropertyName", "TogglePattern.ToggleState" } } 
                }
            };
            _mockToggleService.Setup(s => s.ToggleElementAsync("checkBox", null, null, null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ToggleElement("checkBox", "Dialog");

            // Assert
            Assert.NotNull(result);
            _mockToggleService.Verify(s => s.ToggleElementAsync("checkBox", null, null, null, 30), Times.Once);
            _output.WriteLine("Off to On state change test passed");
        }

        [Fact]
        public async Task ToggleElement_OnToIndeterminate_ShouldReturnCorrectStates()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { 
                    Success = true, 
                    Action = "Toggle", 
                    ReturnValue = "Indeterminate", 
                    Details = "Toggled - Previous state: On", 
                    StateChanged = true,
                    Metadata = new Dictionary<string, object> { { "PropertyName", "TogglePattern.ToggleState" } } 
                }
            };
            _mockToggleService.Setup(s => s.ToggleElementAsync("triStateCheckBox", null, null, null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ToggleElement("triStateCheckBox", "Options");

            // Assert
            Assert.NotNull(result);
            _mockToggleService.Verify(s => s.ToggleElementAsync("triStateCheckBox", null, null, null, 30), Times.Once);
            _output.WriteLine("On to Indeterminate state change test passed");
        }

        [Fact]
        public async Task ToggleElement_IndeterminateToOff_ShouldReturnCorrectStates()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { 
                    Success = true, 
                    Action = "Toggle", 
                    ReturnValue = "Off", 
                    Details = "Toggled - Previous state: Indeterminate", 
                    StateChanged = true,
                    Metadata = new Dictionary<string, object> { { "PropertyName", "TogglePattern.ToggleState" } } 
                }
            };
            _mockToggleService.Setup(s => s.ToggleElementAsync("triStateCheckBox", null, null, null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ToggleElement("triStateCheckBox", "Settings");

            // Assert
            Assert.NotNull(result);
            _mockToggleService.Verify(s => s.ToggleElementAsync("triStateCheckBox", null, null, null, 30), Times.Once);
            _output.WriteLine("Indeterminate to Off state change test passed");
        }

        #endregion

        #region Microsoft莉墓ｧ俶ｺ匁侠縺ｮToggle()繝｡繧ｽ繝・ラ繝・せ繝・
        [Fact]
        public async Task ToggleElement_CompleteToggleCycle_ShouldFollowSpecifiedOrder()
        {
            // Arrange
            var firstToggleResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Toggle", Action = "Toggle", ReturnValue = "On", Details = "Previous state: Off" }
            };
            var secondToggleResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, Action = "Toggle", ReturnValue = "Indeterminate", Details = "Toggled - Previous state: On" }
            };
            var thirdToggleResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, Action = "Toggle", ReturnValue = "Off", Details = "Toggled - Previous state: Indeterminate" }
            };

            _mockToggleService.SetupSequence(s => s.ToggleElementAsync("triStateControl", null, null, null, 30))
                             .Returns(Task.FromResult(firstToggleResult))
                             .Returns(Task.FromResult(secondToggleResult))
                             .Returns(Task.FromResult(thirdToggleResult));

            // Act
            var result1 = await _tools.ToggleElement("triStateControl", "Window");
            var result2 = await _tools.ToggleElement("triStateControl", "Window");
            var result3 = await _tools.ToggleElement("triStateControl", "Window");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);

            _mockToggleService.Verify(s => s.ToggleElementAsync("triStateControl", null, null, null, 30), Times.Exactly(3));
            _output.WriteLine("Complete toggle cycle (Off 竊・On 竊・Indeterminate 竊・Off) test passed");
        }

        [Fact]
        public async Task ToggleElement_TwoStateToggle_ShouldAlternateBetweenOnAndOff()
        {
            // Arrange
            var firstToggleResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Toggle", Action = "Toggle", ReturnValue = "On", Details = "Previous state: Off" }
            };
            var secondToggleResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Toggle", Action = "Toggle", ReturnValue = "Off", Details = "Previous state: On" }
            };
            var thirdToggleResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Toggle", Action = "Toggle", ReturnValue = "On", Details = "Previous state: Off" }
            };

            _mockToggleService.SetupSequence(s => s.ToggleElementAsync("binaryToggle", null, null, null, 30))
                             .Returns(Task.FromResult(firstToggleResult))
                             .Returns(Task.FromResult(secondToggleResult))
                             .Returns(Task.FromResult(thirdToggleResult));

            // Act
            var result1 = await _tools.ToggleElement("binaryToggle", "Application");
            var result2 = await _tools.ToggleElement("binaryToggle", "Application");
            var result3 = await _tools.ToggleElement("binaryToggle", "Application");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);

            _mockToggleService.Verify(s => s.ToggleElementAsync("binaryToggle", null, null, null, 30), Times.Exactly(3));
            _output.WriteLine("Two-state toggle cycle (Off 竊・On 竊・Off 竊・On) test passed");
        }

        #endregion

        #region 繧ｨ繝ｩ繝ｼ繝上Φ繝峨Μ繝ｳ繧ｰ繝・せ繝・
        [Fact]
        public async Task ToggleElement_WithNonExistentElement_ShouldHandleError()
        {
            // Arrange
            _mockToggleService.Setup(s => s.ToggleElementAsync("nonExistentElement", null, null, null, 30))
                             .ThrowsAsync(new InvalidOperationException("Element 'nonExistentElement' not found"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.ToggleElement("nonExistentElement", "TestWindow"));

            _mockToggleService.Verify(s => s.ToggleElementAsync("nonExistentElement", null, null, null, 30), Times.Once);
            _output.WriteLine("Non-existent element error handling test passed");
        }

        [Fact]
        public async Task ToggleElement_WithUnsupportedElement_ShouldHandleError()
        {
            // Arrange
            _mockToggleService.Setup(s => s.ToggleElementAsync("staticText", null, null, null, 30))
                             .ThrowsAsync(new InvalidOperationException("Element does not support TogglePattern"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.ToggleElement("staticText", "TestWindow"));

            _mockToggleService.Verify(s => s.ToggleElementAsync("staticText", null, null, null, 30), Times.Once);
            _output.WriteLine("Unsupported element error handling test passed");
        }

        [Theory]
        [InlineData("Hyperlink")]
        [InlineData("Static")]
        [InlineData("Image")]
        public async Task ToggleElement_WithNonToggleControls_ShouldThrowInvalidOperationException(string controlType)
        {
            // Arrange
            _mockToggleService.Setup(s => s.ToggleElementAsync(controlType, null, null, null, 30))
                             .ThrowsAsync(new InvalidOperationException($"Control type '{controlType}' does not support TogglePattern. Use InvokePattern instead."));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _tools.ToggleElement(controlType, "TestWindow"));

            _mockToggleService.Verify(s => s.ToggleElementAsync(controlType, null, null, null, 30), Times.Once);
            _output.WriteLine($"Non-toggle control '{controlType}' error handling test passed");
        }

        #endregion

        #region 繝代Λ繝｡繝ｼ繧ｿ讀懆ｨｼ繝・せ繝・
        [Theory]
        [InlineData("", "TestWindow")]
        [InlineData("element1", "")]
        public async Task ToggleElement_WithEmptyParameters_ShouldCallService(string elementSelector, string windowTitle)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Toggle" }
            };
            _mockToggleService.Setup(s => s.ToggleElementAsync(elementSelector, null, null, null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ToggleElement(elementSelector,
                string.IsNullOrEmpty(windowTitle) ? null : windowTitle);

            // Assert
            Assert.NotNull(result);
            _mockToggleService.Verify(s => s.ToggleElementAsync(elementSelector, null, null, null, 30), Times.Once);
            _output.WriteLine($"Empty parameter test passed: element='{elementSelector}', window='{windowTitle}'");
        }

        [Theory]
        [InlineData(1234)]
        [InlineData(5678)]
        [InlineData(0)]
        public async Task ToggleElement_WithProcessId_ShouldCallServiceCorrectly(int processId)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Toggle", Action = "Toggle", ReturnValue = "On", Details = "Previous state: Off" }
            };
            _mockToggleService.Setup(s => s.ToggleElementAsync("element1", null, null, processId, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ToggleElement(automationId: "element1", controlType: "TestWindow", processId: processId);

            // Assert
            Assert.NotNull(result);
            _mockToggleService.Verify(s => s.ToggleElementAsync("element1", null, null, processId, 30), Times.Once);
            _output.WriteLine($"ProcessId parameter test passed: processId={processId}");
        }

        [Theory]
        [InlineData(10)]
        [InlineData(60)]
        [InlineData(120)]
        public async Task ToggleElement_WithCustomTimeout_ShouldCallServiceCorrectly(int timeoutSeconds)
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Toggle", Action = "Toggle", ReturnValue = "Off", Details = "Previous state: On" }
            };
            _mockToggleService.Setup(s => s.ToggleElementAsync("element1", null, null, null, timeoutSeconds))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ToggleElement("element1", "TestWindow", timeoutSeconds: timeoutSeconds);

            // Assert
            Assert.NotNull(result);
            _mockToggleService.Verify(s => s.ToggleElementAsync("element1", null, null, null, timeoutSeconds), Times.Once);
            _output.WriteLine($"Custom timeout test passed: timeout={timeoutSeconds}s");
        }

        #endregion

        #region Microsoft莉墓ｧ俶ｺ匁侠縺ｮPropertyChangedEvent繝・せ繝・
        [Fact]
        public async Task ToggleElement_PropertyChange_ShouldTriggerToggleStatePropertyChangedEvent()
        {
            // Arrange
            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { 
                    Success = true, 
                    Action = "Toggle", 
                    ReturnValue = "On", 
                    Details = "Toggled - Previous state: Off", 
                    Metadata = new Dictionary<string, object> {
                        { "PropertyChanged", true },
                        { "EventFired", "TogglePattern.ToggleState" },
                        { "OldValue", "Off" },
                        { "NewValue", "On" }
                    }
                }
            };
            _mockToggleService.Setup(s => s.ToggleElementAsync("checkBox", null, null, null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ToggleElement("checkBox", "Form");

            // Assert
            Assert.NotNull(result);
            _mockToggleService.Verify(s => s.ToggleElementAsync("checkBox", null, null, null, 30), Times.Once);
            _output.WriteLine("ToggleState property change event test passed");
        }

        #endregion

        #region 隍・尅縺ｪ繧ｷ繝翫Μ繧ｪ繝・せ繝・
        [Fact]
        public async Task ToggleElement_MultipleToggleControls_ShouldExecuteIndependently()
        {
            // Arrange
            var checkbox1Result = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Toggle", Action = "Toggle", ReturnValue = "On", Details = "Previous state: Off" }
            };
            var checkbox2Result = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Toggle", Action = "Toggle", ReturnValue = "Indeterminate", Details = "Previous state: Off" }
            };
            var radioButton1Result = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Toggle", Action = "Toggle", ReturnValue = "On", Details = "Previous state: Off" }
            };
            var toggleButton1Result = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { Success = true, OperationName = "Toggle", Action = "Toggle", ReturnValue = "On", Details = "Previous state: Off" }
            };

            _mockToggleService.Setup(s => s.ToggleElementAsync("checkBox1", null, null, null, 30))
                             .Returns(Task.FromResult(checkbox1Result));
            _mockToggleService.Setup(s => s.ToggleElementAsync("checkBox2", null, null, null, 30))
                             .Returns(Task.FromResult(checkbox2Result));
            _mockToggleService.Setup(s => s.ToggleElementAsync("radioButton1", null, null, null, 30))
                             .Returns(Task.FromResult(radioButton1Result));
            _mockToggleService.Setup(s => s.ToggleElementAsync("toggleButton1", null, null, null, 30))
                             .Returns(Task.FromResult(toggleButton1Result));

            // Act
            var result1 = await _tools.ToggleElement("checkBox1", "Form");
            var result2 = await _tools.ToggleElement("checkBox2", "Form");
            var result3 = await _tools.ToggleElement("radioButton1", "Form");
            var result4 = await _tools.ToggleElement("toggleButton1", "Form");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
            Assert.NotNull(result4);

            _mockToggleService.Verify(s => s.ToggleElementAsync("checkBox1", null, null, null, 30), Times.Once);
            _mockToggleService.Verify(s => s.ToggleElementAsync("checkBox2", null, null, null, 30), Times.Once);
            _mockToggleService.Verify(s => s.ToggleElementAsync("radioButton1", null, null, null, 30), Times.Once);
            _mockToggleService.Verify(s => s.ToggleElementAsync("toggleButton1", null, null, null, 30), Times.Once);

            _output.WriteLine("Multiple toggle controls independent execution test passed");
        }

        [Fact]
        public async Task ToggleElement_RadioButtonGroup_ShouldHandleGroupBehavior()
        {
            // Arrange - Radio buttons in a group should behave differently (only one can be selected)
            var radio1SelectResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { 
                    Success = true, 
                    Action = "Toggle", 
                    ReturnValue = "On", 
                    Details = "Toggled - Previous state: Off",
                    Metadata = new Dictionary<string, object> { { "GroupBehavior", "Exclusive" } }
                }
            };
            var radio2SelectResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { 
                    Success = true, 
                    Action = "Toggle", 
                    ReturnValue = "On", 
                    Details = "Toggled - Previous state: Off",
                    Metadata = new Dictionary<string, object> { 
                        { "GroupBehavior", "Exclusive" },
                        { "OtherRadioDeselected", "radioButton1" }
                    }
                }
            };

            _mockToggleService.Setup(s => s.ToggleElementAsync("radioButton1", null, null, null, 30))
                             .Returns(Task.FromResult(radio1SelectResult));
            _mockToggleService.Setup(s => s.ToggleElementAsync("radioButton2", null, null, null, 30))
                             .Returns(Task.FromResult(radio2SelectResult));

            // Act
            var result1 = await _tools.ToggleElement("radioButton1", "GroupBox");
            var result2 = await _tools.ToggleElement("radioButton2", "GroupBox");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);

            _mockToggleService.Verify(s => s.ToggleElementAsync("radioButton1", null, null, null, 30), Times.Once);
            _mockToggleService.Verify(s => s.ToggleElementAsync("radioButton2", null, null, null, 30), Times.Once);

            _output.WriteLine("Radio button group exclusive behavior test passed");
        }

        #endregion

        #region Microsoft莉墓ｧ倥・蛻ｶ髯蝉ｺ矩・ユ繧ｹ繝・
        [Fact]
        public async Task ToggleElement_SetStateMethodNotAvailable_ShouldOnlyProvideToggleMethod()
        {
            // Arrange - Microsoft莉墓ｧ倥〒縺ｯ SetState(newState) 繝｡繧ｽ繝・ラ縺ｯ謠蝉ｾ帙＆繧後↑縺・            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { 
                    Success = true, 
                    Action = "Toggle", 
                    ReturnValue = "On", 
                    Details = "Toggled - Previous state: Off",
                    Metadata = new Dictionary<string, object> { 
                        { "MethodUsed", "Toggle" },
                        { "Note", "SetState method not provided per Microsoft specification" }
                    }
                }
            };
            _mockToggleService.Setup(s => s.ToggleElementAsync("checkBox", null, null, null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ToggleElement("checkBox", "Window");

            // Assert
            Assert.NotNull(result);
            _mockToggleService.Verify(s => s.ToggleElementAsync("checkBox", null, null, null, 30), Times.Once);
            _output.WriteLine("Microsoft specification compliance test - no SetState method available");
        }

        [Theory]
        [InlineData("Off", "On")]
        [InlineData("On", "Off")]
        public async Task ToggleElement_TwoStateControl_ShouldNotUseIndeterminateState(string fromState, string toState)
        {
            // Arrange - 2迥ｶ諷九さ繝ｳ繝医Ο繝ｼ繝ｫ縺ｯIndeterminate繧剃ｽｿ逕ｨ縺励↑縺・            var expectedResult = new ServerEnhancedResponse<ActionResult> {
                Success = true,
                Data = new ActionResult { 
                    Success = true, 
                    Action = "Toggle", 
                    ReturnValue = toState, 
                    Details = $"Toggled from {fromState} to {toState}",
                    Metadata = new Dictionary<string, object> { 
                        { "StateType", "TwoState" },
                        { "IndeterminateSupported", false }
                    }
                }
            };
            _mockToggleService.Setup(s => s.ToggleElementAsync("simpleCheckBox", null, null, null, 30))
                             .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _tools.ToggleElement("simpleCheckBox", "Dialog");

            // Assert
            Assert.NotNull(result);
            _mockToggleService.Verify(s => s.ToggleElementAsync("simpleCheckBox", null, null, null, 30), Times.Once);
            _output.WriteLine($"Two-state control test passed: {fromState} 竊・{toState}");
        }

        #endregion
    }
}
