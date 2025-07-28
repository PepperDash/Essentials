using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Feedbacks;
using PepperDash.Essentials.Core.Rooms;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
	/// <summary>
	/// For rooms that have video codec
	/// </summary>
	public interface IHasVideoCodec:IHasInCallFeedback,IPrivacy
	{
		VideoCodecBase VideoCodec { get; }

        /// <summary>
        /// Make this more specific
        /// </summary>
        //List<PepperDash.Essentials.Devices.Common.Codec.CodecActiveCallItem> ActiveCalls { get; }

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