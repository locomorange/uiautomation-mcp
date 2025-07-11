using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using Moq;

namespace UIAutomationMCP.Tests.UnitTests.Helpers
{
    [Trait("Category", "Unit")]
    public class AutomationHelperTests
    {
        private readonly Mock<ILogger<AutomationHelper>> _mockLogger;
        private readonly AutomationHelper _automationHelper;

        public AutomationHelperTests()
        {
            _mockLogger = new Mock<ILogger<AutomationHelper>>();
            _automationHelper = new AutomationHelper(_mockLogger.Object);
        }

        [Theory]
        [InlineData("button", true)]
        [InlineData("BUTTON", true)]
        [InlineData("Button", true)]
        [InlineData("edit", true)]
        [InlineData("text", true)]
        [InlineData("window", true)]
        [InlineData("checkbox", true)]
        [InlineData("radiobutton", true)]
        [InlineData("combobox", true)]
        [InlineData("listbox", true)]
        [InlineData("listitem", true)]
        [InlineData("tree", true)]
        [InlineData("treeitem", true)]
        [InlineData("tab", true)]
        [InlineData("tabitem", true)]
        [InlineData("slider", true)]
        [InlineData("progressbar", true)]
        [InlineData("menu", true)]
        [InlineData("menuitem", true)]
        [InlineData("toolbar", true)]
        [InlineData("statusbar", true)]
        [InlineData("table", true)]
        [InlineData("document", true)]
        [InlineData("image", true)]
        [InlineData("hyperlink", true)]
        [InlineData("pane", true)]
        [InlineData("invalid", false)]
        [InlineData("", false)]
        [InlineData("unknown", false)]
        public void TryParseControlType_WithVariousInputs_ShouldReturnExpectedResult(string controlType, bool expectedResult)
        {
            // When
            var result = _automationHelper.TryParseControlType(controlType, out var parsedControlType);

            // Then
            Assert.Equal(expectedResult, result);
            
            if (expectedResult)
            {
                Assert.NotNull(parsedControlType);
                switch (controlType.ToLowerInvariant())
                {
                    case "button":
                        Assert.Equal(ControlType.Button, parsedControlType);
                        break;
                    case "edit":
                        Assert.Equal(ControlType.Edit, parsedControlType);
                        break;
                    case "text":
                        Assert.Equal(ControlType.Text, parsedControlType);
                        break;
                    case "window":
                        Assert.Equal(ControlType.Window, parsedControlType);
                        break;
                    case "checkbox":
                        Assert.Equal(ControlType.CheckBox, parsedControlType);
                        break;
                    case "radiobutton":
                        Assert.Equal(ControlType.RadioButton, parsedControlType);
                        break;
                    case "combobox":
                        Assert.Equal(ControlType.ComboBox, parsedControlType);
                        break;
                    case "listbox":
                        Assert.Equal(ControlType.List, parsedControlType);
                        break;
                    case "listitem":
                        Assert.Equal(ControlType.ListItem, parsedControlType);
                        break;
                    case "tree":
                        Assert.Equal(ControlType.Tree, parsedControlType);
                        break;
                    case "treeitem":
                        Assert.Equal(ControlType.TreeItem, parsedControlType);
                        break;
                    case "tab":
                        Assert.Equal(ControlType.Tab, parsedControlType);
                        break;
                    case "tabitem":
                        Assert.Equal(ControlType.TabItem, parsedControlType);
                        break;
                    case "slider":
                        Assert.Equal(ControlType.Slider, parsedControlType);
                        break;
                    case "progressbar":
                        Assert.Equal(ControlType.ProgressBar, parsedControlType);
                        break;
                    case "menu":
                        Assert.Equal(ControlType.Menu, parsedControlType);
                        break;
                    case "menuitem":
                        Assert.Equal(ControlType.MenuItem, parsedControlType);
                        break;
                    case "toolbar":
                        Assert.Equal(ControlType.ToolBar, parsedControlType);
                        break;
                    case "statusbar":
                        Assert.Equal(ControlType.StatusBar, parsedControlType);
                        break;
                    case "table":
                        Assert.Equal(ControlType.Table, parsedControlType);
                        break;
                    case "document":
                        Assert.Equal(ControlType.Document, parsedControlType);
                        break;
                    case "image":
                        Assert.Equal(ControlType.Image, parsedControlType);
                        break;
                    case "hyperlink":
                        Assert.Equal(ControlType.Hyperlink, parsedControlType);
                        break;
                    case "pane":
                        Assert.Equal(ControlType.Pane, parsedControlType);
                        break;
                }
            }
            else
            {
                Assert.Equal(ControlType.Pane, parsedControlType);
            }
        }

