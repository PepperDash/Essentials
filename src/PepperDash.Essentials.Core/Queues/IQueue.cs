using System;

namespace PepperDash.Essentials.Core.Queues
{
    /// <summary>
    /// Defines the contract for IQueue
    /// </summary>
    public interface IQueue<T> : IKeyed, IDisposable where T : class 
    {
        void Enqueue(T item);
        bool Disposed { get; }
    }
}
