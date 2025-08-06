# UIAutomation MCP

## Project Overview

UIAutomation MCP is a Windows UI Automation server that implements the Model Context Protocol (MCP), enabling AI assistants and other applications to interact with Windows desktop applications programmatically. This server provides comprehensive access to Windows UI Automation capabilities through a standardized protocol interface.

**Key Features:**
- **Complete UI Automation Access**: Interact with any Windows application that supports UI Automation
- **Microsoft Learn Compliant**: Follows official Microsoft guidelines for UI Automation control patterns
- **Native AOT Performance**: Optimized for fast startup and low memory usage
- **Multi-Process Architecture**: Secure separation between MCP protocol handling and UI operations
- **Comprehensive Control Support**: Buttons, text fields, lists, grids, and all standard Windows controls

**Use Cases:**
- Automated testing of Windows applications
- AI-powered desktop automation
- Accessibility tool development
- Application integration and workflow automation

## Installation & Setup

### Prerequisites
- Windows 10/11 (UI Automation requires Windows)
- .NET 9.0 Runtime (automatically installed with winget)

### Install via Winget

```powershell
# Install UIAutomation MCP server
winget install Locomorange.UIAutomationMCP
```

### MCP Server Configuration

Add the UIAutomation MCP server to your MCP client configuration. For most MCP clients, add this to your MCP settings JSON file:

```json
{
  "mcpServers": {
    "uiautomation": {
      "command": "uiautomation-mcp",
      "args": [],
      "env": {}
    }
  }
}
```

For Claude Desktop, add to `%APPDATA%\Claude\claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "uiautomation": {
      "command": "C:\\Program Files\\locomorange\\uiautomation-mcp\\uiautomation-mcp.exe",
      "args": []
    }
  }
}
```

### Manual Installation

1. Download the latest release from the [GitHub releases page](https://github.com/locomorange/uiautomation-mcp/releases)
2. Extract to your preferred location
3. Add the executable path to your MCP client configuration

## Basic Usage Example

Once configured, you can use the UIAutomation MCP server through your MCP client. Here are some example commands:

### Take a Screenshot
```
Take a screenshot of the current desktop
```

### Find and Click a Button
```
Find a button with text "OK" and click it
```

### Enter Text in a Field
```
Find the text field with name "Username" and enter "john.doe"
```

### Get Window Information
```
List all open windows and their titles
```

### Navigate Application Menus
```
Open the File menu in Notepad and click "Save As"
```

The server provides comprehensive UI automation capabilities including:
- **Element Discovery**: Find UI elements by name, type, or properties
- **Interaction**: Click, type, select, and manipulate controls
- **Information Gathering**: Get text, properties, and state of UI elements
- **Window Management**: Focus, resize, and manage application windows
- **Grid and Table Operations**: Work with complex data displays

For detailed API documentation and advanced usage examples, see the project documentation.