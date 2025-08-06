# Testing Commands

## MCP Server Testing (Development Environment)

**⚠️ Note**: This method creates new server instances per request and cannot test continuous processes like monitoring functionality.

### MCP Inspector CLI Testing

The recommended way to test the MCP server is using the official MCP Inspector CLI tool.

#### Installation

```bash
npm install -g @modelcontextprotocol/inspector
```

#### Usage

```bash
# Test the server using dotnet run (from project root)
mcp-inspector --cli "dotnet run --project UIAutomationMCP.Server --configuration Debug"

# List available tools
mcp-inspector --cli "dotnet run --project UIAutomationMCP.Server --configuration Debug" --method tools/list

# Call specific tools
mcp-inspector --cli "dotnet run --project UIAutomationMCP.Server --configuration Debug" --method tools/call --tool-name TakeScreenshot --tool-arg maxTokens=1000

# Search for elements
mcp-inspector --cli "dotnet run --project UIAutomationMCP.Server --configuration Debug" --method tools/call --tool-name SearchElements --tool-arg query=button --tool-arg maxResults=5
```

#### Key Benefits

✅ **Pure CLI Operation**: No GUI, perfect for automation and scripting  
✅ **JSON Output**: Structured responses for easy parsing  
✅ **Server Logs**: View server stderr output for debugging  
✅ **Flexible Testing**: Test any tool with any parameters  

#### Monitoring Server Logs

**⚠️ Important**: MCP Inspector CLI does not display server stderr output, so additional logging setup is required for debugging.

The MCP server outputs detailed logs to stderr, providing insight into:

- Tool execution flow
- Error messages and stack traces  
- Performance timing
- Subprocess communication

**MCP Log Notifications Status:**
- MCP Inspector CLI supports displaying server logs via the `serverLogs` field in responses
- However, the current MCP C# SDK version (0.3.0-preview.3) does not include the `AsClientLoggerProvider()` extension method needed for proper MCP log notifications
- The server currently falls back to stderr logging, which is not captured by MCP Inspector CLI
- File logging is therefore essential for development debugging

#### Method 1: File Logging (Essential for Development)

**⚠️ Note**: MCP Inspector CLI does not display server stderr output or MCP log notifications, making file logging essential for debugging.

Enable file logging using environment variables:

```bash
# Enable debug file logging
MCP_DEBUG_FILE_LOGGING=true MCP_DEBUG_LOG_PATH=mcp-debug.log mcp-inspector --cli "dotnet run --project UIAutomationMCP.Server --configuration Debug" --method tools/call --tool-name TakeScreenshot --tool-arg maxTokens=500

# View the log file in real-time (separate terminal)
tail -f mcp-debug.log

# Or view after execution
cat mcp-debug.log
```

**Environment Variables:**
- `MCP_DEBUG_FILE_LOGGING=true` - Enables comprehensive file logging (captures ALL server logs)
- `MCP_DEBUG_LOG_PATH=filename.log` - Sets custom log file path (optional, defaults to `mcp-debug.log`)

**Comprehensive Log Content:**
When enabled, file logging captures complete server execution details:
- ✅ **MCP Protocol**: Server initialization, client handshake, method calls, shutdown
- ✅ **Process Management**: Worker/Monitor subprocess lifecycle and paths  
- ✅ **Service Operations**: Screenshot optimization, element search, UI automation actions
- ✅ **Performance Data**: Execution timing, memory usage, image compression details
- ✅ **Error Diagnostics**: Stack traces, subprocess communication errors, DI container status
- ✅ **All Log Levels**: TRACE, DEBUG, INFORMATION, WARNING, ERROR, CRITICAL

#### Method 2: Direct Server Testing with stderr Capture

For detailed debugging, run the server directly:

```bash
# Capture both JSON response and server logs
echo '{"jsonrpc":"2.0","id":1,"method":"tools/list"}' | dotnet run --project UIAutomationMCP.Server --configuration Debug 2>server-logs.txt 1>response.json

# View server logs
cat server-logs.txt

# View JSON response  
cat response.json
```

#### Method 3: Combined Approach

For comprehensive testing with both JSON responses and logging:

```bash
# Terminal 1: Run test with file logging
MCP_DEBUG_FILE_LOGGING=true MCP_DEBUG_LOG_PATH=debug.log mcp-inspector --cli "dotnet run --project UIAutomationMCP.Server --configuration Debug" --method tools/call --tool-name SearchElements --tool-arg query=button > response.json

# Terminal 2: Monitor logs in real-time
tail -f debug.log

# Analysis
echo "=== Tool Response ===" && cat response.json
echo "=== Server Logs ===" && cat debug.log
```

#### Advanced Usage

```bash
# Save JSON output to file for analysis
mcp-inspector --cli "dotnet run --project UIAutomationMCP.Server --configuration Debug" --method tools/list > tools-output.json

# Test multiple tools in sequence
mcp-inspector --cli "dotnet run --project UIAutomationMCP.Server --configuration Debug" --method tools/call --tool-name GetElementTree --tool-arg maxDepth=2
```
