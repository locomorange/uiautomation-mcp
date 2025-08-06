# Testing Commands

## MCP Server Testing

### MCP Inspector CLI Testing

```bash
# Install MCP Inspector
npm install -g @modelcontextprotocol/inspector

# Test the server
mcp-inspector --cli "dotnet run --project UIAutomationMCP.Server --configuration Debug"

# List tools
mcp-inspector --cli "dotnet run --project UIAutomationMCP.Server --configuration Debug" --method tools/list

# Call tools
mcp-inspector --cli "dotnet run --project UIAutomationMCP.Server --configuration Debug" --method tools/call --tool-name TakeScreenshot --tool-arg maxTokens=1000
```

### File Logging

**Default**: File logging is **OFF** in production (secure by default).

To enable file logging for debugging:
```bash
# Enable file logging explicitly
MCP_LOG_ENABLE_FILE=true dotnet run --project UIAutomationMCP.Server

# With custom settings
MCP_LOG_ENABLE_FILE=true MCP_LOG_FILE_PATH=debug.log MCP_LOG_FILE_FORMAT=json dotnet run
```

**Available Environment Variables:**
- `MCP_LOG_ENABLE_FILE=true` - Enable file logging (default: false)
- `MCP_LOG_FILE_PATH=filename.log` - Log file path (default: mcp-logs.json)
- `MCP_LOG_FILE_FORMAT=json|text` - Format (default: json)

## Logging Architecture

- **McpLoggerProvider**: Handles MCP notifications and file output
- **LogRelayService**: Relays subprocess logs to main logging system
- **Unified Flow**: All logs → McpLoggerProvider → MCP notifications + file output

# important-instruction-reminders
Do what has been asked; nothing more, nothing less.
NEVER create files unless they're absolutely necessary for achieving your goal.
ALWAYS prefer editing an existing file to creating a new one.
NEVER proactively create documentation files (*.md) or README files. Only create documentation files if explicitly requested by the User.