# JSON Serialization Patterns for MCP Tools

This document explains the unified serialization pattern used in the UI Automation MCP Tools implementation.

## Overview

The UI Automation MCP Tools now use a single, unified pattern for JSON serialization. This ensures consistency, maintainability, and prevents runtime errors.

## Unified Serialization Pattern (ALL services)

### Characteristics:
- **Interface**: `Task<ServerEnhancedResponse<T>>`
- **Implementation**: Returns the `ServerEnhancedResponse<T>` object directly
- **MCP Tools**: **MUST** use `JsonSerializationHelper.Serialize()` to prevent "JsonTypeInfo metadata was not provided" errors
- **Error if not implemented**: Runtime JSON serialization errors

### Implementation Pattern:
```csharp
[McpServerTool]
public async Task<object> SomeMethod(string param)
    => JsonSerializationHelper.Serialize(await _someService.SomeMethodAsync(param));
```

## Implementation Guidelines

When adding new MCP tools:

1. **All services use Pattern 1**: `Task<ServerEnhancedResponse<T>>`
2. **Service implementation**: Returns `ServerEnhancedResponse<T>` directly
3. **MCP Tools layer**: Always use `JsonSerializationHelper.Serialize()` wrapper
4. **Test**: Ensure proper JSON serialization without errors

## Common Errors

### "JsonTypeInfo metadata was not provided"
- **Cause**: Missing `JsonSerializationHelper.Serialize()` wrapper in MCP Tools layer
- **Solution**: Add `JsonSerializationHelper.Serialize()` wrapper

## Benefits of Unified Pattern

1. **Consistency**: All services follow the same pattern
2. **Maintainability**: Easy to understand and maintain
3. **Type Safety**: Strongly typed `ServerEnhancedResponse<T>` instead of `Task<object>`
4. **Debugging**: Easier to trace issues with consistent approach
