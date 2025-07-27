# Windows Command Execution

You can execute Windows commands from WSL using:
```bash
/mnt/c/Windows/System32/cmd.exe /c [windows-command]
```

This allows running Windows-specific commands and utilities from the Linux environment.

# Testing Commands

## Single-Command MCP Server Test (stdin closure resolved):
```bash
cd UIAutomationMCP.Server
# Simple initialization test
echo '{"jsonrpc": "2.0", "id": 1, "method": "initialize", "params": {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "test", "version": "1.0"}}}' | dotnet run --configuration Release

# Test with multiple operations
(echo '{"jsonrpc": "2.0", "id": 1, "method": "initialize", "params": {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "test", "version": "1.0"}}}'; echo '{"jsonrpc": "2.0", "id": 2, "method": "tools/call", "params": {"name": "GetElementTree", "arguments": {"maxDepth": 2, "processId": 0}}}') | dotnet run --configuration Release
```
