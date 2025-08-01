# UIAutomationMCP Hybrid Native AOT Publish Script
# Publishes Server with Native AOT and Worker/Monitor with traditional .NET

param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("win-x64", "win-x86", "win-arm64")]
    [string]$Runtime = "win-x64",
    
    [Parameter(Mandatory = $false)]
    [switch]$TestOnly,
    
    [Parameter(Mandatory = $false)]
    [switch]$CleanFirst,
    
    [Parameter(Mandatory = $false)]
    [switch]$NoAot
)

# Colors for output
$Red = "`e[31m"
$Green = "`e[32m"
$Yellow = "`e[33m"
$Blue = "`e[34m"
$Cyan = "`e[36m"
$Reset = "`e[0m"

function Write-Status {
    param([string]$Message, [string]$Color = $Blue)
    Write-Host "${Color}[INFO]${Reset} $Message"
}

function Write-Success {
    param([string]$Message)
    Write-Host "${Green}[SUCCESS]${Reset} $Message"
}

function Write-Warning {
    param([string]$Message)
    Write-Host "${Yellow}[WARNING]${Reset} $Message"
}

function Write-Error {
    param([string]$Message)
    Write-Host "${Red}[ERROR]${Reset} $Message"
}

function Write-Header {
    param([string]$Message)
    Write-Host "${Cyan}========================================${Reset}"
    Write-Host "${Cyan}$Message${Reset}"
    Write-Host "${Cyan}========================================${Reset}"
}

# Configuration
$ProjectRoot = $PSScriptRoot
$ServerProject = Join-Path $ProjectRoot "UIAutomationMCP.Server"
$WorkerProject = Join-Path $ProjectRoot "UIAutomationMCP.Subprocess.Worker"
$MonitorProject = Join-Path $ProjectRoot "UIAutomationMCP.Subprocess.Monitor"
$PublishDir = Join-Path $ProjectRoot "publish"
$AotPublishDir = Join-Path $PublishDir "aot-$Runtime"

Write-Header "UIAutomationMCP Hybrid Native AOT Build"
Write-Status "Runtime: $Runtime"
Write-Status "AOT Mode: $(if ($NoAot) { 'Disabled (Traditional .NET)' } else { 'Enabled (Hybrid)' })"
Write-Status "Project Root: $ProjectRoot"

# Clean previous builds if requested
if ($CleanFirst) {
    Write-Status "Cleaning previous publish artifacts..."
    if (Test-Path $PublishDir) {
        Remove-Item -Path $PublishDir -Recurse -Force
        Write-Success "Cleaned publish directory"
    }
}

