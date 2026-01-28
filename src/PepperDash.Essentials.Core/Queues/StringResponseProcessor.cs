using System;
using Crestron.SimplSharp;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.Queues
{
    /// <summary>
    /// Processes string responses from a communication port or gather using a queue to ensure thread safety
    /// </summary>
    public sealed class StringResponseProcessor : IKeyed, IDisposable
    {
        private readonly Action<string> _processStringAction; 
        private readonly IQueue<IQueueMessage> _queue;
        private readonly IBasicCommunication _coms;
        private readonly CommunicationGather _gather;

        private StringResponseProcessor(string key, Action<string> processStringAction)
        {
            _processStringAction = processStringAction;
            _queue = new GenericQueue(key);

            CrestronEnvironment.ProgramStatusEventHandler += programEvent =>
            {
                if (programEvent != eProgramStatusEventType.Stopping)
                    return;

                Dispose();
            };
        }

        /// <summary>
        /// Constructor that builds an instance and subscribes to coms TextReceived for processing
        /// </summary>
        /// <param name="coms">Com port to process strings from</param>
        /// <param name="processStringAction">Action to process the incoming strings</param>
        public StringResponseProcessor(IBasicCommunication coms, Action<string> processStringAction)
            : this(coms.Key, processStringAction)
        {
            _coms = coms;
            coms.TextReceived += OnResponseReceived;
        }

        /// <summary>
        /// Constructor that builds an instance and subscribes to gather Line Received for processing
        /// </summary>
        /// <param name="gather">Gather to process strings from</param>
        /// <param name="processStringAction">Action to process the incoming strings</param>
        public StringResponseProcessor(CommunicationGather gather, Action<string> processStringAction)
            : this(gather.Port.Key, processStringAction)
        {
            _gather = gather;
            gather.LineReceived += OnResponseReceived;
        }

        private void OnResponseReceived(object sender, GenericCommMethodReceiveTextArgs args)
        {
            _queue.Enqueue(new ProcessStringMessage(args.Text, _processStringAction));
        }

        /// <summary>
        /// Key
        /// </summary>
        public string Key
        {
            get { return _queue.Key; }
        }

        /// <summary>
        /// Disposes the instance and cleans up resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            CrestronEnvironment.GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                if (_coms != null)
                    _coms.TextReceived -= OnResponseReceived;

                if (_gather != null)
                {
                    _gather.LineReceived -= OnResponseReceived;
                    _gather.Stop();
                }

                _queue.Dispose();
            }

            Disposed = true;
        }

        /// <summary>
        /// Gets or sets the Disposed
        /// </summary>
        public bool Disposed { get; private set; }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~StringResponseProcessor()
        {
            Dispose(false);
        }
    }
}
