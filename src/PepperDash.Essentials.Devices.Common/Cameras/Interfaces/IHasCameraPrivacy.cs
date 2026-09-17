using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Describes a camera that can be sent to a privacy position and brought back.
    /// </summary>
    /// <remarks>
    /// <para>Distinct from <see cref="IHasCameraMute"/> and <see cref="IHasCameraOff"/>, which stop
    /// the picture reaching somewhere. Privacy physically points the camera at a harmless shot, so
    /// it remains visibly a camera pointed at a wall rather than a camera that may or may not be
    /// recording — which is the assurance the people in the room are looking for.</para>
    /// <para>Which shot is the harmless one, and where the camera should return to, are properties
    /// of how that camera was installed. They belong to the camera rather than to whatever asks it
    /// for privacy, which knows only when.</para>
    /// </remarks>
    public interface IHasCameraPrivacy : IHasCameraControls
    {
        /// <summary>
        /// Feedback that indicates whether the camera is in its privacy position.
        /// </summary>
        BoolFeedback PrivacyIsOnFeedback { get; }

        /// <summary>
        /// Sends the camera to its privacy position.
        /// </summary>
        void PrivacyOn();

        /// <summary>
        /// Returns the camera to where it should be when privacy lifts.
        /// </summary>
        void PrivacyOff();
    }
}
