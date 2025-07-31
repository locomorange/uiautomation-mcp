using System.Diagnostics;
using UIAutomationMCP.Server.Tools;
using Xunit;
using Xunit.Abstractions;

namespace UIAutomationMCP.Tests.E2E;

/// <summary>
/// Real-world scenario tests demonstrating practical usage of the new patterns.
/// These tests show how the patterns would be used in actual automation scenarios.
/// </summary>
[Collection("UIAutomation")]
public class RealWorldScenariosE2ETests : BaseE2ETest
{
    private readonly ITestOutputHelper _output;

    public RealWorldScenariosE2ETests(ITestOutputHelper output) : base(output)
    {
        _output = output;
    }

    [Fact(Skip = "Demonstration of Excel automation scenario")]
    public async Task Scenario_ExcelAutomation_LargeSpreadsheet()
    {
        // Scenario: Automating Excel with a large spreadsheet that uses virtualization
        // This demonstrates:
        // 1. VirtualizedItemPattern - accessing cells that are not currently visible
        // 2. ItemContainerPattern - finding specific cells by property
        // 3. SynchronizedInputPattern - ensuring input is processed in order

        await ProcessCleanupHelper.ExecuteWithCleanup(async () =>
        {
            var excelProcess = Process.Start("excel.exe");
            await Task.Delay(5000); // Wait for Excel to fully load

            // Step 1: Find the worksheet container
            var worksheetResult = await Tools.FindItemByProperty(
                automationId: "WorksheetGrid",
                propertyName: "ControlType",
                value: "DataGrid",
                timeoutSeconds: 10);

            var worksheetDict = worksheetResult as Dictionary<string, object>;
            Assert.NotNull(worksheetDict);
            Assert.True((bool)worksheetDict["Success"]);
            _output.WriteLine("Found worksheet grid");

            // Step 2: Realize a cell that's far down (likely virtualized)
            var cellResult = await Tools.RealizeVirtualizedItem(
                "Cell_A1000", // Cell A1000 is likely virtualized
                timeoutSeconds: 10);

            var cellDict = cellResult as Dictionary<string, object>;
            Assert.NotNull(cellDict);
            Assert.True((bool)cellDict["Success"]);
            _output.WriteLine("Realized virtualized cell A1000");

            // Step 3: Start synchronized input to ensure proper data entry
            var syncResult = await Tools.StartSynchronizedInput(
                "KeyDown",
                automationId: "Cell_A1000",
                timeoutSeconds: 5);

            var syncDict = syncResult as Dictionary<string, object>;
            Assert.NotNull(syncDict);
            Assert.True((bool)syncDict["Success"]);
            _output.WriteLine("Started synchronized input for data entry");

            // Step 4: Simulate data entry (in real scenario, would use SendKeys or similar)
            await Task.Delay(1000);

            // Step 5: Cancel synchronized input
            await Tools.CancelSynchronizedInput(
                "Cell_A1000");
            _output.WriteLine("Completed Excel automation scenario");

            return excelProcess;
        }, _output, "Excel", 10000);
    }

