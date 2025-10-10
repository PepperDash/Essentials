using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
	/// <summary>
	/// For rooms that have video codec
	/// </summary>
	public interface IHasVideoCodec : IHasInCallFeedback, IPrivacy
	{
		/// <summary>
		/// Gets the VideoCodecBase instance
		/// </summary>
		VideoCodecBase VideoCodec { get; }

		/// <summary>
		/// States: 0 for on hook, 1 for video, 2 for audio, 3 for telekenesis
		/// </summary>
		IntFeedback CallTypeFeedback { get; }

		/// <summary>
		/// When something in the room is sharing with the far end or through other means
		/// </summary>
		BoolFeedback IsSharingFeedback { get; }

	}
}