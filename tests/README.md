# PepperDash Essentials Unit Testing Guide

## Overview

This guide demonstrates how to write unit tests for PepperDash Essentials despite the Crestron hardware dependencies. The key approach is to use **abstraction layers** and **dependency injection** to isolate Crestron-specific functionality.

## Architecture Pattern

### 1. Abstraction Layer
We create interfaces that abstract Crestron hardware components:
- `ICrestronControlSystem` - Abstracts the control system
- `IRelayPort` - Abstracts relay functionality  
- `IDigitalInput` - Abstracts digital inputs
- `IVersiPort` - Abstracts VersiPorts

### 2. Adapters
Adapter classes wrap actual Crestron objects in production:
- `CrestronControlSystemAdapter` - Wraps `CrestronControlSystem`
- `RelayPortAdapter` - Wraps Crestron `Relay`
- `DigitalInputAdapter` - Wraps Crestron `DigitalInput`
- `VersiPortAdapter` - Wraps Crestron `Versiport`

### 3. Testable Classes
Create testable versions of classes that accept abstractions:
- `CrestronProcessorTestable` - Accepts `ICrestronControlSystem` instead of concrete type
- `GenericRelayDeviceTestable` - Accepts `IRelayPort` instead of concrete type

## Writing Tests

### Basic Test Example
```csharp
[Fact]
public void OpenRelay_CallsRelayPortOpen()
{
    // Arrange
    var mockRelayPort = new Mock<IRelayPort>();
    var device = new GenericRelayDeviceTestable("test-relay", mockRelayPort.Object);
    
    // Act
    device.OpenRelay();
    
    // Assert
    mockRelayPort.Verify(r => r.Open(), Times.Once);
}
```

### Testing Events
```csharp
[Fact]
public void StateChange_WhenDigitalInputChanges_RaisesEvent()
{
    // Arrange
    var mockDigitalInput = new Mock<IDigitalInput>();
    var eventRaised = false;
    
    mockDigitalInput.Object.StateChange += (sender, args) => eventRaised = true;
    
    // Act
    mockDigitalInput.Raise(d => d.StateChange += null, 
        mockDigitalInput.Object, 
        new DigitalInputEventArgs(true));
    
    // Assert
    eventRaised.Should().BeTrue();
}
```

## Running Tests

### Locally
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/PepperDash.Essentials.Core.Tests
```

### CI Pipeline
Tests run automatically on:
- Push to main/develop/net8-updates branches
- Pull requests
- GitHub Actions workflow generates coverage reports

## Migration Strategy

To migrate existing code to be testable:

1. **Identify Crestron Dependencies**
   - Search for `using Crestron` statements
   - Find direct hardware interactions

2. **Create Abstractions**
   - Define interfaces for hardware components
   - Keep interfaces focused and simple

3. **Implement Adapters**
   - Wrap Crestron objects with adapters
   - Map Crestron events to abstraction events

4. **Refactor Classes**
   - Accept abstractions via constructor injection
   - Create factory methods for production use

5. **Write Tests**
   - Mock abstractions using Moq
   - Test business logic independently

## Best Practices

### DO:
- Keep abstractions simple and focused
- Test business logic, not Crestron SDK behavior
- Use dependency injection consistently
- Mock at the abstraction boundary
- Test event handling and state changes

### DON'T:
- Try to mock Crestron types directly
- Include hardware-dependent code in tests
- Mix business logic with hardware interaction
- Create overly complex abstractions

## Tools Used

- **xUnit** - Test framework
- **Moq** - Mocking framework
- **FluentAssertions** - Assertion library
- **Coverlet** - Code coverage
- **GitHub Actions** - CI/CD

## Adding New Tests

1. Create test file in appropriate folder
2. Follow naming convention: `[ClassName]Tests.cs`
3. Use Arrange-Act-Assert pattern
4. Include both positive and negative test cases
5. Test edge cases and error conditions

## Troubleshooting

### Common Issues:

**Tests fail with "Type not found" errors**
- Ensure abstractions are properly defined
- Check project references

**Mocked events not firing**
- Use `Mock.Raise()` to trigger events
- Verify event subscription syntax

**Coverage not generating**
- Run with `--collect:"XPlat Code Coverage"`
- Check .gitignore isn't excluding coverage files

## Example Test Project Structure
```
tests/
├── PepperDash.Essentials.Core.Tests/
│   ├── Abstractions/
│   │   ├── DigitalInputTests.cs
│   │   └── VersiPortTests.cs
│   ├── Devices/
│   │   └── CrestronProcessorTests.cs
│   └── PepperDash.Essentials.Core.Tests.csproj
└── README.md
```