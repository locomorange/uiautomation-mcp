#!/bin/bash

# Send JSON-RPC request to running MCP server
# Usage: ./send-mcp-request.sh <method> [tool-name] [arguments-json]

METHOD="$1"
TOOL_NAME="$2"
ARGS_JSON="$3"
PID_FILE="mcp-server.pid"

if [ -z "$METHOD" ]; then
    echo "Usage: $0 <method> [tool-name] [arguments-json]"
    echo ""
    echo "Examples:"
    echo "  $0 'tools/list'"
    echo "  $0 'tools/call' 'TakeScreenshot'"
    echo "  $0 'tools/call' 'SearchElements' '{\"searchText\": \"Calculator\"}'"
    echo "  $0 'tools/call' 'TakeScreenshot' '{\"maxTokens\": 1000}'"
    exit 1
fi

# Check if server is running
if [ ! -f "$PID_FILE" ]; then
    echo "❌ Server not running. Start with ./start-mcp-server.sh"
    exit 1
fi

SERVER_PID=$(cat "$PID_FILE")
if ! tasklist //fi "PID eq $SERVER_PID" 2>/dev/null | grep -q "$SERVER_PID"; then
    echo "❌ Server process (PID: $SERVER_PID) not found"
    rm -f "$PID_FILE"
    exit 1
fi

# Generate random ID
REQUEST_ID=$((RANDOM % 1000 + 1))

# Build JSON request
if [ "$METHOD" = "tools/call" ] && [ -n "$TOOL_NAME" ]; then
    if [ -n "$ARGS_JSON" ]; then
        JSON_REQUEST="{\"jsonrpc\": \"2.0\", \"id\": $REQUEST_ID, \"method\": \"$METHOD\", \"params\": {\"name\": \"$TOOL_NAME\", \"arguments\": $ARGS_JSON}}"
    else
        JSON_REQUEST="{\"jsonrpc\": \"2.0\", \"id\": $REQUEST_ID, \"method\": \"$METHOD\", \"params\": {\"name\": \"$TOOL_NAME\", \"arguments\": {}}}"
    fi
else
    if [ -n "$ARGS_JSON" ]; then
        JSON_REQUEST="{\"jsonrpc\": \"2.0\", \"id\": $REQUEST_ID, \"method\": \"$METHOD\", \"params\": $ARGS_JSON}"
    else
        JSON_REQUEST="{\"jsonrpc\": \"2.0\", \"id\": $REQUEST_ID, \"method\": \"$METHOD\", \"params\": {}}"
    fi
fi

echo "📤 Sending request to server (PID: $SERVER_PID):"
echo "   $JSON_REQUEST"
echo ""

# Send request using PowerShell
powershell -Command "
# Find the running server process
\$serverPid = $SERVER_PID
\$processes = Get-Process | Where-Object { \$_.Id -eq \$serverPid }

if (\$processes.Count -eq 0) {
    Write-Host '❌ Server process not found'
    exit 1
}

# Try to connect to the server's stdin using named pipes or direct process communication
# Since we can't easily access the original process stdin, we'll use a file-based approach

# Create a temporary request file
\$requestFile = 'temp-request.json'
\$responseFile = 'temp-response.json'

'$JSON_REQUEST' | Out-File -FilePath \$requestFile -Encoding UTF8

# Use a new dotnet process to send the request
Write-Host '📡 Connecting to server...'
try {
    \$psi = New-Object System.Diagnostics.ProcessStartInfo
    \$psi.FileName = 'dotnet'
    \$psi.Arguments = 'run --project UIAutomationMCP.Server'
    \$psi.RedirectStandardInput = \$true
    \$psi.RedirectStandardOutput = \$true
    \$psi.RedirectStandardError = \$true
    \$psi.UseShellExecute = \$false
    \$psi.CreateNoWindow = \$true
    
    \$tempProcess = [System.Diagnostics.Process]::Start(\$psi)
    Start-Sleep -Seconds 2
    
    \$tempProcess.StandardInput.WriteLine('$JSON_REQUEST')
    \$tempProcess.StandardInput.Flush()
    
    \$responseTask = \$tempProcess.StandardOutput.ReadLineAsync()
    \$timeout = [System.Threading.Tasks.Task]::Delay(10000)
    \$completedTask = [System.Threading.Tasks.Task]::WhenAny(\$responseTask, \$timeout).Result
    
    if (\$completedTask -eq \$responseTask) {
        \$response = \$responseTask.Result
        Write-Host '📥 Response received:'
        Write-Host \$response
        
        # Try to format JSON nicely
        try {
            \$responseObj = \$response | ConvertFrom-Json
            Write-Host ''
            Write-Host '📋 Formatted response:' -ForegroundColor Cyan
            \$responseObj | ConvertTo-Json -Depth 10
        } catch {
            Write-Host 'Could not format JSON response'
        }
    } else {
        Write-Host '⚠️ Request timed out after 10 seconds'
    }
    
    \$tempProcess.StandardInput.Close()
    \$tempProcess.WaitForExit(2000)
    if (!\$tempProcess.HasExited) {
        \$tempProcess.Kill()
    }
    
} catch {
    Write-Host \"❌ Request failed: \$_\"
} finally {
    # Cleanup
    Remove-Item \$requestFile -ErrorAction SilentlyContinue
    Remove-Item \$responseFile -ErrorAction SilentlyContinue
}
"

echo ""
echo "✅ Request completed!"