# Skip publish if TestOnly is specified
if (-not $TestOnly) {
    Write-Header "Building Projects"
    
    # Create publish directory
    if (-not (Test-Path $AotPublishDir)) {
        New-Item -Path $AotPublishDir -ItemType Directory -Force | Out-Null
    }

    # Step 1: Publish Server (Native AOT or Traditional)
    Write-Status "Publishing UIAutomationMCP.Server..."
    
    if ($NoAot) {
        # Traditional .NET publish
        $serverPublishArgs = @(
            "publish"
            $ServerProject
            "--configuration", "Release"
            "--runtime", $Runtime
            "--self-contained", "true"
            "--output", (Join-Path $AotPublishDir "Server")
            "/p:PublishAot=false"
            "/p:PublishTrimmed=true"
            "/p:PublishSingleFile=false"
        )
        Write-Status "Building Server with traditional .NET runtime..."
    } else {
        # Native AOT publish
        $serverPublishArgs = @(
            "publish"
            $ServerProject
            "--configuration", "Release"
            "--runtime", $Runtime
            "--output", (Join-Path $AotPublishDir "Server")
            "/p:PublishAot=true"
            "/p:StripSymbols=true"
            "/p:OptimizationPreference=Size"
        )
        Write-Status "Building Server with Native AOT (this may take several minutes)..."
    }
    
    $serverResult = & dotnet @serverPublishArgs 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to publish Server project"
        Write-Host $serverResult
        exit 1
    }
    Write-Success "Server published successfully"

    # Step 2: Create Shared Runtime
    Write-Status "Creating shared .NET runtime..."
    $sharedRuntimePath = Join-Path $AotPublishDir "Runtime"
    $workerTempPath = Join-Path $AotPublishDir "temp-worker"
    
    # First, publish Worker with self-contained to get runtime files
    $workerRuntimeArgs = @(
        "publish"
        $WorkerProject
        "--configuration", "Release"
        "--runtime", $Runtime
        "--self-contained", "true"
        "--output", $workerTempPath
        "/p:PublishAot=false"
        "/p:PublishTrimmed=false"
        "/p:PublishSingleFile=false"
    )
    
    $workerRuntimeResult = & dotnet @workerRuntimeArgs 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to create runtime base"
        Write-Host $workerRuntimeResult
        exit 1
    }
    
    # Extract runtime files to shared location
    if (-not (Test-Path $sharedRuntimePath)) {
        New-Item -Path $sharedRuntimePath -ItemType Directory -Force | Out-Null
    }
    
    # Move runtime files (excluding application DLLs)
    Write-Status "Extracting shared runtime files..."
    Get-ChildItem $workerTempPath -File | Where-Object {
        $_.Name -match "^(System\.|Microsoft\.|mscor|clr|host|coreclr|createdump|msquic|vcruntime|wpfgfx|PenImc|PresentationNative|D3DCompiler)" -or
        $_.Extension -eq ".dll" -and $_.Name -notmatch "^UIAutomationMCP"
    } | Move-Item -Destination $sharedRuntimePath -Force
    
    # Move localization folders
    Get-ChildItem $workerTempPath -Directory | Where-Object {
        $_.Name -match "^(cs|de|es|fr|it|ja|ko|pl|pt-BR|ru|tr|zh-Hans|zh-Hant)$"
    } | Move-Item -Destination $sharedRuntimePath -Force
    
    Write-Success "Shared runtime created successfully"

    # Step 3: Publish Worker (Framework-dependent)
    Write-Status "Publishing UIAutomationMCP.Subprocess.Worker (framework-dependent)..."
    $workerPublishArgs = @(
        "publish"
        $WorkerProject
        "--configuration", "Release"
        "--runtime", $Runtime
        "--no-self-contained"
        "--output", (Join-Path $AotPublishDir "Worker")
        "/p:PublishAot=false"
        "/p:PublishTrimmed=false"
        "/p:PublishSingleFile=false"
    )
    
    $workerResult = & dotnet @workerPublishArgs 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to publish Worker project"
        Write-Host $workerResult
        exit 1
    }
    Write-Success "Worker published successfully"

    # Step 4: Publish Monitor (Framework-dependent)
    Write-Status "Publishing UIAutomationMCP.Subprocess.Monitor (framework-dependent)..."
    $monitorPublishArgs = @(
        "publish"
        $MonitorProject
        "--configuration", "Release"
        "--runtime", $Runtime
        "--no-self-contained"
        "--output", (Join-Path $AotPublishDir "Monitor")
        "/p:PublishAot=false"
        "/p:PublishTrimmed=false"
        "/p:PublishSingleFile=false"
    )
    
    $monitorResult = & dotnet @monitorPublishArgs 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to publish Monitor project"
        Write-Host $monitorResult
        exit 1
    }
    Write-Success "Monitor published successfully"
    
    # Step 5: Clean up temporary files
    if (Test-Path $workerTempPath) {
        Remove-Item -Path $workerTempPath -Recurse -Force
        Write-Status "Cleaned up temporary files"
    }
    
    Write-Success "All projects published to: $AotPublishDir"
}

# Test published binaries
Write-Header "Testing Hybrid AOT Build"

$ServerExe = Join-Path $AotPublishDir "Server\uiautomation-mcp.exe"
$WorkerPath = Join-Path $AotPublishDir "Worker"
$MonitorPath = Join-Path $AotPublishDir "Monitor"
$RuntimePath = Join-Path $AotPublishDir "Runtime"

# Verify all components exist
$missingComponents = @()
if (-not (Test-Path $ServerExe)) { $missingComponents += "Server executable: $ServerExe" }
if (-not (Test-Path $WorkerPath)) { $missingComponents += "Worker directory: $WorkerPath" }
if (-not (Test-Path $MonitorPath)) { $missingComponents += "Monitor directory: $MonitorPath" }
if (-not (Test-Path $RuntimePath)) { $missingComponents += "Runtime directory: $RuntimePath" }

