using System.Collections.Generic;
using FluentAssertions;
using Moq;
using PepperDash.Essentials.Core.Abstractions;
using PepperDash.Essentials.Core.Devices;
using Xunit;

namespace PepperDash.Essentials.Core.Tests.Devices
{
    public class CrestronProcessorTests
    {
        [Fact]
        public void Constructor_WithValidProcessor_InitializesSwitchedOutputs()
        {
            // Arrange
            var mockProcessor = new Mock<ICrestronControlSystem>();
            mockProcessor.Setup(p => p.SupportsRelay).Returns(false);
            
            // Act
            var processor = new CrestronProcessorTestable("test-processor", mockProcessor.Object);
            
            // Assert
            processor.Should().NotBeNull();
            processor.Key.Should().Be("test-processor");
            processor.SwitchedOutputs.Should().NotBeNull();
            processor.SwitchedOutputs.Should().BeEmpty();
        }

        [Fact]
        public void GetRelays_WhenProcessorSupportsRelays_CreatesRelayDevices()
        {
            // Arrange
            var mockProcessor = new Mock<ICrestronControlSystem>();
            var mockRelayPort1 = new Mock<IRelayPort>();
            var mockRelayPort2 = new Mock<IRelayPort>();
            
            var relayPorts = new Dictionary<uint, IRelayPort>
            {
                { 1, mockRelayPort1.Object },
                { 2, mockRelayPort2.Object }
            };
            
            mockProcessor.Setup(p => p.SupportsRelay).Returns(true);
            mockProcessor.Setup(p => p.NumberOfRelayPorts).Returns(2);
            mockProcessor.Setup(p => p.RelayPorts).Returns(relayPorts);
            
            // Act
            var processor = new CrestronProcessorTestable("test-processor", mockProcessor.Object);
            
            // Assert
            processor.SwitchedOutputs.Should().HaveCount(2);
            processor.SwitchedOutputs.Should().ContainKey(1);
            processor.SwitchedOutputs.Should().ContainKey(2);
        }

        [Fact]
        public void GetRelays_WhenProcessorDoesNotSupportRelays_DoesNotCreateRelayDevices()
        {
            // Arrange
            var mockProcessor = new Mock<ICrestronControlSystem>();
            mockProcessor.Setup(p => p.SupportsRelay).Returns(false);
            
            // Act
            var processor = new CrestronProcessorTestable("test-processor", mockProcessor.Object);
            
            // Assert
            processor.SwitchedOutputs.Should().BeEmpty();
            mockProcessor.Verify(p => p.NumberOfRelayPorts, Times.Never);
            mockProcessor.Verify(p => p.RelayPorts, Times.Never);
        }
    }

    public class GenericRelayDeviceTests
    {
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

        [Fact]
        public void CloseRelay_CallsRelayPortClose()
        {
            // Arrange
            var mockRelayPort = new Mock<IRelayPort>();
            var device = new GenericRelayDeviceTestable("test-relay", mockRelayPort.Object);
            
            // Act
            device.CloseRelay();
            
            // Assert
            mockRelayPort.Verify(r => r.Close(), Times.Once);
        }

        [Fact]
        public void PulseRelay_CallsRelayPortPulseWithCorrectDelay()
        {
            // Arrange
            var mockRelayPort = new Mock<IRelayPort>();
            var device = new GenericRelayDeviceTestable("test-relay", mockRelayPort.Object);
            const int delayMs = 500;
            
            // Act
            device.PulseRelay(delayMs);
            
            // Assert
            mockRelayPort.Verify(r => r.Pulse(delayMs), Times.Once);
        }

        [Fact]
        public void On_CallsCloseRelay()
        {
            // Arrange
            var mockRelayPort = new Mock<IRelayPort>();
            var device = new GenericRelayDeviceTestable("test-relay", mockRelayPort.Object);
            
            // Act
            device.On();
            
            // Assert
            mockRelayPort.Verify(r => r.Close(), Times.Once);
        }

        [Fact]
        public void Off_CallsOpenRelay()
        {
            // Arrange
            var mockRelayPort = new Mock<IRelayPort>();
            var device = new GenericRelayDeviceTestable("test-relay", mockRelayPort.Object);
            
            // Act
            device.Off();
            
            // Assert
            mockRelayPort.Verify(r => r.Open(), Times.Once);
        }

        [Fact]
        public void PowerToggle_WhenRelayIsOn_CallsOff()
        {
            // Arrange
            var mockRelayPort = new Mock<IRelayPort>();
            mockRelayPort.Setup(r => r.State).Returns(true); // Relay is ON
            var device = new GenericRelayDeviceTestable("test-relay", mockRelayPort.Object);
            
            // Act
            device.PowerToggle();
            
            // Assert
            mockRelayPort.Verify(r => r.Open(), Times.Once);
            mockRelayPort.Verify(r => r.Close(), Times.Never);
        }

        [Fact]
        public void PowerToggle_WhenRelayIsOff_CallsOn()
        {
            // Arrange
            var mockRelayPort = new Mock<IRelayPort>();
            mockRelayPort.Setup(r => r.State).Returns(false); // Relay is OFF
            var device = new GenericRelayDeviceTestable("test-relay", mockRelayPort.Object);
            
            // Act
            device.PowerToggle();
            
            // Assert
            mockRelayPort.Verify(r => r.Close(), Times.Once);
            mockRelayPort.Verify(r => r.Open(), Times.Never);
        }

        [Fact]
        public void IsOn_ReturnsRelayPortState()
        {
            // Arrange
            var mockRelayPort = new Mock<IRelayPort>();
            mockRelayPort.Setup(r => r.State).Returns(true);
            var device = new GenericRelayDeviceTestable("test-relay", mockRelayPort.Object);
            
            // Act
            var isOn = device.IsOn;
            
            // Assert
            isOn.Should().BeTrue();
        }
    }
}