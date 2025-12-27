using System;
using System.Collections.Generic;
using PepperDash.Essentials.Core.Tests.Abstractions;

namespace PepperDash.Essentials.Core.Tests.TestableComponents
{
    /// <summary>
    /// A simplified, testable action sequence that demonstrates abstraction patterns
    /// This shows how we can separate business logic from SDK dependencies
    /// </summary>
    public class TestableActionSequence
    {
        private readonly IQueue<TestableSequencedAction> _actionQueue;
        private readonly IThreadService _threadService;
        private readonly ILogger _logger;
        private readonly List<TestableSequencedAction> _configuredActions;
        
        private object _workerThread;
        private bool _allowActionsToExecute;

        public TestableActionSequence(
            IQueue<TestableSequencedAction> actionQueue,
            IThreadService threadService,
            ILogger logger,
            List<TestableSequencedAction> actions)
        {
            _actionQueue = actionQueue ?? throw new ArgumentNullException(nameof(actionQueue));
            _threadService = threadService ?? throw new ArgumentNullException(nameof(threadService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuredActions = actions ?? new List<TestableSequencedAction>();
        }

        /// <summary>
        /// Starts executing the sequenced actions
        /// </summary>
        public void StartSequence()
        {
            if (_workerThread != null && _threadService.IsThreadRunning(_workerThread))
            {
                _logger.LogDebug(this, "Thread already running. Cannot Start Sequence");
                return;
            }

            _logger.LogDebug(this, "Starting Action Sequence");
            _allowActionsToExecute = true;
            AddActionsToQueue();
            _workerThread = _threadService.CreateAndStartThread(ProcessActions, null);
        }

        /// <summary>
        /// Stops executing the sequenced actions
        /// </summary>
        public void StopSequence()
        {
            _logger.LogDebug(this, "Stopping Action Sequence");
            _allowActionsToExecute = false;
            if (_workerThread != null)
            {
                _threadService.AbortThread(_workerThread);
            }
        }

        /// <summary>
        /// Gets the current status of the sequence
        /// </summary>
        public bool IsRunning => _allowActionsToExecute && _threadService.IsThreadRunning(_workerThread);

        /// <summary>
        /// Gets the number of pending actions
        /// </summary>
        public int PendingActionsCount => _actionQueue.Count;

        /// <summary>
        /// Populates the queue from the configuration information
        /// </summary>
        private void AddActionsToQueue()
        {
            _logger.LogDebug(this, "Adding {0} actions to queue", _configuredActions.Count);

            foreach (var action in _configuredActions)
            {
                _actionQueue.Enqueue(action);
            }
        }

        private object ProcessActions(object obj)
        {
            while (_allowActionsToExecute && _actionQueue.Count > 0)
            {
                var action = _actionQueue.Dequeue();
                if (action == null)
                    break;

                // Delay before executing
                if (action.DelayMs > 0)
                    _threadService.Sleep(action.DelayMs);

                ExecuteAction(action);
            }

            return null;
        }

        private void ExecuteAction(TestableSequencedAction action)
        {
            if (action == null)
                return;

            try
            {
                _logger.LogDebug(this, "Executing action: {0} with delay: {1}ms", action.Name, action.DelayMs);
                action.Execute();
            }
            catch (Exception e)
            {
                _logger.LogVerbose(this, "Error Executing Action: {0}", e);
            }
        }
    }

    /// <summary>
    /// A testable action that can be sequenced
    /// </summary>
    public class TestableSequencedAction
    {
        public string Name { get; set; }
        public int DelayMs { get; set; }
        public Action ActionToExecute { get; set; }

        public TestableSequencedAction(string name, int delayMs = 0, Action actionToExecute = null)
        {
            Name = name;
            DelayMs = delayMs;
            ActionToExecute = actionToExecute ?? (() => { });
        }

        public void Execute()
        {
            ActionToExecute?.Invoke();
        }
    }
}