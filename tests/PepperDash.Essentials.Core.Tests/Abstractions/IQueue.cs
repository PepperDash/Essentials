using System;

namespace PepperDash.Essentials.Core.Tests.Abstractions
{
    /// <summary>
    /// Abstraction for queue operations to enable testing
    /// </summary>
    /// <typeparam name="T">Type of items in the queue</typeparam>
    public interface IQueue<T>
    {
        /// <summary>
        /// Number of items in the queue
        /// </summary>
        int Count { get; }
        
        /// <summary>
        /// Adds an item to the queue
        /// </summary>
        /// <param name="item">Item to add</param>
        void Enqueue(T item);
        
        /// <summary>
        /// Removes and returns the next item from the queue
        /// </summary>
        /// <returns>The next item, or default(T) if queue is empty</returns>
        T Dequeue();
    }
}