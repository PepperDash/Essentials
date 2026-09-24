namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Describes a camera that can be sent to a privacy position and brought back.
    /// </summary>
    /// <remarks>
    /// <para>Distinct from <see cref="IHasCameraMute"/> and <see cref="IHasCameraOff"/>, which stop
    /// the picture reaching somewhere. Privacy physically points the camera at a harmless shot, so
    /// it remains visibly a camera aimed at a wall rather than a camera that may or may not be
    /// recording — which is the assurance the people in the room are looking for.</para>
    /// <para>Which shot is the harmless one, and where the camera should return to, are properties
    /// of how that camera was installed. They belong to the camera rather than to whatever asks it
    /// for privacy, which knows only when.</para>
    /// <para>No feedback, because cameras of this kind cannot be asked where they are pointed —
    /// a preset recall is sent and not acknowledged. Whoever asked for privacy is the only thing
    /// that knows it is in force, and inventing a feedback here would mean a device reporting a
    /// position it merely hopes it reached.</para>
    /// </remarks>
    public interface IHasCameraPrivacy : IHasCameraControls
    {
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
