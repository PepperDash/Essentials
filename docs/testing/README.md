# Unit Testing Infrastructure

This document outlines the unit testing infrastructure and patterns for PepperDash Essentials.

## Overview

The testing infrastructure is designed to enable comprehensive testing of business logic while abstracting away Crestron SDK dependencies. This allows for:

- Fast, reliable unit tests that don't require hardware
- Isolated testing of business logic
- Better code quality through testability
- Easier debugging and maintenance

## Project Structure

```
tests/
├── PepperDash.Essentials.Core.Tests/
│   ├── Abstractions/           # Interface abstractions for SDK dependencies
│   ├── TestableComponents/     # Demonstration components showing abstraction patterns
│   ├── Unit/                   # Unit test files
│   └── GlobalUsings.cs         # Global using statements for tests
```

## Test Project Configuration

The test projects use:
- **.NET 8** - Modern testing framework with better performance
- **xUnit** - Primary testing framework
- **Moq** - Mocking framework for dependencies
- **Coverlet** - Code coverage analysis

## Abstraction Patterns

### Interface Segregation

Create focused interfaces that abstract SDK functionality:

```csharp
public interface IThreadService
{
    void Sleep(int milliseconds);
    object CreateAndStartThread(Func<object, object> threadFunction, object parameter);
    void AbortThread(object thread);
    bool IsThreadRunning(object thread);
}

public interface ILogger
{
    void LogDebug(object source, string message, params object[] args);
    void LogVerbose(object source, string message, params object[] args);
}
```

### Dependency Injection

Components should accept dependencies via constructor injection:

```csharp
public class TestableActionSequence
{
    private readonly IQueue<TestableSequencedAction> _actionQueue;
    private readonly IThreadService _threadService;
    private readonly ILogger _logger;

    public TestableActionSequence(
        IQueue<TestableSequencedAction> actionQueue,
        IThreadService threadService,
        ILogger logger,
        List<TestableSequencedAction> actions)
    {
        _actionQueue = actionQueue ?? throw new ArgumentNullException(nameof(actionQueue));
        _threadService = threadService ?? throw new ArgumentNullException(nameof(threadService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        // ...
    }
}
```

## Writing Tests

### Test Organization

- **One test class per component**
- **Descriptive test method names** that describe the scenario and expected outcome
- **Arrange-Act-Assert pattern** for clear test structure

### Example Test Structure

```csharp
public class TestableActionSequenceTests
{
    private readonly Mock<IQueue<TestableSequencedAction>> _mockQueue;
    private readonly Mock<IThreadService> _mockThreadService;
    private readonly Mock<ILogger> _mockLogger;
    private readonly TestableActionSequence _actionSequence;

    public TestableActionSequenceTests()
    {
        // Arrange - Set up mocks and dependencies
        _mockQueue = new Mock<IQueue<TestableSequencedAction>>();
        _mockThreadService = new Mock<IThreadService>();
        _mockLogger = new Mock<ILogger>();
        
        _actionSequence = new TestableActionSequence(
            _mockQueue.Object,
            _mockThreadService.Object,
            _mockLogger.Object,
            new List<TestableSequencedAction>());
    }

    [Fact]
    public void StartSequence_WhenNoThreadRunning_StartsNewThread()
    {
        // Arrange
        _mockThreadService.Setup(x => x.IsThreadRunning(It.IsAny<object>())).Returns(false);

        // Act
        _actionSequence.StartSequence();

        // Assert
        _mockLogger.Verify(x => x.LogDebug(_actionSequence, "Starting Action Sequence"), Times.Once);
        _mockThreadService.Verify(x => x.CreateAndStartThread(It.IsAny<Func<object, object>>(), null), Times.Once);
    }
}
```

### Test Naming Convention

Use descriptive names that follow the pattern:
`MethodName_Scenario_ExpectedBehavior`

Examples:
- `StartSequence_WhenNoThreadRunning_StartsNewThread`
- `Constructor_WithNullQueue_ThrowsArgumentNullException`
- `StopSequence_SetsAllowActionsToFalseAndAbortsThread`

## Running Tests

### From Command Line

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "TestableActionSequenceTests"

# Run with verbose output
dotnet test --verbosity normal
```

### From Visual Studio

- Use Test Explorer to run and debug tests
- Right-click on test methods to run individually
- Use "Run Tests in Parallel" for faster execution

## Code Coverage

The test projects include Coverlet for code coverage analysis. After running tests with coverage:

```bash
# Generate coverage report
dotnet tool install -g reportgenerator
reportgenerator -reports:"coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

## Best Practices

### 1. Test Independence
- Each test should be independent and able to run in isolation
- Use fresh mock objects for each test
- Don't rely on test execution order

### 2. Mocking Guidelines
- Mock external dependencies (SDK calls, file system, network)
- Don't mock the component under test
- Use strict mocks when behavior verification is important

### 3. Assertions
- Make assertions specific and meaningful
- Test both positive and negative scenarios
- Verify state changes and behavior calls

### 4. Test Data
- Use meaningful test data that represents real scenarios
- Consider edge cases and boundary conditions
- Use constants or factory methods for complex test data

## Migrating Existing Code

To make existing components testable:

1. **Identify SDK Dependencies** - Look for direct Crestron SDK usage
2. **Extract Interfaces** - Create abstractions for SDK functionality
3. **Inject Dependencies** - Modify constructors to accept abstractions
4. **Create Tests** - Write comprehensive unit tests for business logic
5. **Implement Wrappers** - Create concrete implementations for production use

## Example Implementation Wrapper

When implementing the abstractions for production use:

```csharp
public class CrestronThreadService : IThreadService
{
    public void Sleep(int milliseconds)
    {
        Thread.Sleep(milliseconds);
    }

    public object CreateAndStartThread(Func<object, object> threadFunction, object parameter)
    {
        var thread = new Thread(threadFunction, parameter, Thread.eThreadStartOptions.Running);
        return thread;
    }

    public void AbortThread(object thread)
    {
        if (thread is Thread crestronThread)
        {
            crestronThread.Abort();
        }
    }

    public bool IsThreadRunning(object thread)
    {
        return thread is Thread crestronThread && 
               crestronThread.ThreadState == Thread.eThreadStates.ThreadRunning;
    }
}
```

## Future Enhancements

The testing infrastructure can be extended with:

- **Integration test support** for end-to-end scenarios
- **Performance testing** utilities
- **Test data builders** for complex object creation
- **Custom assertions** for domain-specific validations
- **Automated test generation** for common patterns

## Getting Help

For questions about testing patterns or infrastructure:

1. Review existing test examples in the `tests/` directory
2. Check this documentation for guidance
3. Consult the team for complex abstraction scenarios
4. Consider the impact on existing plugin interfaces before major changes