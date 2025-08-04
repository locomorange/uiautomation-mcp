#!/bin/bash

# Stop the background MCP server
PID_FILE="mcp-server.pid"
POWERSHELL_PID_FILE="powershell.pid"
PROJECT_PATH=${MCP_PROJECT_PATH:-"UIAutomationMCP.Server"}

echo "=== MCP Server Stopper ==="

# Stop the .NET server process
if [ -f "$PID_FILE" ]; then
    SERVER_PID=$(cat "$PID_FILE")
    echo "Stopping MCP server (PID: $SERVER_PID)..."
    
    if tasklist //fi "PID eq $SERVER_PID" 2>/dev/null | grep -q "$SERVER_PID"; then
        taskkill //PID "$SERVER_PID" //F 2>/dev/null
        echo "✅ Server process stopped"
    else
        echo "⚠️ Server process not found (may have already stopped)"
    fi
    
    rm -f "$PID_FILE"
else
    echo "⚠️ No server PID file found"
fi

# Stop the PowerShell wrapper process
if [ -f "$POWERSHELL_PID_FILE" ]; then
    POWERSHELL_PID=$(cat "$POWERSHELL_PID_FILE")
    echo "Stopping PowerShell wrapper (PID: $POWERSHELL_PID)..."
    
    if tasklist //fi "PID eq $POWERSHELL_PID" 2>/dev/null | grep -q "$POWERSHELL_PID"; then
        taskkill //PID "$POWERSHELL_PID" //F 2>/dev/null
        echo "✅ PowerShell wrapper stopped"
    else
        echo "⚠️ PowerShell wrapper not found (may have already stopped)"
    fi
    
    rm -f "$POWERSHELL_PID_FILE"
else
    echo "⚠️ No PowerShell PID file found"
fi

# Clean up any remaining dotnet processes related to our project
echo "Cleaning up any remaining dotnet processes..."
powershell -Command "
try {
    Get-Process | Where-Object { 
        \$_.ProcessName -like '*dotnet*' -and 
        (\$_.CommandLine -like '*$PROJECT_PATH*' -or \$_.CommandLine -like '*UIAutomationMCP*')
    } | ForEach-Object {
        Write-Host \"Stopping dotnet process: \$(\$_.Id) - \$(\$_.ProcessName)\"
        try {
            Stop-Process -Id \$_.Id -Force -ErrorAction Stop
            Write-Host \"✅ Successfully stopped process \$(\$_.Id)\"
        } catch {
            Write-Host \"⚠️ Could not stop process \$(\$_.Id): \$_\"
        }
    }
} catch {
    Write-Host \"⚠️ Error during process cleanup: \$_\"
}
" 2>/dev/null || true

# Show final logs before cleanup
echo ""
echo "📋 Final server logs:"
powershell -Command "
# Check development log files
\$devErrorLog = '$PROJECT_PATH/dev-error.log'
\$devResponseLog = '$PROJECT_PATH/dev-response.log'

try {
    if (Test-Path \$devErrorLog) {
        Write-Host ''
        Write-Host '=== Recent Error Log (last 10 lines) ==='
        Get-Content \$devErrorLog -Tail 10 -ErrorAction SilentlyContinue | ForEach-Object { Write-Host \$_ }
    } else {
        Write-Host 'No error log file found at \$devErrorLog'
    }

    if ((Test-Path \$devResponseLog) -and ((Get-Item \$devResponseLog -ErrorAction SilentlyContinue).Length -gt 0)) {
        Write-Host ''
        Write-Host '=== Recent Response Log (last 5 lines) ==='
        Get-Content \$devResponseLog -Tail 5 -ErrorAction SilentlyContinue | ForEach-Object { Write-Host \$_ }
    } else {
        Write-Host 'No response log file found or file is empty at \$devResponseLog'
    }
} catch {
    Write-Host \"⚠️ Error reading log files: \$_\"
}
"

# Clean up temporary files
rm -f temp-request.json temp-response.json mcp-server.log 2>/dev/null

echo ""
echo "🛑 MCP Server shutdown complete!"
echo ""
echo "💡 To start again: ./scripts/start-mcp-server.sh"
echo "💡 To view logs while running: ./scripts/show-mcp-logs.sh"