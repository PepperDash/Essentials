using CrestronMock;
using PepperDash.Essentials;
using PepperDash.Essentials.Core;

namespace EssentialsTests;

public class ControlSystemTests
{
    [Fact]
    public void ControlSystem_Constructor_ShouldBuildSuccessfully()
    {
        // Arrange & Act
        var exception = Record.Exception(() => new ControlSystem());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void ControlSystem_Constructor_ShouldSetGlobalControlSystem()
    {
        // Arrange & Act
        var controlSystem = new ControlSystem();

        // Assert
        Assert.NotNull(Global.ControlSystem);
        Assert.Same(controlSystem, Global.ControlSystem);
    }

    [Fact]
    public void ControlSystem_InitializeSystem_ShouldNotThrow()
    {
        // Arrange
        var controlSystem = new ControlSystem();

        // Act & Assert
        var exception = Record.Exception(() => controlSystem.InitializeSystem());
        Assert.Null(exception);
    }

    [Fact]
    public void ControlSystem_ShouldImplementILoadConfig()
    {
        // Arrange & Act
        var controlSystem = new ControlSystem();

        // Assert
        Assert.True(controlSystem is ILoadConfig);
    }

    [Fact]
    public void ControlSystem_ShouldHaveRequiredInterfaces()
    {
        // Arrange & Act
        var controlSystem = new ControlSystem();

        // Assert - Check that it inherits from base mock and implements hardware interfaces
        Assert.NotNull(controlSystem);
        Assert.True(controlSystem is IComPorts, "ControlSystem should implement IComPorts");
        Assert.True(controlSystem is IRelayPorts, "ControlSystem should implement IRelayPorts");
        Assert.True(controlSystem is IIROutputPorts, "ControlSystem should implement IIROutputPorts");
        Assert.True(controlSystem is IIOPorts, "ControlSystem should implement IIOPorts");
        Assert.True(controlSystem is IDigitalInputPorts, "ControlSystem should implement IDigitalInputPorts");
        Assert.True(controlSystem is IIRInputPort, "ControlSystem should implement IIRInputPort");
    }

    [Fact]
    public void ControlSystem_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var controlSystem = new ControlSystem();

        // Assert - Test by casting to interfaces to access properties
        var comPorts = controlSystem as IComPorts;
        var relayPorts = controlSystem as IRelayPorts;
        var irOutputPorts = controlSystem as IIROutputPorts;
        var ioPorts = controlSystem as IIOPorts;
        var digitalInputPorts = controlSystem as IDigitalInputPorts;
        var irInputPort = controlSystem as IIRInputPort;

        Assert.NotNull(comPorts?.ComPorts);
        Assert.NotNull(relayPorts?.RelayPorts);
        Assert.NotNull(irOutputPorts?.IROutputPorts);
        Assert.NotNull(ioPorts?.IOPorts);
        Assert.NotNull(digitalInputPorts?.DigitalInputPorts);
        Assert.NotNull(irInputPort?.IRInputPort);
    }
}
