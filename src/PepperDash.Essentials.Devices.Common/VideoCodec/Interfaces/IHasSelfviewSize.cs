using PepperDash.Essentials.Core.Feedbacks;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
 /// <summary>
 /// Defines the contract for IHasSelfviewSize
 /// </summary>
	public interface IHasSelfviewSize
	{
		StringFeedback SelfviewPipSizeFeedback { get; }

		void SelfviewPipSizeSet(CodecCommandWithLabel size);

		void SelfviewPipSizeToggle();
	}
}