    [Fact(Skip = "Demonstration of Visual Studio solution explorer automation")]
    public async Task Scenario_VisualStudio_NavigateLargeSolution()
    {
        // Scenario: Navigating a large Visual Studio solution with virtualized tree nodes
        // This demonstrates finding and expanding virtualized project nodes

        await ProcessCleanupHelper.ExecuteWithCleanup(async () =>
        {
            var vsProcess = Process.Start("devenv.exe", "/nosplash");
            await Task.Delay(10000); // VS takes time to load

            // Find Solution Explorer
            var solutionExplorerResult = await Tools.FindItemByProperty(
                automationId: "SolutionExplorer",
                propertyName: "Name",
                value: "Solution Explorer",
                timeoutSeconds: 15);

            var solutionDict = solutionExplorerResult as Dictionary<string, object>;
            Assert.NotNull(solutionDict);
            Assert.True((bool)solutionDict["Success"]);

            var solutionInfo = solutionDict["ElementInfo"] as Dictionary<string, object>;
            var containerId = solutionInfo?["AutomationId"] as string ?? "SolutionExplorer";

            // Find a specific project node (might be virtualized in large solutions)
            var projectResult = await Tools.FindItemByProperty(
                propertyName: "Name",
                value: "MyProject.Tests",
                automationId: containerId,
                timeoutSeconds: 10);

            var projectDict = projectResult as Dictionary<string, object>;
            if (projectDict == null || !(bool)projectDict["Success"])
            {
                // If not found, it might be virtualized - try to realize parent nodes
                _output.WriteLine("Project node might be virtualized, attempting to realize");
                
                var realizeResult = await Tools.RealizeVirtualizedItem(
                    "TestProjects",
                    timeoutSeconds: 10);

                // Retry finding the project
                projectResult = await Tools.FindItemByProperty(
                    propertyName: "Name",
                    value: "MyProject.Tests",
                    automationId: containerId,
                    timeoutSeconds: 10);
                
                projectDict = projectResult as Dictionary<string, object>;
            }

            Assert.NotNull(projectDict);
            Assert.True((bool)projectDict["Success"]);
            _output.WriteLine("Successfully navigated to project in large solution");

            return vsProcess;
        }, _output, "Visual Studio", 15000);
    }

    [Fact(Skip = "Demonstration of Windows File Explorer batch operations")]
    public async Task Scenario_FileExplorer_BatchFileOperations()
    {
        // Scenario: Selecting multiple files in a large directory with virtualization
        // This demonstrates synchronized input for multi-selection

        await ProcessCleanupHelper.ExecuteWithCleanup(async () =>
        {
            var explorerProcess = Process.Start("explorer.exe", @"C:\Windows\System32");
            await Task.Delay(3000);

            // Find the files list container
            var filesListResult = await Tools.FindItemByProperty(
                propertyName: "ClassName",
                value: "UIItemsView",
                automationId: "ItemsView",
                timeoutSeconds: 10);

            var filesDict = filesListResult as Dictionary<string, object>;
            Assert.NotNull(filesDict);
            Assert.True((bool)filesDict["Success"]);

            var filesInfo = filesDict["ElementInfo"] as Dictionary<string, object>;
            var filesId = filesInfo?["AutomationId"] as string ?? "ItemsView";

            // Start synchronized input for multi-selection with Ctrl held
            var syncResult = await Tools.StartSynchronizedInput(
                "KeyDown", // For Ctrl key
                automationId: filesId,
                timeoutSeconds: 5);

            var syncDict = syncResult as Dictionary<string, object>;
            Assert.NotNull(syncDict);
            Assert.True((bool)syncDict["Success"]);

            // In a real scenario, we would:
            // 1. Hold Ctrl
            // 2. Click multiple files
            // 3. Realize virtualized items as needed
            // 4. Perform batch operation

            // Find and realize specific files
            var dllFiles = new[] { "kernel32.dll", "user32.dll", "ntdll.dll" };
            foreach (var dllName in dllFiles)
            {
                // These might be virtualized if the list is scrolled
                var fileResult = await Tools.FindItemByProperty(
                    propertyName: "Name",
                    value: dllName,
                    automationId: filesId,
                    timeoutSeconds: 5);

                var fileDict = fileResult as Dictionary<string, object>;
                if (fileDict == null || !(bool)fileDict["Success"])
                {
                    // Try to realize if virtualized
                    await Tools.RealizeVirtualizedItem(
                        dllName,
                        timeoutSeconds: 5);
                }
            }

            // Cancel synchronized input
            await Tools.CancelSynchronizedInput(
                filesId);
            _output.WriteLine("Completed batch file selection scenario");

            return explorerProcess;
        }, _output, "File Explorer", 8000);
    }

