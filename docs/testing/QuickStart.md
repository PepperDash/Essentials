# Quick Start Guide: Unit Testing

This guide helps you get started with writing unit tests for PepperDash Essentials components.

## Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or VS Code with C# extension

## Running Existing Tests

```bash
# Navigate to the test project
cd tests/PepperDash.Essentials.Core.Tests

# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Creating Your First Test

### 1. Identify the Component

Choose a component that has business logic you want to test. Look for classes that:
- Have clear inputs and outputs
- Contain conditional logic or algorithms
- Are used by multiple parts of the system

### 2. Create Test File

Create a new test file in `tests/PepperDash.Essentials.Core.Tests/Unit/`:

```csharp
using PepperDash.Essentials.Core.Tests.Abstractions;

namespace PepperDash.Essentials.Core.Tests.Unit
{
    public class YourComponentTests
    {
        [Fact]
        public void YourMethod_WithValidInput_ReturnsExpectedResult()
        {
            // Arrange
            var component = new YourComponent();
            var input = "test data";

            // Act
            var result = component.YourMethod(input);

            // Assert
            Assert.Equal("expected result", result);
        }
    }
}
```

### 3. Abstract Dependencies

If your component uses Crestron SDK or other external dependencies:

```csharp
// Create interface in Abstractions/
public interface IYourDependency
{
    string DoSomething(string input);
}

// Modify your component to use the interface
public class YourComponent
{
    private readonly IYourDependency _dependency;
    
    public YourComponent(IYourDependency dependency)
    {
        _dependency = dependency;
    }
    
    public string YourMethod(string input)
    {
        return _dependency.DoSomething(input);
    }
}

// Test with mocks
[Fact]
public void YourMethod_CallsDependency_ReturnsResult()
{
    // Arrange
    var mockDependency = new Mock<IYourDependency>();
    mockDependency.Setup(x => x.DoSomething("input")).Returns("output");
    var component = new YourComponent(mockDependency.Object);

    // Act
    var result = component.YourMethod("input");

    // Assert
    Assert.Equal("output", result);
    mockDependency.Verify(x => x.DoSomething("input"), Times.Once);
}
```

## Testing Patterns

### Testing Exceptions

```csharp
[Fact]
public void Constructor_WithNullDependency_ThrowsArgumentNullException()
{
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => new YourComponent(null));
}
```

### Testing Async Methods

```csharp
[Fact]
public async Task YourAsyncMethod_WithValidInput_ReturnsExpectedResult()
{
    // Arrange
    var component = new YourComponent();

    // Act
    var result = await component.YourAsyncMethod();

    // Assert
    Assert.True(result);
}
```

### Testing Collections

```csharp
[Fact]
public void GetItems_ReturnsExpectedCount()
{
    // Arrange
    var component = new YourComponent();

    // Act
    var items = component.GetItems();

    // Assert
    Assert.Equal(3, items.Count());
    Assert.Contains(items, x => x.Name == "Expected Item");
}
```

## Common Mistakes to Avoid

1. **Testing Implementation Details** - Test behavior, not internal implementation
2. **Overly Complex Tests** - Keep tests simple and focused on one thing
3. **Not Testing Edge Cases** - Include null values, empty collections, boundary conditions
4. **Ignoring Test Performance** - Tests should run quickly to enable fast feedback

## Debugging Tests

### In Visual Studio
- Set breakpoints in test methods
- Use Test Explorer to run individual tests
- Check test output window for detailed information

### From Command Line
```bash
# Run specific test
dotnet test --filter "YourComponentTests.YourMethod_WithValidInput_ReturnsExpectedResult"

# Run with detailed output
dotnet test --verbosity diagnostic
```

## Next Steps

1. **Review Examples** - Look at `TestableActionSequenceTests.cs` for comprehensive examples
2. **Start Small** - Begin with simple components and build up complexity
3. **Read Documentation** - Check `docs/testing/README.md` for detailed patterns
4. **Get Feedback** - Have your tests reviewed by team members

## Need Help?

- Check existing test examples in the codebase
- Review the full testing documentation
- Ask team members for guidance on complex scenarios
- Consider pair programming for your first few tests