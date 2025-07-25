using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Defines the contract for IHasContentSharing
    /// </summary>
    public interface IHasContentSharing
    {
        BoolFeedback SharingContentIsOnFeedback { get; }
        StringFeedback SharingSourceFeedback { get; }

        bool AutoShareContentWhileInCall { get; }

        void StartSharing();
        void StopSharing();
    }

}