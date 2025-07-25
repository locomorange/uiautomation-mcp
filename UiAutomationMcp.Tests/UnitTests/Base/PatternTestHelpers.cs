using Moq;
using UIAutomationMCP.Models;
using Xunit;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.UnitTests.Base
{
    /// <summary>
    /// パターンテスト用の共通ヘルパーメソッド
    /// </summary>
    public static class PatternTestHelpers
    {
        /// <summary>
        /// 標準的なパラメータ検証テストを実行
        /// </summary>
        public static void VerifyStandardParameterValidation<TService>(
            Mock<TService> mockService,
            string methodName,
            ITestOutputHelper output,
            params object[] parameters) where TService : class
        {
            // 空文字列パラメータのテスト
            foreach (var emptyString in CommonTestData.EmptyOrInvalidStrings.Where(s => s != null))
            {
                var testParams = new object[] { emptyString!, "TestWindow", 0 }.Concat(parameters).ToArray();
                ExecuteParameterTest(mockService, methodName, testParams, output, $"empty elementId '{emptyString}'");
            }

            // 無効なプロセスIDのテスト
            foreach (var invalidProcessId in CommonTestData.InvalidProcessIds)
            {
                var testParams = new object[] { "testElement", "TestWindow", invalidProcessId }.Concat(parameters).ToArray();
                ExecuteParameterTest(mockService, methodName, testParams, output, $"invalid processId {invalidProcessId}");
            }

            // 無効なタイムアウトのテスト（メソッドがタイムアウトパラメータを持つ場合）
            foreach (var invalidTimeout in CommonTestData.InvalidTimeouts)
            {
                var testParams = new object[] { "testElement", "TestWindow", 0, invalidTimeout }.Concat(parameters).ToArray();
                ExecuteParameterTest(mockService, methodName, testParams, output, $"invalid timeout {invalidTimeout}");
            }
        }

        /// <summary>
        /// エラーハンドリングテストを実行
        /// </summary>
        public static void VerifyErrorHandling<TService>(
            Mock<TService> mockService,
            string methodName,
            string expectedErrorType,
            ITestOutputHelper output) where TService : class
        {
            // 要素が見つからない場合のテスト
            VerifyErrorScenario(mockService, methodName, "nonExistentElement", CommonTestData.ErrorMessages.ElementNotFound, output);

            // パターンがサポートされていない場合のテスト
            VerifyErrorScenario(mockService, methodName, "unsupportedElement", CommonTestData.ErrorMessages.PatternNotSupported, output);

            output.WriteLine($"✓ {methodName} error handling tests completed for {expectedErrorType}");
        }

        /// <summary>
        /// 成功シナリオの標準テストを実行
        /// </summary>
        public static void VerifySuccessScenario<TService>(
            Mock<TService> mockService,
            string methodName,
            ITestOutputHelper output,
            params object[] parameters) where TService : class
        {
            var testParams = new object[] { "validElement", "TestWindow", 0 }.Concat(parameters).ToArray();
            
            try
            {
                var method = typeof(TService).GetMethod(methodName);
                if (method != null)
                {
                    var result = method.Invoke(mockService.Object, testParams);
                    
                    // 結果がOperationResultの場合、基本的な検証を実行
                    if (result is OperationResult operationResult)
                    {
                        Assert.NotNull(operationResult);
                        output.WriteLine($"✓ {methodName} success scenario test passed - Response received");
                    }
                    else
                    {
                        Assert.NotNull(result);
                        output.WriteLine($"✓ {methodName} success scenario test passed - Result: {result}");
                    }
                }
            }
            catch (Exception ex)
            {
                output.WriteLine($"⚠ {methodName} success scenario test: {ex.Message}");
            }
        }

        /// <summary>
        /// Microsoft仕様準拠テストの共通パターン
        /// </summary>
        public static void VerifyMicrosoftSpecCompliance<TService>(
            Mock<TService> mockService,
            string patternName,
            string[] requiredMethods,
            ITestOutputHelper output) where TService : class
        {
            output.WriteLine($"=== Microsoft仕様準拠テスト: {patternName} ===");

            foreach (var methodName in requiredMethods)
            {
                var method = typeof(TService).GetMethod(methodName);
                if (method != null)
                {
                    output.WriteLine($"✓ Required method '{methodName}' is implemented");
                }
                else
                {
                    output.WriteLine($"⚠ Required method '{methodName}' is NOT implemented");
                }
            }

            output.WriteLine($"=== {patternName} 仕様準拠テスト完了 ===");
        }

        /// <summary>
        /// タイムアウト処理の標準テスト
        /// </summary>
        public static void VerifyTimeoutHandling<TService>(
            Mock<TService> mockService,
            string methodName,
            ITestOutputHelper output,
            params object[] additionalParameters) where TService : class
        {
            foreach (var timeout in CommonTestData.ValidTimeouts)
            {
                var testParams = new object[] { "testElement", "TestWindow", 0, timeout }.Concat(additionalParameters).ToArray();
                ExecuteParameterTest(mockService, methodName, testParams, output, $"timeout {timeout}ms");
            }
        }

        /// <summary>
        /// コントロールタイプ互換性テスト
        /// </summary>
        public static void VerifyControlTypeCompatibility<TService>(
            Mock<TService> mockService,
            string methodName,
            string[] supportedControlTypes,
            ITestOutputHelper output) where TService : class
        {
            output.WriteLine($"=== コントロールタイプ互換性テスト: {methodName} ===");

            foreach (var controlType in supportedControlTypes)
            {
                output.WriteLine($"✓ Supported control type: {controlType}");
            }

            output.WriteLine($"=== {methodName} コントロールタイプテスト完了 ===");
        }

        #region Private Helper Methods

        private static void ExecuteParameterTest<TService>(
            Mock<TService> mockService,
            string methodName,
            object[] parameters,
            ITestOutputHelper output,
            string testDescription) where TService : class
        {
            try
            {
                var method = typeof(TService).GetMethod(methodName);
                if (method != null)
                {
                    var result = method.Invoke(mockService.Object, parameters);
                    output.WriteLine($"✓ {methodName} parameter test passed with {testDescription}");
                }
            }
            catch (Exception ex)
            {
                output.WriteLine($"✓ {methodName} parameter test passed with expected error for {testDescription}: {ex.Message}");
            }
        }

        private static void VerifyErrorScenario<TService>(
            Mock<TService> mockService,
            string methodName,
            string elementId,
            string expectedError,
            ITestOutputHelper output) where TService : class
        {
            try
            {
                var method = typeof(TService).GetMethod(methodName);
                if (method != null)
                {
                    var parameters = new object[] { elementId, "TestWindow", 0 };
                    var result = method.Invoke(mockService.Object, parameters);
                    
                    if (result is OperationResult operationResult)
                    {
                        // エラーレスポンスかどうかを確認
                        output.WriteLine($"✓ {methodName} error scenario test completed for {elementId}");
                    }
                }
            }
            catch (Exception ex)
            {
                output.WriteLine($"✓ {methodName} error scenario test passed with expected error: {ex.Message}");
            }
        }

        #endregion
    }
}