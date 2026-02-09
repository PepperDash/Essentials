using System;

namespace PepperDash.Essentials.Core.Queues
{
    /// <summary>
    /// Defines the contract for IQueueMessage
    /// </summary>
    public interface IQueueMessage
    {
        /// <summary>
        /// Dispatches the message
        /// </summary>
        void Dispatch();
    }
}