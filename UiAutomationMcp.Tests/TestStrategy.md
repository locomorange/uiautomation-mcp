# Test Strategy for UI Automation MCP Server

## Test Categories

### 1. Unit Tests
**Scope**: Test individual components in isolation without external dependencies
**Location**: `/UnitTests/` folder
**Characteristics**:
- Fast execution (< 1ms per test)
- No external dependencies (UI Automation, file system, network)
- High coverage of business logic
- Mockable components only

**Examples**:
- Parameter validation logic
- Data transformation methods
- Static utility methods
- Configuration parsing
- Error message formatting

### 2. Integration Tests
**Scope**: Test components that interact with external systems
**Location**: `/IntegrationTests/` folder
**Characteristics**:
- Slower execution (100ms - 5s per test)
- Requires real UI Automation environment
- Tests actual Windows application interaction
- May require test applications or system components

**Examples**:
- Pattern executor operations on real UI elements
- Worker process communication
- Element discovery and interaction
- Timeout handling with real delays

### 3. End-to-End Tests
**Scope**: Test complete workflows through MCP protocol
**Location**: `/EndToEnd/` folder
**Characteristics**:
- Full system tests
- MCP client → Server → Worker → UI Automation
- Requires test applications
- Longer execution time

## Test Organization

```
UiAutomationMcp.Tests/
├── UnitTests/
│   ├── Services/
│   │   ├── OperationExecutorUnitTests.cs      # Static methods, validation logic
│   │   └── InputProcessorUnitTests.cs         # JSON parsing, validation
│   ├── Models/
│   │   └── SharedModelsUnitTests.cs           # Data model validation
│   └── Helpers/
│       └── ValidationHelpersUnitTests.cs      # Pure functions
├── IntegrationTests/
│   ├── PatternExecutors/
│   │   ├── TextPatternIntegrationTests.cs     # Real UI Automation tests
│   │   └── WindowPatternIntegrationTests.cs   # Real window operations
│   ├── Infrastructure/
│   │   ├── TestApplicationManager.cs          # Test app lifecycle
│   │   └── UIAutomationTestFixture.cs         # Test setup/teardown
│   └── WorkerProcess/
│       └── WorkerCommunicationTests.cs        # Process communication
├── EndToEnd/
│   └── McpProtocolTests.cs                    # Full protocol tests
└── TestApps/                                  # Simple test applications
    ├── SimpleWinFormsApp/                     # Basic controls
    └── WpfTestApp/                            # WPF controls
```

## Test Execution Strategy

### Development Workflow
1. **Unit Tests**: Run continuously during development (IDE, pre-commit)
2. **Integration Tests**: Run before PR submission
3. **End-to-End Tests**: Run in CI/CD pipeline

### CI/CD Pipeline
```bash
# Fast feedback loop
dotnet test --filter Category=Unit

# Pre-merge validation
dotnet test --filter Category=Integration

# Full validation
dotnet test --filter Category=EndToEnd
```

## Test Infrastructure Requirements

### For Integration Tests
- **Test Applications**: Simple WinForms/WPF apps with known controls
- **UI Automation Setup**: Ensure UI Automation services are running
- **Isolation**: Each test should not affect others
- **Cleanup**: Proper resource disposal and state reset

### Test Application Requirements
- **Predictable UI**: Fixed element IDs, known layouts
- **Multiple Patterns**: Cover different UI Automation patterns
- **State Management**: Ability to reset to known state
- **Minimal Dependencies**: Self-contained, lightweight

## Migration Plan

### Phase 1: Separate Existing Tests
1. Move pure logic tests to UnitTests folder
2. Move UI Automation dependent tests to IntegrationTests folder
3. Add proper test categories

### Phase 2: Create Test Infrastructure
1. Build simple test applications
2. Create test fixtures and helpers
3. Implement proper setup/teardown

### Phase 3: Enhance Test Coverage
1. Add missing integration tests
2. Improve test reliability
3. Add performance benchmarks

## Best Practices

### Unit Tests
- Use `[Fact]` for single scenario tests
- Use `[Theory]` for parameterized tests
- Mock all external dependencies
- Test edge cases and error conditions
- Aim for 100% code coverage of testable logic

### Integration Tests
- Use `[Fact]` with descriptive names
- Include setup and cleanup in test methods
- Test realistic scenarios
- Include timeout and retry logic
- Document environmental requirements

### Test Naming
```csharp
// Unit Test
[Fact]
public void ValidateParameters_WithNullElementId_ShouldReturnValidationError()

// Integration Test  
[Fact]
public void ExecuteTextPattern_WithRealTextBox_ShouldReturnActualText()

// End-to-End Test
[Fact]
public void McpProtocol_InvokeButton_ShouldTriggerClickAndReturnSuccess()
```

## Test Categories (Attributes)

```csharp
[Trait("Category", "Unit")]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
[Trait("Category", "Performance")]
```