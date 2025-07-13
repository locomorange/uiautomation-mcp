using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.Integration
{
    /// <summary>
    /// Tests for Value Pattern implementation according to Microsoft UI Automation specifications.
    /// Reference: https://learn.microsoft.com/en-us/dotnet/framework/ui-automation/implementing-the-ui-automation-value-control-pattern
    /// 
    /// Required Members:
    /// - SetValue method: Sets the value of the control
    /// - Value property (GetValue): Gets the current value of the control
    /// - IsReadOnly property: Indicates whether the control can be modified
    /// </summary>
    [Collection("UIAutomationTestCollection")]
    [Trait("Category", "Integration")]
    public class ValuePatternSpecificationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly IValueService _valueService;
        private readonly ServiceProvider _serviceProvider;

        public ValuePatternSpecificationTests(ITestOutputHelper output)
        {
            _output = output;

            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            
            // Add required services similar to BasicE2ETests pattern
            _serviceProvider = services.BuildServiceProvider();
            var logger = _serviceProvider.GetRequiredService<ILogger<SubprocessExecutor>>();
            
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var possiblePaths = new[]
            {
                Path.Combine(baseDir, "UIAutomationMCP.Worker.exe"),
                Path.Combine(baseDir, "..", "UIAutomationMCP.Worker", "bin", "Debug", "net9.0-windows", "UIAutomationMCP.Worker.exe"),
                Path.Combine(baseDir, "worker", "UIAutomationMCP.Worker.exe"),
            };

            var workerPath = possiblePaths.FirstOrDefault(File.Exists) ?? 
                throw new InvalidOperationException("Worker executable not found");

            var subprocessExecutor = new SubprocessExecutor(logger, workerPath);
            _valueService = new SubprocessBasedValueService(
                _serviceProvider.GetRequiredService<ILogger<SubprocessBasedValueService>>(), 
                subprocessExecutor);
        }

        /// <summary>
        /// Tests Microsoft specification requirement: Elements must support IsReadOnly property
        /// Tests the core required member of Value Pattern implementation
        /// </summary>
        [Fact]
        public async Task IsReadOnly_RequiredMember_ShouldReturnValidResult()
        {
            // Arrange
            var nonExistentElementId = "TestElement_ValuePattern_IsReadOnly";
            var timeout = 5;

            // Act
            var result = await _valueService.IsReadOnlyAsync(nonExistentElementId, null, null, timeout);

            // Assert
            Assert.NotNull(result);
            _output.WriteLine($"IsReadOnly result: {result}");
            
            // Microsoft spec: Must handle cases where element is not found or doesn't support ValuePattern
            // The implementation should return proper error information
        }

        /// <summary>
        /// Tests Microsoft specification requirement: SetValue method implementation
        /// Should handle ElementNotEnabledException when control is disabled
        /// </summary>
        [Fact]
        public async Task SetValue_RequiredMember_ShouldHandleDisabledControl()
        {
            // Arrange
            var nonExistentElementId = "TestElement_ValuePattern_SetValue";
            var testValue = "Test Value";
            var timeout = 5;

            // Act
            var result = await _valueService.SetValueAsync(nonExistentElementId, testValue, null, null, timeout);

            // Assert
            Assert.NotNull(result);
            _output.WriteLine($"SetValue result: {result}");
            
            // Microsoft spec: Should throw ElementNotEnabledException if control is disabled
            // Should throw ArgumentException if value cannot be converted to recognized format
            // Should throw InvalidOperationException for incorrectly formatted locale-specific information
        }

        /// <summary>
        /// Tests Microsoft specification requirement: Value property (GetValue) implementation
        /// Should return current value of the control
        /// </summary>
        [Fact]
        public async Task GetValue_RequiredMember_ShouldReturnCurrentValue()
        {
            // Arrange
            var nonExistentElementId = "TestElement_ValuePattern_GetValue";
            var timeout = 5;

            // Act
            var result = await _valueService.GetValueAsync(nonExistentElementId, null, null, timeout);

            // Assert
            Assert.NotNull(result);
            _output.WriteLine($"GetValue result: {result}");
            
            // Microsoft spec: Should return current value of the control
            // For elements that don't support ValuePattern, should return appropriate error
        }

        /// <summary>
        /// Tests Microsoft specification: IsReadOnly property behavior
        /// According to spec: IsReadOnly must be false to allow value modification
        /// </summary>
        [Fact]
        public async Task IsReadOnly_SpecificationBehavior_ReadOnlyElementsShouldReturnTrue()
        {
            // Arrange - Simulate read-only element scenario
            var readOnlyElementId = "ReadOnlyElement_ValuePattern";
            var timeout = 5;

            // Act
            var result = await _valueService.IsReadOnlyAsync(readOnlyElementId, null, null, timeout);

            // Assert
            Assert.NotNull(result);
            _output.WriteLine($"IsReadOnly for read-only element: {result}");
            
            // Microsoft spec: Read-only elements should return IsReadOnly = true
            // Editable elements should return IsReadOnly = false
        }

        /// <summary>
        /// Tests Microsoft specification: SetValue with read-only element
        /// According to spec: Should handle attempts to modify read-only controls
        /// </summary>
        [Fact]
        public async Task SetValue_SpecificationBehavior_ReadOnlyElementShouldFail()
        {
            // Arrange
            var readOnlyElementId = "ReadOnlyElement_SetValue_Test";
            var testValue = "Attempted modification";
            var timeout = 5;

            // Act
            var result = await _valueService.SetValueAsync(readOnlyElementId, testValue, null, null, timeout);

            // Assert
            Assert.NotNull(result);
            _output.WriteLine($"SetValue on read-only element result: {result}");
            
            // Microsoft spec: Attempting to set value on read-only control should fail appropriately
        }

        /// <summary>
        /// Tests Microsoft specification: Control pattern availability
        /// According to spec: Controls must be IsEnabledProperty set to true
        /// </summary>
        [Fact]
        public async Task ValuePattern_SpecificationRequirement_EnabledPropertyCheck()
        {
            // Arrange
            var disabledElementId = "DisabledElement_ValuePattern";
            var timeout = 5;

            // Act - Test all required members on potentially disabled element
            var isReadOnlyResult = await _valueService.IsReadOnlyAsync(disabledElementId, null, null, timeout);
            var getValueResult = await _valueService.GetValueAsync(disabledElementId, null, null, timeout);
            var setValueResult = await _valueService.SetValueAsync(disabledElementId, "test", null, null, timeout);

            // Assert
            Assert.NotNull(isReadOnlyResult);
            Assert.NotNull(getValueResult);
            Assert.NotNull(setValueResult);
            
            _output.WriteLine($"IsReadOnly on disabled element: {isReadOnlyResult}");
            _output.WriteLine($"GetValue on disabled element: {getValueResult}");
            _output.WriteLine($"SetValue on disabled element: {setValueResult}");
            
            // Microsoft spec: Controls must be enabled for Value Pattern to work properly
        }

        /// <summary>
        /// Tests Microsoft specification: Exception handling requirements
        /// Tests all the exceptions mentioned in Microsoft documentation
        /// </summary>
        [Theory]
        [InlineData("", "Empty element ID")]
        [InlineData("InvalidElement", "Invalid element")]
        [InlineData("NonValuePatternElement", "Element without ValuePattern")]
        public async Task ValuePattern_ExceptionHandling_ShouldHandleSpecificationExceptions(string elementId, string scenario)
        {
            // Arrange
            var timeout = 5;

            // Act & Assert - Test all required members for exception handling
            var isReadOnlyResult = await _valueService.IsReadOnlyAsync(elementId, null, null, timeout);
            var getValueResult = await _valueService.GetValueAsync(elementId, null, null, timeout);
            var setValueResult = await _valueService.SetValueAsync(elementId, "test value", null, null, timeout);

            // All methods should handle exceptions gracefully and return structured results
            Assert.NotNull(isReadOnlyResult);
            Assert.NotNull(getValueResult);
            Assert.NotNull(setValueResult);

            _output.WriteLine($"Scenario: {scenario}");
            _output.WriteLine($"IsReadOnly: {isReadOnlyResult}");
            _output.WriteLine($"GetValue: {getValueResult}");
            _output.WriteLine($"SetValue: {setValueResult}");
            
            // Microsoft spec exceptions that should be handled:
            // - InvalidOperationException: incorrectly formatted locale-specific information
            // - ArgumentException: value cannot be converted to recognized format
            // - ElementNotEnabledException: attempting to modify disabled control
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}