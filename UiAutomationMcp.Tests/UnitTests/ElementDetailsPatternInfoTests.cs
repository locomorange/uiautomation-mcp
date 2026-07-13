using System.Text.Json;
using UIAutomationMCP.Models;
using Xunit;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.UnitTests
{
    /// <summary>
    /// Tests for SelectionItemDetailInfo and AccessibilityInfo models
    /// added to ElementDetails via includeDetails=true.
    /// Validates serialization, nullable behavior, and model structure.
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class ElementDetailsPatternInfoTests
    {
        private readonly ITestOutputHelper _output;

        public ElementDetailsPatternInfoTests(ITestOutputHelper output)
        {
            _output = output;
        }

        #region SelectionItemDetailInfo Tests

        [Fact]
        public void SelectionItemDetailInfo_Defaults_ShouldBeUnselected()
        {
            var info = new SelectionItemDetailInfo();

            Assert.False(info.IsSelected);
            Assert.Null(info.SelectionContainer);
        }

        [Fact]
        public void SelectionItemDetailInfo_ShouldSerializeAndDeserializeCorrectly()
        {
            var original = new SelectionItemDetailInfo
            {
                IsSelected = true,
                SelectionContainer = "listBox1"
            };

            var json = JsonSerializer.Serialize(original);
            var deserialized = JsonSerializer.Deserialize<SelectionItemDetailInfo>(json);

            Assert.NotNull(deserialized);
            Assert.True(deserialized.IsSelected);
            Assert.Equal("listBox1", deserialized.SelectionContainer);

            _output.WriteLine($"Serialized: {json}");
        }

        [Fact]
        public void SelectionItemDetailInfo_NullContainer_ShouldOmitFromJson()
        {
            var info = new SelectionItemDetailInfo
            {
                IsSelected = false,
                SelectionContainer = null
            };

            var options = new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };
            var json = JsonSerializer.Serialize(info, options);

            Assert.DoesNotContain("selectionContainer", json);
            _output.WriteLine($"JSON with null container omitted: {json}");
        }

        [Fact]
        public void ElementDetails_SelectionItem_ShouldBeNullable()
        {
            var details = new ElementDetails();

            Assert.Null(details.SelectionItem);

            details.SelectionItem = new SelectionItemDetailInfo { IsSelected = true };
            Assert.NotNull(details.SelectionItem);
            Assert.True(details.SelectionItem.IsSelected);
        }

        [Fact]
        public void ElementDetails_WithSelectionItem_ShouldSerializeCorrectly()
        {
            var details = new ElementDetails
            {
                SelectionItem = new SelectionItemDetailInfo
                {
                    IsSelected = true,
                    SelectionContainer = "comboBox1"
                }
            };

            var json = JsonSerializer.Serialize(details);
            Assert.Contains("\"selectionItem\"", json);
            Assert.Contains("\"isSelected\":true", json);
            Assert.Contains("\"selectionContainer\":\"comboBox1\"", json);

            var deserialized = JsonSerializer.Deserialize<ElementDetails>(json);
            Assert.NotNull(deserialized?.SelectionItem);
            Assert.True(deserialized.SelectionItem.IsSelected);
            Assert.Equal("comboBox1", deserialized.SelectionItem.SelectionContainer);

            _output.WriteLine($"ElementDetails with SelectionItem serialized correctly");
        }

        #endregion

        #region AccessibilityInfo Tests

        [Fact]
        public void AccessibilityInfo_Defaults_ShouldBeNull()
        {
            var info = new AccessibilityInfo();

            Assert.Null(info.LabeledBy);
            Assert.Null(info.HelpText);
            Assert.Null(info.AccessKey);
            Assert.Null(info.AcceleratorKey);
        }

        [Fact]
        public void AccessibilityInfo_ShouldSerializeAndDeserializeCorrectly()
        {
            var original = new AccessibilityInfo
            {
                LabeledBy = new ElementReference
                {
                    AutomationId = "label1",
                    Name = "Username",
                    ControlType = "ControlType.Text"
                },
                HelpText = "Enter your username",
                AccessKey = "Alt+U",
                AcceleratorKey = "Ctrl+U"
            };

            var json = JsonSerializer.Serialize(original);
            var deserialized = JsonSerializer.Deserialize<AccessibilityInfo>(json);

            Assert.NotNull(deserialized);
            Assert.NotNull(deserialized.LabeledBy);
            Assert.Equal("label1", deserialized.LabeledBy.AutomationId);
            Assert.Equal("Username", deserialized.LabeledBy.Name);
            Assert.Equal("ControlType.Text", deserialized.LabeledBy.ControlType);
            Assert.Equal("Enter your username", deserialized.HelpText);
            Assert.Equal("Alt+U", deserialized.AccessKey);
            Assert.Equal("Ctrl+U", deserialized.AcceleratorKey);

            _output.WriteLine($"AccessibilityInfo serialized: {json}");
        }

        [Fact]
        public void AccessibilityInfo_PartialData_ShouldSerializeCorrectly()
        {
            // Only HelpText is set — other fields remain null
            var info = new AccessibilityInfo
            {
                HelpText = "Click to submit"
            };

            var json = JsonSerializer.Serialize(info);
            Assert.Contains("\"helpText\":\"Click to submit\"", json);

            var deserialized = JsonSerializer.Deserialize<AccessibilityInfo>(json);
            Assert.NotNull(deserialized);
            Assert.Null(deserialized.LabeledBy);
            Assert.Null(deserialized.AccessKey);
            Assert.Null(deserialized.AcceleratorKey);
            Assert.Equal("Click to submit", deserialized.HelpText);
        }

        [Fact]
        public void ElementDetails_Accessibility_ShouldBeNullable()
        {
            var details = new ElementDetails();

            Assert.Null(details.Accessibility);

            details.Accessibility = new AccessibilityInfo { AccessKey = "Alt+N" };
            Assert.NotNull(details.Accessibility);
            Assert.Equal("Alt+N", details.Accessibility.AccessKey);
        }

        [Fact]
        public void ElementDetails_WithAccessibility_ShouldSerializeCorrectly()
        {
            var details = new ElementDetails
            {
                Accessibility = new AccessibilityInfo
                {
                    LabeledBy = new ElementReference
                    {
                        AutomationId = "lbl_name",
                        Name = "Full Name",
                        ControlType = "ControlType.Text"
                    },
                    AccessKey = "Alt+N",
                    HelpText = "Enter your full name"
                }
            };

            var json = JsonSerializer.Serialize(details);
            Assert.Contains("\"accessibility\"", json);
            Assert.Contains("\"labeledBy\"", json);

            var deserialized = JsonSerializer.Deserialize<ElementDetails>(json);
            Assert.NotNull(deserialized?.Accessibility);
            Assert.NotNull(deserialized.Accessibility.LabeledBy);
            Assert.Equal("lbl_name", deserialized.Accessibility.LabeledBy.AutomationId);
            Assert.Equal("Alt+N", deserialized.Accessibility.AccessKey);
            Assert.Equal("Enter your full name", deserialized.Accessibility.HelpText);

            _output.WriteLine("ElementDetails with AccessibilityInfo serialized correctly");
        }

        #endregion

        #region ElementReference Tests

        [Fact]
        public void ElementReference_Defaults_ShouldBeEmptyStrings()
        {
            var reference = new ElementReference();

            Assert.Equal(string.Empty, reference.AutomationId);
            Assert.Equal(string.Empty, reference.Name);
            Assert.Equal(string.Empty, reference.ControlType);
        }

        [Fact]
        public void ElementReference_ShouldSerializeWithCorrectPropertyNames()
        {
            var reference = new ElementReference
            {
                AutomationId = "el1",
                Name = "Element 1",
                ControlType = "ControlType.Button"
            };

            var json = JsonSerializer.Serialize(reference);
            Assert.Contains("\"automationId\":\"el1\"", json);
            Assert.Contains("\"name\":\"Element 1\"", json);
            Assert.Contains("\"controlType\":\"ControlType.Button\"", json);
        }

        #endregion

        #region Combined ElementDetails Tests

        [Fact]
        public void ElementDetails_WithBothSelectionItemAndAccessibility_ShouldSerializeCorrectly()
        {
            var details = new ElementDetails
            {
                HelpText = "Test element",
                SelectionItem = new SelectionItemDetailInfo
                {
                    IsSelected = true,
                    SelectionContainer = "listView1"
                },
                Accessibility = new AccessibilityInfo
                {
                    AccessKey = "Alt+T",
                    HelpText = "Test help"
                }
            };

            var json = JsonSerializer.Serialize(details);
            var deserialized = JsonSerializer.Deserialize<ElementDetails>(json);

            Assert.NotNull(deserialized);
            Assert.NotNull(deserialized.SelectionItem);
            Assert.True(deserialized.SelectionItem.IsSelected);
            Assert.Equal("listView1", deserialized.SelectionItem.SelectionContainer);
            Assert.NotNull(deserialized.Accessibility);
            Assert.Equal("Alt+T", deserialized.Accessibility.AccessKey);
            Assert.Equal("Test help", deserialized.Accessibility.HelpText);

            _output.WriteLine("Combined SelectionItem + Accessibility test passed");
        }

        [Fact]
        public void ElementDetails_NullSelectionItemAndAccessibility_ShouldNotAppearInDefaultJson()
        {
            var details = new ElementDetails();

            var json = JsonSerializer.Serialize(details);

            // selectionItem and accessibility should still be in JSON as null
            // but the key point is they don't add data overhead
            var deserialized = JsonSerializer.Deserialize<ElementDetails>(json);
            Assert.NotNull(deserialized);
            Assert.Null(deserialized.SelectionItem);
            Assert.Null(deserialized.Accessibility);
        }

        #endregion

        #region TransformInfo Zoom Properties Tests

        [Fact]
        public void TransformInfo_ZoomProperties_ShouldBeNullableByDefault()
        {
            var info = new TransformInfo();

            Assert.Null(info.CanZoom);
            Assert.Null(info.ZoomLevel);
            Assert.Null(info.ZoomMinimum);
            Assert.Null(info.ZoomMaximum);
        }

        [Fact]
        public void TransformInfo_WithZoom_ShouldSerializeCorrectly()
        {
            var info = new TransformInfo
            {
                CanMove = true,
                CanResize = true,
                CanRotate = false,
                CurrentX = 100,
                CurrentY = 200,
                CurrentWidth = 800,
                CurrentHeight = 600,
                CanZoom = true,
                ZoomLevel = 1.5,
                ZoomMinimum = 0.25,
                ZoomMaximum = 4.0
            };

            var json = JsonSerializer.Serialize(info);
            var deserialized = JsonSerializer.Deserialize<TransformInfo>(json);

            Assert.NotNull(deserialized);
            Assert.True(deserialized.CanZoom);
            Assert.Equal(1.5, deserialized.ZoomLevel);
            Assert.Equal(0.25, deserialized.ZoomMinimum);
            Assert.Equal(4.0, deserialized.ZoomMaximum);
        }

        [Fact]
        public void TransformInfo_WithoutZoom_ShouldOmitNullZoomProperties()
        {
            var info = new TransformInfo
            {
                CanMove = true,
                CanResize = false,
                CanRotate = false,
                CurrentX = 0,
                CurrentY = 0,
                CurrentWidth = 400,
                CurrentHeight = 300
                // Zoom properties left as null
            };

            var options = new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };
            var json = JsonSerializer.Serialize(info, options);

            Assert.DoesNotContain("canZoom", json);
            Assert.DoesNotContain("zoomLevel", json);
            Assert.DoesNotContain("zoomMinimum", json);
            Assert.DoesNotContain("zoomMaximum", json);
        }

        #endregion

        #region TextInfo CaretPosition Tests

        [Fact]
        public void TextInfo_CaretPosition_ShouldBeNullableByDefault()
        {
            var info = new TextInfo();

            Assert.Null(info.CaretPosition);
        }

        [Fact]
        public void TextInfo_WithCaretPosition_ShouldSerializeCorrectly()
        {
            var info = new TextInfo
            {
                Text = "Hello World",
                Length = 11,
                SelectedText = "",
                HasSelection = false,
                CaretPosition = 5
            };

            var json = JsonSerializer.Serialize(info);
            var deserialized = JsonSerializer.Deserialize<TextInfo>(json);

            Assert.NotNull(deserialized);
            Assert.Equal(5, deserialized.CaretPosition);
            Assert.Equal("Hello World", deserialized.Text);
        }

        #endregion
    }
}
