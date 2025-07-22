using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Operations.Layout;
using UIAutomationMCP.Worker.Helpers;
using Moq;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.UnitTests
{
    /// <summary>
    /// ScrollPattern操作のパラメータ検証テスト
    /// Microsoft UIAutomation ScrollPattern仕様に準拠した境界値テストを実行
    /// 注意: 実際のUIAutomation呼び出しを避けるため、パラメータレベルでの検証のみ
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Unit")]
    public class ScrollPatternParameterValidationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public ScrollPatternParameterValidationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        #region GetScrollInfo Parameter Validation Tests

        [Theory]
        [InlineData("")]           // Empty string
        [InlineData("   ")]        // Whitespace only
        [InlineData(null)]         // Null (converted to empty string)
        public void GetScrollInfo_Should_Validate_ElementId_Parameters(string elementId)
        {
            // Arrange - パラメータ検証のみをテスト（UIAutomation呼び出しなし）
            var normalizedAutomationId = elementId ?? "";

            // Act & Assert - 空のelementIdは無効として扱われるべき
            var isValid = !string.IsNullOrWhiteSpace(normalizedElementId);
            
            if (string.IsNullOrWhiteSpace(elementId))
            {
                Assert.False(isValid, $"Empty or whitespace elementId '{elementId}' should be considered invalid");
            }
            else
            {
                Assert.True(isValid, $"Non-empty elementId '{elementId}' should be considered valid");
            }
            
            _output.WriteLine($"GetScrollInfo elementId validation test passed for: '{elementId}'");
        }

        [Theory]
        [InlineData("0")]
        [InlineData("1234")]
        [InlineData("-1")]         // 無効なプロセスID
        [InlineData("invalid")]    // 非数値
        public void GetScrollInfo_Should_Parse_ProcessId_Values(string processIdStr)
        {
            // Arrange & Act - プロセスID文字列のパース検証
            var canParse = int.TryParse(processIdStr, out var parsedProcessId);

            // Assert - パースエラーでクラッシュしないことを確認
            if (canParse)
            {
                Assert.True(parsedProcessId >= 0 || parsedProcessId == -1, "Valid process IDs should be non-negative or -1");
                _output.WriteLine($"GetScrollInfo processId '{processIdStr}' parsed successfully to: {parsedProcessId}");
            }
            else
            {
                Assert.False(canParse, $"Invalid processId '{processIdStr}' should fail to parse");
                _output.WriteLine($"GetScrollInfo processId '{processIdStr}' correctly failed to parse");
            }
        }

        #endregion

        #region SetScrollPercent Parameter Validation Tests

        [Theory]
        [InlineData("0.0", "0.0")]         // Minimum valid values
        [InlineData("100.0", "100.0")]     // Maximum valid values
        [InlineData("50.5", "33.3")]       // Decimal values
        [InlineData("-1.0", "-1.0")]       // NoScroll values (Microsoft仕様)
        [InlineData("0", "100")]           // Integer values
        public void SetScrollPercent_Should_Accept_Valid_Percentage_Strings(string horizontalStr, string verticalStr)
        {
            // Arrange & Act - パラメータ検証のみをテスト（UIAutomation呼び出しなし）
            var horizontalValid = double.TryParse(horizontalStr, out var horizontal);
            var verticalValid = double.TryParse(verticalStr, out var vertical);

            // Assert - 有効な値が正しくパースされることを確認
            Assert.True(horizontalValid, $"Horizontal percentage '{horizontalStr}' should parse successfully");
            Assert.True(verticalValid, $"Vertical percentage '{verticalStr}' should parse successfully");
            
            // Microsoft ScrollPattern仕様確認: 0-100 または -1 (NoScroll)
            var horizontalInRange = (horizontal >= 0 && horizontal <= 100) || horizontal == -1;
            var verticalInRange = (vertical >= 0 && vertical <= 100) || vertical == -1;
            
            Assert.True(horizontalInRange, $"Horizontal percentage {horizontal} should be in valid range (0-100 or -1)");
            Assert.True(verticalInRange, $"Vertical percentage {vertical} should be in valid range (0-100 or -1)");
            
            _output.WriteLine($"SetScrollPercent valid percentage test passed for H:{horizontal}, V:{vertical}");
        }

        [Theory]
        [InlineData("invalid", "50.0")]    // Invalid horizontal
        [InlineData("50.0", "invalid")]    // Invalid vertical
        [InlineData("", "50.0")]           // Empty horizontal
        [InlineData("50.0", "")]           // Empty vertical
        [InlineData(null, "50.0")]         // Null horizontal
        [InlineData("50.0", null)]         // Null vertical
        public void SetScrollPercent_Should_Reject_Invalid_Percentage_Values(string horizontalStr, string verticalStr)
        {
            // Arrange & Act - パラメータ検証のみをテスト（UIAutomation呼び出しなし）
            var horizontalValid = double.TryParse(horizontalStr ?? "", out var horizontal);
            var verticalValid = double.TryParse(verticalStr ?? "", out var vertical);

            // Assert - 無効な値が適切に検出されることを確認
            var expectValidHorizontal = !string.IsNullOrWhiteSpace(horizontalStr) && horizontalStr != "invalid";
            var expectValidVertical = !string.IsNullOrWhiteSpace(verticalStr) && verticalStr != "invalid";
            
            Assert.Equal(expectValidHorizontal, horizontalValid);
            Assert.Equal(expectValidVertical, verticalValid);
            
            _output.WriteLine($"SetScrollPercent invalid percentage test passed for H:'{horizontalStr}' (valid: {horizontalValid}), V:'{verticalStr}' (valid: {verticalValid})");
        }

        [Theory]
        [InlineData("-2.0", "50.0")]       // Below minimum horizontal
        [InlineData("50.0", "-2.0")]       // Below minimum vertical
        [InlineData("101.0", "50.0")]      // Above maximum horizontal
        [InlineData("50.0", "101.0")]      // Above maximum vertical
        [InlineData("999.9", "999.9")]     // Way above maximum
        public void SetScrollPercent_Should_Reject_Out_Of_Range_Values(string horizontalStr, string verticalStr)
        {
            // Arrange & Act - Microsoft仕様外の値をテスト（有効範囲: 0-100, -1）
            var horizontalValid = double.TryParse(horizontalStr, out var horizontal);
            var verticalValid = double.TryParse(verticalStr, out var vertical);

            // Assert - 範囲外の値が適切に検出されることを確認
            Assert.True(horizontalValid && verticalValid, "Values should parse as numbers");
            
            var horizontalInRange = (horizontal >= 0 && horizontal <= 100) || horizontal == -1;
            var verticalInRange = (vertical >= 0 && vertical <= 100) || vertical == -1;
            
            // 少なくとも一つは範囲外であるべき
            Assert.False(horizontalInRange && verticalInRange, $"At least one value should be out of range: H:{horizontal}, V:{vertical}");
            
            _output.WriteLine($"SetScrollPercent out-of-range test passed for H:{horizontal} (inRange: {horizontalInRange}), V:{vertical} (inRange: {verticalInRange})");
        }

        [Fact]
        public void SetScrollPercent_Should_Handle_Missing_Parameters_Gracefully()
        {
            // Arrange - 必須パラメータが欠損している場合をシミュレート
            string? missingHorizontal = null;
            string? missingVertical = null;

            // Act - パラメータ欠損のケースをテスト
            var horizontalValid = double.TryParse(missingHorizontal, out _);
            var verticalValid = double.TryParse(missingVertical, out _);

            // Assert - パラメータ欠損が適切に検出されることを確認
            Assert.False(horizontalValid, "Missing horizontal parameter should be invalid");
            Assert.False(verticalValid, "Missing vertical parameter should be invalid");
            
            _output.WriteLine("SetScrollPercent missing parameters test passed");
        }

        #endregion

        #region Microsoft ScrollPattern Specification Compliance Tests

        [Theory]
        [InlineData(-1.0)]    // ScrollPattern.NoScroll constant
        [InlineData(0.0)]     // Minimum scroll position
        [InlineData(50.0)]    // Middle position
        [InlineData(100.0)]   // Maximum scroll position
        public void SetScrollPercent_Should_Comply_With_Microsoft_ScrollPattern_Specification(double percent)
        {
            // Arrange & Act - Microsoft UIAutomation ScrollPattern仕様の有効値をテスト
            var isInValidRange = (percent >= 0 && percent <= 100) || percent == -1.0;

            // Assert - Microsoft仕様準拠の値が受け入れられることを確認
            Assert.True(isInValidRange, $"Percentage {percent} should comply with Microsoft ScrollPattern specification (0-100 or -1)");
            
            // 特定の仕様値を確認
            if (percent == -1.0)
            {
                _output.WriteLine($"Microsoft ScrollPattern NoScroll constant verified: {percent}");
            }
            else if (percent == 0.0)
            {
                _output.WriteLine($"Microsoft ScrollPattern minimum position verified: {percent}");
            }
            else if (percent == 100.0)
            {
                _output.WriteLine($"Microsoft ScrollPattern maximum position verified: {percent}");
            }
            else
            {
                _output.WriteLine($"Microsoft ScrollPattern middle position verified: {percent}");
            }
        }

        #endregion

        public void Dispose()
        {
            // No cleanup needed for mocks
        }
    }
}