namespace PepperDash.Essentials.Core.Queues
{
    /// <summary>
    /// Defines the contract for IQueueMessage
    /// </summary>
    public interface IQueueMessage
    {
        void Dispatch();
    }
}