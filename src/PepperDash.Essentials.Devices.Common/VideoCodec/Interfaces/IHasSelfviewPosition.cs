using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.VideoCodec.Cisco;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    public interface IHasSelfviewPosition
    {
        StringFeedback SelfviewPipPositionFeedback { get; }

        void SelfviewPipPositionSet(CodecCommandWithLabel position);

        void SelfviewPipPositionToggle();
    }
}