using System;
using Crestron.SimplSharp;
using PepperDash.Core;

namespace PepperDash_Essentials_Core.Queues
{
    public sealed class StringResponseProcessor : IKeyed, IDisposable
    {
        private readonly IQueue<string> _queue;
        private readonly IBasicCommunication _coms;
        private readonly CommunicationGather _gather;

        private StringResponseProcessor(string key, Action<string> processStringAction)
        {
            _queue = new StringQueue(key, processStringAction);

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
            _queue.Enqueue(args.Text);
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
        /// If the instance has been disposed or not.  If it has, you can not use it anymore
        /// </summary>
        public bool Disposed { get; private set; }

        ~StringResponseProcessor()
        {
            Dispose(false);
        }
    }
}