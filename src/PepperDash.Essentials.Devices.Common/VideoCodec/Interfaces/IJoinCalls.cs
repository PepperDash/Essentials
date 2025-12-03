using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    /// <summary>
    /// Defines the contract for IJoinCalls
    /// </summary>
    public interface IJoinCalls
    {
        /// <summary>
        /// Joins a call
        /// </summary>
        /// <param name="activeCall">The active call to join</param>
        void JoinCall(CodecActiveCallItem activeCall);

        /// <summary>
        /// Joins all calls
        /// </summary>
        void JoinAllCalls();
    }
}