using System;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.Queues
{
    /// <summary>
    /// Defines the contract for IQueue
    /// </summary>
    public interface IQueue<T> : IKeyed, IDisposable where T : class 
    {
        /// <summary>
        /// Enqueues an item
        /// </summary>
        /// <param name="item">item to be queued</param>
        void Enqueue(T item);

        /// <summary>
        /// gets the disposed status of the queue
        /// </summary>
        bool Disposed { get; }
    }
}
