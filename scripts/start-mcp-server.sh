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
    if tasklist //fi "PID eq $OLD_PID" 2>/dev/null | grep -q "$OLD_PID"; then
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

# Use PowerShell to start the server and capture PID with logging
powershell -Command "
\$ErrorActionPreference = 'Stop'
try {
    \$psi = New-Object System.Diagnostics.ProcessStartInfo
    \$psi.FileName = 'dotnet'
    \$psi.Arguments = 'run --project $PROJECT_PATH'
    \$psi.RedirectStandardInput = \$true
    \$psi.RedirectStandardOutput = \$true
    \$psi.RedirectStandardError = \$true
    \$psi.UseShellExecute = \$false
    \$psi.CreateNoWindow = \$true

    \$process = [System.Diagnostics.Process]::Start(\$psi)
    if (-not \$process) {
        throw 'Failed to start dotnet process'
    }
    
    Write-Host \"Server started with PID: \$(\$process.Id)\"
    \$process.Id | Out-File -FilePath '$PID_FILE' -Encoding ASCII

    # Store process handle for later use
    \$global:mcpServerProcess = \$process

    # Start logging stderr in background
    \$stderrTask = \$process.StandardError.ReadToEndAsync()
    \$global:stderrTask = \$stderrTask

    # Test initial connection
    Start-Sleep -Seconds 3
\$initRequest = '{\"jsonrpc\": \"2.0\", \"id\": 1, \"method\": \"initialize\", \"params\": {\"protocolVersion\": \"2024-11-05\", \"capabilities\": {}, \"clientInfo\": {\"name\": \"bash-client\", \"version\": \"1.0\"}}}'
\$process.StandardInput.WriteLine(\$initRequest)
\$process.StandardInput.Flush()

\$responseTask = \$process.StandardOutput.ReadLineAsync()
\$timeout = [System.Threading.Tasks.Task]::Delay(5000)
\$completedTask = [System.Threading.Tasks.Task]::WhenAny(\$responseTask, \$timeout).Result

if (\$completedTask -eq \$responseTask) {
    \$response = \$responseTask.Result
    \$responseObj = \$response | ConvertFrom-Json -ErrorAction SilentlyContinue
    if (\$responseObj.result.serverInfo) {
        Write-Host \"✅ Server responding: \$(\$responseObj.result.serverInfo.name) v\$(\$responseObj.result.serverInfo.version)\"
    }
} else {
    Write-Host \"⚠️ Server started but may not be fully ready yet\"
}

# Keep PowerShell session alive to maintain server process
Write-Host \"Server is running in background. Use Ctrl+C to stop or run stop-mcp-server.sh\"
Write-Host \"Process will remain active even if this PowerShell session ends.\"

# Simple keep-alive loop
while (\$true) {
    if (\$process.HasExited) {
        Write-Host \"Server process has exited\"
        Remove-Item '$PID_FILE' -ErrorAction SilentlyContinue
        break
    }
    Start-Sleep -Seconds 5
}

} catch {
    Write-Host \"❌ Failed to start server: \$_\"
    if (\$process -and -not \$process.HasExited) {
        \$process.Kill()
    }
    Remove-Item '$PID_FILE' -ErrorAction SilentlyContinue
    exit 1
}
" &

POWERSHELL_PID=$!
echo $POWERSHELL_PID > "powershell.pid"

# Wait a moment for server to start
sleep 4

if [ -f "$PID_FILE" ]; then
    SERVER_PID=$(cat "$PID_FILE")
    echo "✅ MCP Server started successfully!"
    echo "   Server PID: $SERVER_PID"
    echo "   PowerShell PID: $POWERSHELL_PID"
    echo ""
    echo "📋 Usage:"
    echo "   ./scripts/send-mcp-request.sh 'tools/list'"
    echo "   ./scripts/send-mcp-request.sh 'tools/call' 'TakeScreenshot' '{\"maxTokens\": 1000}'"
    echo "   ./scripts/stop-mcp-server.sh"
    echo ""
    echo "Server is ready for JSON-RPC requests!"
else
    echo "❌ Failed to start server"
    exit 1
fi