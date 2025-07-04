# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is an enterprise-grade C# UIAutomation MCP (Model Context Protocol) Server that exposes comprehensive Windows UI Automation functionality through MCP tools. The server uses a dual-process architecture with sophisticated service layer organization to provide reliable Windows application automation for testing and automation purposes.

## Build and Run Commands

```bash
# Build the entire solution (both projects)
dotnet build win-UIAutomation-mcp.sln --verbosity quiet --nologo

# Build individual projects (suppress output to avoid MCP protocol interference)
dotnet build UiAutomationMcpServer.csproj --verbosity quiet --nologo
dotnet build UiAutomationWorker.csproj --verbosity quiet --nologo

# Run the server (use built executable to avoid build output)
./bin/Debug/net8.0-windows/UiAutomationMcpServer.exe

# Restore packages for solution
dotnet restore --verbosity quiet --nologo
```

## WSL Environment Notes

When working in WSL, you can execute Windows commands directly using:
```bash
/mnt/c/Windows/System32/cmd.exe /c "command here"
```
Example: `/mnt/c/Windows/System32/cmd.exe /c "dotnet --version"` - the quoted section can be any Windows command you need to run from WSL.

**Important**: You can execute any Windows command by replacing the content within the double quotes. This wrapper allows full access to Windows system commands, .NET CLI, and other Windows-specific tools from within the WSL environment.

For building from WSL:
```bash
# Build entire solution
/mnt/c/Windows/System32/cmd.exe /c "cd /d C:\Users\yksh\source\clones\win-UIAutomation-mcp && dotnet build win-UIAutomation-mcp.sln --verbosity quiet --nologo"

# Build individual projects
/mnt/c/Windows/System32/cmd.exe /c "cd /d C:\Users\yksh\source\clones\win-UIAutomation-mcp && dotnet build UiAutomationMcpServer.csproj --verbosity quiet --nologo"
```

**Critical for MCP**: Always use the built executable directly instead of `dotnet run` to avoid build output interfering with MCP protocol:
```bash
/mnt/c/Windows/System32/cmd.exe /c "cd /d C:\Users\yksh\source\clones\win-UIAutomation-mcp && .\\bin\\Debug\\net8.0-windows\\UiAutomationMcpServer.exe"
```

## Architecture

The application uses a **dual-process architecture** with sophisticated service layer organization for maximum reliability and performance:

### **Dual-Process Architecture**
1. **UiAutomationMcpServer** - Main MCP server handling protocol communication
2. **UiAutomationWorker** - Subprocess worker executing UI Automation operations in isolation
3. **Inter-Process Communication** - JSON-based communication via stdin/stdout to prevent COM/native API blocking

