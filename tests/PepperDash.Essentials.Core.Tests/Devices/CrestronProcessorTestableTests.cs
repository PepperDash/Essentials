using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using Moq;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Abstractions;
using PepperDash.Essentials.Core.Factory;
using PepperDash.Essentials.Core.CrestronIO;

namespace PepperDash.Essentials.Core.Tests.Devices
{
    public class CrestronProcessorTestableTests : IDisposable
    {
        public CrestronProcessorTestableTests()
        {
            // Enable test mode for all tests in this class
            CrestronEnvironmentFactory.EnableTestMode();
        }

        public void Dispose()
        {
            // Restore runtime mode after tests
            CrestronEnvironmentFactory.DisableTestMode();
        }

        [Fact]
        public void Constructor_WithNullProcessor_UsesFactoryToGetControlSystem()
        {
            // Arrange & Act
            var processor = new CrestronProcessorTestable("test-processor");

            // Assert
            processor.Should().NotBeNull();
            processor.Processor.Should().NotBeNull();
            processor.Key.Should().Be("test-processor");
        }

        [Fact]
        public void Constructor_WithProvidedProcessor_UsesProvidedProcessor()
        {
            // Arrange
            var mockProcessor = new Mock<ICrestronControlSystem>();
            mockProcessor.Setup(p => p.SupportsRelay).Returns(false);
            mockProcessor.Setup(p => p.RelayPorts).Returns(new Dictionary<uint, IRelayPort>());

            // Act
            var processor = new CrestronProcessorTestable("test-processor", mockProcessor.Object);

            // Assert
            processor.Processor.Should().BeSameAs(mockProcessor.Object);
        }

        [Fact]
        public void GetRelays_WhenProcessorSupportsRelays_CreatesRelayDevices()
        {
            // Arrange
            var mockProvider = new CrestronMockProvider();
            mockProvider.ConfigureMockSystem(system =>
            {
                system.SupportsRelay = true;
                system.NumberOfRelayPorts = 4;
                // Ensure relay ports are initialized
                for (uint i = 1; i <= 4; i++)
                {
                    system.RelayPorts[i] = new MockRelayPort();
                }
            });
            
            CrestronEnvironmentFactory.SetProvider(mockProvider);

            // Act
            var processor = new CrestronProcessorTestable("test-processor");

            // Assert
            processor.SwitchedOutputs.Should().HaveCount(4);
            processor.SwitchedOutputs.Should().ContainKeys(1, 2, 3, 4);

            foreach (var kvp in processor.SwitchedOutputs)
            {
                kvp.Value.Should().BeOfType<GenericRelayDeviceTestable>();
                var relayDevice = kvp.Value as GenericRelayDeviceTestable;
                relayDevice.Key.Should().Be($"test-processor-relay-{kvp.Key}");
            }
        }

        [Fact]
        public void GetRelays_WhenProcessorDoesNotSupportRelays_CreatesNoDevices()
        {
            // Arrange
            var mockProcessor = new Mock<ICrestronControlSystem>();
            mockProcessor.Setup(p => p.SupportsRelay).Returns(false);
            mockProcessor.Setup(p => p.RelayPorts).Returns(new Dictionary<uint, IRelayPort>());

            // Act
            var processor = new CrestronProcessorTestable("test-processor", mockProcessor.Object);

            // Assert
            processor.SwitchedOutputs.Should().BeEmpty();
        }

        [Fact]
        public void GetRelays_HandlesExceptionGracefully()
        {
            // Arrange
            var mockProcessor = new Mock<ICrestronControlSystem>();
            mockProcessor.Setup(p => p.SupportsRelay).Returns(true);
            mockProcessor.Setup(p => p.NumberOfRelayPorts).Throws(new Exception("Test exception"));
            mockProcessor.Setup(p => p.RelayPorts).Returns(new Dictionary<uint, IRelayPort>());

            // Act
            Action act = () => new CrestronProcessorTestable("test-processor", mockProcessor.Object);

            // Assert
            act.Should().NotThrow();
        }
    }

