using System;
using FluentAssertions;
using Moq;
using PepperDash.Essentials.Core.Abstractions;
using Xunit;

namespace PepperDash.Essentials.Core.Tests.Abstractions
{
    public class VersiPortTests
    {
        [Fact]
        public void DigitalIn_ReturnsCorrectValue()
        {
            // Arrange
            var mockVersiPort = new Mock<IVersiPort>();
            mockVersiPort.Setup(v => v.DigitalIn).Returns(true);
            
            // Act
            var digitalIn = mockVersiPort.Object.DigitalIn;
            
            // Assert
            digitalIn.Should().BeTrue();
        }

        [Fact]
        public void SetDigitalOut_SetsCorrectValue()
        {
            // Arrange
            var mockVersiPort = new Mock<IVersiPort>();
            
            // Act
            mockVersiPort.Object.SetDigitalOut(true);
            
            // Assert
            mockVersiPort.Verify(v => v.SetDigitalOut(true), Times.Once);
        }

        [Fact]
        public void AnalogIn_ReturnsCorrectValue()
        {
            // Arrange
            var mockVersiPort = new Mock<IVersiPort>();
            ushort expectedValue = 32768;
            mockVersiPort.Setup(v => v.AnalogIn).Returns(expectedValue);
            
            // Act
            var analogIn = mockVersiPort.Object.AnalogIn;
            
            // Assert
            analogIn.Should().Be(expectedValue);
        }

        [Fact]
        public void VersiportChange_WhenDigitalChanges_RaisesEventWithCorrectType()
        {
            // Arrange
            var mockVersiPort = new Mock<IVersiPort>();
            var eventRaised = false;
            VersiPortEventType? capturedEventType = null;
            object capturedValue = null;
            
            mockVersiPort.Object.VersiportChange += (sender, args) =>
            {
                eventRaised = true;
                capturedEventType = args.EventType;
                capturedValue = args.Value;
            };
            
            // Act
            mockVersiPort.Raise(v => v.VersiportChange += null,
                mockVersiPort.Object,
                new VersiPortEventArgs 
                { 
                    EventType = VersiPortEventType.DigitalInChange,
                    Value = true
                });
            
            // Assert
            eventRaised.Should().BeTrue();
            capturedEventType.Should().Be(VersiPortEventType.DigitalInChange);
            capturedValue.Should().Be(true);
        }

        [Fact]
        public void VersiportChange_WhenAnalogChanges_RaisesEventWithCorrectValue()
        {
            // Arrange
            var mockVersiPort = new Mock<IVersiPort>();
            var eventRaised = false;
            VersiPortEventType? capturedEventType = null;
            object capturedValue = null;
            ushort expectedAnalogValue = 12345;
            
            mockVersiPort.Object.VersiportChange += (sender, args) =>
            {
                eventRaised = true;
                capturedEventType = args.EventType;
                capturedValue = args.Value;
            };
            
            // Act
            mockVersiPort.Raise(v => v.VersiportChange += null,
                mockVersiPort.Object,
                new VersiPortEventArgs 
                { 
                    EventType = VersiPortEventType.AnalogInChange,
                    Value = expectedAnalogValue
                });
            
            // Assert
            eventRaised.Should().BeTrue();
            capturedEventType.Should().Be(VersiPortEventType.AnalogInChange);
            capturedValue.Should().Be(expectedAnalogValue);
        }

        [Fact]
        public void MultipleVersiportChanges_TracksAllChangesCorrectly()
        {
            // Arrange
            var mockVersiPort = new Mock<IVersiPort>();
            var changes = new System.Collections.Generic.List<(VersiPortEventType type, object value)>();
            
            mockVersiPort.Object.VersiportChange += (sender, args) =>
            {
                changes.Add((args.EventType, args.Value));
            };
            
            // Act - Simulate multiple changes
            mockVersiPort.Raise(v => v.VersiportChange += null,
                mockVersiPort.Object,
                new VersiPortEventArgs 
                { 
                    EventType = VersiPortEventType.DigitalInChange,
                    Value = true
                });
            
            mockVersiPort.Raise(v => v.VersiportChange += null,
                mockVersiPort.Object,
                new VersiPortEventArgs 
                { 
                    EventType = VersiPortEventType.AnalogInChange,
                    Value = (ushort)30000
                });
            
            mockVersiPort.Raise(v => v.VersiportChange += null,
                mockVersiPort.Object,
                new VersiPortEventArgs 
                { 
                    EventType = VersiPortEventType.DigitalInChange,
                    Value = false
                });
            
            // Assert
            changes.Should().HaveCount(3);
            changes[0].type.Should().Be(VersiPortEventType.DigitalInChange);
            changes[0].value.Should().Be(true);
            changes[1].type.Should().Be(VersiPortEventType.AnalogInChange);
            changes[1].value.Should().Be((ushort)30000);
            changes[2].type.Should().Be(VersiPortEventType.DigitalInChange);
            changes[2].value.Should().Be(false);
        }
    }
}