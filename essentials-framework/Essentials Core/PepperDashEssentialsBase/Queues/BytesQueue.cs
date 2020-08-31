using System;
using PepperDash.Core;

namespace PepperDash_Essentials_Core.Queues
{
    /// <summary>
    /// Byte implementation of Action queue
    /// </summary>
    public class BytesQueue : IQueue<byte[]>
    {
        private readonly IQueue<byte[]> _queue;

        /// <summary>
        /// Constructor for BytesQueue
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="processBytesAction">Action to process queued bytes</param>
        public BytesQueue(string key, Action<byte[]> processBytesAction)
        {
            _queue = new GenericQueue<byte[]>(key, processBytesAction);
        }

        /// <summary>
        /// Constructor for BytesQueue
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="processBytesAction">Action to process queued bytes</param>
        /// <param name="pacing">Delay in ms between actions being invoked</param>
        public BytesQueue(string key, Action<byte[]> processBytesAction, int pacing)
        {
            _queue = new GenericQueue<byte[]>(key, processBytesAction, pacing);
        }

        /// <summary>
        /// Enqueue a byte array to be processed
        /// </summary>
        /// <param name="item">Byte array to be processed</param>
        public void Enqueue(byte[] item)
        {
            _queue.Enqueue(item);
        }

        /// <summary>
        /// If the instance has been disposed
        /// </summary>
        public bool Disposed
        {
            get { return _queue.Disposed; }
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
    }
}