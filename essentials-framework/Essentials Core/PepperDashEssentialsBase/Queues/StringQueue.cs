using System;

namespace PepperDash_Essentials_Core.Queues
{
    /// <summary>
    /// String implementation of Action Queue
    /// </summary>
    public class StringQueue : IQueue<string>
    {
        private readonly IQueue<string> _queue;

        /// <summary>
        /// Constructor for BytesQueue
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="processStringAction">Action to process queued strings</param>
        public StringQueue(string key, Action<string> processStringAction)
        {
            _queue = new GenericQueue<string>(key, processStringAction);
        }

        /// <summary>
        /// Constructor for StringQueue
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="processStringAction">Action to process queued strings</param>
        /// <param name="pacing">Delay in ms between actions being invoked</param>
        public StringQueue(string key, Action<string> processStringAction, int pacing)
        {
            _queue = new GenericQueue<string>(key, processStringAction, pacing);
        }

        /// <summary>
        /// Enqueue a byte array to be processed
        /// </summary>
        /// <param name="item">Byte array to be processed</param>
        public void Enqueue(string item)
        {
            _queue.Enqueue(item);
        }

        /// <summary>
        /// Key
        /// </summary>
        public string Key
        {
            get { return _queue.Key; }
        }

        /// <summary>
        /// Disposes of resources
        /// </summary>
        public void Dispose()
        {
            _queue.Dispose();
        }

        /// <summary>
        /// If the instance has been disposed
        /// </summary>
        public bool Disposed
        {
            get { return _queue.Disposed; }
        }
    }
}