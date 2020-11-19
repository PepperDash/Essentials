using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.Core.Devices.VideoCodec
{
	/// <summary>
	/// For rooms that have video codec
	/// </summary>
	public interface IHasVideoCodec
	{
		VideoCodecBase VideoCodec { get; }
		BoolFeedback InCallFeedback { get; }

        ///// <summary>
        ///// Make this more specific
        ///// </summary>
        //List<PepperDash.Essentials.Devices.Common.Codec.CodecActiveCallItem> ActiveCalls { get; }

		/// <summary>
		/// States: 0 for on hook, 1 for video, 2 for audio, 3 for telekenesis
		/// </summary>
		IntFeedback CallTypeFeedback { get; }

		/// <summary>
		/// 
		/// </summary>
		BoolFeedback PrivacyModeIsOnFeedback { get; }

		/// <summary>
		/// When something in the room is sharing with the far end or through other means
		/// </summary>
		BoolFeedback IsSharingFeedback { get; }

	}
}