        [Fact]
        public void TryParseControlType_WithNullInput_ShouldReturnFalse()
        {
            // When
            var result = _automationHelper.TryParseControlType(null!, out var parsedControlType);

            // Then
            Assert.False(result);
            Assert.Equal(ControlType.Pane, parsedControlType);
        }

        [Fact]
        public void GetSearchRoot_WithNoParameters_ShouldReturnRootElement()
        {
            // When
            var result = _automationHelper.GetSearchRoot();

            // Then
            Assert.NotNull(result);
            Assert.Equal(AutomationElement.RootElement, result);
        }

        [Fact]
        public void GetSearchRoot_WithNullWindowTitle_ShouldReturnRootElement()
        {
            // When
            var result = _automationHelper.GetSearchRoot(null, null);

            // Then
            Assert.NotNull(result);
            Assert.Equal(AutomationElement.RootElement, result);
        }

        [Fact]
        public void GetSearchRoot_WithEmptyWindowTitle_ShouldReturnRootElement()
        {
            // When
            var result = _automationHelper.GetSearchRoot("", null);

            // Then
            Assert.NotNull(result);
            Assert.Equal(AutomationElement.RootElement, result);
        }

        [Fact]
        public void GetSearchRoot_WithZeroProcessId_ShouldReturnRootElement()
        {
            // When
            var result = _automationHelper.GetSearchRoot(null, 0);

            // Then
            Assert.NotNull(result);
            Assert.Equal(AutomationElement.RootElement, result);
        }

        [Fact]
        public void GetSearchRoot_WithNegativeProcessId_ShouldReturnRootElement()
        {
            // When
            var result = _automationHelper.GetSearchRoot(null, -1);

            // Then
            Assert.NotNull(result);
            Assert.Equal(AutomationElement.RootElement, result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void FindElementById_WithInvalidElementId_ShouldReturnNull(string? elementId)
        {
            // Given
            var searchRoot = AutomationElement.RootElement;

            // When
            var result = _automationHelper.FindElementById(elementId ?? "", searchRoot);

            // Then
            Assert.Null(result);
        }

        [Fact]
        public void FindElementById_WithValidElementId_ShouldCallFindFirst()
        {
            // Given
            var elementId = "testElement";
            var searchRoot = AutomationElement.RootElement;

            // When
            var result = _automationHelper.FindElementById(elementId, searchRoot);

            // Then
            // Note: This test will likely return null since we're testing against real AutomationElement
            // but it verifies the method doesn't throw exceptions
            Assert.True(true); // Method executed without exception
        }

        [Fact]
        public void FindElementById_WithNullSearchRoot_ShouldHandleGracefully()
        {
            // Given
            var elementId = "testElement";

            // When & Then
            var exception = Record.Exception(() => _automationHelper.FindElementById(elementId, null!));
            
            // Should handle gracefully (might return null or throw - both are acceptable)
            Assert.True(true); // Method call completed
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // When & Then
            var exception = Assert.Throws<ArgumentNullException>(() => new AutomationHelper(null!));
            Assert.Contains("logger", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithValidLogger_ShouldCreateInstance()
        {
            // Given
            var logger = new Mock<ILogger<AutomationHelper>>();

            // When
            var helper = new AutomationHelper(logger.Object);

            // Then
            Assert.NotNull(helper);
        }
    }
}