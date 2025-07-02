# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a C# UIAutomation MCP (Model Context Protocol) Server that exposes Windows UI Automation functionality through MCP tools. The server allows MCP clients to interact with Windows applications for testing and automation purposes.

## Build and Run Commands

```bash
# Build the project (suppress output to avoid MCP protocol interference)
dotnet build UiAutomationMcpServer.csproj --verbosity quiet --nologo

# Run the server (use built executable to avoid build output)
./bin/Debug/net8.0-windows/UiAutomationMcpServer.exe

# Restore packages
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
/mnt/c/Windows/System32/cmd.exe /c "cd /d C:\Users\yksh\source\clones\win-UIAutomation-mcp && dotnet build UiAutomationMcpServer.csproj --verbosity quiet --nologo"
```

**Critical for MCP**: Always use the built executable directly instead of `dotnet run` to avoid build output interfering with MCP protocol:
```bash
/mnt/c/Windows/System32/cmd.exe /c "cd /d C:\Users\yksh\source\clones\win-UIAutomation-mcp && .\\bin\\Debug\\net8.0-windows\\UiAutomationMcpServer.exe"
```

## Architecture

The application is structured as a modern .NET hosted service with proper separation of concerns:

### **Project Structure**
- **Program.cs**: Main entry point using Microsoft.Extensions.Hosting
- **Models/**: Data models for MCP requests/responses and UIAutomation entities
- **Services/**: Core business logic and interfaces
  - `IUIAutomationService`: Service interface for UI automation operations
  - `UIAutomationService`: Implementation of UI automation functionality
  - `UIAutomationTools`: MCP tool definitions with attribute-based registration

### **MCP Server Implementation**
1. **Hosted Service Pattern**: Uses `Microsoft.Extensions.Hosting` for proper application lifecycle
2. **Dependency Injection**: Leverages `Microsoft.Extensions.DependencyInjection` for service registration
3. **Structured Logging**: Uses `Microsoft.Extensions.Logging` for comprehensive logging
4. **Attribute-Based Tools**: Uses `[McpServerToolType]` and `[McpServerTool]` for automatic tool discovery

### **Tool Registration**
Each UI automation capability is exposed as an MCP tool:
- `GetWindowInfo` - Retrieve information about open windows
- `GetElementInfo` - Get UI element details within windows
- `ClickElement` - Click UI elements by AutomationId or Name
- `SendKeys` - Send keyboard input to elements
- `MouseClick` - Perform mouse clicks at coordinates
- `TakeScreenshot` - Capture desktop or window screenshots
- `LaunchApplication` - Launch applications with parameters
- `FindElements` - Search for UI elements by criteria

### **Service Layer**
- **UIAutomationService**: Core service implementing all automation logic
- **Dependency Injection**: Tools receive services through constructor injection
- **Async/Await Pattern**: All operations are async for better performance
- **Structured Results**: Consistent response models for all operations

## MCP Protocol Implementation

Uses the official `ModelContextProtocol` NuGet package with:
- **STDIO Transport**: Communication over stdin/stdout
- **Automatic Tool Discovery**: `WithToolsFromAssembly()` for attribute-based registration
- **Type-Safe Operations**: Strongly-typed request/response models
- **Proper Error Handling**: Structured error responses with logging

## Key Dependencies

- **ModelContextProtocol**: Core MCP functionality (prerelease version)
- **Microsoft.Extensions.Hosting**: Application hosting and lifecycle management
- **Microsoft.Extensions.Logging**: Structured logging framework
- **Microsoft.Extensions.DependencyInjection**: Dependency injection container
- **System.Windows.Automation**: Windows UI Automation APIs
- **System.Windows.Forms**: Windows Forms for UI interaction
- **System.Drawing.Common**: Graphics and screenshot capabilities

## Tool Implementation Pattern

Each MCP tool follows this pattern:
1. Parameter validation from the MCP client
2. UI element discovery using AutomationElement APIs
3. Action execution (click, input, screenshot, etc.)
4. Structured response with success/error status

The server handles both element-based operations (using AutomationId/Name) and coordinate-based operations for flexibility.

## Debugging

Debug logging is enabled to `%TEMP%\mcp-server-debug.log` to track:
- Server startup
- Incoming MCP requests
- Outgoing responses
- Errors and exceptions

This logging is essential for troubleshooting MCP protocol communication issues without interfering with the JSON-RPC communication on stdout/stdin.

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

Example hook for common .NET commands:
```json
{
  "hooks": {
    "beforeToolUse": {
      "bash": {
        "dotnet *": "/mnt/c/Windows/System32/cmd.exe /c \"cd /d C:\\Users\\yksh\\source\\clones\\win-UIAutomation-mcp && dotnet $ARGS\"",
        "msbuild *": "/mnt/c/Windows/System32/cmd.exe /c \"cd /d C:\\Users\\yksh\\source\\clones\\win-UIAutomation-mcp && msbuild $ARGS\""
      }
    }
  }
}
```

## Common Issues

- **Build warnings about UIAutomation assemblies**: Expected on some systems, doesn't prevent functionality
- **MCP protocol interference**: Console logging is disabled to prevent interference with MCP JSON-RPC communication
- **WSL compatibility**: All .NET commands must be executed through Windows cmd.exe wrapper

## Debugging

For debugging purposes, logging has been disabled to avoid interference with the MCP protocol. If debugging is needed:
1. Temporarily enable file logging in Program.cs
2. Use Visual Studio debugger for step-through debugging
3. Add temporary console output only during development (remove before deployment)