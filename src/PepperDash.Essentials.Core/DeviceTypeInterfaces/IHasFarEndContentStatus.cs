namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Defines the contract for IHasFarEndContentStatus
    /// </summary>
    public interface IHasFarEndContentStatus
    {
        /// <summary>
        /// Gets whether far end content is being received
        /// </summary>
         BoolFeedback ReceivingContent { get; }
    }
}