### **Project Structure**
**UiAutomationMcpServer** (Main Process):
- **Program.cs**: MCP server entry point with Microsoft.Extensions.Hosting
- **Models/**: Data models for MCP requests/responses and UIAutomation entities
- **Services/**: Sophisticated service layer with specialized categories
- **UIAutomationTools**: 25+ MCP tool definitions with attribute-based registration

**UiAutomationWorker** (Subprocess):
- **Program.cs**: Worker process entry point for UI automation execution
- Direct Windows UI Automation API interaction without MCP protocol interference

### **Service Layer Architecture**
The service layer is organized into **specialized categories** for maximum modularity:

**Core Services:**
- `IUIAutomationHelper` - Shared automation utilities with timeout handling
- `IUIAutomationWorker` - Subprocess execution coordinator
- `IElementUtilityService` - Element value and action utilities

**Specialized Service Categories:**
- **Elements/** - Element discovery, properties, tree navigation, utilities (4 services)
- **Patterns/** - UI Automation pattern implementations (6 specialized services)
- **Windows/** - Window management and screenshot services (2 services)

### **Pattern Services (Microsoft Learn Compliant)**
**6 Specialized Pattern Services:**
- `ICorePatternService` - Basic interactions (invoke, value, toggle, selection)
- `ILayoutPatternService` - Layout operations (expand/collapse, scroll, transform, dock)
- `IRangePatternService` - Range value operations (sliders, progress bars)
- `IWindowPatternService` - Window management patterns
- `ITextPatternService` - Complex text operations
- `IAdvancedPatternService` - Advanced patterns (multiple view, virtualized items)

### **MCP Tool Implementation**
**25+ MCP Tools** organized into logical categories:
- **Window and Element Discovery** (3 tools) - GetWindowInfo, GetElementInfo, FindElements
- **Application Management** (2 tools) - LaunchApplication, WindowAction
- **Core Interaction Patterns** (4 tools) - InvokeElement, SetElementValue, ToggleElement, SelectElement
- **Layout and Navigation** (3 tools) - ExpandCollapseElement, ScrollElement, TransformElement
- **Value and Range Patterns** (2 tools) - GetRangeValue, SetRangeValue
- **Window Management** (3 tools) - WindowAction, DockElement, TakeScreenshot
- **Advanced Patterns** (4 tools) - ChangeView, RealizeVirtualizedItem, ItemContainer, SynchronizedInput
- **Text Pattern Operations** (4 tools) - GetText, FindText, SelectText, GetTextSelection

### **Reliability Features**
- **Timeout Handling**: Comprehensive timeout management across all operations
- **Cancellation Tokens**: Proper async cancellation support
- **Subprocess Isolation**: Prevents main process blocking from native API calls
- **Structured Error Handling**: Consistent error responses with detailed logging

## MCP Protocol Implementation

Uses the official `ModelContextProtocol` NuGet package with:
- **STDIO Transport**: Communication over stdin/stdout
- **Automatic Tool Discovery**: `WithToolsFromAssembly()` for attribute-based registration
- **Type-Safe Operations**: Strongly-typed request/response models
- **Proper Error Handling**: Structured error responses with logging

## Key Dependencies

**UiAutomationMcpServer** (Main Process):
- **ModelContextProtocol**: Core MCP functionality (prerelease version)
- **Microsoft.Extensions.Hosting**: Application hosting and lifecycle management
- **Microsoft.Extensions.Logging**: Structured logging framework
- **Microsoft.Extensions.DependencyInjection**: Dependency injection container
- **System.Drawing.Common**: Graphics and screenshot capabilities
- **Project Reference**: UiAutomationWorker for subprocess operations

**UiAutomationWorker** (Subprocess):
- **System.Windows.Automation**: Direct Windows UI Automation APIs
- **System.Windows.Forms**: Windows Forms for UI interaction
- **System.Text.Json**: JSON serialization for inter-process communication
- **Microsoft.Extensions.Logging**: Console logging for subprocess operations

## Tool Implementation Pattern

Each MCP tool follows this pattern:
1. Parameter validation from the MCP client
2. UI element discovery using AutomationElement APIs
3. Action execution using Windows UI Automation patterns
4. Structured response with success/error status

The server handles element-based operations using AutomationId/Name with Windows UI Automation patterns. Microsoft Learn guideline-compliant pattern support includes:
- **invoke**: Button clicks and menu item execution
- **value**: Text input and retrieval
- **toggle**: Checkbox state changes
- **expandcollapse**: Tree item expansion/collapse
- **selection**: List item selection
- **scroll**: Scrolling operations
- **rangevalue**: Slider and progress bar manipulation
- **transform**: Element movement and resizing
- **multipleview**: View switching
- **window**: Window state management

## Debugging

**Main Process (UiAutomationMcpServer):**
- Console logging is disabled to prevent MCP protocol interference
- Use Visual Studio debugger for step-through debugging
- Temporarily enable file logging in Program.cs if needed
- All debugging output must avoid stdout/stdin to maintain MCP JSON-RPC protocol compliance

**Worker Process (UiAutomationWorker):**
- Console logging is enabled for subprocess operations
- Worker process logs are separate from MCP protocol communication
- Use Visual Studio debugger with "Attach to Process" for worker process debugging
- Worker process can be debugged independently of the main MCP server

## MCP Configuration

The project includes VSCode MCP configuration (`.vscode/mcp.json`) for development:

```json
{
    "servers": {
        "my-mcp-server-b52bb65c": {
            "type": "stdio",
            "command": "/mnt/c/Windows/System32/cmd.exe",
            "args": [
                "/c",
                "dotnet",
                "run",
                "--project",
                "C:/Users/yksh/source/clones/win-UIAutomation-mcp/UiAutomationMcpServer/UiAutomationMcpServer.csproj"
            ]
        }
    }
}
```

**Note**: For production use, prefer the built executable over `dotnet run` to avoid build output interfering with MCP protocol.

## Hook Configuration

This project can benefit from Claude Code hooks to automatically use Windows commands in WSL environment:

```json
{
  "hooks": {
    "beforeToolUse": {
      "bash": "/mnt/c/Windows/System32/cmd.exe /c \"cd /d C:\\Users\\yksh\\source\\clones\\win-UIAutomation-mcp && $COMMAND\""
    }
  }
}
```

This hook automatically wraps bash commands with the Windows cmd.exe wrapper, allowing seamless .NET development from WSL.

## Common Issues

- **Build warnings about UIAutomation assemblies**: Expected on some systems, doesn't prevent functionality
- **MCP protocol interference**: Console logging is disabled to prevent interference with MCP JSON-RPC communication
- **WSL compatibility**: All .NET commands must be executed through Windows cmd.exe wrapper

## Screenshot Capabilities

The TakeScreenshot tool includes advanced features:
- **Token optimization**: Automatically scales image resolution based on maxTokens parameter
- **JPEG compression**: Reduces file size while maintaining quality
- **Base64 encoding**: Direct embedding in MCP responses for immediate use
- **Window-specific capture**: Can capture specific windows or full desktop