# PepperDash Essentials Unit Testing Strategy

## Problem Statement
The PepperDash Essentials framework is tightly coupled to Crestron hardware libraries that only run on Crestron devices, making it impossible to run unit tests on development machines or in CI/CD pipelines.

## Solution: Abstraction Layer Pattern

### 1. Core Abstractions Created
We've implemented abstraction interfaces that decouple business logic from Crestron hardware:

- **`ICrestronControlSystem`** - Abstracts the control system hardware
- **`IRelayPort`** - Abstracts relay functionality
- **`IDigitalInput`** - Abstracts digital inputs with event handling
- **`IVersiPort`** - Abstracts VersiPort I/O

### 2. Adapter Pattern Implementation
Created adapter classes that wrap Crestron objects in production:

```csharp
// Production code uses adapters
var controlSystem = new CrestronControlSystemAdapter(Global.ControlSystem);
var processor = new CrestronProcessorTestable("key", controlSystem);

// Test code uses mocks
var mockControlSystem = new Mock<ICrestronControlSystem>();
var processor = new CrestronProcessorTestable("key", mockControlSystem.Object);
```

### 3. Testable Classes
Refactored classes to accept abstractions via dependency injection:

- **`CrestronProcessorTestable`** - Accepts `ICrestronControlSystem`
- **`GenericRelayDeviceTestable`** - Accepts `IRelayPort`

## Implementation Steps

### Step 1: Identify Dependencies
```bash
# Find Crestron dependencies
grep -r "using Crestron" --include="*.cs"
```

### Step 2: Create Abstractions
Define interfaces that mirror the Crestron API surface you need:
```csharp
public interface IRelayPort
{
    void Open();
    void Close();
    void Pulse(int delayMs);
    bool State { get; }
}
```

### Step 3: Implement Adapters
Wrap Crestron objects with adapters:
```csharp
public class RelayPortAdapter : IRelayPort
{
    private readonly Relay _relay;
    public void Open() => _relay.Open();
    // ... other methods
}
```

### Step 4: Refactor Classes
Accept abstractions in constructors:
```csharp
public class CrestronProcessorTestable
{
    public CrestronProcessorTestable(string key, ICrestronControlSystem processor)
    {
        // Use abstraction instead of concrete type
    }
}
```

### Step 5: Write Tests
Use mocking frameworks to test business logic:
```csharp
[Fact]
public void OpenRelay_CallsRelayPortOpen()
{
    var mockRelay = new Mock<IRelayPort>();
    var device = new GenericRelayDeviceTestable("test", mockRelay.Object);
    
    device.OpenRelay();
    
    mockRelay.Verify(r => r.Open(), Times.Once);
}
```

## Test Project Structure
```
tests/
├── PepperDash.Essentials.Core.Tests/
│   ├── Abstractions/           # Tests for abstraction adapters
│   ├── Devices/                # Device-specific tests
│   └── *.csproj                # Test project file
└── README.md                    # Testing documentation
```

## CI/CD Integration

### GitHub Actions Workflow
The `.github/workflows/ci.yml` file runs tests automatically on:
- Push to main/develop branches
- Pull requests
- Generates code coverage reports

### Running Tests Locally
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific tests
dotnet test --filter "FullyQualifiedName~CrestronProcessor"
```

## Benefits

1. **Unit Testing Without Hardware** - Tests run on any machine
2. **CI/CD Integration** - Automated testing in pipelines
3. **Better Design** - Encourages SOLID principles
4. **Faster Development** - No need for hardware to test logic
5. **Higher Code Quality** - Catch bugs before deployment

## Migration Guide

### For Existing Code
1. Identify classes with Crestron dependencies
2. Create abstraction interfaces
3. Implement adapters
4. Create testable versions accepting abstractions
5. Write unit tests

### For New Code
1. Always code against abstractions, not Crestron types
2. Use dependency injection
3. Write tests first (TDD approach)

## Current Test Coverage
- ✅ CrestronProcessor relay management
- ✅ GenericRelayDevice operations
- ✅ Digital input event handling
- ✅ VersiPort analog/digital operations

## Next Steps
1. Expand abstractions for more Crestron components
2. Increase test coverage across all modules
3. Add integration tests with mock hardware
4. Document testing best practices
5. Create code generation tools for adapters

## Tools Used
- **xUnit** - Test framework
- **Moq** - Mocking library
- **FluentAssertions** - Readable assertions
- **Coverlet** - Code coverage
- **GitHub Actions** - CI/CD

## Summary
By introducing an abstraction layer between the business logic and Crestron hardware dependencies, we've successfully enabled unit testing for the PepperDash Essentials framework. This approach allows development and testing without physical hardware while maintaining full compatibility with Crestron systems in production.