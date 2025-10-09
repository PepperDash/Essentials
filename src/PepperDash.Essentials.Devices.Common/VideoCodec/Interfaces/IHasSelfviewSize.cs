using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
	/// <summary>
	/// Defines the contract for IHasSelfviewSize
	/// </summary>
	public interface IHasSelfviewSize
	{
		/// <summary>
		/// Gets the SelfviewPipSizeFeedback
		/// </summary>
		StringFeedback SelfviewPipSizeFeedback { get; }

		/// <summary>
		/// Sets the selfview size
		/// </summary>
		/// <param name="size">The new selfview size</param>
		void SelfviewPipSizeSet(CodecCommandWithLabel size);

		/// <summary>
		/// Toggles the selfview size
		/// </summary>
		void SelfviewPipSizeToggle();
	}
}