    [Fact(Skip = "Demonstration of web browser virtualized content")]
    public async Task Scenario_EdgeBrowser_InfiniteScrollPage()
    {
        // Scenario: Automating a web page with infinite scroll (virtualized content)
        // This demonstrates accessing virtualized web content

        await ProcessCleanupHelper.ExecuteWithCleanup(async () =>
        {
            var edgeProcess = Process.Start("msedge.exe", "https://example.com/infinite-scroll");
            await Task.Delay(5000);

            // Find the main content area
            var contentResult = await Tools.FindItemByProperty(
                propertyName: "ClassName",
                value: "Chrome_RenderWidgetHostHWND",
                automationId: "ContentArea",
                timeoutSeconds: 10);

            var contentDict = contentResult as Dictionary<string, object>;
            Assert.NotNull(contentDict);
            Assert.True((bool)contentDict["Success"]);

            // Attempt to access content that would require scrolling
            // In infinite scroll, items are virtualized
            for (int i = 0; i < 5; i++)
            {
                var itemId = $"post_{i * 20}"; // Every 20th post
                
                var realizeResult = await Tools.RealizeVirtualizedItem(
                    itemId,
                    timeoutSeconds: 5);

                var realizeDict = realizeResult as Dictionary<string, object>;
                if (realizeDict != null && (bool)realizeDict["Success"])
                {
                    _output.WriteLine($"Successfully realized post {i * 20}");
                    
                    // In real scenario, would interact with the realized content
                    await Task.Delay(500);
                }
            }

            _output.WriteLine("Completed infinite scroll automation");

            return edgeProcess;
        }, _output, "Microsoft Edge", 8000);
    }

    [Fact(Skip = "Demonstration of database application automation")]
    public async Task Scenario_DatabaseApp_QueryResultNavigation()
    {
        // Scenario: Navigating large query results in a database application
        // This demonstrates all three patterns working together

        await ProcessCleanupHelper.ExecuteWithCleanup(async () =>
        {
            var dbAppProcess = Process.Start(@"C:\Program Files\DatabaseApp\dbapp.exe");
            await Task.Delay(5000);

            // Step 1: Find the results grid
            var resultsGridResult = await Tools.FindItemByProperty(
                propertyName: "ControlType",
                value: "DataGrid",
                automationId: "QueryResults",
                timeoutSeconds: 10);

            var gridDict = resultsGridResult as Dictionary<string, object>;
            Assert.NotNull(gridDict);
            Assert.True((bool)gridDict["Success"]);

            var gridInfo = gridDict["ElementInfo"] as Dictionary<string, object>;
            var gridId = gridInfo?["AutomationId"] as string ?? "QueryResults";

            // Step 2: Start synchronized input for navigation
            var syncResult = await Tools.StartSynchronizedInput(
                "KeyDown",
                automationId: gridId,
                timeoutSeconds: 5);

            var syncDict = syncResult as Dictionary<string, object>;
            Assert.NotNull(syncDict);
            Assert.True((bool)syncDict["Success"]);

            // Step 3: Navigate to specific records (might be virtualized)
            var recordIds = new[] { 1000, 5000, 10000, 50000 };
            foreach (var recordId in recordIds)
            {
                // Find record by ID
                var recordResult = await Tools.FindItemByProperty(
                    propertyName: "Name",
                    value: $"Record {recordId}",
                    automationId: gridId,
                    timeoutSeconds: 5);

                var recordDict = recordResult as Dictionary<string, object>;
                if (recordDict == null || !(bool)recordDict["Success"])
                {
                    // Record is virtualized, realize it
                    _output.WriteLine($"Record {recordId} is virtualized, realizing...");
                    
                    var realizeResult = await Tools.RealizeVirtualizedItem(
                        $"Row_{recordId}",
                        timeoutSeconds: 10);

                    var realizeDict = realizeResult as Dictionary<string, object>;
                    Assert.NotNull(realizeDict);
                    Assert.True((bool)realizeDict["Success"]);
                }

                _output.WriteLine($"Successfully accessed record {recordId}");
            }

            // Step 4: Cancel synchronized input
            await Tools.CancelSynchronizedInput(
                gridId);
            _output.WriteLine("Completed database navigation scenario");

            return dbAppProcess;
        }, _output, "Database Application", 12000);
    }
}