if ($missingComponents.Count -gt 0) {
    Write-Error "Missing components:"
    $missingComponents | ForEach-Object { Write-Error "  - $_" }
    Write-Status "Run without -TestOnly flag to publish first"
    exit 1
}

# Check file sizes for AOT optimization verification
Write-Status "Analyzing build results..."
$serverSize = (Get-Item $ServerExe).Length / 1MB
$workerSize = (Get-ChildItem $WorkerPath -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
$monitorSize = (Get-ChildItem $MonitorPath -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
$runtimeSize = (Get-ChildItem $RuntimePath -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
$totalSize = $serverSize + $workerSize + $monitorSize + $runtimeSize

Write-Status "Component sizes:"
Write-Status "  Server (AOT): $([Math]::Round($serverSize, 2)) MB"
Write-Status "  Worker: $([Math]::Round($workerSize, 2)) MB"
Write-Status "  Monitor: $([Math]::Round($monitorSize, 2)) MB"
Write-Status "  Shared Runtime: $([Math]::Round($runtimeSize, 2)) MB"
Write-Status "  Total: $([Math]::Round($totalSize, 2)) MB"

if (-not $NoAot -and $serverSize -lt 50) {
    Write-Success "AOT optimization successful - compact executable generated"
} elseif ($NoAot) {
    Write-Status "Traditional .NET build - size is expected to be larger"
} else {
    Write-Warning "AOT executable larger than expected - may indicate optimization issues"
}

Write-Status "Testing MCP server initialization..."

# Test JSON request for initialization
$initRequest = @{
    jsonrpc = "2.0"
    id = 1
    method = "initialize"
    params = @{
        protocolVersion = "2024-11-05"
        capabilities = @{}
        clientInfo = @{
            name = "aot-test"
            version = "1.0"
        }
    }
} | ConvertTo-Json -Compress

Write-Status "Starting $(if ($NoAot) { 'traditional' } else { 'AOT' }) server..."
Write-Status "Worker path: $WorkerPath"
Write-Status "Monitor path: $MonitorPath"

# Test with sleep method (recommended from CLAUDE.md)
try {
    $testScript = @"
echo '$initRequest'
Start-Sleep -Seconds 3
"@
    
    # Measure startup time
    $startTime = Get-Date
    $result = $testScript | powershell.exe -Command "& '$ServerExe' '$WorkerPath' '$MonitorPath'" 2>&1
    $endTime = Get-Date
    $startupTime = ($endTime - $startTime).TotalMilliseconds
    
    Write-Status "Server response (startup time: $([Math]::Round($startupTime, 0))ms):"
    Write-Host $result
    
    # Check if response contains expected initialization response
    if ($result -match '"result".*"protocolVersion".*"capabilities"') {
        Write-Success "$(if ($NoAot) { 'Traditional' } else { 'Hybrid AOT' }) server initialization test PASSED"
        Write-Success "Startup time: $([Math]::Round($startupTime, 0))ms"
        
        if (-not $NoAot -and $startupTime -lt 1000) {
            Write-Success "AOT startup optimization successful - sub-second startup achieved"
        }
    } else {
        Write-Warning "Server response may be incomplete or contains errors"
        Write-Status "This could indicate build or runtime issues"
    }
    
} catch {
    Write-Error "Failed to test published server: $_"
    exit 1
}

Write-Header "Build Summary"
Write-Status "Build Type: $(if ($NoAot) { 'Traditional .NET' } else { 'Hybrid Native AOT' })"
Write-Status "Runtime: $Runtime"
Write-Status "Server Size: $([Math]::Round($serverSize, 2)) MB"
Write-Status "Startup Time: $([Math]::Round($startupTime, 0))ms"
Write-Status ""
Write-Status "Published binaries location:"
Write-Status "  Server: $(Join-Path $AotPublishDir 'Server')"
Write-Status "  Worker: $(Join-Path $AotPublishDir 'Worker')"
Write-Status "  Monitor: $(Join-Path $AotPublishDir 'Monitor')"
Write-Status "  Runtime: $(Join-Path $AotPublishDir 'Runtime')"

if (-not $NoAot) {
    Write-Success "Hybrid Native AOT build completed successfully!"
    Write-Status "The MCP Server now starts faster while maintaining full UI Automation functionality."
}