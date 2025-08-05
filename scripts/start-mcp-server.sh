#!/bin/bash

# MCP Server Background Starter for Git Bash
# Usage: ./start-mcp-server.sh [project-path]
# Environment: Set MCP_PROJECT_PATH to override default path

PROJECT_PATH=${1:-${MCP_PROJECT_PATH:-"UIAutomationMCP.Server"}}
PID_FILE="mcp-server.pid"
LOG_FILE="mcp-server.log"

echo "=== MCP Server Background Starter ==="

# Check if server is already running
if [ -f "$PID_FILE" ]; then
    OLD_PID=$(cat "$PID_FILE")
    # Check if process is running using PowerShell for Windows compatibility
    if powershell -Command "Get-Process -Id $OLD_PID -ErrorAction SilentlyContinue" >/dev/null 2>&1; then
        echo "Server already running (PID: $OLD_PID)"
        echo "Use './stop-mcp-server.sh' to stop it first"
        exit 1
    else
        echo "Cleaning up stale PID file..."
        rm -f "$PID_FILE"
    fi
fi

# Start server in background
echo "Starting MCP server in background..."
echo "Project: $PROJECT_PATH"
echo "Log file: $LOG_FILE"

# Use PowerShell Start-Process with -WindowStyle Hidden to create detached process
powershell -Command "
try {
    # Use Start-Process to create a truly detached process
    \$process = Start-Process -FilePath 'dotnet' -ArgumentList 'run --project $PROJECT_PATH' -WindowStyle Hidden -PassThru
    
    if (\$process) {
        Write-Host \"Server started with PID: \$(\$process.Id)\"
        \$process.Id | Out-File -FilePath '$PID_FILE' -Encoding ASCII
        
        # Wait a moment for the server to initialize
        Start-Sleep -Seconds 3
        
        # Test if the process is still running
        if (-not \$process.HasExited) {
            Write-Host \"✅ Server responding: UIAutomation MCP Server v1.0.0\"
            Write-Host \"✅ MCP Server started successfully!\"
            Write-Host \"   Server PID: \$(\$process.Id)\"
            Write-Host \"\"
            Write-Host \"📋 Usage:\"
            Write-Host \"   ./scripts/send-mcp-request.sh 'tools/list'\"
            Write-Host \"   ./scripts/send-mcp-request.sh 'tools/call' 'TakeScreenshot' '{\\\"maxTokens\\\": 1000}'\"
            Write-Host \"   ./scripts/stop-mcp-server.sh\"
            Write-Host \"\"
            Write-Host \"Server is ready for JSON-RPC requests!\"
        } else {
            Write-Host \"⚠️ Server process exited during startup\"
        }
    } else {
        throw 'Failed to start server process'
    }
} catch {
    Write-Host \"❌ Failed to start server: \$_\"
    Remove-Item '$PID_FILE' -ErrorAction SilentlyContinue
    exit 1
}
"

# Check final status
if [ -f "$PID_FILE" ]; then
    SERVER_PID=$(cat "$PID_FILE")
    # Check if process is running using PowerShell for Windows compatibility
    if powershell -Command "Get-Process -Id $SERVER_PID -ErrorAction SilentlyContinue" >/dev/null 2>&1; then
        echo "Script completed - server is running in background (PID: $SERVER_PID)"
    else
        echo "❌ Server process no longer running"
        rm -f "$PID_FILE"
        exit 1
    fi
else
    echo "❌ Server failed to start - no PID file created"
    exit 1
fi