    public class GenericRelayDeviceTestableTests : IDisposable
    {
        public GenericRelayDeviceTestableTests()
        {
            CrestronEnvironmentFactory.EnableTestMode();
        }

        public void Dispose()
        {
            CrestronEnvironmentFactory.DisableTestMode();
        }

        [Fact]
        public void Constructor_WithNullRelayPort_ThrowsArgumentNullException()
        {
            // Act & Assert
            Action act = () => new GenericRelayDeviceTestable("test-relay", null);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("relayPort");
        }

        [Fact]
        public void Constructor_WithValidRelayPort_InitializesCorrectly()
        {
            // Arrange
            var mockRelayPort = new MockRelayPort();

            // Act
            var device = new GenericRelayDeviceTestable("test-relay", mockRelayPort);

            // Assert
            device.Should().NotBeNull();
            device.Key.Should().Be("test-relay");
            device.OutputIsOnFeedback.Should().NotBeNull();
        }

        [Fact]
        public void OpenRelay_OpensRelayAndUpdatesFeedback()
        {
            // Arrange
            var mockRelayPort = new MockRelayPort();
            mockRelayPort.SetState(true); // Start with closed relay
            var device = new GenericRelayDeviceTestable("test-relay", mockRelayPort);
            
            bool feedbackFired = false;
            device.OutputIsOnFeedback.OutputChange += (sender, args) => feedbackFired = true;

            // Act
            device.OpenRelay();

            // Assert
            mockRelayPort.State.Should().BeFalse();
            device.IsOn.Should().BeFalse();
            feedbackFired.Should().BeTrue();
        }

        [Fact]
        public void CloseRelay_ClosesRelayAndUpdatesFeedback()
        {
            // Arrange
            var mockRelayPort = new MockRelayPort();
            mockRelayPort.SetState(false); // Start with open relay
            var device = new GenericRelayDeviceTestable("test-relay", mockRelayPort);
            
            bool feedbackFired = false;
            device.OutputIsOnFeedback.OutputChange += (sender, args) => feedbackFired = true;

            // Act
            device.CloseRelay();

            // Assert
            mockRelayPort.State.Should().BeTrue();
            device.IsOn.Should().BeTrue();
            feedbackFired.Should().BeTrue();
        }

        [Fact]
        public void PulseRelay_CallsPulseOnRelayPort()
        {
            // Arrange
            var mockRelayPort = new Mock<IRelayPort>();
            var device = new GenericRelayDeviceTestable("test-relay", mockRelayPort.Object);

            // Act
            device.PulseRelay(500);

            // Assert
            mockRelayPort.Verify(r => r.Pulse(500), Times.Once);
        }

        [Fact]
        public void On_ClosesRelay()
        {
            // Arrange
            var mockRelayPort = new MockRelayPort();
            var device = new GenericRelayDeviceTestable("test-relay", mockRelayPort);

            // Act
            device.On();

            // Assert
            mockRelayPort.State.Should().BeTrue();
            device.IsOn.Should().BeTrue();
        }

        [Fact]
        public void Off_OpensRelay()
        {
            // Arrange
            var mockRelayPort = new MockRelayPort();
            mockRelayPort.SetState(true); // Start with closed relay
            var device = new GenericRelayDeviceTestable("test-relay", mockRelayPort);

            // Act
            device.Off();

            // Assert
            mockRelayPort.State.Should().BeFalse();
            device.IsOn.Should().BeFalse();
        }

        [Fact]
        public void PowerToggle_TogglesRelayState()
        {
            // Arrange
            var mockRelayPort = new MockRelayPort();
            var device = new GenericRelayDeviceTestable("test-relay", mockRelayPort);

            // Act & Assert - First toggle (off to on)
            device.PowerToggle();
            mockRelayPort.State.Should().BeTrue();
            device.IsOn.Should().BeTrue();

            // Act & Assert - Second toggle (on to off)
            device.PowerToggle();
            mockRelayPort.State.Should().BeFalse();
            device.IsOn.Should().BeFalse();
        }

