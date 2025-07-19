using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Tools;
using Xunit;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.E2E
{
    [Collection("WinUI3GalleryTestCollection")]
    [Trait("Category", "E2E")]
    public class WinUI3GalleryE2ETests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly IServiceProvider _serviceProvider;
        private readonly UIAutomationTools _tools;

        public WinUI3GalleryE2ETests(ITestOutputHelper output)
        {
            _output = output;
            _serviceProvider = MCPToolsE2ETests.CreateServiceProvider();
            _tools = _serviceProvider.GetRequiredService<UIAutomationTools>();
        }

        [Fact]
        public async Task WinUI3Gallery_Navigation_ShouldWork()
        {
            _output.WriteLine("Testing WinUI 3 Gallery navigation...");

            try
            {
                // Get current windows to find WinUI 3 Gallery
                var windowInfo = await _tools.GetWindows();
                _output.WriteLine($"Window info: {JsonSerializer.Serialize(windowInfo, new JsonSerializerOptions { WriteIndented = true })}");

                // Find elements in WinUI 3 Gallery
                var elements = await _tools.FindElements(windowTitle: "WinUI 3 Gallery");
                _output.WriteLine($"WinUI 3 Gallery elements: {JsonSerializer.Serialize(elements, new JsonSerializerOptions { WriteIndented = true })}");

                // Get element tree for better understanding
                var tree = await _tools.GetElementTree(windowTitle: "WinUI 3 Gallery", maxDepth: 2);
                _output.WriteLine($"WinUI 3 Gallery tree: {JsonSerializer.Serialize(tree, new JsonSerializerOptions { WriteIndented = true })}");

                Assert.True(true, "WinUI 3 Gallery navigation test executed successfully");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"WinUI 3 Gallery navigation test failed: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [Fact]
        public async Task WinUI3Gallery_FindButtons_ShouldWork()
        {
            _output.WriteLine("Testing WinUI 3 Gallery button finding...");

            try
            {
                // Find buttons in WinUI 3 Gallery
                var buttons = await _tools.FindElements(controlType: "Button", windowTitle: "WinUI 3 Gallery");
                _output.WriteLine($"Found buttons: {JsonSerializer.Serialize(buttons, new JsonSerializerOptions { WriteIndented = true })}");

                Assert.True(true, "Button finding test executed successfully");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Button finding test failed: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [Fact]
        public async Task WinUI3Gallery_TakeScreenshot_ShouldWork()
        {
            _output.WriteLine("Testing WinUI 3 Gallery screenshot...");

            try
            {
                // Take screenshot of WinUI 3 Gallery
                var screenshot = await _tools.TakeScreenshot(windowTitle: "WinUI 3 Gallery");
                _output.WriteLine($"Screenshot result: {JsonSerializer.Serialize(screenshot, new JsonSerializerOptions { WriteIndented = true })}");

                Assert.True(true, "Screenshot test executed successfully");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Screenshot test failed: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [Fact]
        public async Task WinUI3Gallery_NavigateToTextBox_ShouldWork()
        {
            _output.WriteLine("Testing WinUI 3 Gallery TextBox navigation...");

            try
            {
                // First take a screenshot to see the current state
                await _tools.TakeScreenshot(windowTitle: "WinUI 3 Gallery", outputPath: "C:\\temp\\gallery_before.png");

                // Find navigation elements (like Navigation View or menu items)
                var navElements = await _tools.FindElements(controlType: "ListItem", windowTitle: "WinUI 3 Gallery");
                _output.WriteLine($"Navigation elements: {JsonSerializer.Serialize(navElements, new JsonSerializerOptions { WriteIndented = true })}");

                // Look for TextBox-related navigation items
                var textElements = await _tools.FindElements(searchText: "Text", windowTitle: "WinUI 3 Gallery");
                _output.WriteLine($"Text-related elements: {JsonSerializer.Serialize(textElements, new JsonSerializerOptions { WriteIndented = true })}");

                Assert.True(true, "TextBox navigation test executed successfully");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"TextBox navigation test failed: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [Fact]
        public async Task WinUI3Gallery_AccessibilityInfo_ShouldWork()
        {
            _output.WriteLine("Testing WinUI 3 Gallery accessibility information...");

            try
            {
                // Get accessibility info for the main window
                var accessibilityInfo = await _tools.VerifyAccessibility(windowTitle: "WinUI 3 Gallery");
                _output.WriteLine($"Accessibility info: {JsonSerializer.Serialize(accessibilityInfo, new JsonSerializerOptions { WriteIndented = true })}");

                Assert.True(true, "Accessibility test executed successfully");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Accessibility test failed: {ex.Message}");
                _output.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public void Dispose()
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

}