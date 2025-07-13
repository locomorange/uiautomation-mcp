/*
 * Debug script to test actual UI interaction with visible elements
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using UIAutomationMCP.Server.Tools;
using UIAutomationMCP.Tests.E2E;

class DebugUIInteraction
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Debugging UI Interaction ===");
        
        var serviceProvider = MCPToolsE2ETests.CreateServiceProvider();
        var tools = serviceProvider.GetRequiredService<UIAutomationTools>();
        
        try
        {
            Console.WriteLine("1. Launching WinUI 3 Gallery...");
            await tools.LaunchApplicationByName("WinUI 3 Gallery");
            await Task.Delay(3000);
            
            Console.WriteLine("2. Taking initial screenshot...");
            await tools.TakeScreenshot("WinUI 3 Gallery", @"C:\temp\debug_before.png");
            
            Console.WriteLine("3. Finding ALL Edit elements with detailed info...");
            var allEdits = await tools.FindElementsByControlType("Edit", windowTitle: "WinUI 3 Gallery");
            
            var editsData = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(allEdits));
            if (editsData.TryGetProperty("data", out var dataElement) && 
                dataElement.TryGetProperty("elements", out var elementsArray))
            {
                var editElements = elementsArray.EnumerateArray().ToList();
                Console.WriteLine($"Found {editElements.Count} Edit elements:");
                
                foreach (var edit in editElements)
                {
                    if (edit.TryGetProperty("AutomationId", out var idElement) &&
                        edit.TryGetProperty("Name", out var nameElement) &&
                        edit.TryGetProperty("IsVisible", out var visibleElement) &&
                        edit.TryGetProperty("BoundingRectangle", out var rectElement))
                    {
                        var id = idElement.GetString();
                        var name = nameElement.GetString();
                        var isVisible = visibleElement.GetBoolean();
                        
                        Console.WriteLine($"  - ID: {id}");
                        Console.WriteLine($"    Name: {name}");
                        Console.WriteLine($"    Visible: {isVisible}");
                        Console.WriteLine($"    BoundingRect: {rectElement}");
                        Console.WriteLine();
                    }
                }
            }
            
            Console.WriteLine("4. Testing with Minimize button (should cause obvious GUI change)...");
            try
            {
                var minimizeResult = await tools.InvokeElement("Minimize", windowTitle: "WinUI 3 Gallery");
                Console.WriteLine($"Minimize result: {JsonSerializer.Serialize(minimizeResult)}");
                
                await Task.Delay(2000);
                
                Console.WriteLine("5. Taking screenshot after minimize...");
                await tools.TakeScreenshot("WinUI 3 Gallery", @"C:\temp\debug_after_minimize.png");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Minimize test failed: {ex.Message}");
            }
            
            Console.WriteLine("6. Testing SelectElement again to restore window...");
            try
            {
                // Restore the window by selecting Home again
                await tools.SelectElement("Home", windowTitle: "WinUI 3 Gallery");
                await Task.Delay(1000);
                
                Console.WriteLine("7. Taking final screenshot...");
                await tools.TakeScreenshot("WinUI 3 Gallery", @"C:\temp\debug_final.png");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Restore test failed: {ex.Message}");
            }
            
            Console.WriteLine("\n=== Debug completed ===");
            Console.WriteLine("Check screenshots to see if any changes occurred:");
            Console.WriteLine("- C:\\temp\\debug_before.png");
            Console.WriteLine("- C:\\temp\\debug_after_minimize.png");
            Console.WriteLine("- C:\\temp\\debug_final.png");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Debug failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}