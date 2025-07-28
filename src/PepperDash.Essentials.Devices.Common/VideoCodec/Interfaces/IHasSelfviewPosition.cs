using PepperDash.Essentials.Core.Feedbacks;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Defines the contract for IHasSelfviewPosition
    /// </summary>
    public interface IHasSelfviewPosition
    {
        StringFeedback SelfviewPipPositionFeedback { get; }

        void SelfviewPipPositionSet(CodecCommandWithLabel position);

        void SelfviewPipPositionToggle();
    }
}