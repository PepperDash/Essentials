using System;
using FluentAssertions;
using Moq;
using PepperDash.Essentials.Core.Abstractions;
using Xunit;

namespace PepperDash.Essentials.Core.Tests.Abstractions
{
    public class DigitalInputTests
    {
        [Fact]
        public void StateChange_WhenDigitalInputChanges_RaisesEvent()
        {
            // Arrange
            var mockDigitalInput = new Mock<IDigitalInput>();
            var eventRaised = false;
            bool capturedState = false;
            
            mockDigitalInput.Setup(d => d.State).Returns(true);
            
            // Subscribe to the event
            mockDigitalInput.Object.StateChange += (sender, args) =>
            {
                eventRaised = true;
                capturedState = args.State;
            };
            
            // Act - Raise the event
            mockDigitalInput.Raise(d => d.StateChange += null, 
                mockDigitalInput.Object, 
                new DigitalInputEventArgs(true));
            
            // Assert
            eventRaised.Should().BeTrue();
            capturedState.Should().BeTrue();
        }

        [Fact]
        public void State_ReturnsCorrectValue()
        {
            // Arrange
            var mockDigitalInput = new Mock<IDigitalInput>();
            mockDigitalInput.Setup(d => d.State).Returns(true);
            
            // Act
            var state = mockDigitalInput.Object.State;
            
            // Assert
            state.Should().BeTrue();
        }

        [Fact]
        public void MultipleStateChanges_TrackStateCorrectly()
        {
            // Arrange
            var mockDigitalInput = new Mock<IDigitalInput>();
            var stateChanges = new System.Collections.Generic.List<bool>();
            
            mockDigitalInput.Object.StateChange += (sender, args) =>
            {
                stateChanges.Add(args.State);
            };
            
            // Act - Simulate multiple state changes
            mockDigitalInput.Raise(d => d.StateChange += null, 
                mockDigitalInput.Object, 
                new DigitalInputEventArgs(true));
            
            mockDigitalInput.Raise(d => d.StateChange += null, 
                mockDigitalInput.Object, 
                new DigitalInputEventArgs(false));
            
            mockDigitalInput.Raise(d => d.StateChange += null, 
                mockDigitalInput.Object, 
                new DigitalInputEventArgs(true));
            
            // Assert
            stateChanges.Should().HaveCount(3);
            stateChanges[0].Should().BeTrue();
            stateChanges[1].Should().BeFalse();
            stateChanges[2].Should().BeTrue();
        }
    }
}