# Testing Commands

## MCP Server Testing

### ✅ Recommended Testing Methods (Verified Working)

```bash
cd UIAutomationMCP.Server

# Method 1: Sleep-based testing (BEST - Always works)
(echo '{"jsonrpc": "2.0", "id": 1, "method": "initialize", "params": {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "test", "version": "1.0"}}}'; sleep 2) | dotnet run --configuration Release

# Method 2: EOF marker (Works with expected JSON parse error)
(echo '{"jsonrpc": "2.0", "id": 1, "method": "initialize", "params": {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "test", "version": "1.0"}}}'; sleep 3; echo 'EOF') | dotnet run --configuration Release

# Expected successful response:
# {"result":{"protocolVersion":"2024-11-05","capabilities":{"logging":{},"tools":{"listChanged":true}},"serverInfo":{"name":"UIAutomation MCP Server","version":"1.0.0"}},"id":1,"jsonrpc":"2.0"}
```

### ❌ Methods with Limitations

```bash
# Timeout method - May result in empty stdout due to buffering
echo '{"jsonrpc": "2.0", "id": 1, "method": "initialize", "params": {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "test", "version": "1.0"}}}' | timeout 5s dotnet run --configuration Release

# File-based debugging - stderr works, stdout may be empty
echo 'JSON_REQUEST' > input.json && dotnet run --configuration Release 2>debug.log 1>response.json < input.json
```

### 🔍 Multiple Operations Testing

```bash
# Test initialization + tools list
(echo '{"jsonrpc": "2.0", "id": 1, "method": "initialize", "params": {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "test", "version": "1.0"}}}'; echo '{"jsonrpc": "2.0", "id": 2, "method": "tools/list", "params": {}}'; sleep 3) | dotnet run --configuration Release
```

### 📋 STDOUT Buffering Issue Explanation

**Problem**: .NET MCP framework controls stdout buffering internally, causing empty output on rapid process termination.

**Root Cause**: MCP framework's async JSON-RPC response writing vs. process lifecycle timing.

**Solution**: Use sleep-based methods to allow response completion before process termination.

**Important**: This is normal behavior. Production MCP clients (Claude, etc.) handle async responses correctly.
