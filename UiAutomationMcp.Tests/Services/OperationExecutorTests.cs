using UiAutomationWorker.Services;

namespace UIAutomationMCP.Tests.Services
{
    /// <summary>
    /// OperationExecutorのユニットテスト
    /// 静的メソッドの機能をテストします
    /// 注意: 実際のPatternExecutorクラスは具象クラスでありモックが困難なため、
    /// このテストは主に静的メソッドとサポート機能に焦点を当てます
    /// </summary>
    [Collection("UIAutomation Collection")]
    public class OperationExecutorTests
    {
        [Fact]
        public void GetSupportedOperations_ShouldReturnAllSupportedOperations()
        {
            // Act
            var supportedOperations = OperationExecutor.GetSupportedOperations();

            // Assert
            Assert.NotNull(supportedOperations);
            Assert.Contains("findfirst", supportedOperations);
            Assert.Contains("findall", supportedOperations);
            Assert.Contains("getproperties", supportedOperations);
            Assert.Contains("invoke", supportedOperations);
            Assert.Contains("setvalue", supportedOperations);
            Assert.Contains("getvalue", supportedOperations);
            Assert.Contains("toggle", supportedOperations);
            Assert.Contains("select", supportedOperations);
            Assert.Contains("scroll", supportedOperations);
            Assert.Contains("scrollintoview", supportedOperations);
            Assert.Contains("gettext", supportedOperations);
            Assert.Contains("selecttext", supportedOperations);
            Assert.Contains("findtext", supportedOperations);
            Assert.Contains("gettextselection", supportedOperations);
            Assert.Contains("setwindowstate", supportedOperations);
            Assert.Contains("getwindowstate", supportedOperations);
            Assert.Contains("closewindow", supportedOperations);
            Assert.Contains("waitforwindowstate", supportedOperations);
            Assert.Contains("gettree", supportedOperations);
            Assert.Contains("getchildren", supportedOperations);
            Assert.Contains("value", supportedOperations); // Alias
            Assert.Contains("get_value", supportedOperations); // Alias
        }

        [Theory]
        [InlineData("invoke", true)]
        [InlineData("INVOKE", true)] // Case insensitive
        [InlineData("gettext", true)]
        [InlineData("setwindowstate", true)]
        [InlineData("unknown_operation", false)]
        [InlineData("", false)]
        public void IsOperationSupported_ShouldReturnCorrectResult(string operation, bool expectedSupported)
        {
            // Act
            var isSupported = OperationExecutor.IsOperationSupported(operation);

            // Assert
            Assert.Equal(expectedSupported, isSupported);
        }

        [Theory]
        [InlineData("invoke", "Core Patterns")]
        [InlineData("findfirst", "Element Search")]
        [InlineData("scroll", "Layout Patterns")]
        [InlineData("gettree", "Tree Operations")]
        public void GetOperationInfo_ShouldReturnCorrectInfo(string operation, string expectedCategory)
        {
            // Act
            var info = OperationExecutor.GetOperationInfo(operation);

            // Assert
            Assert.NotNull(info);
            Assert.Equal(operation.ToLowerInvariant(), info["operation"]);
            Assert.True((bool)info["supported"]);
            Assert.Equal(expectedCategory, info["category"]);
            Assert.NotNull(info["description"]);
            Assert.NotNull(info["required_parameters"]);
            Assert.NotNull(info["optional_parameters"]);
        }

        [Fact]
        public void GetOperationInfo_UnsupportedOperation_ShouldReturnError()
        {
            // Act
            var info = OperationExecutor.GetOperationInfo("unsupported_operation");

            // Assert
            Assert.NotNull(info);
            Assert.Equal("unsupported_operation", info["operation"]);
            Assert.False((bool)info["supported"]);
            Assert.Equal("Operation not supported", info["error"]);
        }

        [Fact]
        public void GetSupportedOperations_ShouldContainTextPatternOperations()
        {
            // Act
            var supportedOperations = OperationExecutor.GetSupportedOperations();

            // Assert - Verify new text pattern operations are included
            Assert.Contains("gettext", supportedOperations);
            Assert.Contains("selecttext", supportedOperations);
            Assert.Contains("findtext", supportedOperations);
            Assert.Contains("gettextselection", supportedOperations);
        }

        [Fact]
        public void GetSupportedOperations_ShouldContainWindowPatternOperations()
        {
            // Act
            var supportedOperations = OperationExecutor.GetSupportedOperations();

            // Assert - Verify new window pattern operations are included
            Assert.Contains("setwindowstate", supportedOperations);
            Assert.Contains("getwindowstate", supportedOperations);
            Assert.Contains("closewindow", supportedOperations);
            Assert.Contains("waitforwindowstate", supportedOperations);
        }

        [Theory]
        [InlineData("gettext")]
        [InlineData("selecttext")]
        [InlineData("findtext")]
        [InlineData("gettextselection")]
        public void IsOperationSupported_TextPatternOperations_ShouldReturnTrue(string operation)
        {
            // Act
            var isSupported = OperationExecutor.IsOperationSupported(operation);

            // Assert
            Assert.True(isSupported);
        }

        [Theory]
        [InlineData("setwindowstate")]
        [InlineData("getwindowstate")]
        [InlineData("closewindow")]
        [InlineData("waitforwindowstate")]
        public void IsOperationSupported_WindowPatternOperations_ShouldReturnTrue(string operation)
        {
            // Act
            var isSupported = OperationExecutor.IsOperationSupported(operation);

            // Assert
            Assert.True(isSupported);
        }

        [Theory]
        [InlineData("selecttext", new[] { "ElementId", "StartIndex", "Length" })]
        [InlineData("setvalue", new[] { "ElementId", "Value" })]
        [InlineData("value", new[] { "ElementId", "Value" })]
        [InlineData("setwindowstate", new[] { "ElementId", "State" })]
        public void GetOperationInfo_ShouldReturnCorrectRequiredParameters(string operation, string[] expectedParams)
        {
            // Act
            var info = OperationExecutor.GetOperationInfo(operation);

            // Assert
            Assert.NotNull(info);
            Assert.True((bool)info["supported"]);
            var requiredParams = (string[])info["required_parameters"];
            Assert.Equal(expectedParams.Length, requiredParams.Length);
            foreach (var param in expectedParams)
            {
                Assert.Contains(param, requiredParams);
            }
        }

        [Fact]
        public void GetOperationInfo_AllSupportedOperations_ShouldReturnValidInfo()
        {
            // Arrange
            var supportedOperations = OperationExecutor.GetSupportedOperations();

            // Act & Assert
            foreach (var operation in supportedOperations)
            {
                var info = OperationExecutor.GetOperationInfo(operation);
                
                Assert.NotNull(info);
                Assert.Equal(operation.ToLowerInvariant(), info["operation"]);
                Assert.True((bool)info["supported"]);
                Assert.NotNull(info["category"]);
                Assert.NotNull(info["description"]);
                Assert.NotNull(info["required_parameters"]);
                Assert.NotNull(info["optional_parameters"]);
                Assert.False(info.ContainsKey("error"));
            }
        }
    }
}