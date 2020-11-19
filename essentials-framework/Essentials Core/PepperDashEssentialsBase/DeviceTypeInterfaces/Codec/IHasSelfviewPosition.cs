using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Devices;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    public interface IHasSelfviewPosition
    {
        StringFeedback SelfviewPipPositionFeedback { get; }

        void SelfviewPipPositionSet(CodecCommandWithLabel position);

        void SelfviewPipPositionToggle();
    }
}