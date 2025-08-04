#!/bin/bash

# Show MCP Server logs
PID_FILE="mcp-server.pid"
LOG_FILE="mcp-server.log"

echo "=== MCP Server Logs ==="

if [ ! -f "$PID_FILE" ]; then
    echo "❌ Server not running. No PID file found."
    exit 1
fi

SERVER_PID=$(cat "$PID_FILE")
echo "Server PID: $SERVER_PID"

# Check if server is still running
if tasklist //fi "PID eq $SERVER_PID" 2>/dev/null | grep -q "$SERVER_PID"; then
    echo "✅ Server is running"
else
    echo "⚠️ Server process not found (may have stopped)"
fi

echo ""
echo "📋 Checking for server logs..."

# Use PowerShell to capture current stderr from the running process
powershell -Command "
# Try to get the process and its output
\$serverPid = $SERVER_PID
try {
    \$process = Get-Process -Id \$serverPid -ErrorAction Stop
    Write-Host '✅ Process found: \$(\$process.ProcessName)'
    Write-Host 'Process start time: \$(\$process.StartTime)'
    
    # Note: Cannot directly access stderr of running process
    # But we can check if log files exist
    
} catch {
    Write-Host '❌ Could not access process: \$_'
}

# Check for existing log files
if (Test-Path '$LOG_FILE') {
    Write-Host ''
    Write-Host '📄 Found log file: $LOG_FILE'
    Write-Host '--- Log Contents ---'
    Get-Content '$LOG_FILE' | ForEach-Object { Write-Host \$_ }
    Write-Host '--- End of Log ---'
} else {
    Write-Host '⚠️ No log file found at $LOG_FILE'
}

# Check the dev-error.log and dev-response.log files
\$devErrorLog = 'UIAutomationMCP.Server/dev-error.log'
\$devResponseLog = 'UIAutomationMCP.Server/dev-response.log'

if (Test-Path \$devErrorLog) {
    Write-Host ''
    Write-Host '📄 Development Error Log:'
    Write-Host '--- \$devErrorLog ---'
    Get-Content \$devErrorLog -Tail 20 | ForEach-Object { Write-Host \$_ }
    Write-Host '--- End of Error Log ---'
}

if (Test-Path \$devResponseLog) {
    Write-Host ''
    Write-Host '📄 Development Response Log:'
    Write-Host '--- \$devResponseLog ---'
    Get-Content \$devResponseLog -Tail 10 | ForEach-Object { Write-Host \$_ }
    Write-Host '--- End of Response Log ---'
}
"

echo ""
echo "💡 Logs are continuously written while server is running."
echo "💡 Use './stop-mcp-server.sh' to stop the server and see final logs."