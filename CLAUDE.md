# Testing Commands

## MCP Server Testing (Development Environment)

### Shell Script Testing Method (Git Bash Background Operation)

**⚠️ Limitation**: This method also creates new server instances per request, making it unsuitable for session persistence testing.

### Background Server Management

For advanced development workflows, these shell scripts provide complete background server management with comprehensive logging:

**Available Scripts (in `scripts/` folder):**
- `scripts/start-mcp-server.sh` - Start MCP server in background with logging
- `scripts/send-mcp-request.sh` - Send JSON-RPC requests to running server  
- `scripts/show-mcp-logs.sh` - Display server logs while running
- `scripts/stop-mcp-server.sh` - Stop server and show final logs

### Usage Workflow

```bash
# 1. Start server in background
./scripts/start-mcp-server.sh

# 2. Send requests (server runs in background)  
./scripts/send-mcp-request.sh 'tools/list'
./scripts/send-mcp-request.sh 'tools/call' 'TakeScreenshot' '{"maxTokens": 1000}'

# 3. View logs while running
./scripts/show-mcp-logs.sh  

# 4. Stop server when done
./scripts/stop-mcp-server.sh
```

### Key Features

✅ **Background Operation**: Server runs independently, allowing Git Bash return to prompt  
✅ **Comprehensive Logging**: Captures both stdout/stderr and response logs  
✅ **Process Management**: Tracks PIDs for reliable start/stop operations  
✅ **Error Handling**: Graceful cleanup and error reporting  
✅ **Cross-platform**: Works in Git Bash on Windows

### Expected Outputs

**Server Start:**
```
=== MCP Server Background Starter ===
Starting MCP server in background...
✅ Server responding: UIAutomation MCP Server v1.0.0
✅ MCP Server started successfully!
   Server PID: 12345
   PowerShell PID: 67890

📋 Usage:
   ./scripts/send-mcp-request.sh 'tools/list'
   ./scripts/send-mcp-request.sh 'tools/call' 'TakeScreenshot' '{"maxTokens": 1000}'
   ./scripts/stop-mcp-server.sh

Server is ready for JSON-RPC requests!
```

**Request Example:**
```bash
./scripts/send-mcp-request.sh 'tools/list'
# Returns: {"result":{"tools":[{"name":"SearchElements",...}]},"id":2,"jsonrpc":"2.0"}
```

**Log Monitoring:**
```
=== MCP Server Logs ===
Server PID: 12345
✅ Server is running
📄 Development Error Log:
--- UIAutomationMCP.Server/dev-error.log ---
[Recent error entries...]
📄 Development Response Log:  
--- UIAutomationMCP.Server/dev-response.log ---
[Recent response entries...]
```

### Implementation Notes

- Uses PowerShell subprocess for reliable process management on Windows
- Captures PID files for background operation tracking  
- Provides separate error and response log files for debugging
- Automatically cleans up processes and temporary files on shutdown
- Based on GitHub Actions workflow patterns from `.github/workflows/staging-test.yml`
