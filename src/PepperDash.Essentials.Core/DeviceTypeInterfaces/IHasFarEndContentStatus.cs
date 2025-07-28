using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Defines the contract for IHasFarEndContentStatus
    /// </summary>
    public interface IHasFarEndContentStatus
    {
         BoolFeedback ReceivingContent { get; }
    }
}