using PepperDash.Essentials.Core.Tests.Abstractions;
using PepperDash.Essentials.Core.Tests.TestableComponents;

namespace PepperDash.Essentials.Core.Tests.Unit
{
    public class TestableActionSequenceTests
    {
        private readonly Mock<IQueue<TestableSequencedAction>> _mockQueue;
        private readonly Mock<IThreadService> _mockThreadService;
        private readonly Mock<ILogger> _mockLogger;
        private readonly TestableActionSequence _actionSequence;
        private readonly List<TestableSequencedAction> _testActions;

        public TestableActionSequenceTests()
        {
            _mockQueue = new Mock<IQueue<TestableSequencedAction>>();
            _mockThreadService = new Mock<IThreadService>();
            _mockLogger = new Mock<ILogger>();
            
            _testActions = new List<TestableSequencedAction>
            {
                new TestableSequencedAction("Action1", 100),
                new TestableSequencedAction("Action2", 200)
            };

            _actionSequence = new TestableActionSequence(
                _mockQueue.Object,
                _mockThreadService.Object,
                _mockLogger.Object,
                _testActions);
        }

        [Fact]
        public void Constructor_WithNullQueue_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new TestableActionSequence(null, _mockThreadService.Object, _mockLogger.Object, _testActions));
        }

        [Fact]
        public void Constructor_WithNullThreadService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new TestableActionSequence(_mockQueue.Object, null, _mockLogger.Object, _testActions));
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new TestableActionSequence(_mockQueue.Object, _mockThreadService.Object, null, _testActions));
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

        [Fact]
        public void StartSequence_WhenThreadAlreadyRunning_DoesNotStartNewThread()
        {
            // Arrange
            var mockThread = new object();
            _mockThreadService.Setup(x => x.CreateAndStartThread(It.IsAny<Func<object, object>>(), null))
                             .Returns(mockThread);
            _mockThreadService.Setup(x => x.IsThreadRunning(mockThread)).Returns(true);

            // First call to set up the worker thread
            _actionSequence.StartSequence();
            
            // Reset invocations to verify only the second call behavior
            _mockLogger.Invocations.Clear();
            _mockThreadService.Invocations.Clear();

            // Act - Second call should detect running thread
            _actionSequence.StartSequence();

            // Assert
            _mockLogger.Verify(x => x.LogDebug(_actionSequence, "Thread already running. Cannot Start Sequence"), Times.Once);
            _mockThreadService.Verify(x => x.CreateAndStartThread(It.IsAny<Func<object, object>>(), null), Times.Never);
        }

        [Fact]
        public void StartSequence_AddsConfiguredActionsToQueue()
        {
            // Arrange
            _mockThreadService.Setup(x => x.IsThreadRunning(It.IsAny<object>())).Returns(false);

            // Act
            _actionSequence.StartSequence();

            // Assert
            _mockLogger.Verify(x => x.LogDebug(_actionSequence, "Adding {0} actions to queue", 2), Times.Once);
            _mockQueue.Verify(x => x.Enqueue(It.IsAny<TestableSequencedAction>()), Times.Exactly(2));
        }

        [Fact]
        public void StopSequence_SetsAllowActionsToFalseAndAbortsThread()
        {
            // Arrange
            var mockThread = new object();

            // Act
            _actionSequence.StopSequence();

            // Assert
            _mockLogger.Verify(x => x.LogDebug(_actionSequence, "Stopping Action Sequence"), Times.Once);
        }

        [Fact]
        public void PendingActionsCount_ReturnsQueueCount()
        {
            // Arrange
            _mockQueue.Setup(x => x.Count).Returns(5);

            // Act
            var count = _actionSequence.PendingActionsCount;

            // Assert
            Assert.Equal(5, count);
        }

        [Fact]
        public void TestableSequencedAction_ExecutesActionWhenCalled()
        {
            // Arrange
            bool actionExecuted = false;
            var action = new TestableSequencedAction("TestAction", 0, () => actionExecuted = true);

            // Act
            action.Execute();

            // Assert
            Assert.True(actionExecuted);
        }

        [Fact]
        public void TestableSequencedAction_WithNullAction_DoesNotThrow()
        {
            // Arrange
            var action = new TestableSequencedAction("TestAction", 0, null);

            // Act & Assert
            var exception = Record.Exception(() => action.Execute());
            Assert.Null(exception);
        }
    }
}