        [Fact]
        public void IsOn_ReflectsRelayPortState()
        {
            // Arrange
            var mockRelayPort = new MockRelayPort();
            var device = new GenericRelayDeviceTestable("test-relay", mockRelayPort);

            // Act & Assert - Initially off
            device.IsOn.Should().BeFalse();

            // Act & Assert - After closing
            mockRelayPort.Close();
            device.IsOn.Should().BeTrue();

            // Act & Assert - After opening
            mockRelayPort.Open();
            device.IsOn.Should().BeFalse();
        }

        [Fact]
        public void CustomActivate_ReinitializesFeedback()
        {
            // Arrange
            var mockRelayPort = new MockRelayPort();
            var device = new GenericRelayDeviceTestable("test-relay", mockRelayPort);
            var originalFeedback = device.OutputIsOnFeedback;

            // Act
            var result = device.CustomActivate();

            // Assert
            result.Should().BeTrue();
            device.OutputIsOnFeedback.Should().NotBeNull();
            device.OutputIsOnFeedback.Should().NotBeSameAs(originalFeedback);
        }
    }

    public class IntegrationTests : IDisposable
    {
        public IntegrationTests()
        {
            CrestronEnvironmentFactory.EnableTestMode();
        }

        public void Dispose()
        {
            CrestronEnvironmentFactory.DisableTestMode();
        }

        [Fact]
        public void FullSystemIntegration_CreatesAndControlsRelays()
        {
            // Arrange
            var mockProvider = new CrestronMockProvider();
            mockProvider.ConfigureMockSystem(system =>
            {
                system.SupportsRelay = true;
                system.NumberOfRelayPorts = 2;
                system.ProgramIdTag = "INTEGRATION_TEST";
            });
            
            CrestronEnvironmentFactory.SetProvider(mockProvider);

            // Act
            var processor = new CrestronProcessorTestable("integration-processor");

            // Assert processor creation
            processor.Processor.ProgramIdTag.Should().Be("INTEGRATION_TEST");
            processor.SwitchedOutputs.Should().HaveCount(2);

            // Test relay control
            var relay1 = processor.SwitchedOutputs[1] as GenericRelayDeviceTestable;
            relay1.Should().NotBeNull();

            // Test On/Off operations
            relay1.On();
            relay1.IsOn.Should().BeTrue();

            relay1.Off();
            relay1.IsOn.Should().BeFalse();

            // Test toggle
            relay1.PowerToggle();
            relay1.IsOn.Should().BeTrue();

            relay1.PowerToggle();
            relay1.IsOn.Should().BeFalse();

            // Test feedback
            int feedbackCount = 0;
            relay1.OutputIsOnFeedback.OutputChange += (sender, args) => feedbackCount++;

            relay1.On();
            feedbackCount.Should().Be(1);

            relay1.Off();
            feedbackCount.Should().Be(2);
        }

        [Fact]
        public void FactoryPattern_AllowsSwitchingBetweenProviders()
        {
            // Arrange - Start with test mode
            CrestronEnvironmentFactory.EnableTestMode();
            CrestronEnvironmentFactory.IsTestMode.Should().BeTrue();

            var testProcessor = new CrestronProcessorTestable("test-mode");
            testProcessor.Processor.Should().BeOfType<MockControlSystem>();

            // Act - Switch to runtime mode
            CrestronEnvironmentFactory.DisableTestMode();
            CrestronEnvironmentFactory.IsTestMode.Should().BeFalse();

            // Note: In runtime mode without actual Crestron hardware, 
            // it will use the NullControlSystem implementation
            var runtimeProcessor = new CrestronProcessorTestable("runtime-mode");
            runtimeProcessor.Processor.Should().NotBeNull();

            // Act - Switch back to test mode
            CrestronEnvironmentFactory.EnableTestMode();
            CrestronEnvironmentFactory.IsTestMode.Should().BeTrue();

            var testProcessor2 = new CrestronProcessorTestable("test-mode-2");
            testProcessor2.Processor.Should().BeOfType<MockControlSystem>();
        }
    }
}