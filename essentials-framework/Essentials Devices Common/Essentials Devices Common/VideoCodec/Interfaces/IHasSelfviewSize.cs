using PepperDash.Essentials.Devices.Common.VideoCodec.Cisco;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
	public interface IHasSelfviewSize
	{
		StringFeedback SelfviewPipSizeFeedback { get; }

		void SelfviewPipSizeSet(CodecCommandWithLabel size);

		void SelfviewPipSizeToggle();
	}
}