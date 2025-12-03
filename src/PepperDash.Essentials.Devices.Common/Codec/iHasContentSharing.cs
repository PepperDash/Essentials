using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Defines the contract for IHasContentSharing
    /// </summary>
    public interface IHasContentSharing
    {
        /// <summary>
        /// Gets feedback indicating whether content sharing is currently active
        /// </summary>
        BoolFeedback SharingContentIsOnFeedback { get; }

        /// <summary>
        /// Gets feedback about the current sharing source
        /// </summary>
        StringFeedback SharingSourceFeedback { get; }

        /// <summary>
        /// Gets a value indicating whether content should be automatically shared while in a call
        /// </summary>
        bool AutoShareContentWhileInCall { get; }

        /// <summary>
        /// Starts content sharing
        /// </summary>
        void StartSharing();

        /// <summary>
        /// Stops content sharing
        /// </summary>
        void StopSharing();
    }

}