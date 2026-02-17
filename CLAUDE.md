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

## Cache Strategy

### Policy: Server-Side Automatic — NOT Exposed to LLM Tool Args

Caching is an internal performance optimization. LLM cannot reliably judge UI dynamism, and adding cache args would cause confusion/hallucination. The `EnableCacheOptimization` setting in `appsettings.json` provides administrator-level control.

### Category Classification

| Category | Operations | Cache | Rationale |
|----------|-----------|-------|-----------|
| **A. Bulk Read** | `GetElementTree`, `SearchElements` | **Always cached** (when `EnableCacheOptimization=true`) | Traverses hundreds/thousands of elements. `.Cached` properties eliminate per-property COM cross-process calls. Snapshot semantics are acceptable — these operations inherently capture a point-in-time view |
| **B. Single-Element Read** | `FindItemByProperty`, `GetGridInfo`, `FindText`, etc. | **No cache** | Single element lookup; `CacheRequest` overhead outweighs benefit. Fresh data is safer |
| **C. Action** | `InvokeElement`, `SetValue`, `Toggle`, `Scroll`, etc. | **No cache** | Target element must reflect latest state. Freshness is critical |

### Hybrid Approach in `ElementInfoBuilder.CreateElementInfo`

`CreateElementInfo(element, ..., useCached)` uses a single unified method:
- **Basic properties** (Name, AutomationId, ClassName, ControlType, IsEnabled, etc.): `.Cached` or `.Current` based on `useCached` flag
- **SupportedPatterns**: Always `.Current` (`GetSupportedPatterns()` is 1 COM call; patterns are not in the CacheRequest)
- **NativeWindowHandle**: Always `.Current` (unreliable from cache)
- **Table/TableItem headers**: Always `.Current` (header elements are outside CacheRequest scope)

### Guidelines for New Operations

- Bulk traversal returning multiple `ElementInfo` → use `useCached: true` with `CacheRequest`
- Single element lookup or action → use default `useCached: false`
- Never expose cache control as MCP tool arguments

# important-instruction-reminders
Do what has been asked; nothing more, nothing less.
NEVER create files unless they're absolutely necessary for achieving your goal.
ALWAYS prefer editing an existing file to creating a new one.
NEVER proactively create documentation files (*.md) or README files. Only create documentation files if explicitly requested by the User.