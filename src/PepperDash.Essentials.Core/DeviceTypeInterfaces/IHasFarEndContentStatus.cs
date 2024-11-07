using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    public interface IHasFarEndContentStatus
    {
         BoolFeedback ReceivingContent { get; }
    }
}